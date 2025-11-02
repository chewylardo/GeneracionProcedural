using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntGrid
{
    public int width = 5;
    public int height = 5;
    public List<int> values = new List<int>();

    public int this[int x, int y]
    {
        get => values[y * width + x];
        set => values[y * width + x] = value;
    }

    public void Resize(int newWidth, int newHeight)
    {
        List<int> newValues = new List<int>(newWidth * newHeight);
        for (int y = 0; y < newHeight; y++)
            for (int x = 0; x < newWidth; x++)
                newValues.Add((x < width && y < height && values != null && values.Count == width * height) ? this[x, y] : 0);
        width = newWidth;
        height = newHeight;
        values = newValues;
    }
}