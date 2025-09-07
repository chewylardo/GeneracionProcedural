using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class LSystemPlant : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject branchPrefab; // ramas  
    public GameObject leafPrefab;   // hojas  

    [Header("Random Settings (max values)")]
    public int Iterations = 2;   // N�mero m�ximo de iteraciones  
    public float maxAngle = 45f;    // �ngulo m�ximo de ramificaci�n  
    public float maxLength = 0.85f;    // Largo m�ximo de las ramas  

    [Header("Leaf Size Settings")]
    public float minLeafSize = 0.2f;  // Tama�o m�nimo de las hojas  
    public float maxLeafSize = 0.6f;  // Tama�o m�ximo de las hojas  

    [Header("Random Seed")]
    public bool useRandomSeed = true;
    public int seed = 12345;  

    [Header("Axiom Options")]
    public string[] possibleAxioms = {
        "F",
        "F[+F]F[-F]F",
        "F[+F]F[-F]F[&F][/F]"
    };

    [Header("Rule Options")]
    public string[] possibleRules = {
        "F[+F]F[-F]F[&F][/F]",
        "F[+F][-F][&F][^F]",
        "F[+F]F[\\F]/F"
    };

    private string axiom;
    private string currentString;

    private Dictionary<char, string> rules;
    private Stack<TransformInfo> transformStack; // Para guardar/restaurar posici�n y rotaci�n  

    // Variables aleatorias internas  
    private int iterations;
    private float angle;
    private float length;

    void Start()
    {
        //GenerateTree();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            RegenerateTree();
        }
    }

    // Aplica las reglas del L-System a la cadena actual  
    string Generate(string input)
    {
        string result = "";
        foreach (char c in input)
        {
            if (rules.ContainsKey(c))
                result += rules[c];         // Reemplaza 'F' seg�n la regla  
            else
                result += c.ToString();     // Otros caracteres se mantienen  
        }
        return result;
    }

    void Draw()
    {
        transformStack = new Stack<TransformInfo>();

        Vector3 currentPos = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        for (int i = 0; i < currentString.Length; i++)
        {
            char c = currentString[i];

            if (c == 'F') // Crear rama  
            {
                Vector3 newPos = currentPos + rotation * Vector3.up * length;

                // Crear rama  
                GameObject branch = Instantiate(branchPrefab, transform);
                branch.transform.localPosition = (currentPos + newPos) / 2f;
                branch.transform.up = (newPos - currentPos).normalized;
                branch.transform.localScale = new Vector3(0.1f, (newPos - currentPos).magnitude / 2f, 0.1f);

                currentPos = newPos;

                if (leafPrefab != null)
                {
                    GameObject leaf = Instantiate(leafPrefab, transform);
                    leaf.transform.localPosition = currentPos;

                    // Escala aleatoria entre minLeafSize y maxLeafSize  
                    float randomLeafSize = Random.Range(minLeafSize, maxLeafSize);
                    leaf.transform.localScale = Vector3.one * randomLeafSize;
                }
            }
            else if (c == '+') // Rotar a la derecha  
            {
                rotation *= Quaternion.Euler(0, 0, angle);
            }
            else if (c == '-') // Rotar a la izquierda  
            {
                rotation *= Quaternion.Euler(0, 0, -angle);
            }
            else if (c == '&') // Rotar X+  
            {
                rotation *= Quaternion.Euler(angle, 0, 0);
            }
            else if (c == '^') // Rotar X-  
            {
                rotation *= Quaternion.Euler(-angle, 0, 0);
            }
            else if (c == '\\') // Rotar Y+  
            {
                rotation *= Quaternion.Euler(0, angle, 0);
            }
            else if (c == '/') // Rotar Y-  
            {
                rotation *= Quaternion.Euler(0, -angle, 0);
            }
            else if (c == '[') // Guardar posici�n y rotaci�n  
            {
                transformStack.Push(new TransformInfo(currentPos, rotation));
            }
            else if (c == ']') // Restaurar posici�n y rotaci�n  
            {
                TransformInfo ti = transformStack.Pop();
                currentPos = ti.position;
                rotation = ti.rotation;
            }
        }
    }

    void GenerateTree()
    {
        // Inicializar la semilla
        if (useRandomSeed)
        {
            seed = Random.Range(0, 99999); // Genera semilla aleatoria
        }
        Random.InitState(seed);

        // Elegir axioma aleatorio
        axiom = possibleAxioms[Random.Range(0, possibleAxioms.Length)];

        // Elegir regla aleatoria
        string chosenRule = possibleRules[Random.Range(0, possibleRules.Length)];
        rules = new Dictionary<char, string>();
        rules.Add('F', chosenRule);
            
        iterations = Iterations;

        // valores aleatorios dentro de los m�ximos
        angle = Random.Range(15f, maxAngle);
        length = Random.Range(0.5f, maxLength);

        Debug.Log($"�rbol generado -> Seed: {seed}, Axioma: {axiom}, Regla: {chosenRule}, Iteraciones: {iterations}, �ngulo: {angle}, Largo: {length}");

        // Generar cadena final aplicando iteraciones
        currentString = axiom;
        for (int i = 0; i < iterations; i++)
        {
            currentString = Generate(currentString);
        }

        Draw();
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

    // ===============================  
    //  Funciones para reiniciar el �rbol  
    // ===============================  

    void ClearTree()
    {
        // Eliminar todas las ramas y hojas   
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void RegenerateTree()
    {
        ClearTree();
        GenerateTree();
    }
}

// Clase auxiliar para guardar posici�n y rotaci�n  
public class TransformInfo
{
    public Vector3 position;
    public Quaternion rotation;

    public TransformInfo(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
    }
}
