using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelinTerrain : MonoBehaviour
{
    public Terrain terrain; // El terrain ya existente en la escena
    public float noiseHeight = 0.3f; // Altura máxima del terreno
    public float detailScale = 8f;   // Escala del ruido
    public float randomOffsetRange = 1000f;

    private TerrainData terrainData;

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Asignar un Terrain en el inspector");
            return;
        }

        terrainData = terrain.terrainData;
        Create();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Create();
        }
    }

    public void Create()
    {
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        float[,] heights = new float[width, height];

        float randomOffsetX = Random.Range(0f, randomOffsetRange);
        float randomOffsetZ = Random.Range(0f, randomOffsetRange);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float xCoord = (float)x / width * detailScale + randomOffsetX;
                float zCoord = (float)z / height * detailScale + randomOffsetZ;

                float sample = Mathf.PerlinNoise(xCoord, zCoord) * noiseHeight;
                heights[z, x] = sample;
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }
}
