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
    public virtual void Hurt(float damage, int weakpoint, bool fullSlash)
    {
        float dmg = damage;
        float fullSlashMultiplyer = 0;

        if (fullSlash)
        {
            fullSlashMultiplyer = 0.3f;
        }

        dmg *= 1 + (WeakpointMultiplyer(weakpoint) + fullSlashMultiplyer);

        Hurt(dmg);
    }

    // For enemies stab
    public virtual void Hurt(float damage, int weakpoint)
    {
        float dmg = damage;

        dmg *= 1 + WeakpointMultiplyer(weakpoint);

        Hurt(dmg);
    }

    public virtual void Hurt(float damage)
    {
        var dmg = damage;
        dmg += Random.Range(-GameManager.damageVariance / 2, GameManager.damageVariance / 2) * dmg;
        int intDmg = Mathf.RoundToInt(dmg);

        // for debug
        GameManager.Instance.lastDamage = intDmg;

        health -= intDmg;
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
