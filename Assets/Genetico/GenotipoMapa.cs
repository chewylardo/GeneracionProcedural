using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] 
public class GenotipoMapa : MonoBehaviour
{
    // Dimensiones del mapa
    public int largo; 
    public int alto;  

    
    // 0 = muro y 1 = suelo
    public int[,] grid;

    // Constructor de la clase 
    public GenotipoMapa(int w, int h, bool randomizado = true, float sueloObjetivo = 0.4f)
    {
        largo = w;
        alto = h;
        grid = new int[w, h]; 

        // Si randomizado está en true, rellenamos el mapa de manera aleatoria si queremos un mapa aleatorio
        if (randomizado)
        {
            for (int x = 0; x < w; x++) 
            {
                for (int y = 0; y < h; y++) 
                {
                    // Si el valor aleatorio es menor que el objetivo, ponemos suelo, sino muro 
                    grid[x, y] = (Random.value < sueloObjetivo) ? 1 : 0;
                }
            }

            // se asegura de que las esquinas siempre sean suelo
            grid[0, 0] = 1;           // Esquina superior izquierda
            grid[w - 1, h - 1] = 1;   // Esquina inferior derecha
        }
    }

    // muta el mapa para generar variaciones
    // ratioDemutacion = probabilidad de que cada celda cambie
    public GenotipoMapa Mutate(float ratioDemutacion = 0.05f, float sueloObjetivo = 0.4f)
    {
        // copia del mapa original
        GenotipoMapa child = Clone();

       
        for (int x = 0; x < largo; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                
                if (Random.value < ratioDemutacion)
                {
                    // decidimos de nuevo si será suelo o muro según el objetivo
                    child.grid[x, y] = (Random.value < sueloObjetivo) ? 1 : 0;
                }
            }
        }
        return child; // devuelve el nuevo mapa mutado
    }

    // cruza dos genotipos de los mapas para crear un hijo mezclando sus genes
    public static GenotipoMapa Crossover(GenotipoMapa a, GenotipoMapa b)
    {
        // crea un hijo sin randomización
        GenotipoMapa child = new GenotipoMapa(a.largo, a.alto, false);

        // elije una columna aleatoria para dividir
        int split = Random.Range(0, a.largo);

        // se copian las columnas desde a y b según el corte
        for (int x = 0; x < a.largo; x++)
        {
            for (int y = 0; y < a.alto; y++)
            {
                // Antes del corte, tomamos de a; después del corte, de b
                child.grid[x, y] = (x < split) ? a.grid[x, y] : b.grid[x, y];
            }
        }
        return child;
    }

    // Clona el mapa actual (copia idéntica)
    public GenotipoMapa Clone()
    {
        // Creamos un nuevo genotipo vacío
        GenotipoMapa clone = new GenotipoMapa(largo, alto, false);

       
        for (int x = 0; x < largo; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                clone.grid[x, y] = grid[x, y];
            }
        }
        return clone;
    }
}
