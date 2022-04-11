using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    protected int weakpointsHit = 0;
    protected bool fullSlash = false;

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
