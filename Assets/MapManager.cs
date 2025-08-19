using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject PisoPrefab;
    public GameObject ParedPrefab; 
    public DrunkenAgent drunkenAgent;
    int[,] mapa = new int[40, 40];
    public float tama�oCelda = 1f;


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
                Vector3 position = new Vector3(x * tama�oCelda, -y * tama�oCelda, 0); 
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
        // Destruye todo lo que est� dentro del MapManager
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Genera un nuevo mapa
        CrearMap();
    }
}

