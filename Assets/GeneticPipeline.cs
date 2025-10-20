using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class GeneticPipeline : MonoBehaviour
{
    [Header("Referencias")]
    public MapVisualizer visualizer;
    public Fitness1 fitnessComponent; 
    public TextMeshProUGUI logText;


    [Header("Parámetros GA")]
    public int populationSize = 10;
    public int generations = 10;
    public float mutationRate = 0.1f;

    [Header("Textos")]
    public TextMeshProUGUI TxtPopulation;
    public TextMeshProUGUI TxtGeneration;
    public TextMeshProUGUI Txtmutation;

    [Header("Seed")]
    public int seed = -1;

    private List<MapData> population = new List<MapData>();
    private MapData bestSoFar;
    private float bestFitness = float.NegativeInfinity;

   

    public void InitPopulationFromBackward(MapData baseMap)
    {
        population.Clear();
        bestSoFar = null;
        bestFitness = float.NegativeInfinity;

        if (seed != -1) Random.InitState(seed);

        for (int i = 0; i < populationSize; i++)
        {
            MapData clone = baseMap.Clone();
            MutateMap(clone);
            population.Add(clone);
        }

        Debug.Log($"[GA] Corriendo GA - Población inicial creada desde Backward, Seed={seed}");
    }

    void MutateMap(MapData map)
    {
        foreach (var box in map.boxes.Select((b, i) => i).ToList())
        {
            if (Random.value < mutationRate)
            {
                Vector2Int d = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }[Random.Range(0, 4)];
                Vector2Int newPos = map.boxes[box] + d;
                if (map.EstaAdentro(newPos) && !map.EsUnaPared(newPos) && !map.boxes.Contains(newPos))
                    map.boxes[box] = newPos;
            }
        }

        if (Random.value < mutationRate)
        {
            Vector2Int d = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }[Random.Range(0, 4)];
            Vector2Int newPos = map.playerPos + d;
            if (map.EstaAdentro(newPos) && !map.EsUnaPared(newPos) && !map.boxes.Contains(newPos))
                map.playerPos = newPos;
        }
    }

    public IEnumerator RunGACoroutine()
    {
        for (int gen = 0; gen < generations; gen++)
        {
            float genBestFitness = float.NegativeInfinity;
            MapData genBest = null;

            foreach (var map in population)
            {
                float fit = fitnessComponent.Evaluate(map); 

                if (fit > genBestFitness)
                {
                    genBestFitness = fit;
                    genBest = map.Clone();
                }
            }

            bestSoFar = genBest.Clone();
            bestFitness = genBestFitness;

            Debug.Log($"[GA] Corriendo GA - Generación {gen + 1}/{generations}, Mejor fitness: {bestFitness}");
            logText.text = $"[GA] Corriendo GA - Generación {gen + 1}/{generations}, Mejor fitness: {bestFitness}";

            if (visualizer != null)
                visualizer.ShowMap(bestSoFar);

      

            List<MapData> newPop = new List<MapData>();
            newPop.Add(bestSoFar.Clone()); 

            while (newPop.Count < populationSize)
            {
                MapData parentA = TournamentSelection();
                MapData parentB = TournamentSelection();
                MapData child = Crossover(parentA, parentB);
                MutateMap(child);
                newPop.Add(child);
            }

            population = newPop;

            yield return new WaitForSeconds(1f);
        }

        Debug.Log($"[GA] Corriendo GA - Completado. Mejor fitness total: {bestFitness}");
        logText.text += $"[GA] Completado. Mejor fitness total: {bestFitness}\n";
    }

    MapData TournamentSelection()
    {
        int tournamentSize = Mathf.Min(3, population.Count);
        MapData best = null;
        float bestFit = float.NegativeInfinity;

        for (int i = 0; i < tournamentSize; i++)
        {
            MapData cand = population[Random.Range(0, population.Count)];
            float f = fitnessComponent.Evaluate(cand); 

            if (f > bestFit)
            {
                bestFit = f;
                best = cand;
            }
        }

        return best.Clone();
    }

    MapData Crossover(MapData a, MapData b)
    {
        MapData child = a.Clone();

        for (int i = 0; i < child.boxes.Count; i++)
        {
            if (i >= child.boxes.Count / 2) child.boxes[i] = b.boxes[i];
        }

        for (int i = 0; i < child.internalWalls.Count; i++)
        {
            if (i >= child.internalWalls.Count / 2) child.internalWalls[i] = b.internalWalls[i];
        }

        child.playerPos = (Random.value < 0.5f) ? a.playerPos : b.playerPos;

        return child;
    }

    public MapData EliteToMapData() => bestSoFar.Clone();

    public void AddPopulation()
    {
        populationSize++;
        TxtPopulation.text = $"{populationSize}\nTamaño de población";
    }
    public void SubstractPopulation()
    {
        populationSize--;
        TxtPopulation.text = $"{populationSize}\nTamaño de población";
    }
    public void AddGeneration()
    {
        generations++;
        TxtGeneration.text = $"{generations}\nTamaño de generación";
    }
    public void SubstractGeneration()
    {
        generations--;
        TxtGeneration.text = $"{generations}\nTamaño de generación";
    }
    public void AddMutation()
    {
        mutationRate += 0.05f;
        Txtmutation.text = $"{mutationRate}\nN° de Mutaciones";
    }
    public void SubstracMuatation()
    {
        if( mutationRate > 0 ) mutationRate -= 0.05f;
        Txtmutation.text = $"{mutationRate}\nN° de Mutaciones";
    }
}
