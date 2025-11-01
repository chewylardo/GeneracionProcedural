using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    [Header("Configuración del mapa")]
    public int width = 10;
    public int height = 10;
    public List<TileData> allTiles;

    [Header("Opciones visuales")]
    public float tileSpacing = 1f;

    private GridCell[,] grid;

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        // Limpiar escena anterior
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        // Inicializar todas las celdas
        grid = new GridCell[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = new GridCell(allTiles);

        // Ejecutar el algoritmo
        while (true)
        {
            var cell = GetCellWithLowestEntropy();
            if (cell == null) break;

            Collapse(cell);
            if (!Propagate())
            {
                Debug.LogWarning("Contradicción detectada. Reiniciando...");
                Generate();
                return;
            }
        }

        //Visualizar resultado
        RenderResult();
    }

    GridCell GetCellWithLowestEntropy()
    {
        var uncollapsed = new List<GridCell>();
        foreach (var c in grid)
        {
            if (!c.collapsed) uncollapsed.Add(c);
        }


        if (uncollapsed.Count == 0) { return null; }

        return uncollapsed
            .OrderBy(c => c.possibleTiles.Count)
            .ThenBy(_ => Random.value)
            .First();
    }

    void Collapse(GridCell cell)
    {
        var chosen = cell.possibleTiles[Random.Range(0, cell.possibleTiles.Count)];
        cell.possibleTiles.Clear();
        cell.possibleTiles.Add(chosen);
    }

    bool Propagate()
    {
        bool changed;
        do
        {
            changed = false;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = grid[x, y];
                    if (cell.collapsed) continue;

                    var valid = new List<TileData>(cell.possibleTiles);
                    foreach (var tile in valid)
                    {
                        if (!IsCompatible(x, y, tile))
                        {
                            cell.possibleTiles.Remove(tile);
                            changed = true;

                            if (cell.possibleTiles.Count == 0) {  return false; }// contradicción 

                        }
                    }
                }
            }
        } while (changed);

        return true;
    }

    bool IsCompatible(int x, int y, TileData tile)
    {
        // Norte
        if (y < height - 1)
        {
            var neighbor = grid[x, y + 1];
            if (neighbor.collapsed && !tile.norte.Contains(neighbor.GetTile()))
                return false;
        }
        // Sur
        if (y > 0)
        {
            var neighbor = grid[x, y - 1];
            if (neighbor.collapsed && !tile.sur.Contains(neighbor.GetTile()))
                return false;
        }
        // Este
        if (x < width - 1)
        {
            var neighbor = grid[x + 1, y];
            if (neighbor.collapsed && !tile.este.Contains(neighbor.GetTile()))
                return false;
        }
        // Oeste
        if (x > 0)
        {
            var neighbor = grid[x - 1, y];
            if (neighbor.collapsed && !tile.oeste.Contains(neighbor.GetTile()))
                return false;
        }

        return true;
    }

    void RenderResult()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tile = grid[x, y].GetTile();
                if (tile != null)
                {
                    var pos = new Vector3(x * tileSpacing, y * tileSpacing, 0);
                    Instantiate(tile.prefab, pos, Quaternion.identity, transform);
                }
            }
        }
    }
}
