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

    public GameObject GOinputfield;
    public TMP_InputField inputField; // O InputField si no usas TextMeshPro

    public Toggle Toggle;

    // Cambiado a lista
    public List<LSystemPlant> allTrees = new List<LSystemPlant>();
    public TreeDistributor TreeDistributor;

    public bool state = true;

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
                Destroy(tree.gameObject); // Borra el objeto de la escena
            }
        }
        allTrees.Clear(); // Limpia la lista
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
    }

    // Cambia el estado del toggle y aplica semilla aleatoria
    public void State()
    {
        state = !state;

        Debug.Log($"estado {state}");
        GOinputfield.SetActive(!state);

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
    }

    // Regenera todos los árboles
    public void Regenerate()
    {
        foreach (var tree in allTrees)
        {
            if (tree != null)
                tree.RegenerateTree();
        }

        TreeDistributor.DistributeTrees();
    }
}
