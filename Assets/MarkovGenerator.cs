using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Entrena modelo Markov por columnas desde varios grids (concatenando ejemplos).
/// Genera columnas para producir char[,] de salida.
/// </summary>
public class MarkovGenerator : MonoBehaviour
{
    [Header("Parámetros Markov")]
    [Range(1, 5)]
    public int N = 2; // n-gram (columns)
    public int mapWidth = 150;
    public int mapHeight = 14;

    private Dictionary<string, Dictionary<string, int>> columnModel;
    private List<string> trainingColumns = new List<string>();
    private System.Random rng = new System.Random();

    // Train with multiple levels (list of char[,])
    public void Train(List<char[,]> levels)
    {
        // build training string columns: for each level, build columns string top->bottom
        trainingColumns.Clear();

        foreach (var grid in levels)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);

            for (int x = 0; x < w; x++)
            {
                char[] col = new char[h];
                for (int y = 0; y < h; y++) col[y] = grid[x, y];
                trainingColumns.Add(new string(col));
            }
        }

        if (trainingColumns.Count == 0)
        {
            Debug.LogWarning("[Markov] No training columns.");
            columnModel = new Dictionary<string, Dictionary<string, int>>();
            return;
        }

        if (N < 1) N = 1;
        if (N > 5) N = 5;
        if (trainingColumns.Count < N) { N = 1; Debug.LogWarning("[Markov] Few columns; N set to 1."); }

        // build n-gram model (keys are N-1 columns joined by |)
        columnModel = new Dictionary<string, Dictionary<string, int>>();

        for (int i = 0; i < trainingColumns.Count - (N - 1); i++)
        {
            int keyLen = Mathf.Max(0, N - 1);
            var keyCols = trainingColumns.GetRange(i, keyLen);
            string key = string.Join("|", keyCols);
            string next = trainingColumns[i + keyLen];

            if (!columnModel.ContainsKey(key)) columnModel[key] = new Dictionary<string, int>();
            if (!columnModel[key].ContainsKey(next)) columnModel[key][next] = 0;
            columnModel[key][next]++;
        }

        Debug.Log($"[Markov] Entrenado con {columnModel.Count} entradas (N={N}).");
    }

    // generate char[,] using the model
    public char[,] Generate(int width, int height)
    {
        mapWidth = Mathf.Max(1, width);
        mapHeight = Mathf.Max(1, height);

        char[,] outMap = new char[mapWidth, mapHeight];

        if (columnModel == null || columnModel.Count == 0)
        {
            Debug.LogWarning("[Markov] Modelo vacío; generando columnas aleatorias del entrenamiento.");
            // fallback: sample random training column repeated/truncated/padded
            for (int x = 0; x < mapWidth; x++)
            {
                string src = trainingColumns[rng.Next(trainingColumns.Count)];
                for (int y = 0; y < mapHeight; y++)
                    outMap[x, y] = y < src.Length ? src[y] : '-';
            }
            return outMap;
        }

        // pick initial key at random
        List<string> keys = new List<string>(columnModel.Keys);
        string currentKey = keys[rng.Next(keys.Count)];

        List<string> resultCols = new List<string>();
        if (N - 1 > 0)
            resultCols.AddRange(currentKey.Split('|'));
        else
            resultCols.Add(""); // empty

        // generate columns until width
        while (resultCols.Count < mapWidth)
        {
            string keyForLookup;
            if (N - 1 > 0)
            {
                int start = Mathf.Max(0, resultCols.Count - (N - 1));
                keyForLookup = string.Join("|", resultCols.GetRange(start, Mathf.Min(N - 1, resultCols.Count - start)));
            }
            else keyForLookup = "";

            if (!columnModel.ContainsKey(keyForLookup))
            {
                // pick random key
                keyForLookup = keys[rng.Next(keys.Count)];
            }

            string next = ChooseWeighted(columnModel[keyForLookup]);
            resultCols.Add(next);
        }

        // fill outMap from resultCols (top->bottom)
        for (int x = 0; x < mapWidth; x++)
        {
            string col = resultCols[x];
            for (int y = 0; y < mapHeight; y++)
                outMap[x, y] = (y < col.Length) ? col[y] : '-';
        }

        return outMap;
    }

    string ChooseWeighted(Dictionary<string, int> map)
    {
        int total = 0;
        foreach (var v in map.Values) total += v;
        int r = rng.Next(total);
        int acc = 0;
        foreach (var kv in map)
        {
            acc += kv.Value;
            if (r < acc) return kv.Key;
        }
        // fallback
        foreach (var kv in map) return kv.Key;
        return trainingColumns[0];
    }
}
