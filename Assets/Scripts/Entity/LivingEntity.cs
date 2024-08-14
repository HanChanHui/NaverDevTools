using System.Collections;
using UnityEngine;

public class LivingEntity : MonoBehaviour
{
    protected float health;
    protected float maxHealth;
    protected bool dead;
    public bool IsDead { get { return dead; } }
    public float MaxHealth { get { return maxHealth; } }

    public void ResetHealth() {
        dead = false;
        health = maxHealth;
    }

    public virtual void SetHealth(int health) {
        this.maxHealth = health;
        this.health = health;
    }

    public virtual void TakeDamage(float damage, int obstacleDamage = 1, bool isCritical = false, bool showLabel = false) {
        health -= damage;

        if (health <= 0 && !dead) {
            health = 0;
            dead = true;
        }
    }

    public virtual void TakeDamage(Collider c, Vector3 hitPoint, int damage, int obstacleDamage = 1, bool isCritical = false, bool showLabel = false) {
        TakeDamage(damage, obstacleDamage, isCritical, showLabel);
    }
}
