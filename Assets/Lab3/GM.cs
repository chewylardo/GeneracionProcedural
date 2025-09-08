using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GM : MonoBehaviour
{
    public string TxtSeed;
    public bool RandomSeed;

    public int seed = 12345;

    public GameObject ObjInputfield;    //obj input para la seed
    public TMP_InputField inputField;   //input seed

    public TMP_Text txtState;
    public bool state = false;

    [Header("Referencias")]
    public List<LSystemPlant> allTrees = new List<LSystemPlant>();
    public TreeDistributor TreeDistributor;
    public DrunkenAgent DrunkenAgent;
    public MapManager MapManager;
    public DSterrain DSterrain;

    [Header("Arboles")]
    public TMP_Text Iterations;
    public int iterationes = 2;
    public TMP_InputField quantity;

    [Header("Terreno")]
    public TMP_InputField NoiseTxt;
    public TMP_InputField AlturaTxt;

    [Header("Dungeon")]
    public TMP_InputField Pr;
    public TMP_InputField Pc;
    public TMP_InputField ProbRoom;


    private void Start()
    {
        Iterations.text = iterationes.ToString();

        if (state)
            txtState.text = "X";
        else
            txtState.text = "";

        ObjInputfield.SetActive(!state);
    }

    // Agrega todos los LSystemPlant en la escena a la lista
    public void AddTrees()
    {
        allTrees.Clear();
        allTrees.AddRange(FindObjectsOfType<LSystemPlant>());
    }

    // Borra todos los �rboles y limpia referencias
    public void DeleteTrees()
    {
        foreach (var tree in allTrees)
        {
            if (tree != null)
            {
                Destroy(tree.gameObject);
            }
        }

        allTrees.Clear();
    }

    // Aplica la semilla escrita en el inputField
    public void Seed()
    {
        if (inputField.text == inputField.text) return;

        foreach (var tree in allTrees)
        {
            if (tree != null)
                tree.TextToSeed(inputField.text);
        }

        TreeDistributor.TextToSeed(inputField.text);

        DrunkenAgent.TextToSeed(inputField.text);

        DSterrain.TextToSeed(inputField.text);
    }

    // Cambia el estado del toggle y aplica semilla aleatoria
    public void State()
    {
        state = !state;

        if (state)
            txtState.text = "X";
        else
            txtState.text = "";

        Debug.Log($"estado {state}");
        ObjInputfield.SetActive(!state);

        seed = UnityEngine.Random.Range(0, 99999); // Genera semilla aleatoria
        UnityEngine.Random.InitState(seed);

        foreach (var tree in allTrees)
        {
            if (tree != null)
            {
                tree.RandomSeed(state);
                tree.seed = seed;
            }
        }

        TreeDistributor.RandomSeed(state);
        TreeDistributor.seed = seed;

        DrunkenAgent.RandomSeed(state);
        DrunkenAgent.seed = seed;

        DSterrain.RandomSeed(state);
        DSterrain.seed = seed;
    }

    public void AddIteration()
    {
        TreeDistributor.addIteracion();
        iterationes++;
        Iterations.text = iterationes.ToString();
    }
    public void RemoveIteration()
    {
        TreeDistributor.RemoveIteration();
        iterationes--;

        if (iterationes <= 0)
            iterationes = 1;

        Iterations.text = iterationes.ToString();
    }

    public void CountTrees()
    {
        TreeDistributor.CountTrees(quantity.text);
    }

    public void SetAltura()
    {
        float altura = float.Parse(AlturaTxt.text); 
      
        DSterrain.SetAltura(altura);
    }
    public void SetNoise()
    {
        float noise = float.Parse(NoiseTxt.text)/10;

        DSterrain.SetNoise(noise);
    }

    public void SetPr()
    {
        int probCamino = Convert.ToInt32(Pr);
        DrunkenAgent.SetPr(probCamino);
    }
    public void SetPc()
    {
        int probRoom = Convert.ToInt32(Pc);
        DrunkenAgent.SetPc(probRoom);
    }
    public void SetPercetRoom()
    {
        float percentRoom = float.Parse(ProbRoom.text)/10;
        
        DrunkenAgent.SetProbRoom(percentRoom);
    }

    // Regenera todos los �rboles, dungeon y Terreno
    public void Regenerate()
    {
        foreach (var tree in allTrees)
        {
            if (tree != null)
                tree.RegenerateTree();
        }
        MapManager.ReiniciarMapa();

        DSterrain.GenerateTerrain();

        DeleteTrees();
        TreeDistributor.DistributeTrees();
    }
}