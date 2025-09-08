using UnityEngine;
using System;
using Unity.VisualScripting;
using TMPro;
using Unity.VisualScripting.FullSerializer;

public class DSterrain : MonoBehaviour
{
    public int tamaño;   // Potencia (n), se convertirá en (2^n + 1)
    public float ruido;  // Qué tan "ruidoso" es el mapa
    public float altoMapa; // Altura máxima

    [Header("Seed Settings")]
    public int seed = 0;
    public bool useRandomSeed = true;

    [Header("Centro Plano")]
    public int porcentajeCentro = 25; // Porcentaje del terreno que ocupa el centro

    [Header("Textos")]
    public TMP_InputField Altura;
    public TMP_InputField Ruido;


    private float altoMapaInicial;
    private float ruidoInicial;



    private float[,] mapaDS;

    public Terrain terreno;

    [Header("ref")]
    public TreeDistributor distributor;

    void Start()
    {
        if (Altura != null) Altura.text = altoMapa.ToString();
        if (Ruido != null) Ruido.text = ruido.ToString();
        GenerateTerrain();
        distributor.DistributeTrees();
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

    void AplanarCentroEnCero(float[,] mapa, int tamañoCuadro)
    {
        int n = mapa.GetLength(0);
        int inicio = n / 2 - tamañoCuadro / 2;
        int fin = inicio + tamañoCuadro;

        for (int x = inicio; x < fin; x++)
        {
            for (int y = inicio; y < fin; y++)
            {
                mapa[x, y] = 0f; // centro en y = 0 absoluto
            }
        }
    }

    float[,] NormalizarMapaConCentroCero(float[,] mapa, int tamañoCuadro)
    {
        int n = mapa.GetLength(0);
        int inicio = n / 2 - tamañoCuadro / 2;
        int fin = inicio + tamañoCuadro;

        float min = float.MaxValue;
        float max = float.MinValue;

        // Calcular min y max excluyendo el centro
        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                if (x >= inicio && x < fin && y >= inicio && y < fin) continue;
                if (mapa[x, y] < min) min = mapa[x, y];
                if (mapa[x, y] > max) max = mapa[x, y];
            }
        }

        // Normalizar respetando el centro en 0
        float[,] normalizado = new float[n, n];
        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                if (x >= inicio && x < fin && y >= inicio && y < fin)
                {
                    normalizado[x, y] = altoMapa; // centro permanece en 0
                }
                else
                {
                    normalizado[x, y] = Mathf.InverseLerp(min, max, mapa[x, y]);
                }
            }
        }

        return normalizado;
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


    // ---------------- Generación ----------------
    public void GenerateTerrain()
    {
        if (Altura != null) float.TryParse(Altura.text, out altoMapa);
        if (Ruido != null) float.TryParse(Ruido.text, out ruido);

        if (useRandomSeed)
        {
            seed = UnityEngine.Random.Range(1, int.MaxValue);
            Debug.Log("Seed generada: " + seed);
        }
        else
        {
            Debug.Log("Seed ingresada: " + seed);
        }

       // if (SeedInput != null) SeedInput.text = seed.ToString();

        UnityEngine.Random.InitState(seed);

        int size = (int)Math.Pow(2, tamaño) + 1;
        mapaDS = new float[size, size];

        // Inicializar esquinas
        mapaDS[0, 0] = UnityEngine.Random.Range(0f, altoMapa);
        mapaDS[0, size - 1] = UnityEngine.Random.Range(0f, altoMapa);
        mapaDS[size - 1, 0] = UnityEngine.Random.Range(0f, altoMapa);
        mapaDS[size - 1, size - 1] = UnityEngine.Random.Range(0f, altoMapa);

        DiamondSquare(0, 0, size - 1, size - 1, altoMapa);

        int tamañoCuadro = size * porcentajeCentro / 100;
        AplanarCentroEnCero(mapaDS, tamañoCuadro);

        terreno.terrainData.heightmapResolution = size;
        terreno.terrainData.size = new Vector3(200, altoMapa, 200);
        terreno.terrainData.SetHeights(0, 0, NormalizarMapaConCentroCero(mapaDS, tamañoCuadro));
    }

   
    public void RegenerarTerreno()
    {
        GenerateTerrain();
    }
    public void SetAltura(float altura)
    {
        altoMapa = altura;
    }
    public void SetNoise(float noise)
    {
        ruido = noise;
    }

}
