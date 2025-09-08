using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeDistributor : MonoBehaviour
{
    public GM gameManager;
    [Header("Tree Settings")]
    public LSystemPlant treeTemplate;
    public int Iterations = 2;
    public int treeCount = 20;          // Número de árboles a colocar
    
    [Header("Random Seed")]
    public bool useRandomSeed = true;
    public int seed = 12345;            // Semilla para distribución

    [Header("Terrain Settings")]
    public Terrain targetTerrain;       // Terreno donde colocar
    public float minHeight = 0.1f;      // Altura mínima normalizada (0-1)
    public float maxHeight = 0.6f;      // Altura máxima normalizada (0-1)

    private List<GameObject> spawnedTrees = new List<GameObject>();

    public List<Vector2> occupiedPositions = new List<Vector2>();
    public float minDistanceBetweenTrees = 2f; // distancia mínima entre árboles


    [Header("Textos")]
    public TMP_InputField TreeCount;
    

    void Start()
    {
        if (TreeCount != null) TreeCount.text = treeCount.ToString();
        //DistributeTrees();
    }

    public void DistributeTrees()
    {
        // limpiar árboles previos
        foreach (var tree in spawnedTrees)
        {
            if (tree != null) Destroy(tree);
        }
        spawnedTrees.Clear();
        occupiedPositions.Clear();

        if (useRandomSeed)
        {
            seed = UnityEngine.Random.Range(0, 99999); // Semilla inicial
        }

        System.Random posRand = new System.Random(seed);

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

                float posX = (float)(posRand.NextDouble() * tData.size.x);
                float posZ = (float)(posRand.NextDouble() * tData.size.z);
                Vector2 newPos2D = new Vector2(posX, posZ);

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
                    Vector3 candidatePos = new Vector3(worldX, terrainHeight, worldZ);

                    // --- Comprobación 1: árboles instanciados por este script ---
                    bool objectNearby = false;
                    foreach (var tree in spawnedTrees)
                    {
                        if (tree == null) continue;
                        if (Vector3.Distance(tree.transform.position, candidatePos) < minDistanceBetweenTrees)
                        {
                            Debug.LogWarning("oh no arbol");
                            objectNearby = true;
                            break;
                        }
                    }
                    if (objectNearby) continue;

                    // --- Comprobación 2: cualquier otro objeto en la escena (requiere colliders) ---
                    Collider[] colliders = Physics.OverlapSphere(candidatePos, minDistanceBetweenTrees);
                    foreach (var col in colliders)
                    {
                        if (col.gameObject == targetTerrain.gameObject) continue; // ignorar el terreno
                        objectNearby = true;
                        Debug.LogWarning("oh no un objeto");
                        break;
                    }
                    if (objectNearby) continue;

                    // --- Instanciamos el árbol si pasa ambas comprobaciones ---
                    GameObject newTree = Instantiate(treeTemplate.gameObject);
                    newTree.transform.position = new Vector3(worldX, terrainHeight, worldZ);

                    LSystemPlant lsys = newTree.GetComponent<LSystemPlant>();
                    lsys.seed = seed;
                    lsys.useRandomSeed = false;
                    lsys.Iterations = Iterations;
                    lsys.RegenerateTree();

                    spawnedTrees.Add(newTree);
                    occupiedPositions.Add(newPos2D);
                    placed = true;
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"No se pudo colocar árbol #{i} dentro de rango de altura tras {maxAttempts} intentos");
            }
        }

        gameManager.AddTrees();
        Debug.Log($"{spawnedTrees.Count} árboles generados con seed {seed}");
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
    public void addIteracion()
    {
        Iterations++;
    }

    public void RemoveIteration()
    {
        Iterations--;
    }

    public void CountTrees(string count)
    {
        int quantity = Convert.ToInt32(count);
        treeCount = quantity;
    }
}
