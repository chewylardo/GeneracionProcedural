using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapseGenerator : MonoBehaviour
{
    [Header("Entrada por texto")]
    [TextArea(4, 20)]
    public string rawInput;

    [Header("Tiles disponibles")]
    public List<TileData2> allTiles;

    [Header("Parámetros WFC")]
    [Range(1, 6)] public int patternSize = 2;
    public int outputWidth = 16;
    public int outputHeight = 16;
    public float tileSpacing = 1f;

    [Header("Resiliencia / Reinicios")]
    [Tooltip("Número máximo de intentos (reinicios aleatorios) antes de rendirse.")]
    public int maxAttempts = 2000;
    [Tooltip("Usar seed fijo para reproducibilidad (si false, seed aleatorio por intento).")]
    public bool useFixedSeed = false;
    public int fixedSeed = 12345;

    public Transform parentContainer;
    public Transform inputContainer;

    // Internos
    private StringGrid inputExample;

    private Dictionary<string, int> patternKeyToId;
    private List<string[]> patterns;
    private Dictionary<int, float> patternWeights;
    private Dictionary<int, Dictionary<Vector2Int, Dictionary<int, int>>> adjacencyCounts;
    private Dictionary<int, Dictionary<Vector2Int, List<int>>> adjacencyAllowed;

    private System.Random rng;
    private int pWidth, pHeight;
    private PatternCell[,] patternGrid; // la solución final (grid de patrones top-left)

    void Start() => Generate();

    // -------------------- Input parsing --------------------
    void ParseInputFromString()
    {
        inputExample = new StringGrid();
        inputExample.FromRawString(rawInput); // StringGrid debe tener el método que itera por caracteres
        Debug.Log($"[WFC] Input procesado: {inputExample.width}x{inputExample.height}");
    }

    // -------------------- Extracción de patrones y reglas --------------------
    void ExtractPatternsAndRules()
    {
        patternKeyToId = new Dictionary<string, int>();
        patterns = new List<string[]>();
        patternWeights = new Dictionary<int, float>();
        adjacencyCounts = new Dictionary<int, Dictionary<Vector2Int, Dictionary<int, int>>>();

        int iw = inputExample.width;
        int ih = inputExample.height;
        int N = patternSize;

        if (iw < N || ih < N)
        {
            Debug.LogError("[WFC] Input menor que patternSize.");
            return;
        }

        // Extraer patrones (ventanas NxN solapadas)
        for (int y = 0; y <= ih - N; y++)
        {
            for (int x = 0; x <= iw - N; x++)
            {
                string[] pat = new string[N * N];
                int idx = 0;
                for (int yy = 0; yy < N; yy++)
                    for (int xx = 0; xx < N; xx++)
                        pat[idx++] = inputExample[x + xx, y + yy];

                string key = PatternKey(pat);
                if (!patternKeyToId.ContainsKey(key))
                {
                    int id = patterns.Count;
                    patternKeyToId[key] = id;
                    patterns.Add(pat);
                    patternWeights[id] = 0f;
                    adjacencyCounts[id] = new Dictionary<Vector2Int, Dictionary<int, int>>();
                }
                int pid = patternKeyToId[key];
                patternWeights[pid] += 1f;
            }
        }

        BuildAdjacencyRules(iw, ih);
        NormalizePatternWeights();
    }

    void BuildAdjacencyRules(int iw, int ih)
    {
        int N = patternSize;
        var dirs = AdjacencyDirs();

        for (int y = 0; y <= ih - N; y++)
        {
            for (int x = 0; x <= iw - N; x++)
            {
                // patrón central
                string[] pat = new string[N * N];
                int idx = 0;
                for (int yy = 0; yy < N; yy++)
                    for (int xx = 0; xx < N; xx++)
                        pat[idx++] = inputExample[x + xx, y + yy];
                int pid = patternKeyToId[PatternKey(pat)];

                // vecinos en cada dirección
                foreach (var dir in dirs)
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;
                    if (nx < 0 || ny < 0 || nx > iw - N || ny > ih - N) continue;

                    string[] neigh = new string[N * N];
                    idx = 0;
                    for (int yy = 0; yy < N; yy++)
                        for (int xx = 0; xx < N; xx++)
                            neigh[idx++] = inputExample[nx + xx, ny + yy];

                    int npid = patternKeyToId[PatternKey(neigh)];

                    if (!adjacencyCounts[pid].ContainsKey(dir))
                        adjacencyCounts[pid][dir] = new Dictionary<int, int>();
                    if (!adjacencyCounts[pid][dir].ContainsKey(npid))
                        adjacencyCounts[pid][dir][npid] = 0;
                    adjacencyCounts[pid][dir][npid]++;
                }
            }
        }

        // convertir a listas permitidas (sin pesos)
        adjacencyAllowed = new Dictionary<int, Dictionary<Vector2Int, List<int>>>();
        foreach (var kv in adjacencyCounts)
        {
            int pid = kv.Key;
            adjacencyAllowed[pid] = new Dictionary<Vector2Int, List<int>>();
            foreach (var dirkv in kv.Value)
                adjacencyAllowed[pid][dirkv.Key] = dirkv.Value.Keys.ToList();
        }
    }

    void NormalizePatternWeights()
    {
        float total = patternWeights.Values.Sum();
        if (total <= 0) total = 1f;
        var keys = patternWeights.Keys.ToList();
        foreach (var k in keys) patternWeights[k] /= total;

        Debug.Log($"[WFC] Patrones únicos: {patterns.Count}");
    }

    string PatternKey(string[] pat) => string.Join(",", pat);

    // Por defecto usamos 4 direcciones (más robusto); si quieres 8, cámbialo aquí.
    Vector2Int[] AdjacencyDirs()
    {
        return new Vector2Int[] {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1),
            new Vector2Int(1,1), new Vector2Int(-1,1),
            new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };
    }

    // -------------------- Inicialización del grid de patrones --------------------
    void InitializePatternGrid()
    {
        pWidth = outputWidth - patternSize + 1;
        pHeight = outputHeight - patternSize + 1;

        if (pWidth <= 0 || pHeight <= 0)
            throw new Exception($"Output ({outputWidth}x{outputHeight}) must be >= patternSize ({patternSize}).");

        patternGrid = new PatternCell[pWidth, pHeight];
        var ids = Enumerable.Range(0, patterns.Count).ToList();
        for (int x = 0; x < pWidth; x++)
            for (int y = 0; y < pHeight; y++)
                patternGrid[x, y] = new PatternCell(ids) { x = x, y = y };
    }

    // -------------------- Helpers entropía / candidatos --------------------
    PatternCell GetLowestEntropy(PatternCell[,] grid)
    {
        PatternCell best = null;
        float bestEntropy = float.MaxValue;
        var ties = new List<PatternCell>();

        foreach (var c in grid)
        {
            if (c.IsCollapsed) continue;
            float sumP = 0f, sumPLogP = 0f;
            foreach (int pid in c.possible)
            {
                float p = patternWeights.ContainsKey(pid) ? patternWeights[pid] : 1f / patterns.Count;
                sumP += p;
                sumPLogP += p > 0 ? p * Mathf.Log(p) : 0f;
            }
            if (sumP <= 0f) continue;
            float entropy = -(sumPLogP / sumP);
            if (entropy < bestEntropy - 1e-6f) { bestEntropy = entropy; best = c; ties.Clear(); ties.Add(c); }
            else if (Mathf.Abs(entropy - bestEntropy) <= 1e-6f) ties.Add(c);
        }
        if (best == null) return null;
        return ties.Count == 1 ? ties[0] : ties[rng.Next(ties.Count)];
    }

    Dictionary<int, float> GetAdjacencyProbabilities(int patternId, Vector2Int dir)
    {
        if (!adjacencyCounts.ContainsKey(patternId)) return null;
        if (!adjacencyCounts[patternId].ContainsKey(dir)) return null;
        var freq = adjacencyCounts[patternId][dir];
        float tot = freq.Values.Sum();
        if (tot <= 0) return null;
        return freq.ToDictionary(kv => kv.Key, kv => kv.Value / tot);
    }

    // Orden aleatorio ponderado (simple)
    List<int> ShuffleCandidatesByWeightedRandom(Dictionary<int, float> combined)
    {
        var list = new List<int>(combined.Keys);
        var result = new List<int>(list.Count);
        var rnd = rng;

        while (list.Count > 0)
        {
            float tot = 0f;
            foreach (var k in list) tot += combined[k];
            float r = (float)rnd.NextDouble() * tot;
            float acc = 0f;
            for (int i = 0; i < list.Count; i++)
            {
                acc += combined[list[i]];
                if (r <= acc)
                {
                    result.Add(list[i]);
                    list.RemoveAt(i);
                    break;
                }
            }
        }
        return result;
    }

    List<int> GetCandidatesOrderedWorking(PatternCell cell, PatternCell[,] working)
    {
        var candidates = new HashSet<int>(cell.possible);

        // Intersección con vecinos colapsados (consistente)
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

        if (candidates.Count == 0) candidates = new HashSet<int>(cell.possible);

        Dictionary<int, float> combined = new Dictionary<int, float>();
        foreach (int pid in candidates)
            combined[pid] = patternWeights.ContainsKey(pid) ? patternWeights[pid] : 1f / patterns.Count;

        // mezcla con probabilidades condicionales de vecinos colapsados
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
                    combined[kv.Key] = combined[kv.Key] * 0.65f + kv.Value * 0.35f;
        }

        return ShuffleCandidatesByWeightedRandom(combined);
    }

    // -------------------- Backtracking + Propagación --------------------
    PatternCell[,] CloneGrid(PatternCell[,] src)
    {
        var copy = new PatternCell[pWidth, pHeight];
        for (int x = 0; x < pWidth; x++)
            for (int y = 0; y < pHeight; y++)
                copy[x, y] = new PatternCell(src[x, y].possible) { x = x, y = y };
        return copy;
    }

    bool Propagate(PatternCell[,] working, int sx, int sy)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(sx, sy));
        while (q.Count > 0)
        {
            var p = q.Dequeue();
            var cell = working[p.x, p.y];

            foreach (var dir in AdjacencyDirs())
            {
                int nx = p.x + dir.x, ny = p.y + dir.y;
                if (nx < 0 || ny < 0 || nx >= pWidth || ny >= pHeight) continue;
                var neighbor = working[nx, ny];
                if (neighbor.IsCollapsed) continue;

                HashSet<int> newPossible = new HashSet<int>();
                foreach (int nPid in neighbor.possible)
                {
                    bool ok = false;
                    foreach (int cPid in cell.possible)
                    {
                        if (adjacencyAllowed.ContainsKey(cPid) && adjacencyAllowed[cPid].ContainsKey(dir))
                        {
                            if (adjacencyAllowed[cPid][dir].Contains(nPid)) { ok = true; break; }
                        }
                        else
                        {
                            // sin información -> NO válido
                            ok = false;
                        }
                    }
                    if (ok) newPossible.Add(nPid);
                }

                if (newPossible.Count == 0)
                {
                    HashSet<int> relaxed = new HashSet<int>();

                    // Recorremos todos los patrones posibles como candidatos de relajación
                    for (int testPid = 0; testPid < patterns.Count; testPid++)
                    {
                        bool okWithSourceCell = false;

                        // 1) Debe ser compatible con AL MENOS un patrón del "cell" en la dirección 'dir'
                        //    (esto evita introducir vecinos que contradicen la celda que originó la propagación)
                        foreach (int cPid in cell.possible)
                        {
                            if (adjacencyAllowed.ContainsKey(cPid) && adjacencyAllowed[cPid].ContainsKey(dir))
                            {
                                if (adjacencyAllowed[cPid][dir].Contains(testPid))
                                {
                                    okWithSourceCell = true;
                                    break;
                                }
                            }
                            // si no existe información para (cPid,dir) consideramos que NO es compatible
                        }

                        if (!okWithSourceCell)
                            continue; // este candidato no respeta la celda que lo provocó

                        // 2) Además, debe coincidir con todos los vecinos colapsados ADYACENTES a (nx,ny)
                        bool okWithNeighbors = true;
                        foreach (var d2 in AdjacencyDirs())
                        {
                            int ex = nx + d2.x;
                            int ey = ny + d2.y;
                            if (ex < 0 || ey < 0 || ex >= pWidth || ey >= pHeight) continue;

                            var neigh2 = working[ex, ey];
                            if (!neigh2.IsCollapsed) continue;

                            int neighPid = neigh2.Final;
                            Vector2Int opp = new Vector2Int(-d2.x, -d2.y);

                            if (adjacencyAllowed.ContainsKey(neighPid) &&
                                adjacencyAllowed[neighPid].ContainsKey(opp))
                            {
                                if (!adjacencyAllowed[neighPid][opp].Contains(testPid))
                                {
                                    okWithNeighbors = false;
                                    break;
                                }
                            }
                            else
                            {
                                // si el vecino colapsado no tiene info para esa dirección, tratamos como incompatible
                                okWithNeighbors = false;
                                break;
                            }
                        }

                        if (okWithNeighbors)
                            relaxed.Add(testPid);
                    }
                    if (relaxed.Count == 0)
                        return false;

                    neighbor.possible = relaxed.ToList();
                    q.Enqueue(new Vector2Int(nx, ny));
                    continue;
                }


                if (newPossible.Count < neighbor.possible.Count)
                {
                    neighbor.possible = newPossible.ToList();
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
        return true;
    }

    PatternCell GetLowestEntropyWorking(PatternCell[,] working)
    {
        PatternCell best = null;
        float bestEntropy = float.MaxValue;
        var ties = new List<PatternCell>();

        foreach (var c in working)
        {
            if (c.IsCollapsed) continue;
            float sumP = 0f, sumPLogP = 0f;
            foreach (int pid in c.possible)
            {
                float p = patternWeights.ContainsKey(pid) ? patternWeights[pid] : 1f / patterns.Count;
                sumP += p;
                sumPLogP += p > 0 ? p * Mathf.Log(p) : 0f;
            }
            if (sumP <= 0f) continue;
            float entropy = -(sumPLogP / sumP);
            if (entropy < bestEntropy - 1e-6f) { bestEntropy = entropy; best = c; ties.Clear(); ties.Add(c); }
            else if (Mathf.Abs(entropy - bestEntropy) <= 1e-6f) ties.Add(c);
        }
        if (best == null) return null;
        return ties.Count == 1 ? ties[0] : ties[rng.Next(ties.Count)];
    }

    bool SolveBacktracking(PatternCell[,] working)
    {
        // completado?
        if (working.Cast<PatternCell>().All(c => c.IsCollapsed)) return true;

        var cell = GetLowestEntropyWorking(working);
        if (cell == null) return false;

        var candidates = GetCandidatesOrderedWorking(cell, working);
        foreach (int candidate in candidates)
        {
            var clone = CloneGrid(working);
            clone[cell.x, cell.y].possible = new List<int> { candidate };
            bool ok = Propagate(clone, cell.x, cell.y);
            if (!ok) continue;
            if (SolveBacktracking(clone))
            {
                // copiar solución de clone a working (profunda)
                for (int x = 0; x < pWidth; x++)
                    for (int y = 0; y < pHeight; y++)
                        working[x, y].possible = new List<int>(clone[x, y].possible);
                return true;
            }
        }
        return false;
    }

    // -------------------- Generate (con reinicios) --------------------
    public void Generate()
    {
        Debug.Log($"Seed: {fixedSeed}");

        ClearPrevious();

        // parsear input
        ParseInputFromString();

        // extraer patrones
        ExtractPatternsAndRules();

        // init grid
        InitializePatternGrid();

        // configuracion RNG
        if (useFixedSeed) rng = new System.Random(fixedSeed);
        else rng = new System.Random();

        bool solved = false;
        int attempt = 0;

        // guardamos el grid inicial (todas las celdas con todas las posibilidades)
        var originalGrid = CloneGrid(patternGrid);

        while (attempt < maxAttempts && !solved)
        {
            attempt++;

            // Para cada intento usamos una semilla distinta (si no usamos fixedSeed)
            if (!useFixedSeed)
            {
                // cambiamos random interno para variar elecciones
                rng = new System.Random(Guid.NewGuid().GetHashCode());
            }

            // clonamos el grid y resolvemos
            var working = CloneGrid(originalGrid);

            // Nota: para diversificar, podemos aplicar un pequeño shuffling a las listas posibles
            // (no necesario pero ayuda en casos de empates)
            ShuffleInitialPossibilities(working);

            // intentamos resolver con backtracking
            try
            {
                if (SolveBacktracking(working))
                {
                    // éxito: guardamos solución y rompemos
                    patternGrid = working;
                    solved = true;
                    Debug.Log($"[WFC] Solución encontrada en intento {attempt}.");
                    break;
                }
            }
            catch (Exception ex)
            {
                // si lanza excepción, la ignoramos y seguimos con siguiente intento
                Debug.LogWarning($"[WFC] Excepción en intento {attempt}: {ex.Message}");
            }
        }

        if (!solved)
        {
            Debug.LogWarning($"[WFC] No se encontró solución tras {attempt} intentos.");
            return;
        }

        // construir output final a partir de patternGrid
        BuildOutputFromPatternGrid();
        BuildOriginalMapVisual();
    }

    void BuildOriginalMapVisual()
    {
        if (inputExample == null)
        {
            Debug.LogWarning("[WFC] No hay mapa de entrada para visualizar.");
            return;
        }

        Transform parent = inputContainer != null ? inputContainer : transform;

        int iw = inputExample.width;
        int ih = inputExample.height;

        // Posición del mapa original (a la derecha del generado)
        float offsetX = (outputWidth + 2) * tileSpacing;

        Debug.Log("[WFC] Visualizando mapa original...");

        for (int y = 0; y < ih; y++)
        {
            for (int x = 0; x < iw; x++)
            {
                string id = inputExample[x, y];
                var tile = allTiles.FirstOrDefault(t => t.id == id);

                if (tile?.prefab != null)
                {
                    Vector3 pos = new Vector3(
                        offsetX + x * tileSpacing,
                        -y * tileSpacing,
                        0
                    );

                    Instantiate(tile.prefab, pos, Quaternion.identity, parent);
                }
                else
                {
                    Debug.LogWarning($"[WFC] No se encontró prefab para '{id}'");
                }
            }
        }

        Debug.Log("[WFC] Mapa original visualizado.");
    }

    void ShuffleInitialPossibilities(PatternCell[,] working)
    {
        // Para cada celda mezclamos el orden de 'possible' (esto influirá en los empates)
        foreach (var c in working)
        {
            var list = c.possible;
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }

    // -------------------- Reconstrucción final y visualización --------------------
    void BuildOutputFromPatternGrid()
    {
        ClearPrevious();

        int outW = outputWidth;
        int outH = outputHeight;
        int N = patternSize;
        string[] outputTiles = new string[outW * outH];

        for (int ty = 0; ty < outH; ty++)
        {
            for (int tx = 0; tx < outW; tx++)
            {
                bool assigned = false;
                for (int py = Math.Max(0, ty - N + 1); py <= Math.Min(pHeight - 1, ty) && !assigned; py++)
                {
                    for (int px = Math.Max(0, tx - N + 1); px <= Math.Min(pWidth - 1, tx) && !assigned; px++)
                    {
                        int pid = patternGrid[px, py].Final;
                        if (pid < 0 || pid >= patterns.Count) continue;
                        int ox = tx - px;
                        int oy = ty - py;
                        outputTiles[ty * outW + tx] = patterns[pid][oy * N + ox];
                        assigned = true;
                    }
                }

                if (!assigned)
                {
                    int fallback = patternWeights.OrderByDescending(kv => kv.Value).First().Key;
                    var pat = patterns[fallback];
                    int ox = tx % N;
                    int oy = ty % N;
                    outputTiles[ty * outW + tx] = pat[oy * N + ox];
                }
            }
        }

        Transform parent = parentContainer != null ? parentContainer : transform;

        for (int y = 0; y < outH; y++)
            for (int x = 0; x < outW; x++)
            {
                string id = outputTiles[y * outW + x];
                var tile = allTiles.FirstOrDefault(t => t.id == id);
                if (tile?.prefab != null)
                    Instantiate(tile.prefab, new Vector3(x * tileSpacing, -y * tileSpacing, 0), Quaternion.identity, parent);
            }

        Debug.Log("[WFC] Generación completada y visualizada.");
    }

    void ClearPrevious()
    {
        Transform parent = parentContainer != null ? parentContainer : transform;
        var children = new List<Transform>();
        foreach (Transform c in parent) children.Add(c);
        foreach (var c in children) Destroy(c.gameObject);
    }
}
