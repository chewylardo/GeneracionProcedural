using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LevelVisualizer : MonoBehaviour
{
    [Header("Referencias")]
    public MarkovGenerator generator;
    public TextMeshProUGUI outputText; // Asigna un Text UI en el Canvas

    [Header("Visualización en Tiles (opcional)")]
    public GameObject tilePrefab; // Prefab para representar un símbolo
    public float spacing = 1.0f;

    public void GenerateAndDisplay()
    {
        string level = generator.GenerateLevelMarkov();
        Debug.Log($"Nivel generado:\n{level}");

        if (outputText != null)
            outputText.text = level;

        // Si se quiere mostrar como tiles
        if (tilePrefab != null)
            VisualizeTiles(level);
    }

    private void VisualizeTiles(string level)
    {
        // Limpia los objetos anteriores
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        int width = Mathf.CeilToInt(Mathf.Sqrt(level.Length));
        for (int i = 0; i < level.Length; i++)
        {
            int x = i % width;
            int y = i / width;
            GameObject tile = Instantiate(tilePrefab, new Vector3(x * spacing, -y * spacing, 0), Quaternion.identity, transform);
            TextMesh t = tile.GetComponentInChildren<TextMesh>();
            if (t != null)
                t.text = level[i].ToString();
        }
    }
}
