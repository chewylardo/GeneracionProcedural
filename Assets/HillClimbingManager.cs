using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HillClimbingManager : MonoBehaviour
{
    [Header("Referencias")]
    public Backward backwardGenerator; // generador de mapas
    public Fitness2 fitnessComponent;  // función de evaluación
    public MapVisualizer visualizer;   // visualizador

    [Header("Parámetros Hill Climbing")]
    public int maxIterations = 20;    // iteraciones máximas
    public int neighborsPerStep = 20;  // vecinos a evaluar por paso

    private MapData bestSoFar;         // mejor mapa encontrado
    private float bestFitness = float.NegativeInfinity;
    private List<MapData> allmaps = new List<MapData>();

    public TextMeshProUGUI bestfit;


    //Espera a que se genere el mapa inicial
    IEnumerator Start()
    {
        yield return new WaitUntil(() => backwardGenerator != null && backwardGenerator.generado);
        yield return new WaitForSeconds(0.2f);

        MapData initial = backwardGenerator.GenerarMapData(); // mapa inicial
        if (visualizer != null) { 
            visualizer.ShowMap(initial); 
        }

        //Ejecuta Hill Climbing
        StartCoroutine(RunHillClimbing(initial));
    }

    //Algoritmo Hill Climbing (Steepest Ascent)
    IEnumerator RunHillClimbing(MapData initial)
    {
        MapData current = initial.Clone();
        float currentFitness = fitnessComponent.Evaluate(current);
        bestSoFar = current.Clone();
        bestFitness = currentFitness;
        for (int i = 0; i < maxIterations; i++)
        {
            allmaps.Add(GenerateNeighbor(current));
        
        }

        for (int i = 0; i < maxIterations; i++)
        {
            MapData bestNeighbor = null;
            float bestNeighborFit = float.NegativeInfinity;

            MapData neighbor = allmaps[i];
            float fit = fitnessComponent.Evaluate(neighbor);
            bestfit.text = $"Actual fitness = {fit}\nMejor fitness: {bestFitness}";

            if (fit > bestNeighborFit)
            {
                bestNeighborFit = fit;
                bestNeighbor = neighbor;
            }

            //Si encontramos un vecino mejor, lo adoptamos
            if (bestNeighborFit > currentFitness)
            {
                current = bestNeighbor;
                currentFitness = bestNeighborFit;
                bestSoFar = current.Clone();
                bestFitness = currentFitness;
            }

            //Visualización cada 1s
            if (visualizer != null)
            {
                visualizer.ShowMap(neighbor);
                yield return new WaitForSeconds(1f);
            }
        }

        Debug.Log($"Hill Climbing completado. Mejor fitness: {bestFitness}");
        //bestfit.text = $"Mejor fitness: {bestFitness}";

        if (visualizer != null) { 
            visualizer.ShowMap(bestSoFar);
        }
    }

    //Genera un vecino modificando aleatoriamente caja, muro o jugador
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
            if (n.EstaAdentro(newPos) && !n.EsUnaPared(newPos) && !n.boxes.Contains(newPos))
            { 
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
            {
                n.internalWalls[idx] = dest;
            }
        }
        else
        {
            Vector2Int d = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }[Random.Range(0, 4)];
            Vector2Int np = n.playerPos + d;
            if (n.EstaAdentro(np) && !n.EsUnaPared(np) && !n.boxes.Contains(np))
            {
                n.playerPos = np;
            }
        }

        return n;
    }
}
