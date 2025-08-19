using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class DrunkenAgent : MonoBehaviour
{   
    //probabilidad de cambiar de direccion y probabilidad de crear sala
    public int Pc = 5;
    public int Pr = 5;

   
    public int xInitialPos;
    public int yInitialPos;
    int dirX = 1;
    int dirY = 1;


    void Start()
    {
       
    }

        // Update is called once per frame
    void Update()
    {
        
    }

    public int[ , ] Agent(int[,] mapa) {



        yInitialPos = (mapa.GetLength(0) - 1) / 2;
        xInitialPos = (mapa.GetLength(1) - 1) / 2;
        mapa[yInitialPos, xInitialPos] = 1;
        float procentajeDeSalas = 0;
    

        

        while(procentajeDeSalas < 0.5f)
        {
            procentajeDeSalas = 0;
           // Debug.Log(xInitialPos + "," +  yInitialPos);

            //mover al agente cuya posicion inicial es xInitialPos  yInitialPos

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
                    Pc += 5;
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

    private void randomDir()
    {
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
