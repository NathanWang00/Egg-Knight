using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PowerTools;

public enum Area
{
    Center,
    Left,
    Right,
    Back
}

public class Player : Character
{
    [Header("Values")]
    [SerializeField] protected float guardMax = 250;
    [SerializeField] protected float guardDrainRate = 10, guardRecoverRate = 2.5f;
    [SerializeField] protected HealthBar guardBar;
    protected float currentGuard;
    protected bool guardBroken;

    [Header("Animations")]
    [SerializeField] AnimationClip idleAnim;
    [SerializeField] AnimationClip leftDodgeAnim;
    [SerializeField] AnimationClip rightDodgeAnim;
    [SerializeField] AnimationClip backDodgeAnim;
    [SerializeField] AnimationClip guardAnim;
    [SerializeField] AnimationClip windupAnim;
    [SerializeField] AnimationClip stabAnim;
    [SerializeField] AnimationClip slashAnim;

    public enum States
    {
        Idle,
        Guard,
        LeftDodge,
        RightDodge,
        BackDodge,
        Stab,
        Slash
    }

    [Header("State Logic")]
    protected States state = States.Idle;
    protected Area area = Area.Center;
    protected bool actionable = true, guarding = false;

    protected override void Start()
    {
        base.Start();
        guardBar.SetHealth(guardMax);
        currentGuard = guardMax;
    }

    protected virtual void Update()
    {
        if (guarding && state != States.Guard)
        {
            guarding = false;
        }

        if (guarding)
        {
            if (currentGuard > 0)
            {
                currentGuard -= Time.deltaTime * guardDrainRate;
            }
            else
            {
                currentGuard = 0;
                guarding = false;
                state = States.Idle;
                spriteAnim.Play(idleAnim);
                guardBroken = true;
                guardBar.ChangeTransparency(0.5f);
            }
        }
        else
        {
            if (currentGuard < guardMax)
            {
                currentGuard += Time.deltaTime * guardRecoverRate;
            }
            else
            {
                currentGuard = guardMax;
                guardBroken = false;
                guardBar.ChangeTransparency(1);
            }
        }
        guardBar.ChangeHealth(currentGuard);
    }

    public States GetState()
    {
        return state;
    }

    public Area GetArea()
    {
        return area;
    }

    public virtual void Hurt(int damage, Area attackArea)
    {
        if (area == attackArea)
        {
            if (guarding)
            {
                currentGuard -= damage;
            }
            else
            {
                Hurt(damage);
            }
        }
    }

    public void Guard()
    {
        if (!guardBroken)
        {
            state = States.Guard;
            area = Area.Center;
            guarding = true;
            spriteAnim.Play(guardAnim);
        }
    }

    public void Windup()
    {
        area = Area.Center;
        spriteAnim.Play(windupAnim);
    }

    public void Stab()
    {
        state = States.Stab;
        area = Area.Center;
        if (!spriteAnim.IsPlaying(stabAnim))
            spriteAnim.Play(stabAnim);
    }

    public void Slash()
    {
        state = States.Slash;
        area = Area.Center;
        if (!spriteAnim.IsPlaying(slashAnim))
            spriteAnim.Play(slashAnim);
    }

    public void Move(Area direction)
    {
        if (actionable)
        {
            if (direction == Area.Left)
            {
                state = States.LeftDodge;
                area = Area.Left;
                spriteAnim.Play(leftDodgeAnim);
            }
            else if (direction == Area.Right)
            {
                state = States.RightDodge;
                area = Area.Right;
                spriteAnim.Play(rightDodgeAnim);

            }
            else if (direction == Area.Center)
            {
                state = States.Idle;
                area = Area.Center;
                spriteAnim.Play(idleAnim);
            }
            else
            {
                state = States.BackDodge;
                area = Area.Back;
                spriteAnim.Play(backDodgeAnim);
            }
        }
    }
}
