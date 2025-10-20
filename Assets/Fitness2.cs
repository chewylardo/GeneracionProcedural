using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Fitness2 : MonoBehaviour
{
    public float Evaluate(MapData map)
    {
        if (!EsLegal(map))
            return -1000f;

        float accesibilidad = CalcularAccesibilidadJugador(map);
        float cercania = CalcularCercaniaCajasMetas(map);
        float bloqueos = CalcularCajasBloqueadas(map);

        float fitness = (float)(accesibilidad * 0.5
                      + cercania * 0.4
                      - bloqueos * 0.3);

        Debug.Log($"[Fitness2] => Acc:{accesibilidad:F2}  Cerc:{cercania:F2}  Bloq:{bloqueos:F2}  => Total:{fitness:F2}");

        return fitness;
    }

    bool EsLegal(MapData map)
    {
        HashSet<Vector2Int> usados = new HashSet<Vector2Int>();
        foreach (var pos in map.boxes.Concat(map.internalWalls).Append(map.playerPos))
        {
            if (!map.EstaAdentro(pos)) return false;
            if (usados.Contains(pos)) return false;
            usados.Add(pos);
        }
        return true;
    }

    float CalcularAccesibilidadJugador(MapData map)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        HashSet<Vector2Int> visitados = new HashSet<Vector2Int>();
        q.Enqueue(map.playerPos);
        visitados.Add(map.playerPos);

        while (q.Count > 0)
        {
            var a = q.Dequeue();
            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                var n = a + dir;
                if (!map.EstaAdentro(n) || visitados.Contains(n) || map.EsUnaPared(n)) continue;
                visitados.Add(n);
                q.Enqueue(n);
            }
        }

        return (float)visitados.Count / (map.width * map.height);
    }

    float CalcularCercaniaCajasMetas(MapData map)
    {
        if (map.goals == null || map.goals.Count == 0) return 0f;

        float total = 0f;
        foreach (var box in map.boxes)
        {
            float minDist = float.MaxValue;
            foreach (var meta in map.goals)
                minDist = Mathf.Min(minDist, Vector2Int.Distance(box, meta));
            total += minDist;
        }
        return 1f / (1f + (total / map.boxes.Count));
    }

    float CalcularCajasBloqueadas(MapData map)
    {
        int bloqueadas = 0;
        foreach (var b in map.boxes)
        {
            bool up = map.EsUnaPared(b + Vector2Int.up);
            bool down = map.EsUnaPared(b + Vector2Int.down);
            bool left = map.EsUnaPared(b + Vector2Int.left);
            bool right = map.EsUnaPared(b + Vector2Int.right);

            if ((up && left) || (up && right) || (down && left) || (down && right))
                bloqueadas++;
        }
        return (float)bloqueadas / map.boxes.Count;
    }
}
