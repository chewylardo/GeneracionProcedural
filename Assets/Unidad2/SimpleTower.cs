using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SimpleTower : MonoBehaviour
{
    public float maxHealth = 100f;
    public float health;
    public float damageTaken = 0f; // cumulative damage received (for fitness)

    void Awake() { ResetHealth(); }
    public void ResetHealth() { health = maxHealth; damageTaken = 0f; }

    public void TakeDamage(float amount)
    {
        damageTaken += amount;
        health -= amount;
        if (health <= 0f) Destroy(gameObject);
    }
}
