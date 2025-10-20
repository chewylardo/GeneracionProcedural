using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Fitness1 : MonoBehaviour
{
    
    public float Evaluate(MapData map)
    {
        if (!EsLegal(map))
            return -1000f;

        float dispersion = CalcularDispersion(map);
        float simetria = CalcularSimetria(map);
        float accesibilidad = CalcularAccesibilidadBasica(map);
        float variedad = CalcularVariedad(map);

        float fitness = (float)(dispersion * 0.5
                      - simetria * 0.3
                      + accesibilidad * 0.1
                      + variedad * 0.4);

        Debug.Log($"[Fitness1] => Disp:{dispersion:F2}  Sim:{simetria:F2}  Acc:{accesibilidad:F2}  Var:{variedad:F2}  => Total:{fitness:F2}");

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

    float CalcularDispersion(MapData map)
    {
        if (map.boxes.Count < 2) return 0f;
        float sum = 0f;
        int pairs = 0;
        for (int i = 0; i < map.boxes.Count; i++)
        {
            for (int j = i + 1; j < map.boxes.Count; j++)
            {
                sum += Vector2Int.Distance(map.boxes[i], map.boxes[j]);
                pairs++;
            }
        }
        return pairs > 0 ? sum / pairs : 0f;
    }

    float CalcularSimetria(MapData map)
    {
        int ancho = map.width;
        int sim = 0;
        foreach (var box in map.boxes)
        {
            Vector2Int reflejado = new Vector2Int(ancho - 1 - box.x, box.y);
            if (map.boxes.Contains(reflejado))
                sim++;
        }
        return (float)sim / map.boxes.Count;
    }

    float CalcularAccesibilidadBasica(MapData map)
    {
        int libres = 0;
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                Vector2Int p = new Vector2Int(x, y);
                if (!map.EsUnaPared(p))
                    libres++;
            }
        }
        return (float)libres / (map.width * map.height);
    }

    float CalcularVariedad(MapData map)
    {
        float cx = map.boxes.Average(b => (float)b.x);
        float cy = map.boxes.Average(b => (float)b.y);
        float spread = map.boxes.Average(b => Mathf.Abs(b.x - cx) + Mathf.Abs(b.y - cy));
        return spread;
    }
}
