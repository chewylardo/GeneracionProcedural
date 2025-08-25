
using UnityEngine;

public class SmoothingAgent : TerrainAgent
{
    public override void Step(float[,] heights)
    {
        int size = heights.GetLength(0);
        float avg = 0f;
        int count = 0;

        // Calcula el promedio de las alturas vecinas
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int nx = Mathf.Clamp(x + i, 0, size - 1);
                int ny = Mathf.Clamp(y + j, 0, size - 1);
                avg += heights[nx, ny];
                count++;
            }
        }

        // Aplica el suavizado en la celda actual
        heights[x, y] = avg / count;

        // Se mueve
        Move(size);
    }
}
