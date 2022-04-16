using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DmgNumManager : MonoBehaviour
{
    protected List<TextMeshProUGUI> dmgNumbers;
    [SerializeField] protected GameObject numberPrefab;
    [SerializeField] protected float fadeSpeed, vertSpeed, horzSpeed, gravity, sizeRatio, sizeBase, posVariance;

    private static DmgNumManager _instance;
    public static DmgNumManager Instance //Singleton Stuff
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("DmgNumManager is Null");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    protected virtual void Start()
    {
        dmgNumbers = new List<TextMeshProUGUI>();
        dmgNumbers.AddRange(GetComponentsInChildren<TextMeshProUGUI>(true));
    }

    protected virtual void Update()
    {
        foreach (var item in dmgNumbers)
        {
            if (item.gameObject.activeSelf)
            {
                var rb2D = item.GetComponent<Rigidbody2D>();
                var color = item.color;
                color.a -= fadeSpeed * Time.deltaTime;
                if (color.a <= 0)
                {
                    rb2D.velocity = Vector2.zero;
                    item.gameObject.SetActive(false);
                    color.a = 1;
                    item.color = color;
                }
                else
                {
                    item.color = color;
                    rb2D.velocity += new Vector2(0, -gravity) * Time.deltaTime;
                    item.transform.position += (Vector3) rb2D.velocity * Time.deltaTime;
                }
            }
        }
    }

    public void CreateDmg(Vector2 location, int dir, float amount)
    {
        bool found = false;
        TextMeshProUGUI dmgNumber = null;
        foreach (var item in dmgNumbers)
        {
            if (!item.gameObject.activeSelf)
            {
                dmgNumber = item;
                found = true;
                break;
            }
        }
        if (!found)
        {
            dmgNumber = Instantiate(numberPrefab, transform).GetComponent<TextMeshProUGUI>();
            dmgNumbers.Add(dmgNumber);
        }
        dmgNumber.transform.position = location;
        dmgNumber.transform.position += new Vector3 (Random.Range(-posVariance / 2, posVariance / 2), Random.Range(-posVariance / 2, posVariance / 2));
        dmgNumber.gameObject.SetActive(true);
        dmgNumber.text = amount.ToString();
        dmgNumber.fontSize = amount * sizeRatio + sizeBase;
        Vector2 direction = new Vector2(0, vertSpeed);
        if (dir == 1)
        {
            direction.x = horzSpeed;
        }
        else if (dir == -1)
        {
            direction.x = -horzSpeed;
        }
        var rb2D = dmgNumber.GetComponent<Rigidbody2D>();
        rb2D.velocity = direction;
    }
}
