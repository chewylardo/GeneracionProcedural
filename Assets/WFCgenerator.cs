using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// WFC SIN BACKTRACKING — versión rápida y estable.
/// </summary>
public class WFCGenerator : MonoBehaviour
{
    [Header("WFC Parameters")]
    [Range(1, 6)] public int patternSize = 2;
    public int maxAttempts = 40;
    public int maxMillis = 3000;
    public bool useFixedSeed = false;
    public int fixedSeed = 12345;

    private Dictionary<string, int> patternKeyToId;
    private List<string[]> patterns;
    private Dictionary<int, float> patternWeights;
    private Dictionary<int, Dictionary<Vector2Int, List<int>>> adjacencyAllowed;

    private System.Random rng;

    // =========================================================
    // PUBLIC API
    // =========================================================
    public void Train(List<char[,]> levels)
    {
        string raw = BuildRawFromLevels(levels);
        var grid = new StringGrid();
        grid.FromRawString(raw);
        ExtractPatternsAndRules(grid);

        Debug.Log($"[WFC] Train OK — patrones: {patterns.Count}");
    }

    public char[,] Generate(int outW, int outH)
    {
        if (patterns == null)
        {
            Debug.LogError("[WFC] Generate antes de Train.");
            return null;
        }

        if (useFixedSeed) rng = new System.Random(fixedSeed);
        else rng = new System.Random();

        Stopwatch sw = Stopwatch.StartNew();

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (!useFixedSeed)
                rng = new System.Random(Guid.NewGuid().GetHashCode());

            int pW = outW - patternSize + 1;
            int pH = outH - patternSize + 1;

            if (pW <= 0 || pH <= 0)
            {
                Debug.LogError("[WFC] Output demasiado pequeño.");
                return null;
            }

            var grid = CreateInitialPatternGrid(pW, pH);
            Shuffle(grid);

            bool ok = RunGreedy(grid, sw);

            if (ok)
                return BuildOutput(grid, outW, outH);

            if (sw.ElapsedMilliseconds >= maxMillis)
                break;
        }

        Debug.LogWarning("[WFC] Sin solución — devolviendo nivel relajado.");
        return RelaxedFill(outW, outH);
    }

    // =========================================================
    // EXTRACT
    // =========================================================
    void ExtractPatternsAndRules(StringGrid input)
    {
        patternKeyToId = new Dictionary<string, int>();
        patterns = new List<string[]>();
        patternWeights = new Dictionary<int, float>();

        int iw = input.width;
        int ih = input.height;
        int N = patternSize;

        // PATTERNS
        for (int y = 0; y <= ih - N; y++)
        {
            for (int x = 0; x <= iw - N; x++)
            {
                string[] pat = new string[N * N];
                int idx = 0;

                for (int yy = 0; yy < N; yy++)
                    for (int xx = 0; xx < N; xx++)
                        pat[idx++] = input[x + xx, y + yy];

                string key = string.Join(",", pat);

                if (!patternKeyToId.ContainsKey(key))
                {
                    int id = patterns.Count;
                    patternKeyToId[key] = id;
                    patterns.Add(pat);
                    patternWeights[id] = 0;
                }

                patternWeights[patternKeyToId[key]] += 1;
            }
        }

        // NORMALIZE
        float total = patternWeights.Values.Sum();
        foreach (var k in patternWeights.Keys.ToList())
            patternWeights[k] /= total;

        // ADJACENCY 4-dir
        adjacencyAllowed = new Dictionary<int, Dictionary<Vector2Int, List<int>>>();

        Vector2Int[] dirs = {
            new Vector2Int(1,0),
            new Vector2Int(-1,0),
            new Vector2Int(0,1),
            new Vector2Int(0,-1)
        };

        for (int i = 0; i < patterns.Count; i++)
        {
            adjacencyAllowed[i] = new Dictionary<Vector2Int, List<int>>();
            foreach (var d in dirs)
                adjacencyAllowed[i][d] = new List<int>();
        }

        // Build adjacency from input
        for (int y = 0; y <= ih - N; y++)
        {
            for (int x = 0; x <= iw - N; x++)
            {
                int pid = GetPatternId(input, x, y, N);

                foreach (var d in dirs)
                {
                    int nx = x + d.x;
                    int ny = y + d.y;

                    if (nx >= 0 && ny >= 0 && nx <= iw - N && ny <= ih - N)
                    {
                        int npid = GetPatternId(input, nx, ny, N);
                        adjacencyAllowed[pid][d].Add(npid);
                    }
                }
            }
        }
    }

    int GetPatternId(StringGrid g, int x, int y, int N)
    {
        string[] pat = new string[N * N];
        int idx = 0;

        for (int yy = 0; yy < N; yy++)
            for (int xx = 0; xx < N; xx++)
                pat[idx++] = g[x + xx, y + yy];

        return patternKeyToId[string.Join(",", pat)];
    }

    // =========================================================
    // GRID + GREEDY SOLVE
    // =========================================================
    PatternCell[,] CreateInitialPatternGrid(int w, int h)
    {
        var grid = new PatternCell[w, h];
        var ids = Enumerable.Range(0, patterns.Count).ToList();

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                grid[x, y] = new PatternCell(ids) { x = x, y = y };

        return grid;
    }

    void Shuffle(PatternCell[,] grid)
    {
        foreach (var c in grid)
        {
            for (int i = c.possible.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int tmp = c.possible[i];
                c.possible[i] = c.possible[j];
                c.possible[j] = tmp;
            }
        }
    }

    bool RunGreedy(PatternCell[,] grid, Stopwatch sw)
    {
        var queue = new Queue<Vector2Int>();

        while (true)
        {
            if (sw.ElapsedMilliseconds >= maxMillis) return false;

            PatternCell cell = GetLowestEntropy(grid);
            if (cell == null)
                return true; // solved

            int chosen = PickBest(cell);
            cell.possible = new List<int> { chosen };

            queue.Enqueue(new Vector2Int(cell.x, cell.y));

            if (!Propagate(grid, queue)) return false;
        }
    }

    bool Propagate(PatternCell[,] grid, Queue<Vector2Int> q)
    {
        int w = grid.GetLength(0);
        int h = grid.GetLength(1);

        Vector2Int[] dirs = {
            new Vector2Int(1,0),
            new Vector2Int(-1,0),
            new Vector2Int(0,1),
            new Vector2Int(0,-1)
        };

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            var cell = grid[p.x, p.y];

            foreach (var d in dirs)
            {
                int nx = p.x + d.x, ny = p.y + d.y;

                if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                    continue;

                var neighbor = grid[nx, ny];
                if (neighbor.IsCollapsed) continue;

                var allowed = new HashSet<int>();

                foreach (var pid in cell.possible)
                    foreach (var np in adjacencyAllowed[pid][d])
                        allowed.Add(np);

                var newList = neighbor.possible.Where(pid => allowed.Contains(pid)).ToList();

                if (newList.Count == 0)
                    return false;

                if (newList.Count < neighbor.possible.Count)
                {
                    neighbor.possible = newList;
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
        return true;
    }

    PatternCell GetLowestEntropy(PatternCell[,] grid)
    {
        PatternCell best = null;
        float bestEntropy = float.MaxValue;

        foreach (var cell in grid)
        {
            if (cell.possible.Count <= 1) continue;

            float sum = 0, sumLog = 0;

            foreach (int pid in cell.possible)
            {
                float p = patternWeights[pid];
                sum += p;
                sumLog += p * Mathf.Log(p);
            }

            float entropy = -(sumLog / sum);

            if (entropy < bestEntropy)
            {
                bestEntropy = entropy;
                best = cell;
            }
        }

        return best;
    }

    int PickBest(PatternCell cell)
    {
        // picks by weight (highest probability)
        return cell.possible
            .OrderByDescending(pid => patternWeights[pid])
            .First();
    }

    // =========================================================
    // OUTPUT
    // =========================================================
    char[,] BuildOutput(PatternCell[,] pg, int outW, int outH)
    {
        int N = patternSize;
        char[,] final = new char[outW, outH];

        for (int y = 0; y < outH; y++)
        {
            for (int x = 0; x < outW; x++)
            {
                bool assigned = false;

                for (int py = Math.Max(0, y - N + 1); py <= Math.Min(pg.GetLength(1) - 1, y) && !assigned; py++)
                {
                    for (int px = Math.Max(0, x - N + 1); px <= Math.Min(pg.GetLength(0) - 1, x) && !assigned; px++)
                    {
                        int pid = pg[px, py].Final;
                        if (pid == -1) continue;

                        int ox = x - px;
                        int oy = y - py;

                        final[x, y] = patterns[pid][oy * N + ox][0];
                        assigned = true;
                    }
                }

                if (!assigned)
                    final[x, y] = '-';
            }
        }

        return final;
    }

    // =========================================================
    // RELAXED fallback
    // =========================================================
    char[,] RelaxedFill(int w, int h)
    {
        char[,] result = new char[w, h];
        int best = patternWeights.OrderByDescending(kv => kv.Value).First().Key;
        string[] pat = patterns[best];

        int N = patternSize;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                result[x, y] = pat[(y % N) * N + (x % N)][0];

        return result;
    }

    // =========================================================
    // UTILS
    // =========================================================
    string BuildRawFromLevels(List<char[,]> levels)
    {
        List<string> blocks = new List<string>();

        foreach (var g in levels)
        {
            int w = g.GetLength(0);
            int h = g.GetLength(1);
            string[] lines = new string[h];

            for (int y = h - 1; y >= 0; y--)
            {
                char[] row = new char[w];
                for (int x = 0; x < w; x++)
                    row[x] = g[x, y];
                lines[h - 1 - y] = new string(row);
            }

            blocks.Add(string.Join("\n", lines));
        }

        return string.Join("\n\n", blocks);
    }
}

// =========================================================
// SUPPORT CLASSES
// =========================================================
[Serializable]
public class PatternCell
{
    public List<int> possible;
    public int x, y;

    public PatternCell(List<int> poss)
    {
        possible = new List<int>(poss);
    }

    public bool IsCollapsed => possible.Count == 1;
    public int Final => IsCollapsed ? possible[0] : -1;
}

public class StringGrid
{
    public int width { get; private set; }
    public int height { get; private set; }

    private string[] rows;

    public void FromRawString(string raw)
    {
        var lines = raw.Replace("\r", "").Split('\n');
        var valid = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

        height = valid.Count;
        width = valid.Max(s => s.Length);

        rows = new string[height];
        for (int y = 0; y < height; y++)
        {
            string ln = valid[y];
            rows[y] = ln.PadRight(width, '-');
        }
    }

    public string this[int x, int y]
    {
        get
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return "-";
            return rows[y][x].ToString();
        }
    }
}
