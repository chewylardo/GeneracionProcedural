using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructorMapa : MonoBehaviour
{
    [Header("Prefabs del mapa")]
    public GameObject wallPrefab;   
    public GameObject floorPrefab;  

  
    private GameObject mapPadre;

    // metodo que construye visualmente un mapa en la escena a partir de un genotipo asi se puede ver como avanza la evolucion de el mapa de elite con el score mas alto
    public void Construir(GenotipoMapa g)
    {
        
        if (mapPadre != null) 
        {
            Destroy(mapPadre); 
        }


        // crea un objeto vacio que contendrá todas las piezas del mapa
        mapPadre = new GameObject("MapGenerado");

        // recorre el genotipo y construye
        for (int x = 0; x < g.largo; x++) //  columnas
        {
            for (int y = 0; y < g.alto; y++) //  filas
            {
                GameObject prefab;

                if (g.grid[x, y] == 0)
                {
                    // colocar un muro
                    prefab = wallPrefab;
                    Instantiate(prefab,new Vector3(x, 0, y),Quaternion.identity, mapPadre.transform);
                }
                else
                {
                    // colocar suelo
                    prefab = floorPrefab;
                    Instantiate(prefab,new Vector3(x, 0, y),Quaternion.Euler(90, 0, 0), mapPadre.transform);
                }
            }
        }
    }
}
