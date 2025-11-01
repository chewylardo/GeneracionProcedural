using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class MapData
{
    public int width;
    public int height;
    public List<Vector2Int> internalWalls = new List<Vector2Int>();
    public List<Vector2Int> boxes = new List<Vector2Int>();
    public List<Vector2Int> goals = new List<Vector2Int>();
    public Vector2Int playerPos;

    //Constructor
    public MapData(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    //Clonar mapa (copia independiente)
    public MapData Clone()
    {
        MapData c = new MapData(width, height);
        c.internalWalls = new List<Vector2Int>(internalWalls);
        c.boxes = new List<Vector2Int>(boxes);
        c.goals = new List<Vector2Int>(goals);
        c.playerPos = playerPos;
        return c;
    }

    //Verifica si una posición está dentro del mapa (sin bordes)
    public bool EstaAdentro(Vector2Int p) => p.x >= 1 && p.x <= width - 2 && p.y >= 1 && p.y <= height - 2;

    //Verifica si una posición es pared (borde o muro interno)
    public bool EsUnaPared(Vector2Int p) => (p.x == 0 || p.y == 0 || p.x == width - 1 || p.y == height - 1 || internalWalls.Contains(p));

    //Verifica si una posición está bloqueada (pared o caja)
    public bool EstaBloqueada(Vector2Int p) => EsUnaPared(p) || boxes.Contains(p);

    //Evalúa si una caja en pos genera deadlock
    public bool NoSePuedeMover(Vector2Int pos)
    {
        if (goals.Contains(pos)) return false;

        //Esquinas bloqueadas
        if (EstaBloqueada(pos + Vector2Int.up) && EstaBloqueada(pos + Vector2Int.left))
        {
            return true;
        }
        if (EstaBloqueada(pos + Vector2Int.up) && EstaBloqueada(pos + Vector2Int.right))
        {
            return true;
        }
        if (EstaBloqueada(pos + Vector2Int.down) && EstaBloqueada(pos + Vector2Int.left))
        {
            return true;
        }
        if (EstaBloqueada(pos + Vector2Int.down) && EstaBloqueada(pos + Vector2Int.right))
        {
            return true;
        }

        //Bloques 2x2 totalmente ocupados
        Vector2Int[] offs = { Vector2Int.zero, Vector2Int.left, Vector2Int.down, Vector2Int.down + Vector2Int.left };
        foreach (var off in offs)
        {
            Vector2Int a = pos + off;
            Vector2Int b = a + Vector2Int.right;
            Vector2Int c = a + Vector2Int.up;
            Vector2Int d = a + Vector2Int.right + Vector2Int.up;
            if (EsUnaPared(a) && EsUnaPared(b) && EsUnaPared(c) && EsUnaPared(d))
            {
                return true;
            }

        }
        return false;
    }
}
