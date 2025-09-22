using System.Collections;
using System.Collections.Generic;
using System.Linq; // Para usar Select, OrderBy, Take, etc.
using UnityEngine;

public class GeneticaMapa : MonoBehaviour
{
    // dimensiones del mapa
    public int largo = 20;
    public int alto = 20;

    // parametros del algoritmo genético
    public int populationSize = 20; // Cantidad de mapas por generación
    public int generations = 50;    // Cantidad de generaciones a simular

    [Header("Referencias")]
    public ConstructorMapa builder;  

    // Lista que de la población actual de genotipos (mapas)
    private List<GenotipoMapa> population;

    
    void Start()
    {
        // inicializa la población con mapas aleatorios
        population = new List<GenotipoMapa>();
        for (int i = 0; i < populationSize; i++)
        {
            population.Add(new GenotipoMapa(largo, alto, true)); // true = inicialización aleatoria
        }

        // va a la  rutina evolutiva
        StartCoroutine(Evolve());
    }

    // corrutina que maneja el proceso de evolución del algoritmo genético
    System.Collections.IEnumerator Evolve()
    {
        for (int gen = 0; gen < generations; gen++) // recorre cada generacion
        {
            // evalua el fitness de cada mapa en la poblacion
            var score = population
                .Select(g => new { genome = g, fit = Fitness.Evaluar(g) }) // asigna puntuacion
                .OrderByDescending(x => x.fit) // ordena de mayor a menor fitness
                .ToList();

          
            Debug.Log($"Gen {gen} | Best fitness: {score[0].fit}");

            // construye visualmente el mejor mapa encontrado en esta generación, para que se vea en unity
            builder.Construir(score[0].genome);

          
            // elige la mitad superior de la población (la "elite")
            var elites = score.Take(populationSize / 2).Select(x => x.genome).ToList();

          
            // se crea una generacion nueva
            List<GenotipoMapa> nextGen = new List<GenotipoMapa>(elites); // la generacion parte con los elites
            while (nextGen.Count < populationSize)
            {
                // se elige dos padres aleatoriamente de los elites
                GenotipoMapa padreA = elites[Random.Range(0, elites.Count)];
                GenotipoMapa padreB = elites[Random.Range(0, elites.Count)];

                // crea un "hijo" haciendo crossover y aplicando mutacion para crear un poco de azarocidad
                GenotipoMapa hijo = GenotipoMapa.Crossover(padreA, padreB).Mutate();

                // añade el hijo recien creado a una nueva generacion
                nextGen.Add(hijo);
            }

            // la nueva generacion se transforma en la generacion actual
            population = nextGen;

            // esto esta para esperar y poder ver los cambios
            yield return new WaitForSeconds(0.1f);
        }
    }
}
