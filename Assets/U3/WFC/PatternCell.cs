using System.Collections.Generic;

public class PatternCell
{
    public int x, y;
    public List<int> possible;

    public PatternCell(List<int> list)
    {
        possible = new List<int>(list);
    }

    public bool IsCollapsed => possible.Count == 1;
    public int Final => IsCollapsed ? possible[0] : -1;
}
