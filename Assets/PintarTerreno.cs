using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class PintarPorAltura : MonoBehaviour
{
    [Header("Terreno")]
    public Terrain terreno;

    [Header("Texturas")]
    public Texture2D texturaBaja; // verde
    public Texture2D texturaAlta; // gris

    void Start()
    {
        if (terreno == null)
        {
            terreno = GetComponent<Terrain>();
        }

        AplicarTexturas();
    }

    void AplicarTexturas()
    {
        TerrainData tData = terreno.terrainData;

        int anchoSplat = tData.alphamapWidth;
        int altoSplat = tData.alphamapHeight;
        int capas = 2;

        float[,,] splatmap = new float[anchoSplat, altoSplat, capas];

        // Obtener alturas normalizadas (0-1)
        int resAltura = tData.heightmapResolution;
        float[,] alturas = tData.GetHeights(0, 0, resAltura, resAltura);

        for (int y = 0; y < altoSplat; y++)
        {
            for (int x = 0; x < anchoSplat; x++)
            {
                // Remapear coordenadas del splatmap a la altura
                int hx = Mathf.RoundToInt(x / (float)(anchoSplat - 1) * (resAltura - 1));
                int hy = Mathf.RoundToInt(y / (float)(altoSplat - 1) * (resAltura - 1));

                float altura = alturas[hy, hx]; // 0 = bajo, 1 = alto

                splatmap[y, x, 0] = Mathf.Clamp01(1f - altura); // verde
                splatmap[y, x, 1] = Mathf.Clamp01(altura);      // gris
            }
        }

        // Siempre reemplazar los layers existentes
        TerrainLayer layerBaja = new TerrainLayer();
        layerBaja.diffuseTexture = texturaBaja;
        layerBaja.tileSize = new Vector2(10, 10);

        TerrainLayer layerAlta = new TerrainLayer();
        layerAlta.diffuseTexture = texturaAlta;
        layerAlta.tileSize = new Vector2(10, 10);

        tData.terrainLayers = new TerrainLayer[] { layerBaja, layerAlta };

        tData.SetAlphamaps(0, 0, splatmap);
    }
}
