using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exultant : Enemy
{
    [SerializeField] protected float minAttackDelay, maxAttackDelay;

    protected override void Start()
    {
        base.Start();
    }

    protected override void AttackStart()
    {
        base.AttackStart();
        attackDelay = Random.Range(minAttackDelay, maxAttackDelay);
    }

    public override void Hurt(int damage)
    {
        base.Hurt(damage);
    }
}