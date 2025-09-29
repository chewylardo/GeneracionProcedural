using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Backward : MonoBehaviour
{   
    //la tipica largo y alto
    public int largo = 10;
    public int alto = 10;

    public int numObjetivos = 3;
    public int stepsBack = 10; // esto hay que manejarlo bien sino se pega unity
    public int numMurallasInternas = 2;

    public GameObject PrefabMuro;
    public GameObject PrefabPiso;
    public GameObject PrefabCaja;
    public GameObject PrefabObjetivo;
    public GameObject PrefabJugador;

    public List<Vector2Int> objetivos = new List<Vector2Int>();
    public List<Vector2Int> cajas = new List<Vector2Int>();
    public List<Vector2Int> MurallasInternas = new List<Vector2Int>();
    public Vector2Int playerPos;

    private Quaternion RotarPiso = Quaternion.Euler(90, 0, 0);

    private Vector3 GetWorldPos(int x, int y) => new Vector3(x, 0, -y);

    void Start()
    {
        StartCoroutine(CorrutinaBackwardMap());
    }

    System.Collections.IEnumerator CorrutinaBackwardMap()
    {
        //aqui se ponen los muros y el piso
        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < largo; x++)
            {
                Vector3 pos = GetWorldPos(x, y);
                Instantiate(PrefabPiso, pos, RotarPiso);
                if (x == 0 || y == 0 || x == largo - 1 || y == alto - 1)
                {
                    Instantiate(PrefabMuro, pos, Quaternion.identity);
                }
                    
            }
        }

        yield return null; // permite que Unity refresque

        objetivos.Clear();
        cajas.Clear();
        int goalAttempts = 0;
        //de aqui a abajo cajas y objetivos 
        while (objetivos.Count < numObjetivos && goalAttempts < numObjetivos * 50)
        {
            Vector2Int pos = new Vector2Int(Random.Range(1, largo - 1), Random.Range(1, alto - 1));
            if (!objetivos.Contains(pos))
            {
                objetivos.Add(pos);

                cajas.Add(pos);
            }
            goalAttempts++;
        }

        if (objetivos.Count == 0)
        {
            Debug.LogError("No se pudieron colocar metas.");

            yield break;
        }

        //se setupea lo que seria la posicion inicial del player
        Vector2Int[] adj = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        playerPos = cajas[0];

        foreach (var dir in adj)
        {
            Vector2Int p = cajas[0] + dir;
            if (EstaAdentro(p) && !EstaBloqueada(p)) { playerPos = p; break; }
        }

        // aqui se ponen los muros extra que estan dentro del mapa
        MurallasInternas.Clear();
        int MurosPuestos = 0;

        int intentos = 0;

        int maxAttempts = numMurallasInternas * 10;

        while (MurosPuestos < numMurallasInternas && intentos < maxAttempts)
        {
            Vector2Int pos = new Vector2Int(Random.Range(1, largo - 1), Random.Range(1, alto - 1));

            if (cajas.Contains(pos) || objetivos.Contains(pos) || MurallasInternas.Contains(pos)) { 

                intentos++; continue; 
            }
            
            if (Vector2Int.Distance(pos, playerPos) <= 1) { 

                intentos++; continue; 
            }

            MurallasInternas.Add(pos);
            Instantiate(PrefabMuro, GetWorldPos(pos.x, pos.y), Quaternion.identity);
            MurosPuestos++;
            intentos++;
        }

        yield return null;

        //Esto es lo importante, aqui es donde parte el "Backwards from global state"
        for (int i = 0; i < stepsBack; i++)
        {
            for (int j = 0; j < cajas.Count; j++)
                InterntarBackward(j);
            yield return null; // evita congelar Unity, esto si no se muere
        }

      //Instanciar todo lo que queda, como metas y jugador , por lo de justo antes derrepente se demora en cargar
        foreach (var g in objetivos)
        {
            Instantiate(PrefabObjetivo, GetWorldPos(g.x, g.y), RotarPiso);
        }
            
        foreach (var b in cajas)
        {
            
            Instantiate(PrefabCaja, GetWorldPos(b.x, b.y), Quaternion.identity);
        }
          
        Instantiate(PrefabJugador, GetWorldPos(playerPos.x, playerPos.y), Quaternion.identity);
    }

    void InterntarBackward(int index)
    {
        Vector2Int boxPos = cajas[index]; // aqui se guarda la posicion actual de al caja
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };//todas las direcciones a las cuales se puede mover
        dirs = dirs.OrderBy(d => Random.value).ToArray();//y aqui se revuelven para que todas las cajas no se muevan igual


        //aqui se calcula la posicion previa de cada caja en caso de que no se pueda mover asi devolverla al mismo lugar
        //y se ve si hay una posicion del jugador posible que haya realizado el movimiento de la caja
        foreach (var dir in dirs)
        {
            Vector2Int preBox = boxPos - dir;
            Vector2Int prePlayer = boxPos - dir * 2;

            //esto es para chequear si es valida cada opcion
            if (!EstaAdentro(preBox) || !EstaAdentro(prePlayer)) {
                
                continue;
            }
            if (EsUnaPared(preBox) || EsUnaPared(prePlayer))
            {

                continue;
            }
            if (cajas.Contains(preBox) || cajas.Contains(prePlayer))
            {

                continue;
            }
            if (NoSePuedeMover(preBox))
            {

                continue;
            }

            cajas[index] = preBox;
            playerPos = prePlayer;
            return;
        }
    }

    bool EstaAdentro(Vector2Int p) => p.x >= 1 && p.x <= largo - 2 && p.y >= 1 && p.y <= alto - 2;
    bool EsUnaPared(Vector2Int p) => (p.x == 0 || p.y == 0 || p.x == largo - 1 || p.y == alto - 1 || MurallasInternas.Contains(p));
    bool EstaBloqueada(Vector2Int p) => EsUnaPared(p) || cajas.Contains(p);

    bool NoSePuedeMover(Vector2Int pos)
    {
        if (objetivos.Contains(pos)) { return false; }
        if (EstaBloqueada(pos + Vector2Int.up) && EstaBloqueada(pos + Vector2Int.left)) { return true; }
        if (EstaBloqueada(pos + Vector2Int.up) && EstaBloqueada(pos + Vector2Int.right)) { return true; }
        if (EstaBloqueada(pos + Vector2Int.down) && EstaBloqueada(pos + Vector2Int.left)) { return true; }
        if (EstaBloqueada(pos + Vector2Int.down) && EstaBloqueada(pos + Vector2Int.right)) { return true; }

        Vector2Int[] offs = { Vector2Int.zero, Vector2Int.left, Vector2Int.down, Vector2Int.down + Vector2Int.left };
        foreach (var off in offs)
        {
            Vector2Int a = pos + off;
            Vector2Int b = a + Vector2Int.right;
            Vector2Int c = a + Vector2Int.up;
            Vector2Int d = a + Vector2Int.right + Vector2Int.up;
            if (EstaBloqueada(a) && EstaBloqueada(b) && EstaBloqueada(c) && EstaBloqueada(d)) { 
                
                return true;
            }
        }

        return false;
    }
}
