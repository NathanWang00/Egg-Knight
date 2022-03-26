using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] protected bool debugOn;
    [SerializeField] protected GameObject debugObject;
    [SerializeField] protected TextMeshProUGUI vectorText;

    [Header("Touch Controls")]
    [SerializeField] protected float startRadius = 8;
    [SerializeField] protected float stopDistance = 10;
    [SerializeField] protected float stopTime = 1;
    protected float initialAngle, currentAngle, stopTimeTrack;
    protected Vector2 initialPos, touchPos, touchWorldPoint, lastTouch, currentVelo, lastStop;
    protected bool slashOn = false, slashStart = false;

    [Header("Graphics")]
    [SerializeField] protected float minLineChange = 0.1f;
    [SerializeField] protected float fadeSpeed = 5;
    [SerializeField] protected GameObject slashPrefab;
    protected float linePosition = 0;
    protected LineRenderer slashLine;
    protected List<LineRenderer> slashLines;

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
        slashLines = GetComponentsInChildren<LineRenderer>(true).ToList();
    }

    private void Update()
    {   
        if (Input.GetMouseButtonDown(0))
        {
            bool found = false;
            foreach (var item in slashLines)
            {
                if (!item.gameObject.activeSelf)
                {
                    slashLine = item;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                slashLine = Instantiate(slashPrefab).GetComponent<LineRenderer>();
                slashLines.Add(slashLine);
            }
            slashLine.positionCount = 0;
            slashLine.gameObject.SetActive(true);

            initialPos = GetTouchPos();
            initialAngle = 0;
            lastTouch = touchPos;
            slashOn = false;
            slashStart = false;

            stopTimeTrack = 0;
            lastStop = touchPos;
        }
        if (Input.GetMouseButton(0))
        {
            var tempPos = touchPos;
            GetTouchPos();
            if (tempPos != touchPos)
            {
                lastTouch = tempPos;
            }
            
            // slash stuff and graphics
            if (!slashOn)
            {
                if ((touchPos - initialPos).magnitude > startRadius && !slashStart)
                {
                    currentVelo = touchPos - lastTouch;
                    currentAngle = Mathf.Rad2Deg * Mathf.Atan2(currentVelo.y, currentVelo.x);
                    initialAngle = currentAngle;
                    slashStart = true;
                    slashOn = true;
                }
            }
            else
            {
                if ((touchPos - lastStop).magnitude < stopDistance)
                {
                    stopTimeTrack += Time.deltaTime;
                    if (stopTimeTrack > stopTime)
                    {
                        slashOn = false;
                        Debug.Log("too slow");
                        stopTimeTrack = 0;
                    } 
                }
                else
                {
                    lastStop = touchPos;
                    stopTimeTrack = 0;
                }
                
                if (slashOn)
                {
                    if (slashLine.positionCount > 0)
                    {
                        Vector2 lastNode = slashLine.GetPosition(slashLine.positionCount - 1);
                        if ((lastNode - touchWorldPoint).magnitude > minLineChange)
                        {
                            currentVelo = touchWorldPoint - lastNode;
                            currentAngle = Mathf.Rad2Deg * Mathf.Atan2(currentVelo.y, currentVelo.x);
                            if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, initialAngle)) > 85)
                            {
                                slashOn = false;
                                Debug.Log("wrong ang");
                            }
                            else
                            {
                                slashLine.positionCount += 1;
                                slashLine.SetPosition(slashLine.positionCount - 1, new Vector3(touchWorldPoint.x, touchWorldPoint.y, 0));
                            }
                        }
                    }
                    else
                    {
                        slashLine.positionCount += 1;
                        slashLine.SetPosition(slashLine.positionCount - 1, new Vector3(touchWorldPoint.x, touchWorldPoint.y, 0));
                    }
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
        }
        else
        {
            slashLine = null;
        }
        if (Input.GetMouseButtonUp(0))
        {
            slashOn = false;
        }

        foreach (var item in slashLines)
        {
            if (item.gameObject.activeSelf && (item != slashLine || (!slashOn && slashStart)))
            {
                var color = item.endColor;
                color.a -= fadeSpeed * Time.deltaTime;
                if (color.a <= 0)
                {
                    item.gameObject.SetActive(false);
                    color.a = 1;
                    item.endColor = color;
                } 
                else
                {
                    item.endColor = color;
                }
            }
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
