using System.Collections.Generic;
using UnityEngine;

public class MapVisualizer : MonoBehaviour
{
    [Header("Prefabs para el mapa")]
    public GameObject PrefabMuro;
    public GameObject PrefabPiso;
    public GameObject PrefabCaja;
    public GameObject PrefabObjetivo;
    public GameObject PrefabJugador;

    public Quaternion RotarPiso = Quaternion.Euler(90, 0, 0);
    private readonly List<GameObject> spawned = new(); // objetos instanciados en la escena

    //Convierte coordenadas del mapa a posición en mundo 3D
    Vector3 GetWorldPos(int x, int y) => new Vector3(x, 0, -y);

    //Limpia todos los objetos previamente instanciados
    public void Clear()
    {
        foreach (var g in spawned) if (g != null) Destroy(g);
        spawned.Clear();
    }

    //Dibuja el mapa completo según MapData
    public void ShowMap(MapData map)
    {
        Clear();
        if (map == null) { 
            return;
        }

        //Dibujar suelo y muros de borde
        for (int y = 0; y < map.height; y++)
        {
            for (int x = 0; x < map.width; x++)
            {
                Vector3 pos = GetWorldPos(x, y);
                if (PrefabPiso) { 
                    spawned.Add(Instantiate(PrefabPiso, pos, RotarPiso, transform));
                }

                if (x == 0 || y == 0 || x == map.width - 1 || y == map.height - 1)
                {
                    spawned.Add(Instantiate(PrefabMuro, pos, Quaternion.identity, transform));
                }
            }
        }

        //Dibujar muros internos
        foreach (var w in map.internalWalls)
        {
            spawned.Add(Instantiate(PrefabMuro, GetWorldPos(w.x, w.y), Quaternion.identity, transform));
        }

        //Dibujar objetivos
        foreach (var g in map.goals)
        {
            spawned.Add(Instantiate(PrefabObjetivo, GetWorldPos(g.x, g.y), RotarPiso, transform));
        }

        //Dibujar cajas
        foreach (var b in map.boxes)
        {
            spawned.Add(Instantiate(PrefabCaja, GetWorldPos(b.x, b.y), Quaternion.identity, transform));
        }

        //Dibujar jugador
        spawned.Add(Instantiate(PrefabJugador, GetWorldPos(map.playerPos.x, map.playerPos.y), Quaternion.identity, transform));
    }
}
