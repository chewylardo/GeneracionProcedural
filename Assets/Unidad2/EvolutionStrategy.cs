using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.PlayerSettings;

public class EvolutionStrategy : MonoBehaviour
{
    public int mu = 5;                  // Número de padres (población base que se mantiene)
    public int lambda = 10;             // Número de hijos generados en cada generación 
    public int maxGenerations = 20;     // Número máximo de generaciones
    public float mutationRate = 0.1f;   // Intensidad de la mutación

    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform exitPoint;
    public float simulationTime = 10f;

    private List<EnemyParametersAi> population;     // Lista que guarda la población actual


    void Start()
    {
        InitializePopulation();         // Inicializa la población con individuos aleatorios
        StartCoroutine(RunEvolution()); // Inicia la simulación evolutiva como una corrutina
    }

    // Crea la población inicial aleatoria
    void InitializePopulation()
    {
        population = new List<EnemyParametersAi>();
        for (int i = 0; i < mu; i++)
        {
            population.Add(new EnemyParametersAi
            {
                speed = Random.Range(2f, 5f),
                aggression = Random.Range(0f, 1f),
                visionRange = Random.Range(2f, 5f),
                fitness = 0f
            });
        }
    }

    IEnumerator RunEvolution()
    {
        for (int gen = 0; gen < maxGenerations; gen++)
        {
            // Lista para almacenar hijos generados
            List<EnemyParametersAi> offspring = new List<EnemyParametersAi>();

            // Generar lambda hijos a partir de los mu padres
            for (int i = 0; i < lambda; i++)
            {
                EnemyParametersAi parent = population[Random.Range(0, mu)]; // Selección aleatoria de un padre
                EnemyParametersAi child = parent.Clone();                   // Se clona el padre
                Mutate(child);                                              // Se muta el hijo
                offspring.Add(child);                                       // Se agrega a la lista de descendencia
            }

            // Instanciar enemigos en la escena
            List<EnemyController> activeEnemies = new List<EnemyController>();
            foreach (var ind in offspring)
            {
                GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                EnemyController controller = enemyObj.GetComponent<EnemyController>();
                controller.Initialize(ind, exitPoint, simulationTime);
                activeEnemies.Add(controller);
            }

            // Esperar a que todos terminen
            bool allFinished = false;
            while (!allFinished)
            {
                allFinished = true;
                foreach (var e in activeEnemies)
                    if (!e.finished) allFinished = false;
                yield return null;
            }

            // Recoger fitness
            for (int i = 0; i < offspring.Count; i++)
            {
                offspring[i].fitness = activeEnemies[i].fitness;
            }

            // Selección (mu + lambda)
            // Se juntan padres + hijos, se ordenan por fitness, y se seleccionan los mejores 
            population.AddRange(offspring);
            population.Sort((a, b) => b.fitness.CompareTo(a.fitness));  // Orden descendente por fitness
            population = population.GetRange(0, mu);                    // Solo se quedan los mejores mu

            Debug.Log($"Generación {gen + 1} mejor fitness: {population[0].fitness}");
        }
    }

    // Función de mutación: cambia los valores de los parámetros de un individuo
    void Mutate(EnemyParametersAi ind)
    {
        ind.speed += Random.Range(-mutationRate, mutationRate);
        ind.aggression += Random.Range(-mutationRate, mutationRate);
        ind.visionRange += Random.Range(-mutationRate, mutationRate);

        ind.speed = Mathf.Clamp(ind.speed, 1f, 5f);
        ind.aggression = Mathf.Clamp01(ind.aggression);
        ind.visionRange = Mathf.Clamp(ind.visionRange, 1f, 10f);
    }
}
