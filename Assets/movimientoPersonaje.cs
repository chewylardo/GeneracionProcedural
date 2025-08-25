using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Captura la entrada del teclado
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D o flechas izquierda/derecha
        movement.y = Input.GetAxisRaw("Vertical");   // W/S o flechas arriba/abajo
    }

    void FixedUpdate()
    {
        // Mueve al personaje usando Rigidbody2D
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
