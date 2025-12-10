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
    public float verticalSpacing = 5f; 

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

        // ---- MARKOV ----
        var markov = gameObject.AddComponent<MarkovGenerator>();
        markov.N = Mathf.Max(1, markovN);
        markov.mapWidth = generatedWidth;
        markov.mapHeight = generatedHeight;
        markov.Train(levels);

        // ---- WFC ----
        var wfc = gameObject.AddComponent<WFCGenerator>();
        wfc.patternSize = Mathf.Clamp(wfcPatternSize, 1, 6);
        wfc.maxAttempts = Mathf.Max(1, wfcMaxAttempts);
        wfc.useFixedSeed = wfcUseFixedSeed;
        wfc.fixedSeed = wfcFixedSeed;
        wfc.Train(levels);

      
        List<char[,]> markovMaps = new List<char[,]>();
        List<char[,]> wfcMaps = new List<char[,]>();

        for (int i = 0; i < 200; i++)
            markovMaps.Add(markov.Generate(generatedWidth, generatedHeight));

        for (int i = 0; i < 200; i++)
        {
            var w = wfc.Generate(generatedWidth, generatedHeight);
            if (w == null)
                w = markovMaps[rng.Next(markovMaps.Count)];
            wfcMaps.Add(w);
        }

        // Guardar mapas con dificultad
        SaveMapsToTxt(markovMaps, wfcMaps);

        
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

    string GetDifficultyLabel(char[,] map)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        int enemies = 0;
        int emptyCells = 0;

  
        // CONTAR ENEMIGOS Y ESPACIO VACÍO
      
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                char c = map[x, y];

                if (c == '3') enemies++;
                if (c == '0') emptyCells++;
            }
        }

        
        // SCORE FINAL
       
        float score = 0f;

        score += enemies * 2.0f;    
        score -= emptyCells * 0.5f; 

        if (score < 0) score = 0;  

        
        // ETIQUETAS
       
        if (score < 2) return "FACIL";
        if (score < 4) return "MEDIO";
        if (score < 6) return "DIFICIL";
        return "EXPERTO";
    }





}
