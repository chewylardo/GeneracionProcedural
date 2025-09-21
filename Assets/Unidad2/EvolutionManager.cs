using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionManager : MonoBehaviour
{
    [Header("Prefabs y puntos")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform goalPoint;
    public List<Transform> towers = new List<Transform>();

    [Header("ES Settings")]
    public int mu = 5;
    public int lambda = 10;
    public int maxGenerations = 5;
    public float mutationRate = 0.1f;
    public float evalDuration = 10f; // tiempo de simulación por generación

    private List<EnemyParameters> population = new List<EnemyParameters>();

    private void Start()
    {
        InitializePopulation();
        StartCoroutine(RunESCoroutine());
    }

    void InitializePopulation()
    {
        population.Clear();
        for (int i = 0; i < mu; i++)
        {
            population.Add(new EnemyParameters
            {
                speed = Random.Range(1f, 5f),
                aggression = Random.Range(0f, 1f),
                visionRange = Random.Range(5f, 15f),
                fitness = 0f
            });
        }
    }

    public IEnumerator RunESCoroutine()
    {
        for (int generation = 0; generation < maxGenerations; generation++)
        {
            List<EnemyParameters> offspring = new List<EnemyParameters>();

            for (int i = 0; i < lambda; i++)
            {
                EnemyParameters parent = population[Random.Range(0, population.Count)];
                EnemyParameters child = Mutate(parent);
                offspring.Add(child);
            }

            // Instanciar todos los hijos a la vez
            yield return StartCoroutine(EvaluateEnemies(offspring));

            // Selección (mu+lambda)
            population.AddRange(offspring);
            population.Sort((a, b) => b.fitness.CompareTo(a.fitness));
            population = population.GetRange(0, mu);

            Debug.Log($"Generación {generation} - Mejor fitness: {population[0].fitness}");
        }
    }

    EnemyParameters Mutate(EnemyParameters parent)
    {
        return new EnemyParameters
        {
            speed = Mathf.Clamp(parent.speed + Random.Range(-mutationRate, mutationRate), 0.1f, 10f),
            aggression = Mathf.Clamp01(parent.aggression + Random.Range(-mutationRate, mutationRate)),
            visionRange = Mathf.Clamp(parent.visionRange + Random.Range(-mutationRate * 10f, mutationRate * 10f), 1f, 30f),
            fitness = 0f
        };
    }

    IEnumerator EvaluateEnemies(List<EnemyParameters> npcs)
    {
        List<GameObject> spawned = new List<GameObject>();
        float startDist = Vector3.Distance(spawnPoint.position, goalPoint.position);

        // Instanciar todos los enemigos en círculo alrededor del spawnPoint
        for (int i = 0; i < npcs.Count; i++)
        {
            float angle = i * Mathf.PI * 2f / npcs.Count; // ángulo para cada enemigo
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 3f; // radio = 3f

            GameObject e = Instantiate(enemyPrefab, spawnPoint.position + offset, Quaternion.identity);
            EnemyAI ai = e.GetComponent<EnemyAI>();
            ai.Setup(npcs[i], goalPoint.position, towers);
            spawned.Add(e);
        }

        // Simular por evalDuration
        yield return new WaitForSeconds(evalDuration);

        // Calcular fitness de cada uno
        for (int i = 0; i < npcs.Count; i++)
        {
            if (spawned[i] != null)
            {
                float dist = Vector3.Distance(spawned[i].transform.position, goalPoint.position);
                npcs[i].fitness = Mathf.Max(0f, startDist - dist);
                Destroy(spawned[i]);
            }
            else
            {
                npcs[i].fitness = 0f; // si murió, fitness bajo
            }
        }
    }
}