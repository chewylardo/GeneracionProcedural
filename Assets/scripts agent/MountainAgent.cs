using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainAgent : TerrainAgent
{
    public override void Step(float[,] heights)
    {
        int size = heights.GetLength(0);

        // Levanta la altura en el punto actual
        heights[x, y] += 0.01f;

        // Opcional: afecta un área pequeña alrededor
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int nx = Mathf.Clamp(x + i, 0, size - 1);
                int ny = Mathf.Clamp(y + j, 0, size - 1);
                heights[nx, ny] += 0.005f;
            }
        }

        // Se mueve de forma aleatoria
        Move(size);
    }
}
