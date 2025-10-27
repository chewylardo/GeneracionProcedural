using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WFC/Tile Data")]
public class TileData : ScriptableObject
{
    [Header("Datos del Tile")]
    public string tileName;
    public GameObject prefab;

    [Header("Compatibilidad con vecinos")]
    public List<TileData> norte; 
    public List<TileData> sur; 
    public List<TileData> este;  
    public List<TileData> oeste;  
}
