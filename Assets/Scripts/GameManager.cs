using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Android;

public class GameManager : MonoBehaviour
{
    [Header("Debug Section")]
    [SerializeField] protected bool debugOn;
    [SerializeField] protected GameObject debugObject;
    [SerializeField] protected TMPro.TextMeshProUGUI vectorText;

    [Space(10)]

    protected Vector2 touchPos, lastTouch, lastLastTouch;
    protected float angleChange;

    private static GameManager _instance;
    public static GameManager Instance //Singleton Stuff
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("Game Manager is Null");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        Vector2 lastVelo = Vector2.zero, currentVelo = Vector2.zero;
        if (Input.GetMouseButton(0))
        {
            if (Input.touchCount > 0)
            {
                touchPos = Input.GetTouch(0).position;
            }
            else
            {
                touchPos = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            angleChange = 0;
            lastTouch = touchPos;
            lastLastTouch = touchPos;
        } 
        else if (Input.GetMouseButton(0))
        {
            if (touchPos - lastTouch != Vector2.zero)
            {
                lastVelo = lastTouch - lastLastTouch;
                currentVelo = touchPos - lastTouch;
            }
        }

        if (debugObject != null)
        {
            debugObject.SetActive(debugOn);
            if (Input.GetMouseButton(0))
            {
                vectorText.text = "Touch Vector: " + touchPos + "\n" + "Touch Velo: " + currentVelo + "\n" + Vector2.Angle(lastVelo, currentVelo);
            }
        }

        if (Input.GetMouseButton(0) && lastTouch != touchPos)
        {
            lastLastTouch = lastTouch;
            lastTouch = touchPos;
        }
    }
}
