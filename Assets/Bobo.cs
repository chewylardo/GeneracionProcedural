using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class Bobo : MonoBehaviour
{
    public Backward generator; 
    public TextMeshProUGUI resultText;    

    void Start()
    {
        if (generator == null || resultText == null)
        {
            Debug.LogError("Asignar generator y resultText en el Inspector.");
            return;
        }

        int movimientos;
        bool solvable = Solve(generator.playerPos, generator.cajas, generator.objetivos,generator.largo, generator.alto, generator.MurallasInternas, out movimientos);

        if (solvable) { 

            resultText.text = $"Soluble";

        }else {

            resultText.text = $"Insoluble después de {movimientos} movimientos explorados"; 
        }
         
    }

    struct State
    {
        public Vector2Int playerPos;
        public HashSet<Vector2Int> cajas;
        public int movimientos;

        public State(Vector2Int p, IEnumerable<Vector2Int> b, int m)
        {
            playerPos = p;
            cajas = new HashSet<Vector2Int>(b);
            movimientos = m;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is State)) return false;
            State other = (State)obj;
            return playerPos.Equals(other.playerPos) && cajas.SetEquals(other.cajas);
        }

        public override int GetHashCode()
        {
            int hash = playerPos.GetHashCode();
            foreach (var b in cajas.OrderBy(b => b.x * 100 + b.y))
                hash ^= b.GetHashCode();
            return hash;
        }
    }

    bool Solve(Vector2Int playerStart, List<Vector2Int> boxesStart, List<Vector2Int> goals,
               int width, int height, List<Vector2Int> walls, out int movesCount)
    {
        movesCount = 0;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        HashSet<State> visited = new HashSet<State>();
        Queue<State> queue = new Queue<State>();

        State start = new State(playerStart, boxesStart, 0);
        queue.Enqueue(start);
        visited.Add(start);

        bool IsInside(Vector2Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
        bool IsWall(Vector2Int p) => walls.Contains(p);
        bool IsBlocked(Vector2Int p, HashSet<Vector2Int> boxes) => IsWall(p) || boxes.Contains(p);

        while (queue.Count > 0)
        {
            State current = queue.Dequeue();

         
            if (goals.All(g => current.cajas.Contains(g)))
            {
                movesCount = current.movimientos;
                return true;
            }

            foreach (var dir in dirs)
            {
                Vector2Int nextPlayer = current.playerPos + dir;

                if (!IsBlocked(nextPlayer, current.cajas))
                {
                    State nextState = new State(nextPlayer, current.cajas, current.movimientos);
                    if (!visited.Contains(nextState))
                    {
                        visited.Add(nextState);
                        queue.Enqueue(nextState);
                    }
                }
              
                else if (current.cajas.Contains(nextPlayer))
                {
                    Vector2Int boxDest = nextPlayer + dir;
                    if (IsInside(boxDest) && !IsBlocked(boxDest, current.cajas))
                    {
                        HashSet<Vector2Int> newBoxes = new HashSet<Vector2Int>(current.cajas);
                        newBoxes.Remove(nextPlayer);
                        newBoxes.Add(boxDest);

                        State nextState = new State(nextPlayer, newBoxes, current.movimientos + 1); 
                        if (!visited.Contains(nextState))
                        {
                            visited.Add(nextState);
                            queue.Enqueue(nextState);
                        }
                    }
                }
            }

            movesCount = current.movimientos; 
        }

        return false; 
    }
}
