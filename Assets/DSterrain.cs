using UnityEngine;
using System;
using Unity.VisualScripting;

public class DSterrain : MonoBehaviour
{
    public int tamaño;   // Potencia (n), se convertirá en (2^n + 1)
    public float ruido;  // Qué tan "ruidoso" es el mapa
    public float altoMapa; // Altura máxima

    [Header("Seed Settings")]
    public int seed = 0;
    public bool useRandomSeed = true;

    private float[,] mapaDS;

    public Terrain terreno;

    void Start()
    {
        if (useRandomSeed)
        {
            seed = UnityEngine.Random.Range(1, int.MaxValue);
            Debug.Log("Seed generada: " + seed);
        }
        else
        {
            Debug.Log("Seed ingresada: " + seed);
        }

        UnityEngine.Random.InitState(seed);

        tamaño = (int)Math.Pow(2, tamaño) + 1;
        mapaDS = new float[tamaño, tamaño];

        // Inicializar esquinas
        mapaDS[0, 0] = UnityEngine.Random.Range(0f, altoMapa);
        mapaDS[0, tamaño - 1] = UnityEngine.Random.Range(0f, altoMapa);
        mapaDS[tamaño - 1, 0] = UnityEngine.Random.Range(0f, altoMapa);
        mapaDS[tamaño - 1, tamaño - 1] = UnityEngine.Random.Range(0f, altoMapa);

        DiamondSquare(0, 0, tamaño - 1, tamaño - 1, altoMapa);

        // Aplanar centro
        int tamañoCuadro = tamaño / 4;
        AplanarCentroCuadrado(mapaDS, tamañoCuadro);

        // Crear terreno
        terreno.terrainData.heightmapResolution = tamaño;
        terreno.terrainData.size = new Vector3(200, altoMapa, 200);
        terreno.terrainData.SetHeights(0, 0, NormalizarMapa(mapaDS));
    }

    void DiamondSquare(int posx1, int posy1, int posx2, int posy2, float rango)
    {
        int lado = posx2 - posx1;
        if (lado < 2) return;

        int mitad = lado / 2;
        int xmedio = posx1 + mitad;
        int ymedio = posy1 + mitad;

        float esquina1 = mapaDS[posx1, posy1];
        float esquina2 = mapaDS[posx2, posy1];
        float esquina3 = mapaDS[posx1, posy2];
        float esquina4 = mapaDS[posx2, posy2];

        float promedio = (esquina1 + esquina2 + esquina3 + esquina4) / 4f;

        mapaDS[xmedio, ymedio] = promedio + UnityEngine.Random.Range(-rango, rango);

        if (mapaDS[xmedio, posy1] == 0) mapaDS[xmedio, posy1] = (esquina1 + esquina2) / 2f + UnityEngine.Random.Range(-rango, rango);
        if (mapaDS[xmedio, posy2] == 0) mapaDS[xmedio, posy2] = (esquina3 + esquina4) / 2f + UnityEngine.Random.Range(-rango, rango);
        if (mapaDS[posx1, ymedio] == 0) mapaDS[posx1, ymedio] = (esquina1 + esquina3) / 2f + UnityEngine.Random.Range(-rango, rango);
        if (mapaDS[posx2, ymedio] == 0) mapaDS[posx2, ymedio] = (esquina2 + esquina4) / 2f + UnityEngine.Random.Range(-rango, rango);

        rango *= ruido;

        DiamondSquare(posx1, posy1, xmedio, ymedio, rango);
        DiamondSquare(xmedio, posy1, posx2, ymedio, rango);
        DiamondSquare(posx1, ymedio, xmedio, posy2, rango);
        DiamondSquare(xmedio, ymedio, posx2, posy2, rango);
    }

    float[,] NormalizarMapa(float[,] mapa)
    {
        float min = float.MaxValue;
        float max = float.MinValue;

        foreach (float val in mapa)
        {
            if (val < min) min = val;
            if (val > max) max = val;
        }

        float[,] normalizado = new float[mapa.GetLength(0), mapa.GetLength(1)];

        for (int x = 0; x < mapa.GetLength(0); x++)
        {
            for (int y = 0; y < mapa.GetLength(1); y++)
            {
                normalizado[x, y] = Mathf.InverseLerp(min, max, mapa[x, y]);
            }
        }

        return normalizado;
    }

    
    void AplanarCentroCuadrado(float[,] mapa, int tamañoCuadro)
    {
        int n = mapa.GetLength(0);
        int inicio = n / 2 - tamañoCuadro / 2;
        int fin = inicio + tamañoCuadro;

        // Calculamos el valor promedio del centro
        float suma = 0f;
        int cuenta = 0;

        for (int x = inicio; x < fin; x++)
        {
            for (int y = inicio; y < fin; y++)
            {
                suma += mapa[x, y];
                cuenta++;
            }
        }

        float valorPlano = suma / cuenta;

        // Aplicamos el valor fijo a toda la zona central cuadrada
        for (int x = inicio; x < fin; x++)
        {
            for (int y = inicio; y < fin; y++)
            {
                mapa[x, y] = valorPlano;
            }
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
}
