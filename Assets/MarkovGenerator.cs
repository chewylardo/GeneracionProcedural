using System.Collections.Generic;
using UnityEngine;

public class MarkovGenerator : MonoBehaviour
{
    [Header("Configuración del modelo")]
    [Range(1, 5)] public int N = 2; // Tamaño del N-grama
    [TextArea(3, 10)] public string trainingData = "AABBBCCCAAABBCC"; // Ejemplo de entrenamiento
    [Range(10, 200)] public int outputLength = 50;

    private Dictionary<string, List<char>> ngrams;

    /// <summary>
    /// Llama a este método para generar una nueva secuencia.
    /// </summary>
    public string GenerateLevelMarkov()
    {
        if (N < 1)
        {
            Debug.LogError("N debe ser mayor o igual a 1");
            return "";
        }

        BuildModel();

        // Elegir un contexto inicial aleatorio
        List<string> keys = new List<string>(ngrams.Keys);
        if (keys.Count == 0)
        {
            Debug.LogError("No hay suficientes datos para generar N-gramas");
            return "";
        }

        string current = keys[Random.Range(0, keys.Count)];
        string result = current;

        // Generar la secuencia
        for (int i = 0; i < outputLength; i++)
        {
            if (!ngrams.ContainsKey(current))
                break;

            List<char> possibleNext = ngrams[current];
            char next = possibleNext[Random.Range(0, possibleNext.Count)];
            result += next;

            // Actualizar contexto (últimos N-1 caracteres)
            current = result.Substring(result.Length - (N - 1));
        }

        return result;
    }

    /// <summary>
    /// Construye el diccionario de N-gramas a partir del texto de entrenamiento.
    /// </summary>
    private void BuildModel()
    {
        ngrams = new Dictionary<string, List<char>>();

        string data = trainingData.Trim().Replace("\n", "").Replace(" ", "");

        for (int i = 0; i < data.Length - N; i++)
        {
            string key = data.Substring(i, N - 1);
            char next = data[i + N - 1];

            if (!ngrams.ContainsKey(key))
                ngrams[key] = new List<char>();

            ngrams[key].Add(next);
        }

        Debug.Log($"Modelo creado con {ngrams.Count} N-gramas (N={N})");
    }
}
