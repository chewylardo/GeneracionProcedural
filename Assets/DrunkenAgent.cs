using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class DrunkenAgent : MonoBehaviour
{   
    //probabilidad de cambiar de direccion y probabilidad de crear sala
    public int Pc = 5;
    public int Pr = 5;

    public int[,] mapa;
    public int xInitialPos;
    public int yInitialPos;
    
     
    void Start()
    {
        if(mapa.GetLength(0)/2 == 0)
        {
            xInitialPos = mapa.GetLength(0) / 2;
        }
        else
        {
            xInitialPos = mapa.GetLength(0)+1 / 2;
        }

        if(mapa.GetLength(1) / 2 == 0){

            xInitialPos = mapa.GetLength(1) / 2;
        }else {

            xInitialPos = mapa.GetLength(1) +1 / 2;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int[ , ] Agent() {
        float procentajeDeSalas = 0;
        
        while(procentajeDeSalas < 70f)
        {
            

            //mover al agente cuya posicion inicial es xInitialPos  yInitialPos

            int ChanceMover = Random.Range(0, 100);
            int ChanceSala = Random.Range(0, 100);


            if(ChanceMover <= Pc){

                //cambiar direccion y moverse
            }
            else
            {
                //solo moverse en la misma direccion
                //sumar a la probabilidad de cambiar direccion un valor para que aumente su probabilidad, podria ser 5
            }

            if (ChanceMover <= Pr)
            {
                //crear sala
            }
            else
            {
                //sumar a la probabilidad de crear una sala un valor para que aumente su probabilidad, podria ser 5
            }


            int contadorUnos = 0;
            for (int i = 0; i < mapa.GetLength(0); i++)
            {
                for (int j = 0; j < mapa.GetLength(1); j++)
                {
                    if (mapa[i, j] != 0) { contadorUnos++; }
                }
            }
            procentajeDeSalas = (contadorUnos * 100)/mapa.Length;


        }

        return mapa;
    }
}
