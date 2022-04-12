using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Attack { public float damage; public AnimationClip animation; public Area[] areas; }

public class Enemy : Character
{
    [Header ("Attack Stuff")]
    [SerializeField] protected Attack[] attackArray;
    protected int attackIndex = 0;
    protected bool attacking = false;

    [Header("Animations")]
    [SerializeField] protected AnimationClip idle;
    protected SpriteRenderer spriteRenderer;

    // Damage receive stuff
    protected int weakpointsHit = 0;
    protected bool fullSlash = false;

    protected override void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        base.Awake();
    }

    protected void AttackStart()
    {
        AttackStart(0);
    }

    protected void AttackStart(int index)
    {
        // Change the attackIndex beforehand
        spriteAnim.Play(attackArray[index].animation);
        attackIndex = index;
        attacking = true;
    }

    // change color

    protected void AttackHit()
    {
        Attack attack = attackArray[attackIndex];
        foreach (var area in attack.areas)
        {
            GameManager.Instance.AttackPlayer(attack.damage, area);
        }

        spriteRenderer.sortingLayerName = "EnemyAttack";
        // revert color
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
}
