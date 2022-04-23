using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Attack { public float damage; public AnimationClip animation; public Area[] areas; }

public class Enemy : Character
{
    [Header ("Attack Stuff")]
    [SerializeField] protected Attack[] attackArray;
    [SerializeField] protected float attackDelay;
    protected int attackIndex = 0;
    protected bool attacking = false;
    protected float attackTimeTrack = 0;

    [Header("Animations")]
    [SerializeField] protected AnimationClip idle;
    [SerializeField] protected Material hurtMat, attackMat;
    protected SpriteRenderer spriteRenderer;
    protected static float flashTime = 0.4f, defaultFlash = 0.7f;
    protected float flashTracker = 0;
    protected Material originalMat;

    // Damage receive stuff
    protected int weakpointsHit = 0, hitDirection = 0;
    protected bool fullSlash = false;
    protected Vector2 hitLocation;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMat = spriteRenderer.material;
        hurtMat.SetFloat("_FlashAmount", defaultFlash);
        attackMat.SetFloat("_FlashAmount", defaultFlash);
    }

    protected virtual void Update()
    {
        if (!attacking)
        {
            attackTimeTrack += Time.deltaTime;
            if (attackTimeTrack > attackDelay)
            {
                AttackStart();
                attackTimeTrack = 0;
            }
        }

        // Resets color for flash
        if (flashTracker > 0)
        {
            flashTracker -= Time.deltaTime;
            spriteRenderer.material.SetFloat("_FlashAmount", defaultFlash * flashTracker / flashTime);
            if (flashTracker <= 0)
            {
                spriteRenderer.material.SetFloat("_FlashAmount", defaultFlash);
                spriteRenderer.material = originalMat;
            }
        }
    }

    protected override void Die()
    {
        base.Die();
        SpawnManager.Instance.NextWave();
        Destroy(gameObject);
    }

    protected void AttackStart()
    {
        AttackStart(0);
    }

    protected void AttackStart(int index)
    {
        if (!GameManager.Instance.GetGameOver())
        {
            // Change the attackIndex beforehand
            spriteAnim.Play(attackArray[index].animation);
            attackIndex = index;
            attacking = true;
        }
    }

    protected void AttackHit()
    {
        Attack attack = attackArray[attackIndex];
        foreach (var area in attack.areas)
        {
            GameManager.Instance.AttackPlayer(attack.damage, area);
        }

        spriteRenderer.sortingLayerName = "EnemyAttack";
    }

    // Attack end triggers are handled by animation due to timing
    protected void AttackEnd()
    {
        spriteAnim.Play(idle);
        spriteRenderer.sortingLayerName = "Enemy";
        attacking = false;
        // revert hitbox
        // Add an automatic attack trigger for death and hit interrupts
    }

    protected virtual void AttackFlash()
    {
        Flash(attackMat, flashTime);
    }

    protected void Flash(Material mat, float time)
    {
        hurtMat.SetFloat("_FlashAmount", defaultFlash);
        spriteRenderer.material = mat;
        flashTracker = time;
    }

    public override void Hurt(int damage)
    {
        base.Hurt(damage);
        if (spriteRenderer.material != attackMat)
        {
            Flash(hurtMat, flashTime);
        }
    }

    public int GetWeakpointsHit()
    {
        return weakpointsHit;
    }

    public void WeakpointHit(int amount)
    {
        weakpointsHit += amount;
    }

    public void ResetWeakpoints()
    {
        weakpointsHit = 0;
    }

    public bool GetSlash()
    {
        return fullSlash;
    }

    public void FullSlashed(bool slashed)
    {
        fullSlash = slashed;
    }

    public void HitLocation(Vector2 location)
    {
        hitLocation = location;
    }

    public Vector2 GetHitLocation()
    {
        return hitLocation;
    }

    public void HitDirection(int direction)
    {
        hitDirection = direction;
    }

    public int GetHitDirection()
    {
        return hitDirection;
    }

    // for debugging
    public void DoubleHealth()
    {
        health *= 2;
        healthBar.SetHealth(health);
    }
}
