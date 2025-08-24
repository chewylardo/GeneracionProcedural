using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class DrunkenAgent : MonoBehaviour
{   
    //probabilidad de cambiar de direccion y probabilidad de crear sala
    public int Pc = 3;
    public int Pr = 1;

   
    public int xInitialPos;
    public int yInitialPos;
    private int dirX = 1;
    private int dirY = 1;
    public int separacion = 3;


    void Start()
    {
       
    }

        // Update is called once per frame
    void Update()
    {
        
    }

    public int[ , ] Agent(int[,] mapa) {


        //posicion central siempre 
        yInitialPos = (mapa.GetLength(0) - 1) / 2;
        xInitialPos = (mapa.GetLength(1) - 1) / 2;
        mapa[yInitialPos, xInitialPos] = 1;
        float procentajeDeSalas = 0;
    

        
        //mientras que el 50% no sea caminable no se detendra
        while(procentajeDeSalas < 0.5f)
        {
            procentajeDeSalas = 0;
           // Debug.Log(xInitialPos + "," +  yInitialPos);

          

            int ChanceDir = Random.Range(0, 100);
            int ChanceSala = Random.Range(0, 100);


            if(ChanceDir > Pc){

                int newX = xInitialPos + dirX;
                int newY = yInitialPos + dirY;

                if (newX >= 0 && newX < mapa.GetLength(0) && newY >= 0 && newY < mapa.GetLength(1))
                {
                    xInitialPos = newX;
                    yInitialPos = newY;
                    mapa[xInitialPos, yInitialPos] = 1;
                    Pc += 1;
                }
                else{
                    randomDir();
                
                }

            }
            else
            {
                randomDir();
                int newX = xInitialPos + dirX;
                int newY = yInitialPos + dirY;

                if (newX >= 0 && newX < mapa.GetLength(0) && newY >= 0 && newY < mapa.GetLength(1))
                {
                    xInitialPos = newX;
                    yInitialPos = newY;
                    mapa[xInitialPos, yInitialPos] = 1;
                    Pc = 0;
                }
                else
                {
                    randomDir();
                  
                }

            }


            if(ChanceDir > Pr)
            {


                Pr += 1;

            }
            else
            {

                //calcuar el tamaño de la sala generada
                int AltoSala = Random.Range(2, 7);
                int AnchoSala = Random.Range(2, 7);


                int inicioX = xInitialPos - AnchoSala / 2;
                int finX = xInitialPos + AnchoSala / 2;
                int inicioY = yInitialPos - AltoSala / 2;
                int finY = yInitialPos + AltoSala / 2;
                // Debug.Log(inicioX + " y " +  finX );
                // Debug.Log(inicioY + " y " +  finY );
                if (LaSalaesValida(inicioX,finX,inicioY,finY,mapa))
                {
                    for (int i = Mathf.Max(0, inicioX); i <= Mathf.Min(mapa.GetLength(0) - 1, finX); i++)
                    {
                        for (int j = Mathf.Max(0, inicioY); j <= Mathf.Min(mapa.GetLength(1) - 1, finY); j++)
                        {
                            mapa[i, j] = 1;
                        }
                    }
                }

                Pr = 0;
            }
                
            

            //verificar % de suelo caminable
            int contadorUnos = 0;
            for (int i = 0; i < mapa.GetLength(0); i++)
            {
                for (int j = 0; j < mapa.GetLength(1); j++)
                {
                    if (mapa[i, j] == 1) { contadorUnos++; }
                }
            }
            procentajeDeSalas = contadorUnos/(float)mapa.Length;


        }

        return mapa;
    }



    private bool LaSalaesValida(int inicioX, int finX, int inicioY, int finY, int[,]mapa)
    {
       //comprueba viendo la separacion con otras salas si intersecta con otra
        inicioX = Mathf.Max(0, inicioX - separacion);
        finX = Mathf.Min(mapa.GetLength(1) - 1, finX + separacion);
        inicioY = Mathf.Max(0, inicioY - separacion);
        finY = Mathf.Min(mapa.GetLength(0) - 1, finY + separacion);

        //si alguna de las posiciones toca otra sala no crear sala
        for (int i = inicioX; i <= inicioX; i++)
        {
            for (int j = inicioY; j <= finY; j++)
            {
                if (mapa[i, j] == 1) { 
                    return false;
                }
            }
        }
        return true;
    }
    private void randomDir()
    {   
        //movimiento solo en 4 direcciones
        int direcciones = Random.Range(0,4);
        switch (direcciones)
        {
            case 0:
                dirX = 1;
                dirY = 0;
                break;
            case 1:
                dirX = -1;
                dirY = 0;
                break;
            case 2:
                dirX = 0;
                dirY = 1;
                break;
            case 3:
                dirX = 0;
                dirY = -1;
                break;
        
        }

    }


}
