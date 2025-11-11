using System.Collections.Generic;
using UnityEngine;

public class MarkovMapGenerator : MonoBehaviour
{
    [Header("Configuración del modelo Markov (columna por columna)")]
    [Range(1, 5)] public int N = 2;
    [TextArea(10, 50)] public string trainingData;
    [Range(5, 300)] public int mapWidth = 50;
    [Range(5, 200)] public int mapHeight = 14;
    public float spacing = 1.0f;

    [Header("Prefabs por símbolo")]
    public List<char> symbols = new List<char> { '-', '#', '?', 'p', 'E', 'x' };
    public List<GameObject> prefabs = new List<GameObject>();
    public GameObject defaultPrefab;

    [Header("Visualización")]
    public float mapOffset = 4.0f;

    private Dictionary<string, Dictionary<string, int>> columnModel;
    private List<string> columns = new List<string>();
    private Transform mapParent;

    void Start() => GenerateAndDisplay();

    [ContextMenu("Generar nuevo mapa")]
    public void GenerateAndDisplay()
    {
        if (mapParent != null)
            DestroyImmediate(mapParent.gameObject);

        mapParent = new GameObject("GeneratedMaps").transform;
        mapParent.parent = this.transform;

        BuildColumnModel();
        List<string> generatedColumns = GenerateColumns(mapWidth);

        VisualizeTrainingData(Vector3.zero);
        VisualizeGeneratedColumns(generatedColumns, new Vector3((mapWidth + mapOffset) * spacing, 0, 0));
        Debug.Log("Mapas generados columna por columna correctamente.");
    }

    // entrenamiento por columnas
    private void BuildColumnModel()
    {
        columnModel = new Dictionary<string, Dictionary<string, int>>();
        columns.Clear();

        if (string.IsNullOrWhiteSpace(trainingData))
        {
            Debug.LogError("El campo 'trainingData' está vacío.");
            return;
        }

        // Convertir a matriz (cada línea = fila)
        string[] lines = trainingData.TrimEnd().Split('\n');
        mapHeight = lines.Length;
        int width = 0;
        foreach (string l in lines)
        {
            if (l.Length > width) width = l.Length;
        }
           

        // Leer columna por columna (de arriba hacia abajo)
        for (int x = 0; x < width; x++)
        {
            string col = "";
            for (int y = 0; y < mapHeight; y++)
            {
                string line = lines[y];
                char c = (x < line.Length) ? line[x] : '-'; // relleno vacío
                col += c;
            }
            columns.Add(col);
        }

        if (columns.Count < N)
        {
            Debug.LogWarning($"Muy pocas columnas ({columns.Count}) para N={N}. Bajando N a 1.");
            N = 1;
        }

        // Entrenar modelo Markov
        for (int i = 0; i < columns.Count - N; i++)
        {
            string key = string.Join("|", columns.GetRange(i, N - 1));
            string next = columns[i + N - 1];

            if (!columnModel.ContainsKey(key))
                
                columnModel[key] = new Dictionary<string, int>();

            if (!columnModel[key].ContainsKey(next))
                columnModel[key][next] = 0;

            columnModel[key][next]++;

        }

        Debug.Log($"modelo entrenado con {columnModel.Count} n-gramas (N={N}) leyendo columnas ({columns.Count} columnas en total).");
    }

    // generar mapa nuevo
    private List<string> GenerateColumns(int totalColumns)
    {
        List<string> result = new List<string>();
        if (columnModel == null || columnModel.Count == 0)
        {
            Debug.LogError("modelo vacio. asegurate de tener trainingData valido.");
            return result;
        }

        List<string> keys = new List<string>(columnModel.Keys);
        string currentKey = keys[Random.Range(0, keys.Count)];
        result.AddRange(currentKey.Split('|'));

        for (int i = result.Count; i < totalColumns; i++)
        {
            if (!columnModel.ContainsKey(currentKey))
                currentKey = keys[Random.Range(0, keys.Count)];

            string nextCol = WeightedColumnChoice(columnModel[currentKey]);
            result.Add(nextCol);

            int start = Mathf.Max(0, result.Count - (N - 1));
            currentKey = string.Join("|", result.GetRange(start, Mathf.Min(N - 1, result.Count - start)));
        }

        return result;
    }

    private string WeightedColumnChoice(Dictionary<string, int> options)
    {
        int total = 0;
        foreach (var kv in options) total += kv.Value;
        int r = Random.Range(0, total);
        int cumulative = 0;

        foreach (var kv in options)
        {
            cumulative += kv.Value;
            if (r < cumulative)
                return kv.Key;
        }

        foreach (var kv in options)
            return kv.Key;

        return columns[0];
    }

    // visualizacion
    private void VisualizeGeneratedColumns(List<string> generatedColumns, Vector3 offset)
    {
        GameObject generatedParent = new GameObject("GeneratedMap");
        generatedParent.transform.parent = mapParent;

        for (int x = 0; x < generatedColumns.Count; x++)
        {
            string col = generatedColumns[x];
            for (int y = 0; y < col.Length; y++)
            {
                char c = col[y];
                GameObject prefab = GetPrefab(c);
                if (prefab == null) prefab = defaultPrefab;
                if (prefab != null)
                {
                    Vector3 pos = new Vector3(x * spacing, -y * spacing, 0) + offset;
                    Instantiate(prefab, pos, Quaternion.identity, generatedParent.transform);
                }
            }
        }
    }

    private void VisualizeTrainingData(Vector3 offset)
    {
        GameObject trainingParent = new GameObject("TrainingMap");
        trainingParent.transform.parent = mapParent;

        string[] lines = trainingData.TrimEnd().Split('\n');
        for (int y = 0; y < lines.Length; y++)
        {
            string line = lines[y];
            for (int x = 0; x < line.Length; x++)
            {
                char c = line[x];
                GameObject prefab = GetPrefab(c);
                if (prefab == null) prefab = defaultPrefab;
                if (prefab != null)
                {
                    Vector3 pos = new Vector3(x * spacing, -y * spacing, 0) + offset;
                    Instantiate(prefab, pos, Quaternion.identity, trainingParent.transform);
                }
            }
        }
    }

    private GameObject GetPrefab(char symbol)
    {
        int index = symbols.IndexOf(symbol);
        if (index >= 0 && index < prefabs.Count)
            return prefabs[index];
        return null;
    }
}
