using System.Collections.Generic;

public class GridCell
{
    public List<TileData> possibleTiles = new List<TileData>();
    public bool collapsed => possibleTiles.Count == 1;

    public GridCell(List<TileData> allTiles)
    {
        possibleTiles.AddRange(allTiles);
    }

    public TileData GetTile() => collapsed ? possibleTiles[0] : null;
}
