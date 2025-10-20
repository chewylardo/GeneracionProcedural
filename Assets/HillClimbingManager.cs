using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HillClimbingManager : MonoBehaviour
{
    [Header("Referencias")]
    public Backward backwardGenerator;
    public Fitness2 fitnessComponent;
    public MapVisualizer visualizer;

    [Header("Parámetros Hill Climbing")]
    public int maxIterations = 20;
    public int neighborsPerStep = 20;

    private MapData bestSoFar;
    private float bestFitness = float.NegativeInfinity;
    private List<MapData> allmaps = new List<MapData>();

    [Header("Textos")]
    public TextMeshProUGUI bestfit;
    public TextMeshProUGUI TxtIterations;
    public TextMeshProUGUI logText;

    [Header("Seed (reproducibilidad)")]
    public int seed = -1;

    public void StartHillClimbing()
    {
        StartCoroutine(EsperarYComenzar());
    }

    private IEnumerator EsperarYComenzar()
    {
        yield return new WaitUntil(() => backwardGenerator != null && backwardGenerator.generado);
        yield return new WaitForSeconds(0.2f);

        if (seed == -1)
            seed = backwardGenerator.seed;

        Debug.Log($"[HillClimbing] Usando seed = {seed}");
        Random.InitState(seed);

        MapData initial = backwardGenerator.GenerarMapData();
        if (visualizer != null) visualizer.ShowMap(initial);

        StartCoroutine(RunHillClimbing(initial));
    }

    public IEnumerator RunHillClimbing(MapData initial)
    {
        MapData current = initial.Clone();
        float currentFitness = fitnessComponent.Evaluate(current);
        bestSoFar = current.Clone();
        bestFitness = currentFitness;

        for (int i = 0; i < maxIterations; i++)
        {
            MapData bestNeighbor = null;
            float bestNeighborFit = float.NegativeInfinity;

            MapData neighbor = GenerateNeighbor(current);
            float fit = fitnessComponent.Evaluate(neighbor);
            bestfit.text = $"Actual fitness = {fit}\nMejor fitness: {bestFitness}";

            if (fit > bestNeighborFit)
            {
                bestNeighborFit = fit;
                bestNeighbor = neighbor;
            }

            if (bestNeighborFit > currentFitness)
            {
                current = bestNeighbor;
                currentFitness = bestNeighborFit;
                bestSoFar = current.Clone();
                bestFitness = currentFitness;
            }

            Debug.Log($"[HillClimbing] Iteración {i + 1}, Fitness actual: {fit}, Mejor fitness: {bestFitness}");
            logText.text = $"[HillClimbing] Iteración {i + 1}, Fitness actual: {fit}, Mejor fitness: {bestFitness}\n";

            if (visualizer != null)
            {
                visualizer.ShowMap(neighbor);
                yield return new WaitForSeconds(1f);
            }
        }

        Debug.Log($"Hill Climbing completado. Mejor fitness: {bestFitness}");
        logText.text = $"Hill Climbing completado. Mejor fitness: {bestFitness}\n";
        if (visualizer != null) visualizer.ShowMap(bestSoFar);
    }

    MapData GenerateNeighbor(MapData current)
    {
        MapData n = current.Clone();
        int op = Random.Range(0, 3);

        if (op == 0 && n.boxes.Count > 0)
        {
            int idx = Random.Range(0, n.boxes.Count);
            Vector2Int box = n.boxes[idx];
            Vector2Int d = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }[Random.Range(0, 4)];
            Vector2Int newPos = box + d;

            // evita esquinas y paredes cercanas
            if (n.EstaAdentro(newPos) && !n.EsUnaPared(newPos) && !n.boxes.Contains(newPos))
            {
                if (!IsNextToWallOrCorner(n, newPos))
                    n.boxes[idx] = newPos;
            }
        }
        else if (op == 1 && n.internalWalls.Count > 0)
        {
            int idx = Random.Range(0, n.internalWalls.Count);
            Vector2Int pos = n.internalWalls[idx];
            Vector2Int d = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }[Random.Range(0, 4)];
            Vector2Int dest = pos + d;
            if (n.EstaAdentro(dest) && !n.EsUnaPared(dest) && !n.boxes.Contains(dest))
                n.internalWalls[idx] = dest;
        }
        else
        {
            Vector2Int d = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }[Random.Range(0, 4)];
            Vector2Int np = n.playerPos + d;
            if (n.EstaAdentro(np) && !n.EsUnaPared(np) && !n.boxes.Contains(np))
                n.playerPos = np;
        }

        return n;
    }

    //Función auxiliar agregada (sin modificar otras partes del código)
    bool IsNextToWallOrCorner(MapData map, Vector2Int pos)
    {
        // Al lado de una pared
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        int walls = 0;
        foreach (var d in dirs)
        {
            if (map.EsUnaPared(pos + d)) walls++;
        }

        // 1 pared = pegado, 2 paredes = esquina
        return walls >= 1;
    }

    public void AddIterations()
    {
        maxIterations++;
        TxtIterations.text = $"{maxIterations}\nN° de Iteraciones";
    }
    public void SubstractIterations()
    {
        maxIterations--;
        TxtIterations.text = $"{maxIterations}\nN° de Iteraciones";
    }
    public void CleanTxt()
    {
        bestfit.text = "";
        TxtIterations.text = "";
    }
}
