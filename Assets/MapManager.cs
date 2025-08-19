using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject PisoPrefab;
    public GameObject ParedPrefab; 
    void Start()
    {
        CrearMap();
    }

    void Update()
    {
        
    }

    
    int[,] mapa = new int[40, 40];
    

    public float tama�oCelda = 1f;
    void CrearMap()
    { 

     //por ahora se esta creando con puros suelos por que la biparticion no esta hecha
     for (int y = 0; y < mapa.GetLength(0); y++){
         for (int x = 0; x < mapa.GetLength(1); x++){
                Vector3 position = new Vector3(x * tama�oCelda, -y * tama�oCelda, 0); 
                GameObject toInstantiate = null;

                if (mapa[y, x] == 0){
                    toInstantiate = ParedPrefab; ;
                }
                else if (mapa[y, x] == 1){
                    toInstantiate = PisoPrefab;
                }

                if (toInstantiate != null)
                    Instantiate(toInstantiate, position, Quaternion.identity, this.transform);
                
         }
        }
    }
}

