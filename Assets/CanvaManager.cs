using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CanvaManager : MonoBehaviour
{
    public GameObject panelArboles;
    public GameObject panelDungeon;
    public GameObject panelTerreno;
    // Mostrar el panel
    public void MostrarPanelArboles()
    {
        panelArboles.SetActive(true);
    }

    // Ocultar el panel
    public void OcultarPanelArboles()
    {
        panelArboles.SetActive(false);
    }

    public void MostrarPanelDungeon()
    {
        panelDungeon.SetActive(true);
    }

    // Ocultar el panel
    public void OcultarPanelDungeon()
    {
        panelDungeon.SetActive(false);
    }

    public void MostrarPanelTerreno()
    {
        panelTerreno.SetActive(true);
    }

    // Ocultar el panel
    public void OcultarPanelTerreno()
    {
        panelTerreno.SetActive(false);
    }




}
