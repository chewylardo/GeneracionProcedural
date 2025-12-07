using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Renderiza un char[,] usando listas paralelas: symbols[i] -> prefabs[i].
/// </summary>
public class LevelRenderer : MonoBehaviour
{
    [Header("Paralelo: símbolo -> prefab")]
    public List<char> symbols = new List<char>();        // ejemplo: 'X','S','Q','E','-'
    public List<GameObject> prefabs = new List<GameObject>();
    public GameObject defaultPrefab;                     // opcional para símbolos no mapeados

    [Header("Escalado y spacing")]
    public Vector3 tileScale = Vector3.one;
    public float spacing = 1f;

    private GameObject parentContainer;

    public void ClearRendered()
    {
        if (parentContainer != null) Destroy(parentContainer);
        parentContainer = null;
    }

    GameObject GetPrefabForSymbol(char c)
    {
        int idx = symbols.IndexOf(c);
        if (idx >= 0 && idx < prefabs.Count) return prefabs[idx];
        return defaultPrefab;
    }

    // render y offset en unidades world
    public void Render(char[,] map, Vector3 offset, string parentName = "GeneratedMap")
    {
        if (map == null) return;
        if (parentContainer == null)
        {
            parentContainer = new GameObject("RenderedMaps");
            parentContainer.transform.parent = this.transform;
        }

        GameObject container = new GameObject(parentName);
        container.transform.parent = parentContainer.transform;

        int w = map.GetLength(0);
        int h = map.GetLength(1);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                char s = map[x, y];
                if (s == '-') continue; // vacío, no render

                GameObject prefab = GetPrefabForSymbol(s);
                if (prefab == null) continue;

                Vector3 pos = new Vector3(x * spacing, y * spacing, 0f) + offset;
                GameObject go = Instantiate(prefab, pos, Quaternion.identity, container.transform);
                go.transform.localScale = tileScale;
                go.name = $"Tile_{s}_{x}_{y}";
            }
        }
    }
}
