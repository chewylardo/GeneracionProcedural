using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private void Start()
    {
        Iterations.text = iterationes.ToString();

        if(state)
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

    // Borra todos los árboles y limpia referencias
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

        seed = Random.Range(0, 99999); // Genera semilla aleatoria
        Random.InitState(seed);

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

    // Regenera todos los árboles, dungeon y Terreno
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
