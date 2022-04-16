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

    protected virtual void Awake()
    {
        spriteAnim = GetComponent<SpriteAnim>();
    }

    // For enemies slash
    public virtual void Hurt(int damage, int weakpoint, bool fullSlash)
    {
        // moved modifiers to gamemanager to calc the damage beforehand
        Hurt(damage);
    }

    public virtual void Hurt(int damage)
    {
        // for debug
        GameManager.Instance.lastDamage = damage;

        health -= damage;
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
