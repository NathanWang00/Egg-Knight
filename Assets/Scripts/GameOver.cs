using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOver : MonoBehaviour
{
    protected bool ending = false;
    [SerializeField] protected Image bg;
    [SerializeField] protected TextMeshProUGUI[] texts;
    [SerializeField] protected float appearSpeed = 0;
    protected float currentAlpha = 0;

    private void Awake()
    {
        SetColor(0);
    }

    private void Update()
    {
        if (ending)
        {
            currentAlpha += appearSpeed * Time.deltaTime;
            SetColor(currentAlpha);
        }
    }

    protected void SetColor(float alpha)
    {
        var color = bg.color;
        color.a = alpha;
        bg.color = color;
        foreach (var text in texts)
        {
            color = text.color;
            color.a = alpha;
            text.color = color;
        }
    }

    public void End()
    {
        ending = true;
    }
}
