using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapseGenerator : MonoBehaviour
{
    [Header("Entrada y tiles")]
    public IntGrid inputExample;             // matriz de ejemplo (IDs)
    public List<TileData2> allTiles;         // lista de tiles (id + prefab)

    [Header("Parámetros de extracción y salida")]
    [Range(1, 6)] public int patternSize = 2; // N de patrones (NxN)
    public int outputWidth = 16;              // ancho deseado en tiles (>= patternSize)
    public int outputHeight = 16;             // alto deseado en tiles (>= patternSize)

    [Header("Visualización")]
    public float tileSpacing = 1f;
    public Transform parentContainer;

    // Estructuras internas
    private Dictionary<string, int> patternKeyToId;
    private List<int[]> patterns;
    private Dictionary<int, float> patternWeights;  // peso/frecuencia de cada patrón (normalizado)
    private Dictionary<int, Dictionary<Vector2Int, Dictionary<int, int>>> adjacencyCounts;
    private Dictionary<int, Dictionary<Vector2Int, List<int>>> adjacencyAllowed;
    private System.Random rng;

    // Estado de resolución (grid de patrones)
    private int pWidth, pHeight;
    private PatternCell[,] patternGrid;

    void Start() => Generate();

    // --------------------- EXTRACCIÓN Y REGLAS ---------------------
    void ExtractPatternsAndRules()
    {
        patternKeyToId = new Dictionary<string, int>();
        patterns = new List<int[]>();
        patternWeights = new Dictionary<int, float>();
        adjacencyCounts = new Dictionary<int, Dictionary<Vector2Int, Dictionary<int, int>>>();

        int iw = inputExample.width;
        int ih = inputExample.height;
        int N = patternSize;

        // --- 1) Recorrer la entrada y extraer cada ventana NxN (overlap) ---
        // Recorremos todas las ventanas NxN que caben dentro del input.
        // Cada ventana genera un patrón aplanado (int[] length N*N).
        for (int y = 0; y <= ih - N; y++)
        {
            for (int x = 0; x <= iw - N; x++)
            {
                // construimos el patrón en forma aplanada (fila-major)
                //pat = pattern
                int[] pat = new int[N * N];
                int idx = 0;
                for (int yy = 0; yy < N; yy++)
                    for (int xx = 0; xx < N; xx++)
                        pat[idx++] = inputExample[x + xx, y + yy];

                // usamos una clave string simple para identificar patrones exclusivos
                string key = PatternKey(pat);

                // Si no existe almacenado, crear nueva entrada
                if (!patternKeyToId.ContainsKey(key))
                {
                    int id = patterns.Count;
                    patternKeyToId[key] = id;
                    patterns.Add(pat);
                    patternWeights[id] = 0f;
                    adjacencyCounts[id] = new Dictionary<Vector2Int, Dictionary<int, int>>();
                }
                int pid = patternKeyToId[key];
                patternWeights[pid] += 1f;  // contamos frecuencia del patrón
            }
        }

        // --- 2) Construir conteos de adyacencia entre patrones ---
        // Para cada ubicación donde se extrajo un patrón, comprobamos las ventanas desplazadas
        // (en las 8 direcciones) y contabilizamos qué patrón vecino aparece.
        Vector2Int[] dirs = AdjacencyDirs();
        for (int y = 0; y <= ih - N; y++)
        {
            for (int x = 0; x <= iw - N; x++)
            {
                // Reconstruimos el patrón actual
                int[] pat = new int[N * N];
                int idx = 0;
                for (int yy = 0; yy < N; yy++)
                    for (int xx = 0; xx < N; xx++)
                        pat[idx++] = inputExample[x + xx, y + yy];
                int pid = patternKeyToId[PatternKey(pat)];

                // comprobamos cada dirección; si la ventana desplazada existe en la entrada,
                // registramos el patrón vecino observado.
                foreach (var dir in dirs)
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;

                    // Si la ventana desplazada sale del input, la ignoramos.
                    if (nx < 0 || ny < 0 || nx > iw - N || ny > ih - N) continue;

                    int[] neighborPat = new int[N * N];
                    idx = 0;
                    for (int yy = 0; yy < N; yy++)
                        for (int xx = 0; xx < N; xx++)
                            neighborPat[idx++] = inputExample[nx + xx, ny + yy];
                    //npid = neighbor pattern id
                    int npid = patternKeyToId[PatternKey(neighborPat)];

                    // Incrementar conteo adjacencyCounts[pid][dir][npid]
                    if (!adjacencyCounts[pid].ContainsKey(dir))
                        adjacencyCounts[pid][dir] = new Dictionary<int, int>();
                    if (!adjacencyCounts[pid][dir].ContainsKey(npid))
                        adjacencyCounts[pid][dir][npid] = 0;
                    adjacencyCounts[pid][dir][npid]++;  // sumar ocurrencia observada
                }
            }
        }

        // --- 3) Convertir conteos en listas de patrones permitidos (adjacencyAllowed) ---
        adjacencyAllowed = new Dictionary<int, Dictionary<Vector2Int, List<int>>>();
        foreach (var kv in adjacencyCounts)
        {
            int pid = kv.Key;
            adjacencyAllowed[pid] = new Dictionary<Vector2Int, List<int>>();
            foreach (var dkv in kv.Value)
            {
                var dir = dkv.Key;
                var dict = dkv.Value;
                adjacencyAllowed[pid][dir] = dict.Keys.ToList();    // solo IDs permitidos (sin pesos)
            }
        }

        // --- 4) Normalizar pesos de patrones a probabilidades (suma = 1) ---
        float total = patternWeights.Values.Sum();
        if (total <= 0) total = 1f;
        var keys = patternWeights.Keys.ToList();
        foreach (var k in keys) patternWeights[k] /= total;

        // Debug: imprimir resumen para depuración (ayuda a saber si realmente hay varios patrones)
        Debug.Log($"[WFC] Extracted {patterns.Count} unique patterns (N={N}).");
        var top = patternWeights.OrderByDescending(kv => kv.Value).Take(10).ToList();
        for (int i = 0; i < top.Count; i++)
        {
            Debug.Log($"[WFC] Pattern {top[i].Key} weight={top[i].Value:F3}  sample={PatternKey(patterns[top[i].Key])}");
        }
    }

    // Devuelve las 8 direcciones (incluye diagonales)
    static Vector2Int[] AdjacencyDirs()
    {
        return new Vector2Int[] {
            new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1),
            new Vector2Int(1,1), new Vector2Int(-1,1), new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };
    }

    // Convierte un patrón ( int[]) a clave string "a,b,c,..."
    string PatternKey(int[] pat) => string.Join(",", pat);

    // Inicializa patternGrid con todas las posibilidades por celda
    void InitializePatternGrid()
    {
        // El grid de patrones tiene dimensiones (output - N + 1) en cada eje:
        // cada posición corresponde al top-left de una ventana NxN en la salida de tiles.
        pWidth = outputWidth - patternSize + 1;
        pHeight = outputHeight - patternSize + 1;
        if (pWidth <= 0 || pHeight <= 0)
            throw new Exception($"Output ({outputWidth}x{outputHeight}) must be >= patternSize ({patternSize}).");

        patternGrid = new PatternCell[pWidth, pHeight];
        var allPatternIds = Enumerable.Range(0, patterns.Count).ToList();
        for (int x = 0; x < pWidth; x++)
            for (int y = 0; y < pHeight; y++)
                patternGrid[x, y] = new PatternCell(allPatternIds) { x = x, y = y };
    }

    // Selecciona la celda de menor entropía (o una al azar si hay empate).
    PatternCell GetLowestEntropyCell()
    {
        PatternCell best = null;
        float bestEntropy = float.MaxValue;
        List<PatternCell> ties = new List<PatternCell>();

        // Calculamos entropía aproximada usando patternWeights como distribución base.
        foreach (var cell in patternGrid)
        {
            if (cell.IsCollapsed) continue;
            float sumP = 0f, sumPLogP = 0f;
            foreach (int pid in cell.possible)
            {
                float p = patternWeights.ContainsKey(pid) ? patternWeights[pid] : 1f / patterns.Count;
                sumP += p;
                sumPLogP += p > 0 ? p * Mathf.Log(p) : 0f;
            }
            if (sumP <= 0f) continue;
            float entropy = -(sumPLogP / sumP);
            if (entropy < bestEntropy - 1e-6f) { bestEntropy = entropy; best = cell; ties.Clear(); ties.Add(cell); }
            else if (Mathf.Abs(entropy - bestEntropy) <= 1e-6f) ties.Add(cell);
        }
        if (best == null) return null;
        // desempate aleatorio entre empates
        return ties.Count == 1 ? ties[0] : ties[rng.Next(ties.Count)];
    }

    // Obtiene probabilidades de adyacencia (normalizadas) para un patternId en una dirección dada
    Dictionary<int, float> GetAdjacencyProbabilities(int patternId, Vector2Int dir)
    {
        if (!adjacencyCounts.ContainsKey(patternId)) return null;
        if (!adjacencyCounts[patternId].ContainsKey(dir)) return null;
        var freq = adjacencyCounts[patternId][dir];
        float tot = freq.Values.Sum();
        if (tot <= 0) return null;
        return freq.ToDictionary(kv => kv.Key, kv => kv.Value / tot);
    }

    // Propagación de restricciones: al colapsar una celda (o reducir sus posibilidades),
    // esta función actualiza vecinos recursivamente. Si se produce contradicción devuelve false.
    bool Propagate(PatternCell[,] workingGrid, int sx, int sy)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(sx, sy));
        while (q.Count > 0)
        {
            var p = q.Dequeue();
            var cell = workingGrid[p.x, p.y];

            // Para cada dirección, actualizamos el vecino correspondiente
            foreach (var dir in AdjacencyDirs())
            {
                int nx = p.x + dir.x, ny = p.y + dir.y;
                if (nx < 0 || ny < 0 || nx >= pWidth || ny >= pHeight) continue;    // vecinos colapsados no necesitan reducirse
                var neighbor = workingGrid[nx, ny];
                if (neighbor.IsCollapsed) continue;

                // reconstruimos la nueva lista de posibles para el vecino,
                // permitiendo únicamente aquellos neighborPattern que tienen soporte en 'cell'
                HashSet<int> newPossible = new HashSet<int>();
                foreach (int nPid in neighbor.possible)
                {
                    bool ok = false;
                    foreach (int cPid in cell.possible)
                    {
                        // Si tenemos reglas para cPid en la dirección 'dir', verificar si nPid está permitido.
                        if (adjacencyAllowed.ContainsKey(cPid) && adjacencyAllowed[cPid].ContainsKey(dir))
                        {
                            if (adjacencyAllowed[cPid][dir].Contains(nPid)) { ok = true; break; }
                        }
                        else
                        {
                            // si no hay información para esa dirección, se permite (conservador)
                            ok = true; break;
                        }
                    }
                    if (ok) newPossible.Add(nPid);
                }

                if (newPossible.Count == 0) return false;   // contradicción -> backtrack necesario

                // Si se redujo el conjunto de posibilidades, actualizamos y encolamos para propagar.
                if (newPossible.Count < neighbor.possible.Count)
                {
                    neighbor.possible = newPossible.ToList();   // reducir opciones
                    q.Enqueue(new Vector2Int(nx, ny));          // encolar vecino para propagar más
                }
            }
        }
        return true;    // propagación completada sin contradicción
    }

    // Copia profunda del grid de trabajo (necesario para backtracking)
    PatternCell[,] ClonePatternGrid(PatternCell[,] src)
    {
        PatternCell[,] copy = new PatternCell[pWidth, pHeight];
        for (int x = 0; x < pWidth; x++)
            for (int y = 0; y < pHeight; y++)
                copy[x, y] = new PatternCell(src[x, y].possible) { x = x, y = y };
        return copy;
    }

    // -------------------- SELECCIÓN DE CANDIDATOS (ALEATORIO PONDERADO) --------------------
    // Esta función mezcla (sin reemplazo) los candidatos en un orden aleatorio pero sesgado
    // por su peso combinado, para evitar elegir siempre el candidato más probable.
    List<int> ShuffleCandidatesByWeightedRandom(Dictionary<int, float> combined)
    {
        var list = new List<int>(combined.Keys);
        var result = new List<int>(list.Count);
        var rnd = rng;

        // muestreamos repetidamente sin reemplazo, proporcional al peso actual
        while (list.Count > 0)
        {
            float tot = 0f;
            foreach (var k in list) tot += combined[k];
            float r = (float)rnd.NextDouble() * tot;
            float acc = 0f;
            int chosen = list[0];
            for (int i = 0; i < list.Count; i++)
            {
                acc += combined[list[i]];
                if (r <= acc)
                {
                    chosen = list[i];
                    list.RemoveAt(i);   // removemos elegido para no repetir
                    break;
                }
            }
            result.Add(chosen);
        }
        return result;
    }

    // Obtiene la lista de candidatos ordenada (aleatorizada pero preferente por peso)
    List<int> GetCandidatesOrderedWorking(PatternCell cell, PatternCell[,] working)
    {
        var candidates = new HashSet<int>(cell.possible);

        // Intersectar con restricciones impuestas por vecinos colapsados
        foreach (var dir in AdjacencyDirs())
        {
            int nx = cell.x + dir.x, ny = cell.y + dir.y;
            if (nx < 0 || ny < 0 || nx >= pWidth || ny >= pHeight) continue;
            var neighbor = working[nx, ny];
            if (!neighbor.IsCollapsed) continue;
            int neighborPid = neighbor.Final;
            Vector2Int opp = new Vector2Int(-dir.x, -dir.y);
            if (adjacencyAllowed.ContainsKey(neighborPid) && adjacencyAllowed[neighborPid].ContainsKey(opp))
                candidates.IntersectWith(new HashSet<int>(adjacencyAllowed[neighborPid][opp]));
            if (candidates.Count == 0) break;
        }

        // Si la intersección resultó vacía (contradicción local), restaurar posibilidades originales (fallback conservador).
        if (candidates.Count == 0) candidates = new HashSet<int>(cell.possible);

        // Base de pesos: patternWeights (probabilidades de aparición en el input).
        Dictionary<int, float> combined = new Dictionary<int, float>();
        foreach (int pid in candidates)
            combined[pid] = patternWeights.ContainsKey(pid) ? patternWeights[pid] : 1f / patterns.Count;

        // Influencia suave desde vecinos colapsados: mezclamos (65% base, 35% condicional)
        foreach (var dir in AdjacencyDirs())
        {
            int nx = cell.x + dir.x, ny = cell.y + dir.y;
            if (nx < 0 || ny < 0 || nx >= pWidth || ny >= pHeight) continue;
            var neighbor = working[nx, ny];
            if (!neighbor.IsCollapsed) continue;
            int neighborPid = neighbor.Final;
            var probs = GetAdjacencyProbabilities(neighborPid, new Vector2Int(-dir.x, -dir.y));
            if (probs == null) continue;
            foreach (var kv in probs)
                if (combined.ContainsKey(kv.Key))
                    combined[kv.Key] = combined[kv.Key] * 0.65f + kv.Value * 0.35f; // mezcla 65/35
        }

        // Devolver candidatos en orden aleatorio ponderado (para diversificar soluciones).
        return ShuffleCandidatesByWeightedRandom(combined);
    }

    // ---------------- Backtracking solver (usa orden aleatorio ponderado) ----------------
    // Resuelve el WFC usando backtracking recursivo; prueba candidatos en orden
    // devuelto por GetCandidatesOrderedWorking y propaga restricciones.
    bool SolveBacktracking(PatternCell[,] working)
    {
        // 1) Comprobar si ya todo colapsado -> éxito
        bool allCollapsed = true;
        for (int x = 0; x < pWidth && allCollapsed; x++)
            for (int y = 0; y < pHeight; y++)
                if (!working[x, y].IsCollapsed) { allCollapsed = false; break; }
        if (allCollapsed)
        {
            patternGrid = working;  // copiar solución
            return true;
        }

        // 2) Elegir celda con menor entropía
        var cell = GetLowestEntropyCellWorking(working);
        if (cell == null) return false;

        // 3) Obtener candidatos (orden aleatorizado-ponderado) y probar cada uno con backtracking
        var candidates = GetCandidatesOrderedWorking(cell, working);
        foreach (int candidate in candidates)
        {
            var clone = ClonePatternGrid(working);
            // Colapsamos la celda en el clone al candidato actual
            clone[cell.x, cell.y].possible = new List<int> { candidate };
            // Propagamos restricciones a partir de esa colapsación
            bool ok = Propagate(clone, cell.x, cell.y);
            if (!ok) continue;                                               // contradicción en esta rama -> probar siguiente candidato
            if (SolveBacktracking(clone)) return true;                      // si la rama resolvió, retornamos true
        }
        return false;   // ninguna rama funcionó -> retroceder
    }

    // Versión de GetLowestEntropyCell que opera sobre un grid de trabajo (clone)
    PatternCell GetLowestEntropyCellWorking(PatternCell[,] working)
    {
        PatternCell best = null;
        float bestEntropy = float.MaxValue;
        List<PatternCell> ties = new List<PatternCell>();
        foreach (var cell in working)
        {
            if (cell.IsCollapsed) continue;
            float sumP = 0f, sumPLogP = 0f;
            foreach (int pid in cell.possible)
            {
                float p = patternWeights.ContainsKey(pid) ? patternWeights[pid] : 1f / patterns.Count;
                sumP += p;
                sumPLogP += p > 0 ? p * Mathf.Log(p) : 0f;
            }
            if (sumP <= 0f) continue;
            float entropy = -(sumPLogP / sumP);
            if (entropy < bestEntropy - 1e-6f) { bestEntropy = entropy; best = cell; ties.Clear(); ties.Add(cell); }
            else if (Mathf.Abs(entropy - bestEntropy) <= 1e-6f) ties.Add(cell);
        }
        if (best == null) return null;
        return ties.Count == 1 ? ties[0] : ties[rng.Next(ties.Count)];
    }

    // ---------------- Public entry: Generate ----------------
    // Punto de entrada público: ejecuta todo el pipeline (extracción, solución, reconstrucción y visualización).
    public void Generate()
    {
        rng = new System.Random();

        // Validaciones básicas para evitar errores.
        if (inputExample == null || allTiles == null || allTiles.Count == 0)
        {
            Debug.LogError("[WFC] Falta inputExample o allTiles.");
            return;
        }
        if (inputExample.width < patternSize || inputExample.height < patternSize)
        {
            Debug.LogError("[WFC] inputExample debe ser >= patternSize.");
            return;
        }
        if (outputWidth < patternSize || outputHeight < patternSize)
        {
            Debug.LogError("[WFC] outputWidth/Height deben ser >= patternSize.");
            return;
        }

        // 1) Extraer patrones y reglas
        ExtractPatternsAndRules();

        // 2) Inicializar grid de patrones (posibles = todos los patrones)
        InitializePatternGrid();

        // Debug: info inicial de posibilidades por celda
        int totalCells = pWidth * pHeight;
        int avgPoss = patternGrid.Cast<PatternCell>().Select(c => c.possible.Count).Sum() / Math.Max(1, totalCells);
        Debug.Log($"[WFC] patternGrid {pWidth}x{pHeight}, avg initial possibilities per cell = {avgPoss}");

        // 3) Resolver con backtracking
        var working = ClonePatternGrid(patternGrid);
        bool solved = SolveBacktracking(working);
        if (!solved)
        {
            Debug.LogWarning("[WFC] No se encontró solución con las restricciones dadas.");
            return;
        }

        // 4) Reconstruir la cuadrícula de tiles (outputWidth x outputHeight) a partir de patternGrid
        // Cada celda de patternGrid corresponde al top-left de un bloque NxN en la salida.
        int outW = outputWidth;
        int outH = outputHeight;
        int N = patternSize;
        int[] outputTiles = new int[outW * outH];

        // Para cada tile (tx,ty) buscamos un patrón que lo cubra (prefiriendo patrones top-left que incluyan la celda)
        for (int ty = 0; ty < outH; ty++)
        {
            for (int tx = 0; tx < outW; tx++)
            {
                bool assigned = false;
                for (int py = Math.Max(0, ty - N + 1); py <= Math.Min(pHeight - 1, ty); py++)
                {
                    for (int px = Math.Max(0, tx - N + 1); px <= Math.Min(pWidth - 1, tx); px++)
                    {
                        //pattern id = pid
                        var pid = patternGrid[px, py].Final;
                        if (pid < 0 || pid >= patterns.Count) continue;
                        int[] pat = patterns[pid];
                        int ox = tx - px;                   // offset dentro del patrón
                        int oy = ty - py;
                        int val = pat[oy * N + ox];
                        outputTiles[ty * outW + tx] = val;
                        assigned = true;
                        goto NEXT_TILE;                     // saltamos al siguiente tile
                    }
                }
            NEXT_TILE:
                if (!assigned)
                {
                    // Si algo falló, usar patrón más probable como fallback (mínima degradación).
                    int fallbackPid = patternWeights.OrderByDescending(kv => kv.Value).First().Key;
                    int[] pat = patterns[fallbackPid];
                    int ox = Math.Clamp(tx, 0, N - 1);
                    int oy = Math.Clamp(ty, 0, N - 1);
                    outputTiles[ty * outW + tx] = pat[oy * N + ox];
                }
            }
        }

        // 5) Visualizar: instanciar prefabs
        ClearPrevious();
        Transform parent = parentContainer != null ? parentContainer : transform;
        for (int y = 0; y < outH; y++)
            for (int x = 0; x < outW; x++)
            {
                int id = outputTiles[y * outW + x];
                var tileData = allTiles.FirstOrDefault(t => t.id == id);
                if (tileData != null && tileData.prefab != null)
                    Instantiate(tileData.prefab, new Vector3(x * tileSpacing, -y * tileSpacing, 0), Quaternion.identity, parent);
            }

        Debug.Log("[WFC] Generation complete.");
    }

    void ClearPrevious()
    {
        Transform parent = parentContainer != null ? parentContainer : transform;
        foreach (Transform child in parent) Destroy(child.gameObject);
    }
}
