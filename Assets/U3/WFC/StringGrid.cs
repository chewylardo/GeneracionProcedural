using System;
using System.Collections.Generic;

/*public class StringGrid
{
    public int width;
    public int height;
    private string[] data; // row-major: y*width + x

    public void FromRawString(string raw)
    {
        // split by lines
        string[] rows = raw.Replace("\r", "").Split('\n');
        List<string> lines = new List<string>();
        foreach (var r in rows)
            if (!string.IsNullOrWhiteSpace(r)) lines.Add(r);

        height = lines.Count;
        width = 0;
        foreach (var l in lines) if (l.Length > width) width = l.Length;

        data = new string[width * height];
        for (int y = 0; y < height; y++)
        {
            string line = lines[y];
            for (int x = 0; x < width; x++)
            {
                data[y * width + x] = (x < line.Length) ? line[x].ToString() : "-";
            }
        }
    }

    public string this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return "-";
            return data[y * width + x];
        }
        set
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return;
            data[y * width + x] = value;
        }
    }
}
*/