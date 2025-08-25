
using UnityEngine;

public abstract class TerrainAgent 
{
    public int x, y; // posici�n actual del agente en el mapa

    // Acci�n que define cada agente concreto
    public abstract void Step(float[,] heights);

    // Movimiento aleatorio estilo "drunk agent"
    protected void Move(int size)
    {
        x = Mathf.Clamp(x + Random.Range(-1, 2), 0, size - 1);
        y = Mathf.Clamp(y + Random.Range(-1, 2), 0, size - 1);
    }




}
