using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeDistributor : MonoBehaviour
{
    public GM gameManager;
    [Header("Tree Settings")]
    public LSystemPlant treeTemplate;   // Arrastra un GameObject que tenga el script LSystemPlant
    public int treeCount = 20;          // N�mero de �rboles a colocar
    
    [Header("Random Seed")]
    public bool useRandomSeed = true;
    public int seed = 12345;            // Semilla para distribuci�n

    [Header("Terrain Settings")]
    public Terrain targetTerrain;       // Terreno donde colocar
    public float minHeight = 0.1f;      // Altura m�nima normalizada (0-1)
    public float maxHeight = 0.6f;      // Altura m�xima normalizada (0-1)

    private List<GameObject> spawnedTrees = new List<GameObject>();

    private List<Vector2> occupiedPositions = new List<Vector2>();
    public float minDistanceBetweenTrees = 2f; // distancia m�nima entre �rboles


    void Start()
    {
        DistributeTrees();
    }

    public void DistributeTrees()
    {
        // limpiar si ya existen
        foreach (var tree in spawnedTrees)
        {
            if (tree != null) Destroy(tree);
        }
        spawnedTrees.Clear();

        if (useRandomSeed)
        {
            seed = Random.Range(0, 99999); // Genera semilla aleatoria
        }
        Random.InitState(seed);

        if (targetTerrain == null)
        {
            Debug.LogError("Asigna un Terrain en TreeDistributor");
            return;
        }

        TerrainData tData = targetTerrain.terrainData;
        Vector3 terrainPos = targetTerrain.transform.position;

        for (int i = 0; i < treeCount; i++)
        {
            bool placed = false;
            int attempts = 0;
            const int maxAttempts = 50;


            while (!placed && attempts < maxAttempts)
            {
                attempts++;

                float posX = Random.Range(0, tData.size.x);
                float posZ = Random.Range(0, tData.size.z);
                Vector2 newPos2D = new Vector2(posX, posZ);

                // Verificar distancia m�nima
                bool tooClose = false;
                foreach (var pos in occupiedPositions)
                {
                    if (Vector2.Distance(pos, newPos2D) < minDistanceBetweenTrees)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                float worldX = terrainPos.x + posX;
                float worldZ = terrainPos.z + posZ;
                float terrainHeight = targetTerrain.SampleHeight(new Vector3(worldX, 0, worldZ)) + terrainPos.y;

                float normalizedHeight = (terrainHeight - terrainPos.y) / tData.size.y;

                if (normalizedHeight >= minHeight && normalizedHeight <= maxHeight)
                {
                    GameObject newTree = Instantiate(treeTemplate.gameObject);
                    newTree.transform.position = new Vector3(worldX, terrainHeight, worldZ);

                    LSystemPlant lsys = newTree.GetComponent<LSystemPlant>();
                    lsys.seed = seed;
                    lsys.useRandomSeed = false;
                    lsys.RegenerateTree();

                    spawnedTrees.Add(newTree);
                    occupiedPositions.Add(newPos2D);
                    placed = true;
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"No se pudo colocar �rbol #{i} dentro de rango de altura tras {maxAttempts} intentos");
            }
        }

        gameManager.AddTrees();

        Debug.Log($"{spawnedTrees.Count} �rboles generados con seed {seed}");
    }
    public void TextToSeed(string txt)
    {
        int NumberSeed = Convert.ToInt32(txt);
        seed = NumberSeed;
    }

    public void RandomSeed(bool state)
    {
        useRandomSeed = state;
    }
}
