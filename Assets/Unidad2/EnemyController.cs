using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyParametersAi
{
    public float speed;       // Velocidad de movimiento
    public float aggression;  // Probabilidad de elegir caminos arriesgados
    public float visionRange; // Distancia que puede “ver” para evitar muros
    public float fitness;     // Qué tan bien escapó

    public EnemyParametersAi Clone() 
    {
        return new EnemyParametersAi
        {
            speed = this.speed,
            aggression = this.aggression,
            visionRange = this.visionRange,
            fitness = this.fitness
        };
    }
}

public class EnemyController : MonoBehaviour
{
    public EnemyParametersAi parameters; // parámetros de este enemigo
    public Transform exitPoint;
    public float maxTime = 10f; // tiempo máximo de simulación

    public float fitness; // puntaje de desempeño
    public bool finished = false;

    private Rigidbody rb;
    private float elapsedTime = 0f; // tiempo transcurrido
    private Vector3 targetDirection;

    private Vector3[] directions = new Vector3[8]; // posibles direcciones de exploración 

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Sensores: N, NE, E, SE, S, SW, W, NW
        directions[0] = Vector3.forward;
        directions[1] = (Vector3.forward + Vector3.right).normalized;
        directions[2] = Vector3.right;
        directions[3] = (Vector3.back + Vector3.right).normalized;
        directions[4] = Vector3.back;
        directions[5] = (Vector3.back + Vector3.left).normalized;
        directions[6] = Vector3.left;
        directions[7] = (Vector3.forward + Vector3.left).normalized;
    }

    public void Initialize(EnemyParametersAi parametersAi, Transform exit, float maxTime)
    {
        parameters = parametersAi;  // asigna parámetros del genoma
        exitPoint = exit;
        this.maxTime = maxTime;
        ChooseNewDirection();    // elige dirección inicial
    }

    void FixedUpdate()
    {
        if (finished) return;

        elapsedTime += Time.fixedDeltaTime;

        // Mover usando Rigidbody
        rb.MovePosition(rb.position + targetDirection * parameters.speed * Time.fixedDeltaTime);

        // Rotar suavemente hacia la dirección
        if (targetDirection != Vector3.zero)
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(targetDirection), 0.2f));

        // Evitar muros
        if (!IsDirectionFree(targetDirection))
            ChooseNewDirection();

        // Calcular fitness
        float distanceToExit = Vector3.Distance(rb.position, exitPoint.position);
        fitness = 1f / (distanceToExit + 1f);

        // Si llegó a la salida
        if (distanceToExit < 1f)
        {
            fitness = 1000f / (elapsedTime + 1f);
            finished = true;
            Destroy(gameObject);
            return;
        }

        // Si se acabó el tiempo
        if (elapsedTime >= maxTime)
        {
            finished = true;
            Destroy(gameObject);
            return;
        }
    }

    bool IsDirectionFree(Vector3 dir)
    {
        float rayDistance = parameters.visionRange + parameters.speed * Time.fixedDeltaTime;
        Vector3 rayOrigin = rb.position + Vector3.up * 0.5f; // elevar un poco para evitar suelo
        return !Physics.Raycast(rayOrigin, dir, rayDistance);   // true si no hay muro en esa dirección
    }

    void ChooseNewDirection()
    {
        List<Vector3> freeDirections = new List<Vector3>();
        foreach (var dir in directions)
        {
            if (IsDirectionFree(dir))
                freeDirections.Add(dir);    // guardar direcciones libres
        }

        if (freeDirections.Count > 0)
        {
            // Elegir dirección que minimice distancia a salida
            Vector3 bestDir = freeDirections[0];
            float minDist = Vector3.Distance(rb.position + bestDir, exitPoint.position);

            foreach (var dir in freeDirections)
            {
                float dist = Vector3.Distance(rb.position + dir, exitPoint.position);
                if (dist < minDist)
                {
                    bestDir = dir;
                    minDist = dist;
                }
            }

            // Elegir dirección según aggression:
            // - aggression bajo = más aleatorio
            // - aggression alto = más "inteligente" hacia la salida
            if (Random.value > parameters.aggression) // cuanto mayor aggression, menos aleatorio
                targetDirection = freeDirections[Random.Range(0, freeDirections.Count)]; // camino aleatorio
            else
                targetDirection = bestDir; // camino inteligente hacia la salida

        }
        else
        {
            // Si está completamente rodeado, girar aleatoriamente 90-180°
            targetDirection = Quaternion.Euler(0, Random.Range(90, 180), 0) * targetDirection;
        }
    }
}