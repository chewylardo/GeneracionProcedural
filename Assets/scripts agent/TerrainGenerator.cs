using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int totalSteps = 2000;          // pasos en total
    public float stepDelay = 0.05f;        // segundos entre pasos
    public int numMountainAgents = 5;
    public int numSmoothingAgents = 5;

    private List<TerrainAgent> agents;
    private Terrain terrain;
    private float[,] heights;
    private int res;

    void Start()
    {
        terrain = Terrain.activeTerrain;
        res = terrain.terrainData.heightmapResolution;
        heights = new float[res, res];

        agents = new List<TerrainAgent>();

        // Crear agentes de montaña
        for (int i = 0; i < numMountainAgents; i++)
        {
            agents.Add(new MountainAgent
            {
                x = Random.Range(0, res),
                y = Random.Range(0, res)
            });
        }

        // Crear agentes de suavizado
        for (int i = 0; i < numSmoothingAgents; i++)
        {
            agents.Add(new SmoothingAgent
            {
                x = Random.Range(0, res),
                y = Random.Range(0, res)
            });
        }

        // Iniciar corutina
        StartCoroutine(GenerateStepByStep());
    }

    IEnumerator GenerateStepByStep()
    {
        for (int step = 0; step < totalSteps; step++)
        {
            foreach (var agent in agents)
                agent.Step(heights);

            // aplicar cambios al terreno
            terrain.terrainData.SetHeights(0, 0, heights);

            // esperar un poquito antes del siguiente paso
            yield return new WaitForSeconds(stepDelay);
        }
    }
}
