using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManagerProy2 : MonoBehaviour
{
    private bool hasInit = false;
    public Transform algoritmos;

    [Header("Objetos")]
    public GameObject PanelBase;
    public GameObject BtnMaximize;
    public GameObject InputField;

    [Header("Textos")]
    public TextMeshProUGUI TxtSeed;

    [Header("Referencias")]
    public Backward backward;
    public HillClimbingManager hillclimbing;

    [Header("Lista de Paneles en orden")]
    public List<GameObject> paneles;  
    private int indiceActual = 0;

    void Start()
    {
        // Al iniciar, mostrar solo el primer panel
        ActualizarPaneles();
    }

    // Muestra el panel siguiente (si existe)
    public void MostrarSiguiente()
    {
        if (indiceActual < paneles.Count - 1)
        {
            indiceActual++;
        }
        else
        {
            // Si ya estamos en el último, vuelve al primero
            indiceActual = 0;
        }
        ActualizarPaneles();
    }

    // Muestra el panel anterior (si existe)
    public void MostrarAnterior()
    {
        if (indiceActual > 0)
        {
            indiceActual--;
        }
        else
        {
            // Si estamos en el primero, ir al último 
            indiceActual = paneles.Count - 1;
        }
        ActualizarPaneles();
    }

    // Muestra el panel actual y desactiva los demás
    private void ActualizarPaneles()
    {
        for (int i = 0; i < paneles.Count; i++)
        {
            paneles[i].SetActive(i == indiceActual);
        }
    }
    public void Generate()
    {
        if (hasInit)
        {
            DestroyAllChildren(algoritmos);
        }
        backward.Init();

        hasInit = true;
    }

    //borrar mapa 
    public static void DestroyAllChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    public void SetSeed(string seed)
    {
        if(int.Parse(seed) == -1)
            TxtSeed.text = $"Seed: {seed}";

        backward.seed = int.Parse(seed);
        hillclimbing.seed = int.Parse(seed);
    }
    
    public void Minimize()
    {
        PanelBase.SetActive(false);
        BtnMaximize.SetActive(true);
        InputField.SetActive(false);
    }
    public void Maximize()
    {
        PanelBase.SetActive(true);
        BtnMaximize.SetActive(false);
        InputField.SetActive(true);
    }
}
