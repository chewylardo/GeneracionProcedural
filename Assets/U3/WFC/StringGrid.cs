using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StringGrid
{
    public int width;
    public int height;
    public List<string> values;

    public string this[int x, int y]
    {
        get => values[y * width + x];
        set => values[y * width + x] = value;
    }

    public void Resize(int w, int h)
    {
        width = w;
        height = h;
        values = new List<string>(new string[w * h]);
    }

    // Convierte un string con saltos de línea en matriz
    public void FromRawString(string raw)
    {
        var lines = raw.Trim().Split('\n');

        height = lines.Length;
        width = lines[0].Trim().Length;

        values = new List<string>(width * height);

        foreach (var line in lines)
        {
            string clean = line.Trim();

            foreach (char c in clean)
                values.Add(c.ToString());
        }
    }


    public string ToRawString()
    {
        string result = "";
        for (int y = 0; y < height; y++)
        {
            List<string> row = new List<string>();
            for (int x = 0; x < width; x++)
                row.Add(this[x, y]);
            result += string.Join(" ", row);
            if (y < height - 1) result += "\n";
        }
        return result;
    }
}
