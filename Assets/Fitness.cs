using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fitness : MonoBehaviour
{
    [Header("Configuración de Fitness")]
    [Range(0f, 1f)]
    public float sueloObjetivo = 0.4f; // Porcentaje deseado de suelo en el mapa

    [Header("Pesos")]
    public float pesoSuelo = 3f;        
    public float pesoCaminos = 1f;      
    public float pesoSalas = 0.05f;     
    public float pesoConectividad = 5f; 

    // instancia única del Fitness 
    private static Fitness instance;

    private void Awake()
    {
        instance = this; // para usar en metodos estaticos
    }

    // Evalúa la "calidad" de un mapa genotipo mapa
    public static float Evaluar(GenotipoMapa g)
    {
        if (instance == null)
        {
            Debug.LogError("No hay instancia de Fitness en la escena");
            return 0f;
        }

        // número total de celdas y contar cuantas son suelo
        int totalCeldas = g.largo * g.alto;
        int floor = 0;
        for (int x = 0; x < g.largo; x++)
        {
            for (int y = 0; y < g.alto; y++)
            {
                if (g.grid[x, y] == 1) floor++;
            }
        }

        // ratio de suelo (porcentaje respecto a total de celdas)
        float ratioSuelo = (float)floor / totalCeldas;
        float score = 0f;


        // proporción de suelo con penalización fuerte si hay más suelo que el objetivo
        if (ratioSuelo > instance.sueloObjetivo)
        {
            score -= instance.pesoSuelo * (ratioSuelo - instance.sueloObjetivo) * 5f;
        }
        else
        {
            //  si se acerca al objetivo da mas puntaje
            score += instance.pesoSuelo * (1f - Mathf.Abs(ratioSuelo - instance.sueloObjetivo));
        }

        
        //Conectividad local , que es basicamente cuántos suelos están conectados desde la esquina inicial
        int conectadas = ConectividadTotal(g);
        score += instance.pesoCaminos * ((float)conectadas / totalCeldas);

       
        // aqui se toman todas las salas y premia las más grandes
        List<int> tamaniosSalas = ObtenerTamaniosSalas(g);
        score += instance.pesoSalas * SumSalas(tamaniosSalas);

       
        // se mide aqui si todo el suelo está interconectado
        float conectividadGlobal = MapaConectado(g);
        score += instance.pesoConectividad * conectividadGlobal;

        return score; 
    }

    // Suma el tamaño de las salas pero solo si son mayores o iguales a 4
    private static float SumSalas(List<int> tamaniosSalas)
    {
        float s = 0f;
        foreach (int tam in tamaniosSalas)
        {
            if (tam >= 4)
            {
                s += tam; // puntaje a salas grandes
            }
        }
        return s;
    }

    // Calcula cuantas celdas de suelo están conectadas empezando desde (0,0)
    private static int ConectividadTotal(GenotipoMapa g)
    {
        bool[,] visitados = new bool[g.largo, g.alto]; // Matriz de visitados
        Queue<Vector2Int> q = new Queue<Vector2Int>(); // Cola para BFS
        q.Enqueue(new Vector2Int(0, 0));
        visitados[0, 0] = true;

        // Direcciones posibles (arriba, abajo, izquierda, derecha)
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };
        int conectadas = 0;

        // BFS recorriendo las celdas de suelo
        while (q.Count > 0)
        {
            Vector2Int pos = q.Dequeue();
            if (g.grid[pos.x, pos.y] == 1) conectadas++;

            for (int i = 0; i < 4; i++)
            {
                int nx = pos.x + dx[i];
                int ny = pos.y + dy[i];
                // validacion de que la celda este dentro de los límites
                if (nx >= 0 && ny >= 0 && nx < g.largo && ny < g.alto)
                {
                    // visita solo suelos que no hayan sido visitados antes
                    if (!visitados[nx, ny] && g.grid[nx, ny] == 1)
                    {
                        visitados[nx, ny] = true;
                        q.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }
        return conectadas;
    }

    // encuentra todas las salas del mapa y retorna sus tamaños
    private static List<int> ObtenerTamaniosSalas(GenotipoMapa g)
    {
        bool[,] visitados = new bool[g.largo, g.alto];
        List<int> tamanios = new List<int>();

        // recorre toda la grilla
        for (int x = 0; x < g.largo; x++)
        {
            for (int y = 0; y < g.alto; y++)
            {
                // si es suelo y no ha sido visitado, se explora la sala con BFS
                if (!visitados[x, y] && g.grid[x, y] == 1)
                {
                    tamanios.Add(BFSGrupo(g, x, y, visitados));
                }
            }
        }
        return tamanios;
    }

    // BFS para contar el tamaño de una sala de suelo
    private static int BFSGrupo(GenotipoMapa g, int startX, int startY, bool[,] visitados)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(startX, startY));
        visitados[startX, startY] = true;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };
        int tamaño = 0;

        while (q.Count > 0)
        {
            Vector2Int pos = q.Dequeue();
            tamaño++; 

            for (int i = 0; i < 4; i++)
            {
                int nx = pos.x + dx[i];
                int ny = pos.y + dy[i];
                if (nx >= 0 && ny >= 0 && nx < g.largo && ny < g.alto)
                {
                    if (!visitados[nx, ny] && g.grid[nx, ny] == 1)
                    {
                        visitados[nx, ny] = true;
                        q.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }
        return tamaño;
    }

    // evalúa si todos los suelos estan conectados entre si
    private static float MapaConectado(GenotipoMapa g)
    {
        int totalSuelo = 0;
        Vector2Int? start = null;

        // cuenta cuantos suelos hay y elegimos un punto de inicio
        for (int x = 0; x < g.largo; x++)
        {
            for (int y = 0; y < g.alto; y++)
            {
                if (g.grid[x, y] == 1)
                {
                    totalSuelo++;
                    if (start == null) start = new Vector2Int(x, y);
                }
            }
        }

        // si no hay suelo, la conectividad es 0
        if (totalSuelo == 0 || start == null) return 0f;

        // BFS desde un punto de suelo
        bool[,] visitados = new bool[g.largo, g.alto];
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(start.Value);
        visitados[start.Value.x, start.Value.y] = true;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };
        int alcanzados = 0;

        while (q.Count > 0)
        {
            Vector2Int pos = q.Dequeue();
            alcanzados++;

            for (int i = 0; i < 4; i++)
            {
                int nx = pos.x + dx[i];
                int ny = pos.y + dy[i];
                if (nx >= 0 && ny >= 0 && nx < g.largo && ny < g.alto)
                {
                    if (!visitados[nx, ny] && g.grid[nx, ny] == 1)
                    {
                        visitados[nx, ny] = true;
                        q.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }

        // retorna el porcentaje de conectividad (1 = todos los suelos conectados)
        return (float)alcanzados / totalSuelo;
    }
}
