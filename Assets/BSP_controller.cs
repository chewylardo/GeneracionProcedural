using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controlador principal para la generación de mapas usando BSP
public class BSP_controller : MonoBehaviour
{
    // Clase interna que representa un nodo del árbol BSP
    [System.Serializable]
    public class Node
    {
        public Rect area;      // Área rectangular que representa este nodo
        public Node left, right; // Hijos izquierdo y derecho

        // Constructor que inicializa el área del nodo
        public Node(Rect area)
        {
            this.area = area;
        }
    }

    public int maxDepth = 4; // Profundidad máxima de la recursión BSP
    public float minSize = 4f; // Tamaño mínimo permitido para dividir un área
    public Rect initialArea = new Rect(0, 0, 32, 32); // Área inicial a dividir
    public List<Rect> finalAreas = new List<Rect>(); // Lista de áreas finales (habitaciones)

    public GameObject floorPrefab; // Prefab para el suelo (asignar en el Inspector)
    public GameObject wallPrefab;  // Prefab para el muro (asignar en el Inspector)

    // Método que se ejecuta al iniciar la escena
    void Start()
    {
        Node root = new Node(initialArea); // Crear nodo raíz con el área inicial
        Split(root, 0);                    // Dividir recursivamente el área
        CollectLeaves(root);               // Recolectar las áreas finales (hojas)
        RellenarMapa();                    // Instanciar suelos y muros en el mapa
    }

    // Método recursivo que divide un nodo en dos subáreas
    void Split(Node node, int depth)
    {
        // Condición de parada: profundidad máxima o área demasiado pequeña
        if (depth >= maxDepth || node.area.width < minSize * 2 || node.area.height < minSize * 2)
            return;

        // Decidir si la división será horizontal o vertical
        bool splitHorizontally = node.area.width < node.area.height;
        if (Random.value > 0.5f) splitHorizontally = !splitHorizontally;

        if (splitHorizontally)
        {
            // División horizontal: elegir posición de corte en Y
            float splitY = Random.Range(minSize, node.area.height - minSize);
            // Crear subárea inferior
            node.left = new Node(new Rect(node.area.x, node.area.y, node.area.width, splitY));
            // Crear subárea superior
            node.right = new Node(new Rect(node.area.x, node.area.y + splitY, node.area.width, node.area.height - splitY));
        }
        else
        {
            // División vertical: elegir posición de corte en X
            float splitX = Random.Range(minSize, node.area.width - minSize);
            // Crear subárea izquierda
            node.left = new Node(new Rect(node.area.x, node.area.y, splitX, node.area.height));
            // Crear subárea derecha
            node.right = new Node(new Rect(node.area.x + splitX, node.area.y, node.area.width - splitX, node.area.height));
        }

        // Llamada recursiva para seguir dividiendo las subáreas
        Split(node.left, depth + 1);
        Split(node.right, depth + 1);
    }

    // Método que recolecta todas las áreas finales (hojas) del árbol BSP
    void CollectLeaves(Node node)
    {
        // Si el nodo no tiene hijos, es una hoja
        if (node.left == null && node.right == null)
        {
            finalAreas.Add(node.area); // Agregar área a la lista de áreas finales
        }
        else
        {
            // Recorrer recursivamente los hijos
            if (node.left != null) CollectLeaves(node.left);
            if (node.right != null) CollectLeaves(node.right);
        }
    }

    // Método que instancia los suelos y muros en el mapa según las áreas finales
    void RellenarMapa()
    {
        foreach (Rect area in finalAreas)
        {
            // Instanciar suelos dentro del área
            for (int x = Mathf.FloorToInt(area.x); x < Mathf.CeilToInt(area.x + area.width); x++)
            {
                for (int y = Mathf.FloorToInt(area.y); y < Mathf.CeilToInt(area.y + area.height); y++)
                {
                    // Instanciar un prefab de suelo en cada posición
                    Instantiate(floorPrefab, new Vector3(x, 0, y), Quaternion.identity, this.transform);
                }
            }

            // Instanciar muros alrededor del área
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



