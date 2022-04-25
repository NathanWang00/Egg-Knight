using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shellbeast : Enemy
{
    protected override void Start()
    {
        base.Start();
        attackEnabled = false;
    }

    public override void Hurt(int damage)
    {
        attackEnabled = true;
        base.Hurt(damage);
    }

    protected override float WeakpointMultiplyer(int weakpoint)
    {
        if (weakpoint < 1)
        {
            return -0.9f;
        }

        switch (weakpoint)
        {
            case 1:
                return 1.5f;

            case 2:
                return 1.5f;

            case 3:
                return 1.5f;
        }
        if (weakpoint > 3)
        {
            Debug.Log("More weakpoints than max");
            return 1.5f;
        }

        return -0.9f;
    }
}
