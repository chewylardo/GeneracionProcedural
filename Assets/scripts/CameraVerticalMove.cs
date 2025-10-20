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
    public float minX = 10f;
    public float maxX = 30f;

    void Update()
    {
        float moveY = 0f;
        float moveX = 0f;

        // Detecta teclas de flecha arriba / abajo
        if (Input.GetKey(KeyCode.UpArrow))
        {
            moveY = 1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            moveY = -1f;
        }
        else if(Input.GetKey(KeyCode.LeftArrow))
        { 
            moveX = 1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            moveX = -1f;
        }

        // Calcula nueva posición
        Vector3 newPosition = transform.position + new Vector3(moveX, moveY * speed * Time.deltaTime, 0);

        // Aplica límites si están definidos
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

        // Mueve la cámara
        transform.position = newPosition;
    }
}
