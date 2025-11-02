using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternCell
{
    public List<int> possible;  // lista de patternIds posibles en esta celda
    public int x, y;            // posición en grid de patrones
    public bool IsCollapsed => possible.Count == 1;
    public int Final => IsCollapsed ? possible[0] : -1;
    public PatternCell(IEnumerable<int> all) { possible = new List<int>(all); }
}
