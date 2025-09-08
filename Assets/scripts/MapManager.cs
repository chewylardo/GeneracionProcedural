using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject PisoPrefab;
    public GameObject ParedPrefab;
    public DrunkenAgent drunkenAgent;
    int[,] mapa = new int[40, 40];
    public float tamañoCelda = 1f;

    void Start()
    {
        mapa = drunkenAgent.Agent(mapa);
        CrearMap();
    }

    void CrearMap()
    {
        int alto = mapa.GetLength(0);
        int ancho = mapa.GetLength(1);

        // 1️⃣ Crear el mapa interno
        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                Vector3 position = new Vector3(x * tamañoCelda, 0, y * tamañoCelda);
                GameObject toInstantiate = null;
                Quaternion rotation = Quaternion.identity;

                if (mapa[y, x] == 0)
                {
                    toInstantiate = ParedPrefab;
                    rotation = Quaternion.identity;
                }
                else if (mapa[y, x] == 1)
                {
                    toInstantiate = PisoPrefab;
                    rotation = Quaternion.Euler(90, 0, 0);
                }

                if (toInstantiate != null)
                    Instantiate(toInstantiate, position, rotation, this.transform);
            }
        }

        // 2️⃣ Crear el muro externo alrededor del mapa (sin tocar la matriz)
        for (int x = -1; x <= ancho; x++)
        {
            // muro arriba
            Instantiate(ParedPrefab, new Vector3(x * tamañoCelda, 0, -1 * tamañoCelda), Quaternion.identity, this.transform);
            // muro abajo
            Instantiate(ParedPrefab, new Vector3(x * tamañoCelda, 0, alto * tamañoCelda), Quaternion.identity, this.transform);
        }

        for (int y = 0; y < alto; y++)
        {
            // muro izquierda
            Instantiate(ParedPrefab, new Vector3(-1 * tamañoCelda, 0, y * tamañoCelda), Quaternion.identity, this.transform);
            // muro derecha
            Instantiate(ParedPrefab, new Vector3(ancho * tamañoCelda, 0, y * tamañoCelda), Quaternion.identity, this.transform);
        }
    }


    public void ReiniciarMapa()
    {
        // Destruye todos los hijos
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // Genera un nuevo mapa
        mapa = new int[40, 40];
        mapa = drunkenAgent.Agent(mapa);
        CrearMap();
    }
}
