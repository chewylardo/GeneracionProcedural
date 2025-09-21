using UnityEngine;

public class TimeControl : MonoBehaviour
{
    [Header("Velocidad de tiempo")]
    public float fastForwardScale = 3f;  // Velocidad aumentada
    private float normalScale = 1f;      // Tiempo normal

    void Update()
    {
        // Al pulsar X, aumentar la velocidad del tiempo
        if (Input.GetKeyDown(KeyCode.X))
        {
            Time.timeScale = fastForwardScale;
        }

        // Al pulsar Z, regresar al tiempo normal
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Time.timeScale = normalScale;
        }
    }
}
