using System.Collections.Generic;
using UnityEngine;

public class WFCGenerator : MonoBehaviour
{
    [Header("WFC Params")]
    public int patternSize = 2;
    public int maxAttempts = 200;
    public bool useFixedSeed = false;
    public int fixedSeed = 12345;

    private System.Random rng;


    private List<char[,]> patterns = new List<char[,]>();


    private Dictionary<int, Dictionary<Vector2Int, HashSet<int>>> adjacencyRules =
        new Dictionary<int, Dictionary<Vector2Int, HashSet<int>>>();

    public void Train(List<char[,]> levels)
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogWarning("[WFC] No levels provided. Training aborted.");
            return;
        }

      
        rng = useFixedSeed ? new System.Random(fixedSeed) : new System.Random();

       
        patterns.Clear();
        adjacencyRules.Clear();

        
        ExtractPatternsAndRules(levels);

        Debug.Log($"[WFC] Trained with {patterns.Count} patterns.");
    }

    public char[,] Generate(int width, int height)
    {
        if (patterns.Count == 0)
        {
            Debug.LogWarning("[WFC] No patterns trained.");
            return null;
        }

        // Seed
        rng = useFixedSeed ? new System.Random(fixedSeed) : new System.Random();

        // We attempt up to maxAttempts times
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var result = AttemptGenerate(width, height);
            if (result != null)
                return result;
        }

        Debug.LogWarning("[WFC] Generation failed after max attempts.");
        return null;
    }

 
    private char[,] AttemptGenerate(int width, int height)
    {
        int W = width;
        int H = height;

      
        List<int>[,] wave = new List<int>[W, H];

       
        for (int x = 0; x < W; x++)
        {
            for (int y = 0; y < H; y++)
            {
                wave[x, y] = new List<int>();
                for (int p = 0; p < patterns.Count; p++)
                    wave[x, y].Add(p);
            }
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();

      
        while (true)
        {
            Vector2Int? cell = FindLowestEntropyCell(wave, W, H);
            if (cell == null)
                break; // all collapsed

            Vector2Int pos = cell.Value;
            if (wave[pos.x, pos.y].Count == 0)
                return null; // contradiction

            // Collapse: pick one pattern
            int chosen = wave[pos.x, pos.y][rng.Next(wave[pos.x, pos.y].Count)];
            wave[pos.x, pos.y].Clear();
            wave[pos.x, pos.y].Add(chosen);

            queue.Enqueue(pos);
            if (!PropagateQueue(wave, queue, W, H))
                return null;
        }

   
        char[,] output = new char[W, H];
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                int patIdx = wave[x, y][0];
                char[,] pat = patterns[patIdx];
                output[x, y] = pat[0, 0]; // top-left char
            }

        return output;
    }


    private Vector2Int? FindLowestEntropyCell(List<int>[,] wave, int W, int H)
    {
        int minCount = int.MaxValue;
        Vector2Int? best = null;

        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                int c = wave[x, y].Count;
                if (c > 1 && c < minCount)
                {
                    minCount = c;
                    best = new Vector2Int(x, y);
                }
            }

        return best;
    }

    private bool PropagateQueue(List<int>[,] wave, Queue<Vector2Int> queue, int W, int H)
    {
       
        Vector2Int[] dirs = new Vector2Int[]
        {
            new Vector2Int(1, 0),   new Vector2Int(-1, 0),
            new Vector2Int(0, 1),   new Vector2Int(0, -1),
            new Vector2Int(1, 1),   new Vector2Int(-1, -1),
            new Vector2Int(1, -1),  new Vector2Int(-1, 1)
        };

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();
            int x = pos.x;
            int y = pos.y;

            foreach (var d in dirs)
            {
                int nx = x + d.x;
                int ny = y + d.y;

                if (nx < 0 || nx >= W || ny < 0 || ny >= H)
                    continue;

                bool changed = false;

                List<int> allowedNeighbors = new List<int>(wave[nx, ny]);

                for (int i = allowedNeighbors.Count - 1; i >= 0; i--)
                {
                    int p = allowedNeighbors[i];

                    bool anyCompatible = false;
                    foreach (int myP in wave[x, y])
                    {
                        if (adjacencyRules[myP][d].Contains(p))
                        {
                            anyCompatible = true;
                            break;
                        }
                    }

                    if (!anyCompatible)
                    {
                        wave[nx, ny].Remove(p);
                        changed = true;
                    }
                }

                if (wave[nx, ny].Count == 0)
                    return false;

                if (changed)
                    queue.Enqueue(new Vector2Int(nx, ny));
            }
        }

        return true;
    }

 
    private void ExtractPatternsAndRules(List<char[,]> levels)
    {
        HashSet<string> seen = new HashSet<string>();

    
        Vector2Int[] dirs = new Vector2Int[]
        {
            new Vector2Int(1, 0),   new Vector2Int(-1, 0),
            new Vector2Int(0, 1),   new Vector2Int(0, -1),
            new Vector2Int(1, 1),   new Vector2Int(-1, -1),
            new Vector2Int(1, -1),  new Vector2Int(-1, 1)
        };

        foreach (var grid in levels)
        {
            int W = grid.GetLength(0);
            int H = grid.GetLength(1);

            for (int x = 0; x <= W - patternSize; x++)
                for (int y = 0; y <= H - patternSize; y++)
                {
                    char[,] pat = new char[patternSize, patternSize];

                    for (int px = 0; px < patternSize; px++)
                        for (int py = 0; py < patternSize; py++)
                            pat[px, py] = grid[x + px, y + py];

                    string key = PatternToString(pat);
                    if (!seen.Contains(key))
                    {
                        seen.Add(key);
                        patterns.Add(pat);
                    }
                }
        }

  
        for (int i = 0; i < patterns.Count; i++)
        {
            adjacencyRules[i] = new Dictionary<Vector2Int, HashSet<int>>();

            foreach (var d in dirs)
                adjacencyRules[i][d] = new HashSet<int>();
        }

        // Build adjacency rules
        for (int i = 0; i < patterns.Count; i++)
            for (int j = 0; j < patterns.Count; j++)
                foreach (var d in dirs)
                    if (CheckCompatible(patterns[i], patterns[j], d))
                        adjacencyRules[i][d].Add(j);
    }

    // -------------------------------------------------------
    private bool CheckCompatible(char[,] A, char[,] B, Vector2Int dir)
    {
        int ps = patternSize;

        int ax0 = Mathf.Clamp(dir.x, 0, ps - 1);
        int ay0 = Mathf.Clamp(dir.y, 0, ps - 1);

        int bx0 = Mathf.Clamp(-dir.x, 0, ps - 1);
        int by0 = Mathf.Clamp(-dir.y, 0, ps - 1);

        int overlapX = ps - Mathf.Abs(dir.x);
        int overlapY = ps - Mathf.Abs(dir.y);

        if (overlapX <= 0 || overlapY <= 0)
            return false;

        for (int x = 0; x < overlapX; x++)
            for (int y = 0; y < overlapY; y++)
                if (A[ax0 + x, ay0 + y] != B[bx0 + x, by0 + y])
                    return false;

        return true;
    }

    private string PatternToString(char[,] pat)
    {
        string s = "";
        for (int y = 0; y < patternSize; y++)
            for (int x = 0; x < patternSize; x++)
                s += pat[x, y];
        return s;
    }
}
