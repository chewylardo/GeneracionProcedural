using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraVerticalMove : MonoBehaviour
{
    [Header("Velocidad de movimiento vertical")]
    public float speed = 5f;

    [Header("Límites opcionales (en eje Y)")]
    public float minY = 10f;
    public float maxY = 30f;

    void Update()
    {
        float moveY = 0f;

        // Detecta teclas de flecha arriba / abajo
        if (Input.GetKey(KeyCode.UpArrow))
        {
            moveY = 1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            moveY = -1f;
        }

        // Calcula nueva posición
        Vector3 newPosition = transform.position + new Vector3(0, moveY * speed * Time.deltaTime, 0);

        // Aplica límites si están definidos
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        // Mueve la cámara
        transform.position = newPosition;
    }
}
