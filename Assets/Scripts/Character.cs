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
    protected SpriteAnimNodes nodes;
    [SerializeField] protected HealthBar healthBar;
    [SerializeField] protected AudioManager soundEffectManager;

    protected virtual void Awake()
    {
        spriteAnim = GetComponent<SpriteAnim>();
        nodes = GetComponent<SpriteAnimNodes>();
        if (healthBar == null)
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
    public virtual void Hurt(float damage, int weakpoint, bool fullSlash, Vector2 location, int dir)
    {
        float dmg = damage;
        float fullSlashMultiplyer = 0;

        if (fullSlash)
        {
            fullSlashMultiplyer = 0.3f;
        }
        dmg *= 1 + fullSlashMultiplyer;
        dmg *= 1 + (WeakpointMultiplyer(weakpoint));
        int intDmg = GameManager.Instance.DamageVariance(dmg);
        Hurt(intDmg);
        DmgNumManager.Instance.CreateDmg(location, dir, intDmg);
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

    protected virtual float WeakpointMultiplyer(int weakpoint)
    {
        if (weakpoint < 1)
        {
            return 0;
        }

        switch (weakpoint)
        {
            case 1:
                return 0.3f;

            case 2:
                return 0.75f;

            case 3:
                return 1.5f;
        }
        if (weakpoint > 3)
        {
            Debug.Log("More weakpoints than max");
            return 1.5f;
        }

        return 0;
    }
}
