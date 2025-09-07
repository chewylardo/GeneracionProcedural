using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelinTerrain : MonoBehaviour
{
    public Terrain terrain;
    public float noiseHeight = 0.3f; // Altura máxima del terreno
    public float detailScale = 8f;   // Escala del ruido

    // Offsets globales
    private float randomOffsetX;
    private float randomOffsetZ;

    private TerrainData terrainData;

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Asignar un Terrain en el inspector");
            return;
        }

        terrainData = terrain.terrainData;
        GenerateOffsets();
        Create();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateOffsets();
            Create();
        }
    }

    private void GenerateOffsets()
    {
        randomOffsetX = Random.Range(0f, 1000f);
        randomOffsetZ = Random.Range(0f, 1000f);

        Debug.Log($"coordenadas: X = {randomOffsetX}, Z = {randomOffsetZ}");
    }
    public void Create()
    {
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        float[,] heights = new float[width, height];

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float xCoord = (float)x / width * detailScale + randomOffsetX;
                float zCoord = (float)z / height * detailScale + randomOffsetZ;

                float sample = Mathf.PerlinNoise(xCoord, zCoord) * noiseHeight;
                heights[z, x] = sample;

                // Actualizar min y max
                if (sample < minHeight) minHeight = sample;
                if (sample > maxHeight) maxHeight = sample;
            }
        }

        terrainData.SetHeights(0, 0, heights);

        // Debug de min y max
        Debug.Log($"Altura mínima: {minHeight}, Altura máxima: {maxHeight}");
    }

}
