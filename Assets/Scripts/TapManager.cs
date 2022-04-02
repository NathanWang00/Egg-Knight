using System.Collections.Generic;
using UnityEngine;

public class TapManager : MonoBehaviour
{
    protected List<SpriteRenderer> tapAttacks;
    [SerializeField] protected GameObject tapPrefab;
    [SerializeField] protected float fadeSpeed;

    protected void Start()
    {
        tapAttacks = new List<SpriteRenderer>();
        tapAttacks.AddRange(GetComponentsInChildren<SpriteRenderer>(true));
    }

    protected void Update()
    {
        foreach (var item in tapAttacks)
        {
            if (item.gameObject.activeSelf)
            {
                var color = item.color;
                color.a -= fadeSpeed * Time.deltaTime;
                if (color.a <= 0)
                {
                    item.gameObject.SetActive(false);
                    color.a = 1;
                    item.color = color;
                }
                else
                {
                    item.color = color;
                }
            }
        }
    }

    public void StartTap(Vector2 location)
    {
        bool found = false;
        SpriteRenderer tapAttack = null;
        foreach (var item in tapAttacks)
        {
            if (!item.gameObject.activeSelf)
            {
                tapAttack = item;
                found = true;
                break;
            }
        }
        if (!found)
        {
            tapAttack = Instantiate(tapPrefab, transform).GetComponent<SpriteRenderer>();
            tapAttacks.Add(tapAttack);
        }
        tapAttack.transform.position = location;
        tapAttack.gameObject.SetActive(true);
    }
}
