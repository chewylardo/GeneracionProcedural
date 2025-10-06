using UnityEngine;
using System.Linq;

public class Fitness2 : MonoBehaviour
{
    [Header("Pesos de evaluación")]
    public float w_boxesOnGoals = 10f; // cajas sobre objetivos
    public float w_deadlocks = 5f;     // penalización por deadlocks
    public float w_distance = 2f;      // peso de distancia promedio
    public float w_openSpace = 1f;     // peso de espacio libre

    //Evalúa qué tan bueno es un mapa
    public float Evaluate(MapData map)
    {
        if (map == null) return float.NegativeInfinity;

        //Contar cajas sobre objetivos
        int boxesOnGoals = map.boxes.Count(b => map.goals.Contains(b));

        //Contar cajas bloqueadas (deadlocks)
        int deadlocks = map.boxes.Count(b => map.NoSePuedeMover(b));

        //Calcular distancia promedio de cajas a objetivos
        float distPromedio = 0f;
        foreach (var box in map.boxes)
        {
            int min = map.goals.Min(g => Mathf.Abs(g.x - box.x) + Mathf.Abs(g.y - box.y));
            distPromedio += min;
        }
        if (map.boxes.Count > 0)
        {
            distPromedio /= map.boxes.Count;
        }

        //Calcular espacio libre
        int areaInt = (map.width - 2) * (map.height - 2);
        int bloqueadas = map.internalWalls.Count + map.boxes.Count;
        float openRatio = Mathf.Max(0f, (areaInt - bloqueadas) / (float)areaInt);

        //Combinar factores con pesos
        float fitness = 0f;
        fitness += w_boxesOnGoals * boxesOnGoals;
        fitness -= w_deadlocks * deadlocks;
        fitness += w_distance * distPromedio;
        fitness += w_openSpace * openRatio * 10f;

        //Penalizar si el jugador está en posición inválida
        if (!map.EstaAdentro(map.playerPos) || map.EsUnaPared(map.playerPos) || map.boxes.Contains(map.playerPos))
        {
            fitness -= 1000f;
        }
           

        return fitness;
    }
}
