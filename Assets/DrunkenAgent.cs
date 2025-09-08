using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using Unity.VisualScripting.FullSerializer;

public class DrunkenAgent : MonoBehaviour
{

    [Header("Configuración inicial")]
    public int PcConfig = 3;              // probabilidad de cambiar de dirección
    public int PrConfig = 1;              // probabilidad de crear sala
    public float porcentajeConfig = 0.5f; // porcentaje de salas a generar

    public int xInitialPos;
    public int yInitialPos;
    public int separacion = 3;


    private int Pc;
    private int Pr;
    private float porcentajeInicial;
    private int dirX = 1;
    private int dirY = 1;


    [Header("Seed Settings")]
    public int seed = 0;
    public bool useRandomSeed = true;


    [Header("Textos")]
    public TMP_InputField PC;
    public TMP_InputField PR;
    public TMP_InputField Probabilidad;

    private void Start()
    {
        // Precargar UI con los valores configurados
        if (PC != null) PC.text = PcConfig.ToString();
        if (PR != null) PR.text = PrConfig.ToString();
        if (Probabilidad != null) Probabilidad.text = porcentajeConfig.ToString();
    }

    public int[,] Agent(int[,] mapa)
    {
        // Inicializar con los valores de configuración
        Pc = PcConfig;
        Pr = PrConfig;
        porcentajeInicial = porcentajeConfig;

        // si no se ingresó una seed fija, generar una aleatoria cada vez
        if (useRandomSeed)
        {
            seed = (int)(DateTime.Now.Ticks % int.MaxValue);
            Debug.Log("Seed generada aleatoriamente: " + seed);
        }
        else
        {
            Debug.Log("Usando seed ingresada: " + seed);
        }

        // Inicializar la semilla justo antes de usar Random para que siempre sea reproducible
        UnityEngine.Random.InitState(seed);

        // posicion central siempre 
        yInitialPos = (mapa.GetLength(0) - 1) / 2;
        xInitialPos = (mapa.GetLength(1) - 1) / 2;
        mapa[yInitialPos, xInitialPos] = 1;
        float procentajeDeSalas = 0;

        // mientras que el % no alcance el requerido, seguimos generando
        while (procentajeDeSalas < porcentajeInicial)
        {
            procentajeDeSalas = 0;

            int ChanceDir = Random.Range(0, 100);
            int ChanceSala = Random.Range(0, 100);

            if (ChanceDir > Pc)
            {
                int newX = xInitialPos + dirX;
                int newY = yInitialPos + dirY;

                if (newX >= 0 && newX < mapa.GetLength(0) && newY >= 0 && newY < mapa.GetLength(1))
                {
                    xInitialPos = newX;
                    yInitialPos = newY;
                    mapa[xInitialPos, yInitialPos] = 1;
                    Pc += 1;
                }
                else
                {
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

            if (ChanceSala > Pr)
            {
                Pr += 1;
            }
            else
            {
                // calcular el tamaño de la sala generada
                int AltoSala = Random.Range(2, 7);
                int AnchoSala = Random.Range(2, 7);

                int inicioX = xInitialPos - AnchoSala / 2;
                int finX = xInitialPos + AnchoSala / 2;
                int inicioY = yInitialPos - AltoSala / 2;
                int finY = yInitialPos + AltoSala / 2;

                if (LaSalaesValida(inicioX, finX, inicioY, finY, mapa))
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

            // calcular % de celdas caminables
            int contadorUnos = 0;
            for (int i = 0; i < mapa.GetLength(0); i++)
            {
                for (int j = 0; j < mapa.GetLength(1); j++)
                {
                    if (mapa[i, j] == 1) { contadorUnos++; }
                }
            }
            procentajeDeSalas = contadorUnos / (float)mapa.Length;
        }

        return mapa;
    }

    private bool LaSalaesValida(int inicioX, int finX, int inicioY, int finY, int[,] mapa)
    {
        // comprueba viendo la separación con otras salas si intersecta con otra
        inicioX = Mathf.Max(0, inicioX - separacion);
        finX = Mathf.Min(mapa.GetLength(1) - 1, finX + separacion);
        inicioY = Mathf.Max(0, inicioY - separacion);
        finY = Mathf.Min(mapa.GetLength(0) - 1, finY + separacion);

        for (int i = inicioX; i <= inicioX; i++)
        {
            for (int j = inicioY; j <= finY; j++)
            {
                if (mapa[i, j] == 1)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void randomDir()
    {
        // movimiento solo en 4 direcciones
        int direcciones = Random.Range(0, 4);
        switch (direcciones)
        {
            case 0: dirX = 1; dirY = 0; break;
            case 1: dirX = -1; dirY = 0; break;
            case 2: dirX = 0; dirY = 1; break;
            case 3: dirX = 0; dirY = -1; break;
        }
    }

    public void TextToSeed(string txt)
    {
        int NumberSeed = Convert.ToInt32(txt);
        seed = NumberSeed;
    }

    public void RandomSeed(bool state)
    {
        useRandomSeed = state;
    }

    public void SetPr(int prtxt)
    {
        PrConfig = prtxt;
    } 
    public void SetPc(int pctxt)
    {
        PcConfig = pctxt;
    }
    public void SetProbRoom(int probRoomtxt)
    {
        porcentajeConfig = probRoomtxt;
    }
}
