using System.Collections.Generic;
using UnityEngine;

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

    private System.Random rng = new System.Random();

    void Start()
    {
        RunFullFlow();
    }

    public void RunFullFlow()
    {
        // Clear previous renderings
        renderer.ClearRendered();

        var levels = loader.LoadLevels();
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("[Controller] No hay niveles cargados en LevelLoader.");
            return;
        }

        // TRAIN MARKOV
        var markov = gameObject.AddComponent<MarkovGenerator>();
        markov.N = Mathf.Max(1, markovN);
        markov.mapWidth = generatedWidth;
        markov.mapHeight = generatedHeight;
        markov.Train(levels);

        // TRAIN WFC
        // TRAIN WFC
        var wfc = gameObject.AddComponent<WFCGenerator>();
        wfc.patternSize = Mathf.Clamp(wfcPatternSize, 1, 6);
        wfc.maxAttempts = Mathf.Max(1, wfcMaxAttempts);
        wfc.useFixedSeed = wfcUseFixedSeed;
        wfc.fixedSeed = wfcFixedSeed;
        wfc.Train(levels);


        // GENERATE 30 maps each
        List<char[,]> markovMaps = new List<char[,]>();
        List<char[,]> wfcMaps = new List<char[,]>();

        for (int i = 0; i < 30; i++)
        {
            var m = markov.Generate(generatedWidth, generatedHeight);
            markovMaps.Add(m);
        }

        for (int i = 0; i < 30; i++)
        {
            var w = wfc.Generate(generatedWidth, generatedHeight);
            if (w == null)
            {
                // in case WFC failed for this attempt, fallback to a random markov map to avoid nulls
                w = markovMaps[rng.Next(markovMaps.Count)];
            }
            wfcMaps.Add(w);
        }

        // choose one random from each set
        var chosenMarkov = markovMaps[rng.Next(markovMaps.Count)];
        var chosenWfc = wfcMaps[rng.Next(wfcMaps.Count)];

        // render: Markov arriba (offset 0), WFC abajo (offset negative)
        renderer.Render(chosenMarkov, new Vector3(0, 0, 0), "Markov_Map");
        renderer.Render(chosenWfc, new Vector3(0, -(generatedHeight * renderer.spacing) - verticalSpacing, 0), "WFC_Map");

        Debug.Log("[Controller] Renderizados: Markov (arriba) y WFC (abajo).");
    }
}
