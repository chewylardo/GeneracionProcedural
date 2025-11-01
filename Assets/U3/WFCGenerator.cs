using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class NeighborRule
{
    // Lista de IDs de tiles permitidos en cada dirección para este tile
    public List<int> north = new List<int>();
    public List<int> south = new List<int>();
    public List<int> east = new List<int>();
    public List<int> west = new List<int>();

    // Helper para obtener por dirección
    public List<int> Get(Direction d)
    {
        switch (d)
        {
            case Direction.North: return north;
            case Direction.South: return south;
            case Direction.East: return east;
            case Direction.West: return west;
            default: return new List<int>();
        }
    }
}

public enum Direction { North, South, East, West }

public static class DirectionUtil
{
    public static Vector2Int ToOffset(Direction d)
    {
        switch (d)
        {
            case Direction.North: return new Vector2Int(0, 1);
            case Direction.South: return new Vector2Int(0, -1);
            case Direction.East: return new Vector2Int(1, 0);
            case Direction.West: return new Vector2Int(-1, 0);
        }
        return Vector2Int.zero;
    }

    public static Direction Opposite(Direction d)
    {
        switch (d)
        {
            case Direction.North: return Direction.South;
            case Direction.South: return Direction.North;
            case Direction.East: return Direction.West;
            case Direction.West: return Direction.East;
        }
        return d;
    }

    public static IEnumerable<Direction> All => new[] { Direction.North, Direction.South, Direction.East, Direction.West };
}

[Serializable]
public class TileRules
{
    // index matches tile ID
    public List<NeighborRule> rules = new List<NeighborRule>();
}

public class WFCGenerator : MonoBehaviour
{
    [Header("Tiles / Rules")]
    [Tooltip("Array of sprites; index = tile ID (0..N-1)")]
    public Sprite[] tileSprites;

    [Tooltip("Define las reglas de adyacencia para cada tile (por ID)")]
    public TileRules tileRules;

    [Header("Grid")]
    public int width = 20;
    public int height = 12;
    public float tileSize = 0.32f; // 32 px -> 0.32 world units if 100px = 1 unit; ajusta según tu escala

    [Header("Visualización")]
    public Transform parentForTiles; // prefab parent
    public GameObject tilePrefab; // prefab con SpriteRenderer (si no, se crea simple GameObject)
    public bool visualizeSteps = true;
    public float stepDelay = 0.05f; // 0 => instantáneo

    // Internal structures
    private Cell[,] grid;
    private System.Random rng = new System.Random();

    [Serializable]
    private class Cell
    {
        // posibles IDs en esta celda
        public HashSet<int> possible;
        public bool Collapsed => possible.Count == 1;

        public Cell(IEnumerable<int> allOptions)
        {
            possible = new HashSet<int>(allOptions);
        }

        public Cell Clone()
        {
            var c = new Cell(new int[0]);
            c.possible = new HashSet<int>(this.possible);
            return c;
        }

        public int GetCollapsedValue()
        {
            if (!Collapsed) throw new InvalidOperationException("Cell not collapsed");
            foreach (var v in possible) return v;
            return -1;
        }
    }

    #region Unity lifecycle & API
    private void Start()
    {
        // si no hay parent, crear uno
        if (parentForTiles == null)
        {
            var go = new GameObject("WFC_Tiles");
            parentForTiles = go.transform;
            parentForTiles.SetParent(this.transform, false);
        }

        // Asignar tilePrefab si no existe
        if (tilePrefab == null)
        {
            tilePrefab = CreateDefaultTilePrefab();
        }

        // Validaciones simples
        if (tileSprites == null || tileSprites.Length == 0)
        {
            Debug.LogError("Asigna tileSprites en el inspector.");
            return;
        }

        if (tileRules == null || tileRules.rules.Count != tileSprites.Length)
        {
            Debug.LogWarning("tileRules.rules no coincide con tileSprites. Intentaré inicializar reglas por defecto permitiendo todo.");
            InitializePermissiveRules();
        }

        // comenzar generación
        StartCoroutine(RunWFC());
    }

    private GameObject CreateDefaultTilePrefab()
    {
        var go = new GameObject("TilePrefab");
        go.AddComponent<SpriteRenderer>();
        go.SetActive(false);
        return go;
    }
    #endregion

    #region Core: Inicialización y clonación
    private void InitializeGrid()
    {
        grid = new Cell[width, height];
        var allIds = Enumerable.Range(0, tileSprites.Length).ToArray();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = new Cell(allIds);
    }

    private void InitializePermissiveRules()
    {
        tileRules = new TileRules();
        tileRules.rules = new List<NeighborRule>();
        int n = tileSprites != null ? tileSprites.Length : 0;
        for (int i = 0; i < n; i++)
        {
            var nr = new NeighborRule();
            // permitir todos (conservador pero evita chocar)
            nr.north = Enumerable.Range(0, n).ToList();
            nr.south = Enumerable.Range(0, n).ToList();
            nr.east = Enumerable.Range(0, n).ToList();
            nr.west = Enumerable.Range(0, n).ToList();
            tileRules.rules.Add(nr);
        }
    }

    private Cell[,] CloneGrid(Cell[,] original)
    {
        var clone = new Cell[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                clone[x, y] = original[x, y].Clone();
        return clone;
    }
    #endregion

    #region WFC Main (backtracking recursion with propagation)
    private IEnumerator RunWFC()
    {
        ClearPreviousTiles();
        InitializeGrid();
        bool success;
        if (visualizeSteps)
        {
            yield return StartCoroutine(SolveCoroutine());
            success = true; // SolveCoroutine handles success/failure messages
        }
        else
        {
            success = SolveWithBacktracking(grid);
            if (success)
            {
                RenderFinalGrid();
            }
            else
            {
                Debug.LogError("WFC: No se encontró solución (intenta cambiar reglas o tamaño).");
            }
        }
    }

    // Coroutine version to visualize steps
    private IEnumerator SolveCoroutine()
    {
        // We'll do backtracking but step-by-step: choose a cell, try options sequentially
        var stateStack = new Stack<Cell[,]>();
        stateStack.Push(CloneGrid(grid));

        while (true)
        {
            // check if solved
            if (IsFullyCollapsed(grid))
            {
                RenderFinalGrid();
                Debug.Log("WFC: Completo.");
                yield break;
            }

            // find cell with minimal entropy (>1)
            var pos = GetLowestEntropyCellPosition(grid);
            if (pos == null)
            {
                Debug.LogError("WFC: No hay celdas seleccionables (contradicción). Intentando backtrack...");
                // backtrack
                if (stateStack.Count > 0)
                {
                    grid = stateStack.Pop();
                    ClearPreviousTiles();
                    RenderPartial(grid);
                    yield return new WaitForSeconds(stepDelay);
                    continue;
                }
                else
                {
                    Debug.LogError("WFC: Backtrack agotado. Abortando.");
                    yield break;
                }
            }

            int x = pos.Value.x, y = pos.Value.y;
            var options = grid[x, y].possible.ToList();
            // shuffle options
            Shuffle(options);

            bool assigned = false;
            foreach (var option in options)
            {
                // snapshot for backtracking
                stateStack.Push(CloneGrid(grid));

                // assign
                grid[x, y].possible = new HashSet<int>() { option };

                // propagate
                bool ok = Propagate(grid, new Vector2Int(x, y));
                ClearPreviousTiles();
                RenderPartial(grid);
                yield return new WaitForSeconds(stepDelay);

                if (ok)
                {
                    assigned = true;
                    break; // continue with next selection
                }
                else
                {
                    // contradiction -> restore previous state from stack and try next option
                    if (stateStack.Count > 0)
                    {
                        grid = stateStack.Pop();
                        ClearPreviousTiles();
                        RenderPartial(grid);
                        yield return new WaitForSeconds(stepDelay);
                    }
                    else
                    {
                        Debug.LogError("WFC: Contradicción sin posibilidad de retroceder. Abortando.");
                        yield break;
                    }
                }
            }

            if (!assigned)
            {
                // tried all options, need to backtrack more
                Debug.Log("WFC: Todas las opciones provocaron contradicción, haciendo backtrack.");
                if (stateStack.Count > 0)
                {
                    grid = stateStack.Pop();
                    ClearPreviousTiles();
                    RenderPartial(grid);
                    yield return new WaitForSeconds(stepDelay);
                }
                else
                {
                    Debug.LogError("WFC: Backtrack agotado. Abortando.");
                    yield break;
                }
            }
        }
    }

    // Non-visual solver used when visualizeSteps == false
    private bool SolveWithBacktracking(Cell[,] workingGrid)
    {
        if (IsFullyCollapsed(workingGrid))
        {
            grid = workingGrid;
            return true;
        }

        var pos = GetLowestEntropyCellPosition(workingGrid);
        if (pos == null) return false; // contradiction

        int x = pos.Value.x, y = pos.Value.y;
        var options = workingGrid[x, y].possible.ToList();
        Shuffle(options);

        foreach (var option in options)
        {
            var candidate = CloneGrid(workingGrid);
            candidate[x, y].possible = new HashSet<int>() { option };
            if (Propagate(candidate, new Vector2Int(x, y)))
            {
                if (SolveWithBacktracking(candidate))
                {
                    // success: copy to main grid and return
                    grid = candidate;
                    return true;
                }
            }
            // else try next option
        }

        // all options failed
        return false;
    }
    #endregion

    #region Propagation & Utilities
    // Propagate constraints from the starting cell (seed). Returns false if contradiction (some cell has 0 options)
    private bool Propagate(Cell[,] workingGrid, Vector2Int start)
    {
        var q = new Queue<Vector2Int>();
        q.Enqueue(start);

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            int px = p.x, py = p.y;

            foreach (var dir in DirectionUtil.All)
            {
                var off = DirectionUtil.ToOffset(dir);
                int nx = px + off.x, ny = py + off.y;
                if (!InBounds(nx, ny)) continue;

                var beforeCount = workingGrid[nx, ny].possible.Count;

                // compute allowed set for neighbor based on cells in p
                var allowed = new HashSet<int>();
                foreach (var tileId in workingGrid[px, py].possible)
                {
                    var neighborAllowedForThisTile = tileRules.rules[tileId].Get(DirectionUtil.Opposite(dir));
                    foreach (var a in neighborAllowedForThisTile) allowed.Add(a);
                }

                // intersect neighbor possible with allowed
                workingGrid[nx, ny].possible.IntersectWith(allowed);

                if (workingGrid[nx, ny].possible.Count == 0)
                {
                    return false; // contradiction
                }

                if (workingGrid[nx, ny].possible.Count < beforeCount)
                {
                    // changed -> need to propagate further from neighbor
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
        return true;
    }

    private bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

    private bool IsFullyCollapsed(Cell[,] workingGrid)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (!workingGrid[x, y].Collapsed) return false;
        return true;
    }

    private Vector2Int? GetLowestEntropyCellPosition(Cell[,] workingGrid)
    {
        int best = int.MaxValue;
        Vector2Int? bestPos = null;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int count = workingGrid[x, y].possible.Count;
                if (count == 0) return null; // contradiction
                if (count == 1) continue; // already collapsed, skip
                if (count < best)
                {
                    best = count;
                    bestPos = new Vector2Int(x, y);
                }
            }
        return bestPos;
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i >= 1; i--)
        {
            int j = rng.Next(i + 1);
            var tmp = list[j];
            list[j] = list[i];
            list[i] = tmp;
        }
    }
    #endregion

    #region Rendering
    private Dictionary<Vector2Int, GameObject> visualTiles = new Dictionary<Vector2Int, GameObject>();

    private void ClearPreviousTiles()
    {
        foreach (var kv in visualTiles)
            if (kv.Value) Destroy(kv.Value);
        visualTiles.Clear();
    }

    // Render entire final collapsed grid (safe version)
    private void RenderFinalGrid()
    {
        ClearPreviousTiles();

        // Si por alguna razon no esta completamente colapsado, avisar y completar
        if (!IsFullyCollapsed(grid))
        {
            Debug.LogWarning("RenderFinalGrid: el grid NO está totalmente colapsado. Completaré celdas restantes eligiendo aleatoriamente entre las opciones (solo para visualización).");
            // completar cada celda no colapsada escogiendo una opción aleatoria
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!grid[x, y].Collapsed)
                    {
                        var opts = grid[x, y].possible.ToList();
                        Shuffle(opts);
                        grid[x, y].possible = new HashSet<int>() { opts[0] };
                    }
                }
            }
        }

        // ahora sí renderizar (todas las celdas deben estar colapsadas)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var id = grid[x, y].GetCollapsedValue();
                InstantiateTileAt(id, x, y);
            }
        }
    }


    // Partial render: show for each cell either collapsed tile or a composite visualization of count of options
    private void RenderPartial(Cell[,] workingGrid)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var pos = new Vector2Int(x, y);
                if (visualTiles.ContainsKey(pos))
                {
                    Destroy(visualTiles[pos]);
                    visualTiles.Remove(pos);
                }

                if (workingGrid[x, y].Collapsed)
                {
                    int id = workingGrid[x, y].GetCollapsedValue();
                    var go = InstantiateTileAt(id, x, y);
                    visualTiles[pos] = go;
                }
                else
                {
                    // show a placeholder with a text of possible count (simple)
                    var go = new GameObject($"cell_{x}_{y}");
                    go.transform.SetParent(parentForTiles, false);
                    go.transform.localPosition = new Vector3(x * tileSize, y * tileSize, 0f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = CreateCountSprite(workingGrid[x, y].possible.Count);
                    sr.sortingOrder = 0;
                    visualTiles[pos] = go;
                }
            }
    }

    // Instantiates a tile sprite at grid coords
    private GameObject InstantiateTileAt(int id, int x, int y)
    {
        var go = Instantiate(tilePrefab, parentForTiles);
        go.name = $"tile_{x}_{y}_id{id}";
        go.transform.localPosition = new Vector3(x * tileSize, y * tileSize, 0f);
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = tileSprites[id];
        sr.sortingOrder = 1;
        go.SetActive(true);
        visualTiles[new Vector2Int(x, y)] = go;
        return go;
    }

    // Minimal helper: create a 32x32 sprite that encodes the count as a colored square with a number.
    // To keep simple and dependency-free, we create a tiny generated texture; it's not optimized but ok for debug.
    private Sprite CreateCountSprite(int count)
    {
        int px = 32;
        Texture2D tex = new Texture2D(px, px);
        Color bg = Color.white;
        Color fg = Color.black;

        // fill bg
        var cols = new Color[px * px];
        for (int i = 0; i < cols.Length; i++) cols[i] = bg;
        tex.SetPixels(cols);

        // draw a simple border
        for (int i = 0; i < px; i++)
        {
            tex.SetPixel(i, 0, fg);
            tex.SetPixel(i, px - 1, fg);
            tex.SetPixel(0, i, fg);
            tex.SetPixel(px - 1, i, fg);
        }

        tex.Apply();
        tex.filterMode = FilterMode.Point;
        Sprite s = Sprite.Create(tex, new Rect(0, 0, px, px), new Vector2(0.5f, 0.5f), 100f);
        return s;
    }
    #endregion
}