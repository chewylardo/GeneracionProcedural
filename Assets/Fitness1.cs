using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Fitness1 : MonoBehaviour
{
    // Fitness para GA
    public float Evaluate(MapData map)
    {
        if (!Legal(map)) return 0f;

        float score = 0f;

        //Diversidad de cajas
        for (int i = 0; i < map.boxes.Count; i++)
            for (int j = i + 1; j < map.boxes.Count; j++)
                score += Vector2Int.Distance(map.boxes[i], map.boxes[j]);

        //Diversidad de muros internos
        for (int i = 0; i < map.internalWalls.Count; i++)
            for (int j = i + 1; j < map.internalWalls.Count; j++)
                score += Vector2Int.Distance(map.internalWalls[i], map.internalWalls[j]) * 0.1f;

        //contar espacios libres
        int openSpaces = 0;
        for (int x = 0; x < map.width; x++)
            for (int y = 0; y < map.height; y++)
            {
                Vector2Int p = new Vector2Int(x, y);
                if (!map.boxes.Contains(p) && !map.internalWalls.Contains(p) && !map.goals.Contains(p))
                    openSpaces++;
            }
        score += openSpaces * 0.05f;

        //Penalización por cajas pegadas a paredes
        foreach (var box in map.boxes)
        {
            if (map.EsUnaPared(box + Vector2Int.up)) score -= 5f;
            if (map.EsUnaPared(box + Vector2Int.down)) score -= 5f;
            if (map.EsUnaPared(box + Vector2Int.left)) score -= 5f;
            if (map.EsUnaPared(box + Vector2Int.right)) score -= 5f;
        }

        return score;
    }


    bool Legal(MapData map)
    {
        // Jugador dentro
        if (!map.EstaAdentro(map.playerPos)) return false;

        // Cajas válidas
        foreach (var box in map.boxes)
        {
            if (!map.EstaAdentro(box)) return false;
            if (map.goals.Contains(box)) return false; // caja sobre objetivo
            if (map.NoSePuedeMover(box)) return false; // atrapada en esquina
            if (map.EsUnaPared(box)) return false; // pegada a pared
        }

        // Jugador no sobre caja
        if (map.boxes.Contains(map.playerPos)) return false;

        // Muros no sobre objetivos
        foreach (var wall in map.internalWalls)
            if (map.goals.Contains(wall)) return false;

        return true;
    }
}