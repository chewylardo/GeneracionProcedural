using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/*public class DrunkenAgent : MonoBehaviour
{
    public int Pc = 3; // probabilidad de girar
    public int Pr = 1; // probabilidad de crear sala
    public int xInitialPos;
    public int yInitialPos;
    private int dirX = 1;
    private int dirY = 0;
    private int dirXAnterior = 0;
    private int dirYAnterior = 0;
    public int separacion = 2; // separación mínima entre salas
    private int pasosDesdeUltimoGiro = 0;
    public int minLongitudCorredor = 4; // largo mínimo de los caminos, ajustable en Unity
    public int margen = 1; // margen para evitar los bordes del mapa
    public float porcentajeVacio;

    void Start() { }
    void Update() { }

    public int[,] Agent(int[,] mapa)
    {
        // posición inicial central
        yInitialPos = (mapa.GetLength(0) - 1) / 2;
        xInitialPos = (mapa.GetLength(1) - 1) / 2;
        mapa[yInitialPos, xInitialPos] = 1;

        float porcentajeDeSalas = 0;

        while (porcentajeDeSalas < porcentajeVacio)
        {
            porcentajeDeSalas = 0;

            // mover agente
            MoverAgente(mapa);

            // solo intentar girar o crear sala si pasó minLongitudCorredor bloques
            if (pasosDesdeUltimoGiro >= minLongitudCorredor)
            {
                // crear sala
                if (Random.Range(0, 100) < Pr)
                {
                    bool salaCreada = CrearSalaAleatoria(mapa);
                    if (salaCreada)
                        pasosDesdeUltimoGiro = 0;
                    else
                        Pr += 1;
                }

                // girar
                if (Random.Range(0, 100) < Pc)
                {
                    GiroAleatorio();
                }
            }

            // crear corredor desde sala
            CrearCorredorDesdeSala(mapa);

            // actualizar porcentaje de suelo caminable
            int contadorUnos = 0;
            for (int i = 0; i < mapa.GetLength(0); i++)
                for (int j = 0; j < mapa.GetLength(1); j++)
                    if (mapa[i, j] == 1)
                        contadorUnos++;

            porcentajeDeSalas = contadorUnos / (float)mapa.Length;
        }

        return mapa;
    }

    private void MoverAgente(int[,] mapa)
    {
        int newX = xInitialPos + dirX;
        int newY = yInitialPos + dirY;

        if (newX >= margen && newX < mapa.GetLength(1) - margen &&
            newY >= margen && newY < mapa.GetLength(0) - margen)
        {
            xInitialPos = newX;
            yInitialPos = newY;
            mapa[yInitialPos, xInitialPos] = 1;
            pasosDesdeUltimoGiro++;
        }
        else
        {
            GiroAleatorio();
        }
    }

    private void GiroAleatorio()
    {
        List<Vector2Int> posiblesDirs = new List<Vector2Int>
        {
            new Vector2Int(1,0),
            new Vector2Int(-1,0),
            new Vector2Int(0,1),
            new Vector2Int(0,-1)
        };

        // evitar volver a la dirección anterior
        posiblesDirs.RemoveAll(d => d.x == -dirX && d.y == -dirY);

        Vector2Int nuevaDir = posiblesDirs[Random.Range(0, posiblesDirs.Count)];
        dirXAnterior = dirX;
        dirYAnterior = dirY;

        dirX = nuevaDir.x;
        dirY = nuevaDir.y;

        pasosDesdeUltimoGiro = 0;
    }

    private bool CrearSalaAleatoria(int[,] mapa)
    {
        List<Vector2Int> posiblesTamanos = new List<Vector2Int>
        {
            new Vector2Int(3,3),
            new Vector2Int(4,4),
            new Vector2Int(5,5),
            new Vector2Int(6,6)
        };
        posiblesTamanos = posiblesTamanos.OrderBy(t => Random.value).ToList();

        foreach (var size in posiblesTamanos)
        {
            int ancho = size.x;
            int alto = size.y;

            int inicioX = Mathf.Max(margen, xInitialPos - ancho / 2);
            int finX = Mathf.Min(mapa.GetLength(1) - 1 - margen, xInitialPos + ancho / 2);
            int inicioY = Mathf.Max(margen, yInitialPos - alto / 2);
            int finY = Mathf.Min(mapa.GetLength(0) - 1 - margen, yInitialPos + alto / 2);

            if (LaSalaesValida(inicioX, finX, inicioY, finY, mapa))
            {
                for (int i = inicioX; i <= finX; i++)
                    for (int j = inicioY; j <= finY; j++)
                        mapa[j, i] = 1; // solo la sala real

                return true;
            }
        }
        return false;
    }

    private void CrearCorredorDesdeSala(int[,] mapa)
    {
        // solo crear corredor si estamos sobre una sala
        if (!EsSala(xInitialPos, yInitialPos, mapa))
            return;

        List<Vector2Int> direcciones = new List<Vector2Int>
        {
            new Vector2Int(1,0),
            new Vector2Int(-1,0),
            new Vector2Int(0,1),
            new Vector2Int(0,-1)
        };
        direcciones = direcciones.OrderBy(d => Random.value).ToList();

        foreach (var dir in direcciones)
        {
            int longitud = Random.Range(minLongitudCorredor, minLongitudCorredor + 4); // largo mínimo ajustable
            int nx = xInitialPos;
            int ny = yInitialPos;
            bool puedeColocar = true;

            for (int k = 0; k < longitud; k++)
            {
                nx += dir.x;
                ny += dir.y;

                if (nx < margen || nx >= mapa.GetLength(1) - margen ||
                    ny < margen || ny >= mapa.GetLength(0) - margen)
                {
                    puedeColocar = false;
                    break;
                }

                // bloquear solo si toca una sala (no sobreescribir)
                if (EsSala(nx, ny, mapa))
                {
                    puedeColocar = false;
                    break;
                }
            }

            if (puedeColocar)
            {
                nx = xInitialPos;
                ny = yInitialPos;
                for (int k = 0; k < longitud; k++)
                {
                    nx += dir.x;
                    ny += dir.y;
                    mapa[ny, nx] = 1;
                }
                break; // solo un corredor por iteración
            }
        }
    }

    private bool LaSalaesValida(int inicioX, int finX, int inicioY, int finY, int[,] mapa)
    {
        int checkInicioX = Mathf.Max(margen, inicioX - separacion);
        int checkFinX = Mathf.Min(mapa.GetLength(1) - 1 - margen, finX + separacion);
        int checkInicioY = Mathf.Max(margen, inicioY - separacion);
        int checkFinY = Mathf.Min(mapa.GetLength(0) - 1 - margen, finY + separacion);

        for (int i = checkInicioX; i <= checkFinX; i++)
            for (int j = checkInicioY; j <= checkFinY; j++)
                if (EsSala(i, j, mapa))
                    return false;

        return true;
    }

    private bool EsSala(int x, int y, int[,] mapa)
    {
        return mapa[y, x] == 1;
    }
   
}*/

