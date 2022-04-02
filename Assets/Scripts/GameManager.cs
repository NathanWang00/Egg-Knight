using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] protected bool debugOn;
    [SerializeField] protected GameObject debugObject;
    [SerializeField] protected TextMeshProUGUI vectorText;
    [SerializeField] protected LineRenderer ratioLine;

    [Header("Screen Calc")]
    [SerializeField] protected Camera letterboxCam;
    [SerializeField] protected CanvasScaler canvasScaler;
    protected float extraX = 0, extraY = 0;

    [Header("Attack Controls")]
    [SerializeField] protected float startRadius = 8;
    [SerializeField] protected float stopDistance = 10;
    [SerializeField] protected float stopTime = 1;
    [SerializeField] protected float tapTime = 0.1f;
    [SerializeField] [Range(0f, 1f)] protected float playerEnemyRatio = 0.5f;
    protected float initialAngle, currentAngle, stopTimeTrack, tapTimeTrack;
    protected Vector2 initialPos, touchPos, touchWorldPoint, lastTouch, currentVelo, lastStop;
    protected bool slashOn = false, slashStart = false, playerOrigin = false, tapConditions = false;

    [Header("Slash Graphics")]
    [SerializeField] protected float minLineChange = 0.1f;
    [SerializeField] protected float fadeSpeed = 5;
    [SerializeField] protected GameObject slashPrefab;
    protected float linePosition = 0;
    protected LineRenderer slashLine;
    protected List<LineRenderer> slashLines;

    [Header("Player Controls")]
    protected bool guardOn = false;

    protected TapManager tapManager;

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
        tapManager = GetComponentInChildren<TapManager>(true);
    }

    private void Update()
    {
        var camDimensions = new Vector2 (letterboxCam.pixelWidth, letterboxCam.pixelHeight);
        var camRatio = camDimensions.x / camDimensions.y;
        if (camRatio != 9/16)
        {
            if (camRatio > 0.57)
            {
                extraX = camDimensions.x - (camDimensions.y * 9 / 16);
            }
            if (camRatio < 0.56)
            {
                extraY = camDimensions.y - (camDimensions.x * 16 / 9);
                canvasScaler.matchWidthOrHeight = 0;
            }
            else
            {
                canvasScaler.matchWidthOrHeight = 1;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            // position tracking
            initialPos = GetTouchPos();
            initialAngle = 0;
            lastTouch = touchPos;
            stopTimeTrack = 0;
            lastStop = touchPos;

            // slash start
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
            slashLine.positionCount = 1;
            slashLine.SetPosition(slashLine.positionCount - 1, new Vector3(touchWorldPoint.x, touchWorldPoint.y, 0));
            slashLine.gameObject.SetActive(true);
            slashOn = false;
            slashStart = false;

            playerOrigin = touchPos.y < Camera.main.pixelHeight * playerEnemyRatio;

            // tap start
            if (playerOrigin)
            {
                guardOn = true;
            }
            else
            {
                tapTimeTrack = 0;
                tapConditions = true;
            }
            
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
                    tapConditions = false;
                }
                else
                {
                    if (tapConditions)
                    {
                        if (tapTimeTrack == 0)
                        {
                            // lazy solution so that Time.deltaTime won't be added when it's 0
                            tapTimeTrack = 0.0001f;
                        }
                        else
                        {
                            tapTimeTrack += Time.deltaTime;
                            if (tapTimeTrack > tapTime)
                            {
                                tapConditions = false;
                            }
                        }
                    }
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
            if (Input.GetMouseButton(0))
            {
                vectorText.text = "Touch Vector: " + touchPos + "\n" + "Touch Velo: " + currentVelo + "\n" + "Angle: " + currentAngle;
            }
        }
        else
        {
            slashLine = null;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (tapConditions)
            {
                tapManager.StartTap(touchWorldPoint);
            }
            slashOn = false;
            guardOn = false;
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

        // Debug stuff
        debugObject.SetActive(debugOn);
        var pos = new Vector3(0, (playerEnemyRatio * Camera.main.pixelHeight) + extraY/2, 1);
        var worldPos = Camera.main.ScreenToWorldPoint(pos);
        worldPos.z = 1;
        ratioLine.SetPosition(0, worldPos);
        pos.x = Camera.main.pixelWidth + extraX/2;
        worldPos = Camera.main.ScreenToWorldPoint(pos);
        worldPos.z = 1;
        ratioLine.SetPosition(1, worldPos);
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
