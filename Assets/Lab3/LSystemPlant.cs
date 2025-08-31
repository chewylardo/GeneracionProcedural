using System.Collections.Generic;
using UnityEngine;

public class LSystemPlant : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject branchPrefab; // ramas
    public GameObject leafPrefab;   // hojas

    [Header("Random Settings (max values)")]
    public int maxIterations = 5;   // Número máximo de iteraciones
    public float maxAngle = 35f;    // Ángulo máximo de ramificación
    public float maxLength = 3f;    // Largo máximo de las ramas

    [Header("Leaf Size Settings")]
    public float minLeafSize = 0.2f;  // Tamaño mínimo de las hojas
    public float maxLeafSize = 0.6f;  // Tamaño máximo de las hojas

    [Header("Axiom Options")]
    public string[] possibleAxioms = { 
        "F", 
        "F[+F]F", 
        "F[-F]F", 
        "F[+F][-F]" 
    };

    [Header("Rule Options")]
    public string[] possibleRules = {
        "F[+F]F[-F]F",
        "F[+F]F",       
        "F[-F]F", 
        "F[+F][-F]F" 
    };

    private string axiom;
    private string currentString;

    private Dictionary<char, string> rules;
    private Stack<TransformInfo> transformStack; // Para guardar/restaurar posición y rotación

    // Variables aleatorias internas
    private int iterations;
    private float angle;
    private float length;

    void Start()
    {
        GenerateTree();
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
                result += rules[c];         // Reemplaza 'F' según la regla
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
                branch.transform.position = (currentPos + newPos) / 2f;
                branch.transform.up = (newPos - currentPos).normalized;
                branch.transform.localScale = new Vector3(0.1f, (newPos - currentPos).magnitude / 2f, 0.1f);

                currentPos = newPos;

                // ¿Es el final de la rama?
                bool isEndOfBranch = true;
                if (i + 1 < currentString.Length)
                {
                    char next = currentString[i + 1];
                    if (next == 'F' || next == '+' || next == '-')
                        isEndOfBranch = false;
                }

                if (isEndOfBranch && leafPrefab != null)
                {
                    GameObject leaf = Instantiate(leafPrefab, currentPos, Quaternion.identity, transform);

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
            else if (c == '[') // Guardar posición y rotación
            {
                transformStack.Push(new TransformInfo(currentPos, rotation));
            }
            else if (c == ']') // Restaurar posición y rotación
            {
                TransformInfo ti = transformStack.Pop();
                currentPos = ti.position;
                rotation = ti.rotation;
            }
        }
    }

    void GenerateTree()
    {
        // Elegir axioma aleatorio
        axiom = possibleAxioms[Random.Range(0, possibleAxioms.Length)];

        // Elegir regla aleatoria
        string chosenRule = possibleRules[Random.Range(0, possibleRules.Length)];
        rules = new Dictionary<char, string>();
        rules.Add('F', chosenRule);

        // valores aleatorios dentro de los máximos
        iterations = Random.Range(2, maxIterations + 1);
        angle = Random.Range(15f, maxAngle);
        length = Random.Range(0.5f, maxLength);

        Debug.Log($"Árbol generado -> Axioma: {axiom}, Regla: {chosenRule}, Iteraciones: {iterations}, Ángulo: {angle}, Largo: {length}");

        // Generar cadena final aplicando iteraciones
        currentString = axiom;
        for (int i = 0; i < iterations; i++)    
        {
            currentString = Generate(currentString);
        }

        Draw();
    }

    // ===============================
    //  Funciones para reiniciar el árbol
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

// Clase auxiliar para guardar posición y rotación
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
