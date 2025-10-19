using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManagerProy2 : MonoBehaviour
{
    private bool hasInit = false;
    private bool random = true;
    public Transform algoritmos;

    [Header("Objetos")]
    public GameObject PanelBase;
    public GameObject BtnMaximize;
    public GameObject BtnInputField;
    public TextMeshProUGUI isRandom;

    [Header("Textos")]
    public TextMeshProUGUI TxtSeed;

    [Header("Referencias")]
    public Backward backward;
    public HillClimbingManager hillclimbing;
    public GeneticPipeline gaPipeline;

    [Header("Lista de Paneles en orden")]
    public List<GameObject> paneles;
    private int indiceActual = 0;

    void Start()
    {
        ActualizarPaneles();
    }

    public void MostrarSiguiente()
    {
        if (indiceActual < paneles.Count - 1) indiceActual++;
        else indiceActual = 0;
        ActualizarPaneles();
    }

    public void MostrarAnterior()
    {
        if (indiceActual > 0) indiceActual--;
        else indiceActual = paneles.Count - 1;
        ActualizarPaneles();
    }

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
            backward.generado = false;
            hillclimbing.CleanTxt();
        }

        if (random)
        {
            backward.seed = -1;
            hillclimbing.seed = -1;
            gaPipeline.seed = -1;
        }

        StartCoroutine(PipelineCoroutine());

        hasInit = true;
    }

    private IEnumerator PipelineCoroutine()
    {
        // 1️⃣ Backward
        Debug.Log("[Manager] Corriendo Backward");
        if (backward.seed == -1) backward.seed = Random.Range(0, int.MaxValue);
        gaPipeline.seed = backward.seed;
        hillclimbing.seed = backward.seed;

        backward.Init();
        yield return new WaitUntil(() => backward.generado);

        // 2️⃣ Genetic Algorithm
        Debug.Log("[Manager] Corriendo GA");
        gaPipeline.InitPopulationFromBackward(backward.GenerarMapData());
        yield return StartCoroutine(gaPipeline.RunGACoroutine());

        // 3️⃣ Hill Climbing sobre el mejor de GA
        Debug.Log("[Manager] Corriendo HillClimb");
        MapData bestGA = gaPipeline.EliteToMapData();
        yield return StartCoroutine(hillclimbing.RunHillClimbing(bestGA));
    }

    public static void DestroyAllChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    public void SetSeed(string seed)
    {
        int s = int.Parse(seed);
        if (s != -1) TxtSeed.text = $"Seed: {s}";
        backward.seed = s;
        hillclimbing.seed = s;
        gaPipeline.seed = s;
    }

    public void Minimize()
    {
        PanelBase.SetActive(false);
        BtnMaximize.SetActive(true);
    }

    public void Maximize()
    {
        PanelBase.SetActive(true);
        BtnMaximize.SetActive(false);
    }

    public void IsRandom()
    {
        random = !random;
        if (random)
        {
            isRandom.text = "X";
            BtnInputField.SetActive(false);

            int newSeed = Random.Range(0, int.MaxValue);
            backward.seed = newSeed;
            hillclimbing.seed = newSeed;
            gaPipeline.seed = newSeed;

            TxtSeed.text = $"Seed: {newSeed}";
        }
        else
        {
            isRandom.text = "";
            BtnInputField.SetActive(true);
        }
    }
}
