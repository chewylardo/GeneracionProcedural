using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Carga varios TXT (arrastrados en inspector) y devuelve List<char[,]>
/// Robusto: filtra líneas vacías y rellena con '-' si una línea es más corta.
/// </summary>
public class LevelLoader : MonoBehaviour
{
    [Header("Arrastra aquí los TXT (hasta 5)")]
    public List<TextAsset> levelFiles = new List<TextAsset>();

    // Carga y devuelve todos los niveles válidos como char[,]
    public List<char[,]> LoadLevels()
    {
        List<char[,]> levels = new List<char[,]>();

        foreach (var file in levelFiles)
        {
            if (file == null) continue;

            string[] rawLines = file.text.Replace("\r", "").Split('\n');

            List<string> validLines = new List<string>();
            foreach (var l in rawLines)
            {
                if (!string.IsNullOrWhiteSpace(l))
                    validLines.Add(l);
            }

            if (validLines.Count == 0) continue;

            int width = 0;
            foreach (var l in validLines) if (l.Length > width) width = l.Length;
            int height = validLines.Count;

            char[,] grid = new char[width, height];

            for (int y = 0; y < height; y++)
            {
                string line = validLines[y];
                int gridY = height - 1 - y; // invertir Y: línea 0 -> abajo
                for (int x = 0; x < width; x++)
                {
                    if (x < line.Length) grid[x, gridY] = line[x];
                    else grid[x, gridY] = '-';
                }
            }

            levels.Add(grid);
        }

        return levels;
    }
}
