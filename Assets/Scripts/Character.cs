using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PowerTools;

public class Character : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int health;
    protected bool dead = false;

    // animations
    protected SpriteAnim spriteAnim;
    protected HealthBar healthBar;

    protected virtual void Awake()
    {
        spriteAnim = GetComponent<SpriteAnim>();
        healthBar = GetComponentInChildren<HealthBar>();
    }

    protected virtual void Start()
    {
        if (healthBar != null)
        {
            healthBar.SetHealth(health);
        }
    }

    // For enemies slash
    public virtual void Hurt(int damage, int weakpoint, bool fullSlash)
    {
        // moved modifiers to gamemanager to calc the damage beforehand, this is just for future effects
        Hurt(damage);
    }

    public virtual void Hurt(int damage)
    {
        health -= damage;
        if (healthBar != null)
        {
            healthBar.ChangeHealth(health);
        }
        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        dead = true;
        Debug.Log("Dead");
    }
}
