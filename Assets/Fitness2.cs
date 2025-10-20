using UnityEngine;

public class Fitness2 : MonoBehaviour
{
    public float penalizacionEsquinas = 5f;
    public float penalizacionBordes = 3f;
    public float penalizacionObjetivoCubierto = 10f;

    public int mapLargo = 10;
    public int mapAlto = 10;

    public float Evaluate(MapData map)
    {
        float fitness = 0f;

        // Fitness más variable
        fitness += PenalizarCajasAtrapadas(map) * Random.Range(0.8f, 1.2f);
        fitness += PenalizarMurosSobreObjetivos(map) * Random.Range(0.9f, 1.1f);
        fitness += PenalizarJugadorCercaCaja(map) * Random.Range(0.7f, 1.3f);
        fitness += Random.Range(-2f, 2f);

        return fitness;
    }

    float PenalizarCajasAtrapadas(MapData map)
    {
        float penalizacion = 0f;
        foreach (var box in map.boxes)
        {
            bool esquinaArrIzq = map.internalWalls.Contains(box + Vector2Int.up) && map.internalWalls.Contains(box + Vector2Int.left);
            bool esquinaArrDer = map.internalWalls.Contains(box + Vector2Int.up) && map.internalWalls.Contains(box + Vector2Int.right);
            bool esquinaAbIzq = map.internalWalls.Contains(box + Vector2Int.down) && map.internalWalls.Contains(box + Vector2Int.left);
            bool esquinaAbDer = map.internalWalls.Contains(box + Vector2Int.down) && map.internalWalls.Contains(box + Vector2Int.right);

            if (esquinaArrIzq || esquinaArrDer || esquinaAbIzq || esquinaAbDer)
                penalizacion -= penalizacionEsquinas;

            if (box.x <= 0 || box.x >= mapLargo - 1 || box.y <= 0 || box.y >= mapAlto - 1)
                penalizacion -= penalizacionBordes;
        }
        return penalizacion;
    }

    float PenalizarMurosSobreObjetivos(MapData map)
    {
        float penalizacion = 0f;
        foreach (var goal in map.goals)
        {
            if (map.internalWalls.Contains(goal))
                penalizacion -= penalizacionObjetivoCubierto;
        }
        return penalizacion;
    }

    float PenalizarJugadorCercaCaja(MapData map)
    {
        float penalizacion = 0f;
        foreach (var box in map.boxes)
        {
            if (Vector2Int.Distance(box, map.playerPos) < 1.5f)
                penalizacion -= 1f;
        }
        return penalizacion;
    }
}