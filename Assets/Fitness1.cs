using System.Collections.Generic;
using UnityEngine;

public class Fitness1 : MonoBehaviour
{
    public float penalizacionEsquinas = 5f;
    public float penalizacionBordes = 3f;
    public float penalizacionObjetivoCubierto = 10f;

    // Definir límites del mapa
    public int mapLargo = 10;
    public int mapAlto = 10;

    public float Evaluate(MapData map)
    {
        float fitness = 0f;

        fitness += PenalizarCajasAtrapadas(map);
        fitness += PenalizarMurosSobreObjetivos(map);
        fitness += PenalizarJugadorCercaCaja(map);

        return fitness;
    }

    float PenalizarCajasAtrapadas(MapData map)
    {
        float penalizacion = 0f;
        foreach (var box in map.boxes)
        {
            // Esquinas (usando paredes internas + límites)
            bool esquinaArrIzq = map.internalWalls.Contains(box + Vector2Int.up) && map.internalWalls.Contains(box + Vector2Int.left);
            bool esquinaArrDer = map.internalWalls.Contains(box + Vector2Int.up) && map.internalWalls.Contains(box + Vector2Int.right);
            bool esquinaAbIzq = map.internalWalls.Contains(box + Vector2Int.down) && map.internalWalls.Contains(box + Vector2Int.left);
            bool esquinaAbDer = map.internalWalls.Contains(box + Vector2Int.down) && map.internalWalls.Contains(box + Vector2Int.right);

            if (esquinaArrIzq || esquinaArrDer || esquinaAbIzq || esquinaAbDer)
                penalizacion -= penalizacionEsquinas;

            // Bordes
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