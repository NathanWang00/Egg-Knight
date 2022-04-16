using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private float maxHealth, originalWidth;

    public void SetHealth(float health)
    {
        maxHealth = health;
        originalWidth = transform.localScale.x;
    }

    public void ChangeHealth(float health)
    {
        var tempHealth = health;
        if (health < 0)
        {
            tempHealth = 0;
        }
        var x = (tempHealth / maxHealth) * originalWidth;
        transform.localScale = new Vector3(x, transform.localScale.y);
    }
}
