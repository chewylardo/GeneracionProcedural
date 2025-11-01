using UnityEngine;

public class Fitness2 : MonoBehaviour
{
    // Fitness para Hill Climbing
    public float Evaluate(MapData map)
    {
        if (!Legal(map)) return 0f;

        float score = 0f;

        // Cercanía cajas a objetivos
        foreach (var box in map.boxes)
        {
            float minDist = float.MaxValue;
            foreach (var goal in map.goals)
            {
                float d = Vector2Int.Distance(box, goal);
                if (d < minDist) minDist = d;
            }
            score += 1f / (minDist + 1f);
        }

        //Accesibilidad del jugador a cajas y objetivos
        foreach (var box in map.boxes)
        {
            score += 1f / (Vector2Int.Distance(map.playerPos, box) + 1f);
        }
        foreach (var goal in map.goals)
        {
            score += 1f / (Vector2Int.Distance(map.playerPos, goal) + 1f);
        }
           

        //Dispersión de cajas y muros
        for (int i = 0; i < map.boxes.Count; i++) {  
            for (int j = i + 1; j < map.boxes.Count; j++)
            {
                    score += Vector2Int.Distance(map.boxes[i], map.boxes[j]) * 0.1f;
            }
        }
          
        for (int i = 0; i < map.internalWalls.Count; i++)
        {
            for (int j = i + 1; j < map.internalWalls.Count; j++)
            {
                score += Vector2Int.Distance(map.internalWalls[i], map.internalWalls[j]) * 0.05f;

            }
        }
        

        return score;
    }

    bool Legal(MapData map)
    {
        if (!map.EstaAdentro(map.playerPos)) { return false; }
        if (map.boxes.Contains(map.playerPos)) { return false; }

        foreach (var box in map.boxes)
        {
            if (!map.EstaAdentro(box)) { return false; }
            if (map.goals.Contains(box)) { return false; }
            if (map.NoSePuedeMover(box)) { return false; } // atrapada
            if (map.EsUnaPared(box)) { return false; }
        }

        foreach (var wall in map.internalWalls) { 
            if (map.goals.Contains(wall)) { return false; }
        }
          

        return true;
    }
}