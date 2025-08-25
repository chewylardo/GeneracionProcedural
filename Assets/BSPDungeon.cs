using System.Collections.Generic;
using UnityEngine;

public class BSPDungeon2D : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int dungeonWidth = 50;
    public int dungeonHeight = 50;
    public int minRoomSize = 6;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject specialObjectPrefab; // Objeto a instanciar en la primera sala

    private Node rootNode;
    private List<Node> leafNodes = new List<Node>();
    private bool specialObjectPlaced = false; // Para que solo se instancie una vez

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        // Crear raíz del BSP
        rootNode = new Node(0, 0, dungeonWidth, dungeonHeight);
        SplitNode(rootNode);
        CreateRooms(rootNode);
        ConnectRooms(rootNode);
        DrawDungeon();
    }

    void SplitNode(Node node)
    {
        if (node.width > minRoomSize * 2 || node.height > minRoomSize * 2)
        {
            bool splitHorizontally = (Random.value > 0.5f);
            if (node.width > node.height && node.width / node.height >= 1.25f)
                splitHorizontally = false;
            else if (node.height > node.width && node.height / node.width >= 1.25f)
                splitHorizontally = true;

            int max = (splitHorizontally ? node.height : node.width) - minRoomSize;
            if (max <= minRoomSize)
            {
                leafNodes.Add(node);
                return;
            }

            int split = Random.Range(minRoomSize, max);

            if (splitHorizontally)
            {
                node.left = new Node(node.x, node.y, node.width, split);
                node.right = new Node(node.x, node.y + split, node.width, node.height - split);
            }
            else
            {
                node.left = new Node(node.x, node.y, split, node.height);
                node.right = new Node(node.x + split, node.y, node.width - split, node.height);
            }

            SplitNode(node.left);
            SplitNode(node.right);
        }
        else
        {
            leafNodes.Add(node);
        }
    }

    void CreateRooms(Node node)
    {
        if (node == null) return;

        if (node.left == null && node.right == null)
        {
            int roomWidth = Random.Range(minRoomSize, node.width - 1);
            int roomHeight = Random.Range(minRoomSize, node.height - 1);
            int roomX = Random.Range(node.x, node.x + node.width - roomWidth);
            int roomY = Random.Range(node.y, node.y + node.height - roomHeight);

            node.room = new RectInt(roomX, roomY, roomWidth, roomHeight);

            // Instanciar objeto especial en la primera sala
            if (!specialObjectPlaced && specialObjectPrefab != null)
            {
                Vector2Int center = node.GetRoomCenter();
                Instantiate(specialObjectPrefab, new Vector3(center.x, center.y, 0), Quaternion.identity);
                specialObjectPlaced = true;
            }
        }
        else
        {
            CreateRooms(node.left);
            CreateRooms(node.right);
        }
    }

    void ConnectRooms(Node node)
    {
        if (node == null || node.left == null || node.right == null) return;

        Vector2Int roomA = node.left.GetRoomCenter();
        Vector2Int roomB = node.right.GetRoomCenter();

        if (Random.value > 0.5f)
        {
            CreateHorizontalCorridor(roomA.x, roomB.x, roomA.y);
            CreateVerticalCorridor(roomA.y, roomB.y, roomB.x);
        }
        else
        {
            CreateVerticalCorridor(roomA.y, roomB.y, roomA.x);
            CreateHorizontalCorridor(roomA.x, roomB.x, roomB.y);
        }

        ConnectRooms(node.left);
        ConnectRooms(node.right);
    }

    void CreateHorizontalCorridor(int x1, int x2, int y)
    {
        int startX = Mathf.Min(x1, x2);
        int endX = Mathf.Max(x1, x2);
        for (int x = startX; x <= endX; x++)
        {
            Instantiate(floorPrefab, new Vector3(x, y, 0), Quaternion.identity);
        }
    }

    void CreateVerticalCorridor(int y1, int y2, int x)
    {
        int startY = Mathf.Min(y1, y2);
        int endY = Mathf.Max(y1, y2);
        for (int y = startY; y <= endY; y++)
        {
            Instantiate(floorPrefab, new Vector3(x, y, 0), Quaternion.identity);
        }
    }

    void DrawDungeon()
    {
        foreach (Node node in leafNodes)
        {
            if (node.room.width > 0 && node.room.height > 0)
            {
                for (int x = node.room.x; x < node.room.x + node.room.width; x++)
                {
                    for (int y = node.room.y; y < node.room.y + node.room.height; y++)
                    {
                        Instantiate(floorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                    }
                }
            }
        }
    }

    public class Node
    {
        public int x, y, width, height;
        public Node left, right;
        public RectInt room;

        public Node(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Vector2Int GetRoomCenter()
        {
            if (room.width == 0) return Vector2Int.zero;
            int centerX = room.x + room.width / 2;
            int centerY = room.y + room.height / 2;
            return new Vector2Int(centerX, centerY);
        }
    }
}
