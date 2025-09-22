using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyParameters
{
    public float speed;
    public float aggression; // 0 a 1
    public float visionRange;
    public float fitness;

    public EnemyParameters Clone()
    {
        return new EnemyParameters
        {
            speed = this.speed,
            aggression = this.aggression,
            visionRange = this.visionRange,
            fitness = this.fitness
        };
    }
}

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    public EnemyParameters parameters;
    private Vector3 goal;
    private List<Transform> towers = new List<Transform>();
    private Rigidbody rb;

    // combate
    public float attackRange = 5f;
    public float attackCooldown = 1.0f;
    private float attackTimer = 0f;
    public float damage = 10f;
    public float health = 30f;

    private bool wantsToAttack = false;

    // obstáculo
    public float obstacleCheckDistance = 1.5f;
    public float obstacleRadius = 0.5f; // radio del SphereCast
    public LayerMask obstacleMask;

    // movimiento
    private Vector3 moveDir = Vector3.zero;
    private bool avoidingObstacle = false;
    private Vector3 obstacleFollowDir;
    private float obstacleAvoidTimer = 0f; // tiempo mínimo antes de chequear de nuevo
    public float avoidCooldown = 0.2f; // medio segundo antes de cambiar de dirección

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void Setup(EnemyParameters p, Vector3 goalPos, List<Transform> towersList)
    {
        parameters = p.Clone();
        goal = goalPos;
        towers = towersList;
    }

    void FixedUpdate()
    {
        Vector3 enemyPos = transform.position;

        // --- Dirección hacia objetivo ---
        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var t in towers)
        {
            if (t == null) continue;
            Collider col = t.GetComponent<Collider>();
            if (col == null) continue;

            float d = Vector3.Distance(enemyPos, col.ClosestPoint(enemyPos));
            if (d < nearestDist)
            {
                nearestDist = d;
                nearest = t;
            }
        }

        Vector3 targetDir;
        if (nearest != null && nearestDist <= parameters.visionRange)
        {
            if (!wantsToAttack)
                wantsToAttack = Random.value < parameters.aggression;

            if (wantsToAttack)
            {
                if (nearestDist <= attackRange && TryAttackTower(nearest, nearestDist))
                    return;

                Collider col = nearest.GetComponent<Collider>();
                Vector3 targetPoint = (col != null) ? col.ClosestPoint(enemyPos) : nearest.position;
                targetDir = (targetPoint - enemyPos).normalized;
            }
            else
            {
                targetDir = (goal - enemyPos).normalized;
            }
        }
        else
        {
            wantsToAttack = false;
            targetDir = (goal - enemyPos).normalized;
        }

        if (moveDir == Vector3.zero)
            moveDir = targetDir;

        RaycastHit hit;
        // --- SphereCast para detectar obstáculos ---
        if (Physics.SphereCast(enemyPos, obstacleRadius, moveDir, out hit, obstacleCheckDistance, obstacleMask))
        {
            if (!avoidingObstacle)
            {
                avoidingObstacle = true;
                obstacleAvoidTimer = avoidCooldown;
                // Elegimos dirección de giro según normal
                obstacleFollowDir = Vector3.Cross(Vector3.up, hit.normal).normalized;
                moveDir = obstacleFollowDir;
            }
            else
            {
                // Mientras evitamos, decrementamos el timer
                obstacleAvoidTimer -= Time.fixedDeltaTime;
                if (obstacleAvoidTimer <= 0f)
                {
                    // Solo chequeamos giro si ya pasó el cooldown
                    if (Physics.SphereCast(enemyPos, obstacleRadius, moveDir, out hit, obstacleCheckDistance, obstacleMask))
                    {
                        moveDir = Vector3.Cross(Vector3.up, hit.normal).normalized;
                    }
                    obstacleAvoidTimer = avoidCooldown;
                }
            }
        }
        else
        {
            // despejado
            avoidingObstacle = false;
            moveDir = targetDir;
        }


        // --- Movimiento ---
        rb.MovePosition(rb.position + moveDir * parameters.speed * Time.fixedDeltaTime);

        if (Vector3.Distance(enemyPos, goal) < 0.7f)
            Destroy(gameObject);
    }

    private bool TryAttackTower(Transform tower, float distance)
    {
        if (tower == null) return false;

        Vector3 enemyPos = transform.position;
        Collider col = tower.GetComponent<Collider>();
        if (col != null)
            distance = Vector3.Distance(enemyPos, col.ClosestPoint(enemyPos));

        if (distance <= attackRange)
        {
            attackTimer -= Time.fixedDeltaTime;
            if (attackTimer <= 0f)
            {
                attackTimer = attackCooldown;
                var tw = tower.GetComponent<SimpleTower>();
                if (tw != null)
                {
                    tw.TakeDamage(damage);
                    Debug.Log($"{gameObject.name} hizo {damage} de daño a {tw.gameObject.name}");
                }
            }
            return true;
        }
        return false;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f) Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (parameters != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, parameters.visionRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + moveDir.normalized * obstacleCheckDistance);
        }
    }
}
