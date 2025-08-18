using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controlador principal para la generaci�n de mapas usando BSP
public class BSP_controller : MonoBehaviour
{
    // Clase interna que representa un nodo del �rbol BSP
    [System.Serializable]
    public class Node
    {
        public Rect area;      // �rea rectangular que representa este nodo
        public Node left, right; // Hijos izquierdo y derecho

        // Constructor que inicializa el �rea del nodo
        public Node(Rect area)
        {
            this.area = area;
        }
    }

    public int maxDepth = 4; // Profundidad m�xima de la recursi�n BSP
    public float minSize = 4f; // Tama�o m�nimo permitido para dividir un �rea
    public Rect initialArea = new Rect(0, 0, 32, 32); // �rea inicial a dividir
    public List<Rect> finalAreas = new List<Rect>(); // Lista de �reas finales (habitaciones)

    public GameObject floorPrefab; // Prefab para el suelo (asignar en el Inspector)
    public GameObject wallPrefab;  // Prefab para el muro (asignar en el Inspector)

    // M�todo que se ejecuta al iniciar la escena
    void Start()
    {
        Node root = new Node(initialArea); // Crear nodo ra�z con el �rea inicial
        Split(root, 0);                    // Dividir recursivamente el �rea
        CollectLeaves(root);               // Recolectar las �reas finales (hojas)
        RellenarMapa();                    // Instanciar suelos y muros en el mapa
    }

    // M�todo recursivo que divide un nodo en dos sub�reas
    void Split(Node node, int depth)
    {
        // Condici�n de parada: profundidad m�xima o �rea demasiado peque�a
        if (depth >= maxDepth || node.area.width < minSize * 2 || node.area.height < minSize * 2)
            return;

        // Decidir si la divisi�n ser� horizontal o vertical
        bool splitHorizontally = node.area.width < node.area.height;
        if (Random.value > 0.5f) splitHorizontally = !splitHorizontally;

        if (splitHorizontally)
        {
            // Divisi�n horizontal: elegir posici�n de corte en Y
            float splitY = Random.Range(minSize, node.area.height - minSize);
            // Crear sub�rea inferior
            node.left = new Node(new Rect(node.area.x, node.area.y, node.area.width, splitY));
            // Crear sub�rea superior
            node.right = new Node(new Rect(node.area.x, node.area.y + splitY, node.area.width, node.area.height - splitY));
        }
        else
        {
            // Divisi�n vertical: elegir posici�n de corte en X
            float splitX = Random.Range(minSize, node.area.width - minSize);
            // Crear sub�rea izquierda
            node.left = new Node(new Rect(node.area.x, node.area.y, splitX, node.area.height));
            // Crear sub�rea derecha
            node.right = new Node(new Rect(node.area.x + splitX, node.area.y, node.area.width - splitX, node.area.height));
        }

        // Llamada recursiva para seguir dividiendo las sub�reas
        Split(node.left, depth + 1);
        Split(node.right, depth + 1);
    }

    // M�todo que recolecta todas las �reas finales (hojas) del �rbol BSP
    void CollectLeaves(Node node)
    {
        // Si el nodo no tiene hijos, es una hoja
        if (node.left == null && node.right == null)
        {
            finalAreas.Add(node.area); // Agregar �rea a la lista de �reas finales
        }
        else
        {
            // Recorrer recursivamente los hijos
            if (node.left != null) CollectLeaves(node.left);
            if (node.right != null) CollectLeaves(node.right);
        }
    }

    // M�todo que instancia los suelos y muros en el mapa seg�n las �reas finales
    void RellenarMapa()
    {
        foreach (Rect area in finalAreas)
        {
            // Instanciar suelos dentro del �rea
            for (int x = Mathf.FloorToInt(area.x); x < Mathf.CeilToInt(area.x + area.width); x++)
            {
                for (int y = Mathf.FloorToInt(area.y); y < Mathf.CeilToInt(area.y + area.height); y++)
                {
                    // Instanciar un prefab de suelo en cada posici�n
                    Instantiate(floorPrefab, new Vector3(x, 0, y), Quaternion.identity, this.transform);
                }
            }

            // Instanciar muros alrededor del �rea
            // Muros horizontales (inferior y superior)
            for (int x = Mathf.FloorToInt(area.x) - 1; x <= Mathf.CeilToInt(area.x + area.width); x++)
            {
                // Muro inferior
                Instantiate(wallPrefab, new Vector3(x, 0, Mathf.FloorToInt(area.y) - 1), Quaternion.identity, this.transform);
                // Muro superior
                Instantiate(wallPrefab, new Vector3(x, 0, Mathf.CeilToInt(area.y + area.height)), Quaternion.identity, this.transform);
            }
            // Muros verticales (izquierdo y derecho)
            for (int y = Mathf.FloorToInt(area.y); y < Mathf.CeilToInt(area.y + area.height); y++)
            {
                // Muro izquierdo
                Instantiate(wallPrefab, new Vector3(Mathf.FloorToInt(area.x) - 1, 0, y), Quaternion.identity, this.transform);
                // Muro derecho
                Instantiate(wallPrefab, new Vector3(Mathf.CeilToInt(area.x + area.width), 0, y), Quaternion.identity, this.transform);
            }
        }
    }
}



