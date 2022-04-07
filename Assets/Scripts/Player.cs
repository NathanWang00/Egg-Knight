using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PowerTools;

public class Player : MonoBehaviour
{
    [Header("Animations")]
    [SerializeField] AnimationClip idleAnim;
    [SerializeField] AnimationClip leftDodgeAnim;
    [SerializeField] AnimationClip rightDodgeAnim;
    [SerializeField] AnimationClip backDodgeAnim;
    [SerializeField] AnimationClip guardAnim;
    [SerializeField] AnimationClip windupAnim;
    [SerializeField] AnimationClip stabAnim;
    [SerializeField] AnimationClip slashAnim;
    protected SpriteAnim spriteAnim;

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

    public enum Area
    {
        Center,
        Left,
        Right,
        Back
    }

    [Header("State Logic")]
    protected States state = States.Idle;
    protected Area area = Area.Center;
    protected bool actionable = true, guarding = false;

    private void Awake()
    {
        spriteAnim = GetComponent<SpriteAnim>();
    }

    public States GetState()
    {
        return state;
    }

    public Area GetArea()
    {
        return area;
    }

    public void Guard()
    {
        state = States.Guard;
        area = Area.Center;
        spriteAnim.Play(guardAnim);
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
        spriteAnim.Play(stabAnim);
    }

    public void Slash()
    {
        state = States.Slash;
        area = Area.Center;
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
