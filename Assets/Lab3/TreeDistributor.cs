using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeDistributor : MonoBehaviour
{
    public GM gameManager;
    [Header("Tree Settings")]
    public LSystemPlant treeTemplate;
    public int Iterations = 2;
    public int treeCount = 20;

    [Header("Random Seed")]
    public bool useRandomSeed = true;
    public int seed = 12345;

    [Header("Terrain Settings")]
    public Terrain targetTerrain;
    public float minHeight = 0.1f;
    public float maxHeight = 0.6f;

    private List<GameObject> spawnedTrees = new List<GameObject>();
    public List<Vector2> occupiedPositions = new List<Vector2>();
    public float minDistanceBetweenTrees = 2f;

    [Header("Textos")]
    public TMP_InputField TreeCount;
    public TMP_Text seedText;


    private void Update()
    {
        
    }

    void Start()
    {
        if (TreeCount != null) TreeCount.text = treeCount.ToString();
    }

    public void DistributeTrees()
    {
        foreach (var tree in spawnedTrees)
        {
            if (tree != null) Destroy(tree);
        }
        spawnedTrees.Clear();
        occupiedPositions.Clear();

        if (useRandomSeed)
        {
            seed = UnityEngine.Random.Range(0, 99999);
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

                    bool objectNearby = false;
                    foreach (var tree in spawnedTrees)
                    {
                        if (tree == null) continue;
                        if (Vector3.Distance(tree.transform.position, candidatePos) < minDistanceBetweenTrees)
                        {
                            objectNearby = true;
                            break;
                        }
                    }
                    if (objectNearby) continue;

                    Collider[] colliders = Physics.OverlapSphere(candidatePos, minDistanceBetweenTrees);
                    foreach (var col in colliders)
                    {
                        if (col.gameObject == targetTerrain.gameObject) continue;
                        objectNearby = true;
                        break;
                    }
                    if (objectNearby) continue;

                    GameObject newTree = Instantiate(treeTemplate.gameObject);
                    newTree.transform.position = candidatePos;

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
                Debug.LogWarning($"No se pudo colocar árbol #{i} tras {maxAttempts} intentos");
            }
        }

        gameManager.AddTrees();
        Debug.Log($"{spawnedTrees.Count} árboles generados con seed {seed}");
        seedText.text = seed.ToString();
    }

    public void TextToSeed(string txt)
    {
        seed = Convert.ToInt32(txt);
    }

    public void RandomSeed(bool state)
    {
        useRandomSeed = state;
    }

    public void addIteracion() { Iterations++; }
    public void RemoveIteration() { Iterations--; }
    public void CountTrees(string count) { treeCount = Convert.ToInt32(count); }
}
