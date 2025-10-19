using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class Backward : MonoBehaviour
{
    [Header("Dimensiones del mapa")]
    public int largo = 10;
    public int alto = 10;

    [Header("Parámetros de generación")]
    public int numObjetivos = 3;
    public int stepsBack = 10;
    public int numMurallasInternas = 2;

    [Header("Referencias")]
    public MapVisualizer visualizer;

    [Header("Salida de datos")]
    public List<Vector2Int> objetivos = new List<Vector2Int>();
    public List<Vector2Int> cajas = new List<Vector2Int>();
    public List<Vector2Int> MurallasInternas = new List<Vector2Int>();
    public Vector2Int playerPos;

    public bool generado = false; // Flag para saber cuándo terminó

    [Header("Textos")]
    public TextMeshProUGUI TxtTamaño;
    public TextMeshProUGUI TxtObjetivos;
    public TextMeshProUGUI TxtMuros;
    public TextMeshProUGUI TxtStepbacks;

    [Header("Seed (reproducibilidad)")]
    public int seed = -1;

    public void Init()
    {
        StartCoroutine(CorrutinaBackwardMap());
    }

    System.Collections.IEnumerator CorrutinaBackwardMap()
    {
        // Si la seed es -1, generamos una aleatoria
        if (seed == -1)
        {
            seed = Random.Range(0, int.MaxValue);
        }

        Debug.Log($"[Backward] Usando seed = {seed}");
        Random.InitState(seed);

        objetivos.Clear();
        cajas.Clear();
        MurallasInternas.Clear();

        // 1️ Coloca metas y cajas 
        int goalAttempts = 0;
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

        //  2️ Coloca jugador
        Vector2Int[] adj = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        playerPos = cajas[0];
        foreach (var dir in adj)
        {
            Vector2Int p = cajas[0] + dir;
            if (EstaAdentro(p) && !EstaBloqueada(p)) { playerPos = p; break; }
        }

        // 3️ Coloca muros internos 
        int MurosPuestos = 0;
        int intentos = 0;
        int maxAttempts = numMurallasInternas * 10;
        while (MurosPuestos < numMurallasInternas && intentos < maxAttempts)
        {
            Vector2Int pos = new Vector2Int(Random.Range(1, largo - 1), Random.Range(1, alto - 1));
            if (cajas.Contains(pos) || objetivos.Contains(pos) || MurallasInternas.Contains(pos)) { intentos++; continue; }
            if (Vector2Int.Distance(pos, playerPos) <= 1) { intentos++; continue; }

            MurallasInternas.Add(pos);
            MurosPuestos++;
            intentos++;
        }

        yield return null;

        // 4️ Movimiento backward (asegura solvencia) y visualización paso a paso
        for (int i = 0; i < stepsBack; i++)
        {
            for (int j = 0; j < cajas.Count; j++)
                InterntarBackward(j);

            if (visualizer != null)
            {
                visualizer.ShowMap(GenerarMapData());
                yield return new WaitForSeconds(1f); // ← pausa de 1 segundo
            }
        }

        generado = true; // Marca que terminó
    }

    void InterntarBackward(int index)
    {
        Vector2Int boxPos = cajas[index];
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        dirs = dirs.OrderBy(d => Random.value).ToArray();

        foreach (var dir in dirs)
        {
            Vector2Int preBox = boxPos - dir;
            Vector2Int prePlayer = boxPos - dir * 2;

            if (!EstaAdentro(preBox) || !EstaAdentro(prePlayer)) continue;
            if (EsUnaPared(preBox) || EsUnaPared(prePlayer)) continue;
            if (cajas.Contains(preBox) || cajas.Contains(prePlayer)) continue;
            if (NoSePuedeMover(preBox)) continue;

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
        if (EstaBloqueada(pos + Vector2Int.up) && EstaBloqueada(pos + Vector2Int.left)) 
        { 
            return true; 
        }
        if (EstaBloqueada(pos + Vector2Int.up) && EstaBloqueada(pos + Vector2Int.right))
        {
            return true;
        }
        if (EstaBloqueada(pos + Vector2Int.down) && EstaBloqueada(pos + Vector2Int.left))
        {
            return true;
        }
        if (EstaBloqueada(pos + Vector2Int.down) && EstaBloqueada(pos + Vector2Int.right))
        {
            return true;
        }

        Vector2Int[] offs = { Vector2Int.zero, Vector2Int.left, Vector2Int.down, Vector2Int.down + Vector2Int.left };
        foreach (var off in offs)
        {
            Vector2Int a = pos + off;
            Vector2Int b = a + Vector2Int.right;
            Vector2Int c = a + Vector2Int.up;
            Vector2Int d = a + Vector2Int.right + Vector2Int.up;
            if (EstaBloqueada(a) && EstaBloqueada(b) && EstaBloqueada(c) && EstaBloqueada(d))
            {
                return true;
            }
           

        }

        return false;
    }

    //exporta el mapa como estructura MapData
    public MapData GenerarMapData()
    {
        MapData data = new MapData(largo, alto);
        data.internalWalls = new List<Vector2Int>(MurallasInternas);
        data.boxes = new List<Vector2Int>(cajas);
        data.goals = new List<Vector2Int>(objetivos);
        data.playerPos = playerPos;
        return data;
    }

    public void AddTamaño()
    {
        largo++;
        alto++;
        TxtTamaño.text = $"{alto}\nTamaño del mapa";
    }
    public void SubstractTamaño()
    {
        largo--;
        alto--;
        TxtTamaño.text = $"{alto}\nTamaño del mapa";
    }
    public void AddObjetivos()
    {
        numObjetivos++;
        TxtObjetivos.text = $"{numObjetivos}\nN° de Objetivos";
    }
    public void SubstractObjetivos()
    {
        numObjetivos--;
        TxtObjetivos.text = $"{numObjetivos}\nN° de Objetivos";
    }
    public void AddMuros()
    {
        numMurallasInternas++;
        TxtMuros.text = $"{numMurallasInternas}\nN° de Murallas";
    }
    public void SubstractMuros()
    {
        numMurallasInternas--;
        TxtMuros.text = $"{numMurallasInternas}\nN° de Murallas";
    }
    public void AddStepBacks()
    {
        stepsBack++;
        TxtStepbacks.text = $"{stepsBack}\nN° de Stepbacks";
    }
    public void SubstractStepBacks()
    {
        stepsBack--;
        TxtStepbacks.text = $"{stepsBack}\nN° de Stepbacks";
    }
}
