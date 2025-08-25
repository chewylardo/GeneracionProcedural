using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelinGeneration : MonoBehaviour
{
    public GameObject Cube;
    public int sizeX = 20;
    public int sizeZ = 20;
    public float noiseHeight = 1.5f;
    public float Offset = 1.1f;

    void Start()
    {
        Create();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Recreate();
        }
    }

    public void Recreate()
    {
        Clear();
        Create();
    }

    public void Create()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                Vector3 pos = new Vector3(x * Offset,
                    GenerateNoise(x, z, 8f) * noiseHeight,
                    z * Offset);

                GameObject Block = Instantiate(Cube, pos, Quaternion.identity);
                Block.transform.SetParent(this.transform);
            }
        }
    }

    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private float GenerateNoise(int x, int z, float detailScale)
    {
        float randomOffsetX = Random.Range(0f, 1000f);
        float randomOffsetZ = Random.Range(0f, 1000f);

        float xNoise = (x + randomOffsetX) / detailScale;
        float zNoise = (z + randomOffsetZ) / detailScale;

        return Mathf.PerlinNoise(xNoise, zNoise);
    }

}
