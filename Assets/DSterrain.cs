using UnityEngine;
using System;
using Unity.VisualScripting;

public class DSterrain : MonoBehaviour
{
    public int tamaño;   // Potencia (n), se convertirá en (2^n + 1) eso asegura la figura 3 x 3
    public float ruido;   // que tan "ruidoso" es el mapa
    public float altoMapa; // la altura maxima para el mapa en base al nivel del mar 
    private float[,] mapaDS;

    public Terrain terreno;

    void Start()
    {
        //esto es para asegurarse que sea un mapa multiplo de 3 para poder hacer el diamond square
        // x 0 x
        // 0 0 0
        // x 0 x
        tamaño = (int)Math.Pow(2, tamaño) + 1;
        mapaDS = new float[tamaño, tamaño];

        // se inician las puntas del mapaa aqui
        mapaDS[0, 0] = UnityEngine.Random.Range(0f, altoMapa);
        mapaDS[0, tamaño - 1] = UnityEngine.Random.Range(0f, altoMapa);
            mapaDS[tamaño - 1, 0] = UnityEngine.Random.Range(0f, altoMapa);
         mapaDS[tamaño - 1, tamaño - 1] = UnityEngine.Random.Range(0f, altoMapa);
    
        // Ejecutar algoritmo recursivo
        //los mapas los tube que poner -1 por que sino se salia del rango del arreglo
        DiamondSquare(0, 0, tamaño -1, tamaño -1 , altoMapa);

        // se crea el terreno
        terreno.terrainData.heightmapResolution = tamaño;
        terreno.terrainData.size = new Vector3(200, altoMapa, 200);
        terreno.terrainData.SetHeights(0, 0, NormalizarMapa(mapaDS));
    }
    //se pasan todas las esquinas del mapa para poder empezar la division recursiva para que se genere el diamond square
    void DiamondSquare(int posx1, int posy1, int posx2, int posy2, float rango)
    {
        int lado = posx2 - posx1;
        if (lado < 2){
            return;
            // si no, no termina y se bugea
        }

        int mitad = lado / 2;
        int xmedio = posx1 + mitad;
        int ymedio = posy1 + mitad;

        // marcar las esquinas de esta iteracion y definir el centro, el punto del centro es un lugar random entre el los valores delimitados
        float esquina1 = mapaDS[posx1, posy1];
        float esquina2 = mapaDS[posx2, posy1];
        float esquina3 = mapaDS[posx1, posy2];
        float esquina4 = mapaDS[posx2, posy2];


        float promedio = (esquina1 + esquina2 + esquina3 + esquina4) / 4f;

          mapaDS[xmedio, ymedio] = promedio + UnityEngine.Random.Range(-rango, rango);

        // sacar los puntos entre las esquinas para luego "randomizarlos un poco" y asi se le da un efecto mas "realista a los picos o montes"
        
        if (mapaDS[xmedio, posy1] == 0) { 

            mapaDS[xmedio, posy1] = (esquina1 + esquina2) / 2f + UnityEngine.Random.Range(-rango, rango);
        }
        if (mapaDS[xmedio, posy2] == 0)
        {
            mapaDS[xmedio, posy2] = (esquina3 + esquina4) / 2f + UnityEngine.Random.Range(-rango, rango);

        }
        if (mapaDS[posx1, ymedio] == 0)
        {
            mapaDS[posx1, ymedio] = (esquina1 + esquina3) / 2f + UnityEngine.Random.Range(-rango, rango);
        }
        if (mapaDS[posx2, ymedio] == 0){
            mapaDS[posx2, ymedio] = (esquina2 + esquina4) / 2f + UnityEngine.Random.Range(-rango, rango);
        }

    //se llama denuevo para cada esquina  la recursion de diamond square, se reduce la variabilidad para que asi se mantenga "coherencia" de montaña
    rango *= ruido; // reducir la variación

        DiamondSquare(posx1, posy1, xmedio, ymedio, rango);
       
        DiamondSquare(xmedio, posy1, posx2, ymedio, rango);
        DiamondSquare(posx1, ymedio, xmedio, posy2, rango);
        DiamondSquare(xmedio, ymedio, posx2, posy2, rango);
    }

    // se normaliza el mapa para que se vea mejor y evitar picos muy elevados , caidas muy abroptas o cortes de mapa similares a 90º
    //es como para suavizar el mapa
    float[,] NormalizarMapa(float[,] mapa)
    {
        float min = float.MaxValue;
        float max = float.MinValue;
        //minimo y maximo del mapa
        foreach (float val in mapa){
            if (val < min){
                min = val;
            }
            if (val > max)
            {
                max = val;
            }
        }
        //aqui se van a guardar los valores normalizados
        
       
        float[,] normalizado = new float[mapa.GetLength(0), mapa.GetLength(1)];
       
        for (int x = 0; x < mapa.GetLength(0); x++)
        {
            for (int y = 0; y < mapa.GetLength(1); y++)
            {   //aqui se normalizan cada uno de los valores
                normalizado[x, y] = Mathf.InverseLerp(min, max, mapa[x, y]);
            } }



        return normalizado;
    }
}
