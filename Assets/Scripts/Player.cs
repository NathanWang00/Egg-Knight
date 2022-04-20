using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PowerTools;
using UnityEngine.UI;

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
    [SerializeField] protected Material hurtMat, attackMat;
    protected static float flashTime = 0.4f, defaultFlash = 0.7f;
    protected float flashTracker = 0;
    protected Image image;
    protected Material originalMat;

    public Vector2 coreNode;

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

    protected override void Awake()
    {
        base.Awake();
        image = GetComponent<Image>();
        originalMat = image.material;
        hurtMat.SetFloat("_FlashAmount", defaultFlash);
        attackMat.SetFloat("_FlashAmount", defaultFlash);
    }

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

        // Resets color for flash
        if (flashTracker > 0)
        {
            flashTracker -= Time.deltaTime;
            image.material.SetFloat("_FlashAmount", defaultFlash * flashTracker / flashTime);
            if (flashTracker <= 0)
            {
                image.material.SetFloat("_FlashAmount", defaultFlash);
                image.material = originalMat;
            }
        }
    }

    public States GetState()
    {
        return state;
    }

    public Area GetArea()
    {
        return area;
    }

    public virtual bool Hurt(int damage, Area attackArea)
    {
        bool hit = false;
        if (area == attackArea)
        {
            if (guarding)
            {
                currentGuard -= damage;
            }
            else
            {
                hit = true;
                Hurt(damage);
            }
        }
        return hit;
    }

    public override void Hurt(int damage)
    {
        base.Hurt(damage);
        if (image.material != attackMat)
        {
            Flash(hurtMat, flashTime);
        }
    }

    public Vector2 EggPosition()
    {
        return nodes.GetPositionRaw(0);
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

    protected void Flash(Material mat, float time)
    {
        hurtMat.SetFloat("_FlashAmount", defaultFlash);
        image.material = mat;
        flashTracker = time;
    }
}
