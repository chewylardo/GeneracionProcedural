using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarkovPrefabLevelGenerator : MonoBehaviour
{
    [Header("Prefabs del terreno")]
    public GameObject[] tilePrefabs; 
    [Header("Ejemplo (cada string = 1 fila)")]
    public List<string> exampleLevel;
    [Header("Salida")]
    public int width = 10;
    public int height = 10;
    [Tooltip("radio del vecindario (1 => 3x3, 2 => 5x5)")]
    [Range(1, 3)] public int neighborhoodSize = 1;
    public float cellSize = 1f;
    [Tooltip("semilla aleatoria (negativo = aleatoria)")]
    public int seed = -1;

    private System.Random rnd;
    // Diccionario: patrónKey -> lista de valores centrales (repetidos para ponderación)
    private Dictionary<string, List<int>> patternMap;
    // Distribución global de centros (fallback)
    private List<int> globalCenters;

    void Start()
    {
        if (seed >= 0) rnd = new System.Random(seed);
        else rnd = new System.Random();

        if (tilePrefabs == null || tilePrefabs.Length < 5)
        {
            Debug.LogError("Asigna 5 prefabs (0..4) en tilePrefabs.");
            return;
        }

        if (exampleLevel == null || exampleLevel.Count == 0)
        {
            Debug.LogError("Agrega un ejemplo en exampleLevel.");
            return;
        }

        BuildPatternMapFromExample();
        int[,] generated = GenerateMap(width, height);
        DrawLevel(generated);
    }

    // Construye patternMap extrayendo ventanas (2N+1)^2 de la matriz ejemplo.
    // Usa 'X' como padding fuera de bordes.
    void BuildPatternMapFromExample()
    {
        patternMap = new Dictionary<string, List<int>>();
        globalCenters = new List<int>();

        int h = exampleLevel.Count;
        int w = exampleLevel[0].Length;

        // Convertir a matriz int
        int[,] ex = new int[h, w];
        for (int y = 0; y < h; y++)
        {
            if (exampleLevel[y].Length != w)
                Debug.LogWarning("Las filas del ejemplo no tienen la misma longitud.");
            for (int x = 0; x < w; x++)
            {
                ex[y, x] = exampleLevel[y][x] - '0';
            }
        }

        int N = neighborhoodSize;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                string key = BuildWindowKeyFromGrid(ex, x, y, N, paddedChar: 'X');
                int center = ex[y, x];

                if (!patternMap.ContainsKey(key))
                    patternMap[key] = new List<int>();

                patternMap[key].Add(center);
                globalCenters.Add(center);
            }
        }

        Debug.Log($"PatternMap construido. Patrones distintos: {patternMap.Count}. Centros totales: {globalCenters.Count}");
    }

    // Construye la clave de ventana (string) para la celda (cx,cy) en grid.
    // Fuera de bounds => paddedChar.
    string BuildWindowKeyFromGrid(int[,] grid, int cx, int cy, int N, char paddedChar)
    {
        int h = grid.GetLength(0);
        int w = grid.GetLength(1);
        List<char> vals = new List<char>();

        for (int dy = -N; dy <= N; dy++)
        {
            for (int dx = -N; dx <= N; dx++)
            {
                int nx = cx + dx;
                int ny = cy + dy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                {
                    vals.Add(paddedChar);
                }
                else
                {
                    int v = grid[ny, nx];
                    // Convertir a carácter '0'..'9'
                    vals.Add((char)('0' + v));
                }
            }
        }
        return new string(vals.ToArray());
    }


    // Genera el mapa de salida tamaño outW x outH.
    // Rellena por filas (y ascendente), y para cada celda usa la vecindad parcial
    // (posiciones no generadas = '_' ; fuera de bounds = 'X') para buscar coincidencias compatibles.
    int[,] GenerateMap(int outW, int outH)
    {
        int[,] outGrid = new int[outH, outW];
        for (int y = 0; y < outH; y++)
            for (int x = 0; x < outW; x++)
                outGrid[y, x] = -1; // no asignado

        int N = neighborhoodSize;
        int windowSize = (2 * N + 1) * (2 * N + 1);

        // Precompute keys list for iteration
        var storedKeys = patternMap.Keys.ToList();

        for (int y = 0; y < outH; y++)
        {
            for (int x = 0; x < outW; x++)
            {
                // crear partialKey:
                // - 'X' => fuera de bounds
                // - '_' => celda aún no asignada (o que está a la derecha/abajo)
                // - '0'.. => celdas ya generadas
                char[] partial = new char[windowSize];
                int idx = 0;
                for (int dy = -N; dy <= N; dy++)
                {
                    for (int dx = -N; dx <= N; dx++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= outW || ny >= outH)
                        {
                            partial[idx++] = 'X';
                        }
                        else
                        {
                            int val = outGrid[ny, nx];
                            if (val == -1) partial[idx++] = '_';
                            else partial[idx++] = (char)('0' + val);
                        }
                    }
                }
                string partialKey = new string(partial);

                // buscar coincidencias compatibles
                List<int> candidates = new List<int>(); // repetición usada para ponderar
                foreach (var sk in storedKeys)
                {
                    if (IsCompatible(sk, partialKey))
                    {
                        // agregar todos los centros asociados (repetidos) para ponderación
                        candidates.AddRange(patternMap[sk]);
                    }
                }

                int chosen;
                if (candidates.Count > 0)
                {
                    chosen = candidates[rnd.Next(candidates.Count)];
                }
                else
                {
                    // fallback: si no hubo coincidencias, usar distribución global
                    if (globalCenters.Count > 0)
                        chosen = globalCenters[rnd.Next(globalCenters.Count)];
                    else
                        chosen = rnd.Next(tilePrefabs.Length);
                }

                outGrid[y, x] = chosen;
            }
        }

        return outGrid;
    }


    // Chequea compatibilidad entre una clave almacenada (sin '_' pero puede tener 'X')
    // y una clave parcial (con '_' y 'X'). Retorna true si en todas las posiciones donde partial != '_'
    // se cumple partialChar == storedChar.
    bool IsCompatible(string storedKey, string partialKey)
    {
        if (storedKey.Length != partialKey.Length) return false;
        for (int i = 0; i < storedKey.Length; i++)
        {
            char p = partialKey[i];
            if (p == '_') continue;         // no importa
            if (p == 'X')                   // partial says out-of-bounds -> stored must also be X
            {
                if (storedKey[i] != 'X') return false;
            }
            else
            {
                if (storedKey[i] != p) return false;
            }
        }
        return true;
    }

    // Instancia prefabs según la matriz generada (agrupados como hijos).
    void DrawLevel(int[,] map)
    {
        // limpiar hijos viejos
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform t in transform) toDestroy.Add(t.gameObject);
        foreach (var go in toDestroy) DestroyImmediate(go);

        int h = map.GetLength(0);
        int w = map.GetLength(1);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int v = map[y, x];
                if (v >= 0 && v < tilePrefabs.Length && tilePrefabs[v] != null)
                {
                    Vector3 pos = new Vector3(x * cellSize, -y * cellSize, 0f);
                    Instantiate(tilePrefabs[v], pos, Quaternion.identity, transform);
                }
            }
        }
    }
}
