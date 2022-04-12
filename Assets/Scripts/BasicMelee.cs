using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMelee : Enemy
{
    [Header("Specific Attack Stuff")]
    [SerializeField] protected float attackDelay;
    protected float attackTimeTrack = 0;
    // add variance

    protected void Update()
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
    }
}
