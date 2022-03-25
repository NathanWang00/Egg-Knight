using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] protected bool debugOn;
    [SerializeField] protected GameObject debugObject;
    [SerializeField] protected TextMeshProUGUI vectorText;

    [Space(10)]

    [Header("Touch Controls")]
    [SerializeField] protected float startRadius = 10; // amount of area needed to start the slash
    protected float initialAngle, trackDelay = 0.0116f, currentDelay = 0, currentAngle;
    protected Vector2 initialPos, touchPos, touchWorldPoint, lastTouch, currentVelo;
    protected bool slashOn = false;

    [Space(10)]

    [Header("Graphics")]
    [SerializeField] protected float minLineChange = 0.1f;
    protected float linePosition = 0;
    protected LineRenderer slashLine;

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

    private void Start()
    {
        slashLine = GetComponentInChildren<LineRenderer>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            initialPos = GetTouchPos();
            initialAngle = 0;
            lastTouch = touchPos;
            currentDelay = trackDelay - Time.deltaTime;
            slashLine.positionCount = 0;
        }
        if (Input.GetMouseButton(0))
        {
            currentDelay += Time.deltaTime;
            if (currentDelay >= trackDelay)
            {
                GetTouchPos();

                currentDelay -= trackDelay;
                currentVelo = touchPos - lastTouch;
                currentAngle = Mathf.Rad2Deg * Mathf.Atan2(currentVelo.y, currentVelo.x);

                if (currentAngle < 0)
                {
                    currentAngle += 360;
                }

                // slash stuff and graphics
                if (!slashOn)
                {
                    if ((touchPos - initialPos).magnitude > startRadius)
                    {
                        initialAngle = currentAngle;
                        slashOn = true;
                        Debug.Log("on");
                    }
                }
                else
                {
                    if (slashLine.positionCount > 0)
                    {
                        if (((Vector2)slashLine.GetPosition(slashLine.positionCount - 1) - touchWorldPoint).magnitude > minLineChange)
                        {
                            slashLine.positionCount += 1;
                            slashLine.SetPosition(slashLine.positionCount - 1, new Vector3(touchWorldPoint.x, touchWorldPoint.y, 0));
                        }
                    }
                    else
                    {
                        slashLine.positionCount += 1;
                        slashLine.SetPosition(slashLine.positionCount - 1, new Vector3(touchWorldPoint.x, touchWorldPoint.y, 0));
                    }
                }

                // debug stuff
                if (debugObject != null)
                {
                    debugObject.SetActive(debugOn);
                    if (Input.GetMouseButton(0))
                    {
                        vectorText.text = "Touch Vector: " + touchPos + "\n" + "Touch Velo: " + currentVelo + "\n" + "Angle: " + currentAngle;
                    }
                }

                // setting last touch to current
                if (Input.GetMouseButton(0) && lastTouch != touchPos)
                {
                    lastTouch = touchPos;
                }
            }
        } 
        if (Input.GetMouseButtonUp(0))
        {
            slashOn = false;
        }
    }

    public Vector2 GetTouchPos() // also sets touchPos. If there's no touch, gives the last touchPos
    {
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
            touchWorldPoint = Camera.main.ScreenToWorldPoint(touchPos);
        }
        return touchPos;
    }
}
