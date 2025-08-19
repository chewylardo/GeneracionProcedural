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

    void Update()
    {
        
    }

    


    //nofunciona

    void CrearMap()
    { 
        //por ahora se esta creando con puros suelos por que la biparticion no esta hecha
        for (int y = 0; y < mapa.GetLength(0); y++){
            for (int x = 0; x < mapa.GetLength(1); x++){
                Vector3 position = new Vector3(x * tamañoCelda, -y * tamañoCelda, 0); 
                GameObject toInstantiate = null;

                if (mapa[y, x] == 0){
                    toInstantiate = ParedPrefab; 
                }
                else if (mapa[y, x] == 1){
                    toInstantiate = PisoPrefab;
                }
                if (toInstantiate != null)
                    Instantiate(toInstantiate, position, Quaternion.identity, this.transform);
                
            }
        }
    }
    public void ReiniciarMapa() //llamar en un boton
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

