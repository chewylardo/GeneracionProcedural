using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class GenerationController : MonoBehaviour
{
    public LevelLoader loader;
    public LevelRenderer renderer;

    [Header("Generación")]
    public int generatedWidth = 150;
    public int generatedHeight = 14;
    public float verticalSpacing = 5f; // separación entre Markov arriba y WFC abajo

    [Header("WFC Params (opcionales override)")]
    public int wfcPatternSize = 2;
    public int wfcMaxAttempts = 500;
    public bool wfcUseFixedSeed = false;
    public int wfcFixedSeed = 12345;

    [Header("Markov Params")]
    public int markovN = 2;

    [Header("Carpeta donde guardar TXT (arrastra una carpeta del proyecto)")]
    public UnityEngine.Object saveFolder;

    private string saveFolderPath;
    private System.Random rng = new System.Random();

    void Start()
    {
#if UNITY_EDITOR
        if (saveFolder != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(saveFolder);

            if (AssetDatabase.IsValidFolder(assetPath))
                saveFolderPath = Path.Combine(Application.dataPath, assetPath.Replace("Assets/", ""));
            else
                saveFolderPath = Application.dataPath;
        }
        else
        {
            saveFolderPath = Application.dataPath;
        }
#else
        saveFolderPath = Application.dataPath;  
#endif

        RunFullFlow();
    }

    public void RunFullFlow()
    {
        renderer.ClearRendered();

        var levels = loader.LoadLevels();
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("[Controller] No hay niveles cargados en LevelLoader.");
            return;
        }

        // ---- TRAIN MARKOV ----
        var markov = gameObject.AddComponent<MarkovGenerator>();
        markov.N = Mathf.Max(1, markovN);
        markov.mapWidth = generatedWidth;
        markov.mapHeight = generatedHeight;
        markov.Train(levels);

        // ---- TRAIN WFC ----
        var wfc = gameObject.AddComponent<WFCGenerator>();
        wfc.patternSize = Mathf.Clamp(wfcPatternSize, 1, 6);
        wfc.maxAttempts = Mathf.Max(1, wfcMaxAttempts);
        wfc.useFixedSeed = wfcUseFixedSeed;
        wfc.fixedSeed = wfcFixedSeed;
        wfc.Train(levels);

        // ---- GENERATE 30 MAPS EACH ----
        List<char[,]> markovMaps = new List<char[,]>();
        List<char[,]> wfcMaps = new List<char[,]>();

        for (int i = 0; i < 30; i++)
            markovMaps.Add(markov.Generate(generatedWidth, generatedHeight));

        for (int i = 0; i < 30; i++)
        {
            var w = wfc.Generate(generatedWidth, generatedHeight);
            if (w == null)
                w = markovMaps[rng.Next(markovMaps.Count)];
            wfcMaps.Add(w);
        }

        // Guardar mapas con dificultad
        SaveMapsToTxt(markovMaps, wfcMaps);

        // ---- Choose one random map from each and render ----
        var chosenMarkov = markovMaps[rng.Next(markovMaps.Count)];
        var chosenWfc = wfcMaps[rng.Next(wfcMaps.Count)];

        string diffMarkov = GetDifficultyLabel(chosenMarkov);
        string diffWfc = GetDifficultyLabel(chosenWfc);

        Debug.Log($"Dificultad Markov: {diffMarkov}");
        Debug.Log($"Dificultad WFC: {diffWfc}");

        renderer.Render(chosenMarkov, new Vector3(0, 0, 0), "Markov_Map");
        renderer.Render(chosenWfc, new Vector3(0, -(generatedHeight * renderer.spacing) - verticalSpacing, 0), "WFC_Map");

        Debug.Log("[Controller] Renderizados: Markov (arriba) y WFC (abajo).");
    }

    // -----------------------------------------------------
    //           SAVE MAPS + DIFFICULTY
    // -----------------------------------------------------
    void SaveMapsToTxt(List<char[,]> markovMaps, List<char[,]> wfcMaps)
    {
        string pathMarkov = saveFolderPath + "/MarkovMaps.txt";
        string pathWFC = saveFolderPath + "/WFCMaps.txt";

        // ---- MARKOV ----
        using (StreamWriter sw = new StreamWriter(pathMarkov))
        {
            for (int i = 0; i < markovMaps.Count; i++)
            {
                string difficulty = GetDifficultyLabel(markovMaps[i]);
                sw.WriteLine($"=== MARKOV MAP {i + 1} ===");
                sw.WriteLine($"DIFICULTAD: {difficulty}");
                WriteGrid(markovMaps[i], sw);
                sw.WriteLine();
            }
        }

        // ---- WFC ----
        using (StreamWriter sw = new StreamWriter(pathWFC))
        {
            for (int i = 0; i < wfcMaps.Count; i++)
            {
                string difficulty = GetDifficultyLabel(wfcMaps[i]);
                sw.WriteLine($"=== WFC MAP {i + 1} ===");
                sw.WriteLine($"DIFICULTAD: {difficulty}");
                WriteGrid(wfcMaps[i], sw);
                sw.WriteLine();
            }
        }

        Debug.Log($"TXT generados en:\n{pathMarkov}\n{pathWFC}");
    }

    void WriteGrid(char[,] grid, StreamWriter sw)
    {
        int w = grid.GetLength(0);
        int h = grid.GetLength(1);

        for (int y = h - 1; y >= 0; y--)
        {
            string line = "";
            for (int x = 0; x < w; x++)
                line += grid[x, y];
            sw.WriteLine(line);
        }
    }

    //           DIFFICULTY ANALYSIS SYSTEM
   
    string GetDifficultyLabel(char[,] map)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

     
        // CONTAR ENEMIGOS
      
        int enemies = 0;
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (map[x, y] == '3')
                    enemies++;

     
        // VARIACIÓN DE PLATAFORMAS
  
        List<int> platformHeights = new List<int>();
        for (int x = 0; x < w; x++)
        {
            int lowestPlatformY = -1;
            for (int y = 0; y < h; y++)
            {
                if (map[x, y] == '2')  // plataforma
                {
                    lowestPlatformY = y;
                    break;
                }
            }
            if (lowestPlatformY != -1)
                platformHeights.Add(lowestPlatformY);
        }

        float variation = 0f;
        if (platformHeights.Count > 1)
        {
            for (int i = 1; i < platformHeights.Count; i++)
                variation += Mathf.Abs(platformHeights[i] - platformHeights[i - 1]);
            variation /= (platformHeights.Count - 1); // promedio
        }

      
        // CONTAR HUECOS (COLUMNAS SIN PLATAFORMA)
      
        int gaps = 0;
        for (int x = 0; x < w; x++)
        {
            bool hasPlatform = false;
            for (int y = 0; y < h; y++)
            {
                if (map[x, y] == '2')
                {
                    hasPlatform = true;
                    break;
                }
            }
            if (!hasPlatform) gaps++;
        }

      
        // SCORE FINAL CON PESOS
      
        float score = enemies * 1.5f   // enemigos 
                    + variation * 2f  // variación 
                    + gaps * 4;    // huecos 

       
        // ETIQUETAS SEGÚN SCORE
       
        if (score < 10) return "FACIL";
        if (score < 20) return "MEDIO";
        if (score < 35) return "DIFICIL";
        return "EXPERTO";
    }



}
