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
    //[SerializeField] protected LineRenderer sideDodgeLineLeft;
    //[SerializeField] protected LineRenderer sideDodgeLineRight;
    //[SerializeField] protected LineRenderer backDodgeLine;

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
    //[SerializeField] [Range(0f, 1f)] protected float backDodgeRatio = 0.1f;
    //[SerializeField] [Range(0f, 0.5f)] protected float sideDodgeRatio = 0.33f;
    protected float initialAngle, currentAngle, stopTimeTrack, tapTimeTrack;
    protected Vector2 initialPos, touchPos, touchWorldPoint, lastTouch, currentVelo, lastStop;
    protected bool slashOn = false, slashStart = false, playerOrigin = false, tapConditions = false;

    [Header("Slash Graphics")]
    [SerializeField] protected float minLineChange = 0.1f;
    [SerializeField] protected float fadeSpeed = 5;
    [SerializeField] protected GameObject slashPrefab;
    [SerializeField] protected GameObject dodgePrefab;
    protected float linePosition = 0;
    protected LineRenderer slashLine;
    protected List<LineRenderer> slashLines = new List<LineRenderer>();
    //protected LineRenderer dodgeLine;
    protected List<LineRenderer> dodgeLines = new List<LineRenderer>();

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
        var lines = GetComponentsInChildren<LineRenderer>(true).ToList();
        foreach (var line in lines)
        {
            if (line.CompareTag("Slash"))
            {
                slashLines.Add(line);
            }
            else if (line.CompareTag("Dodge"))
            {
                dodgeLines.Add(line);
            }
        }
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

            playerOrigin = touchPos.y < Camera.main.pixelHeight * playerEnemyRatio;


            // slash start
            bool found = false;
            if (playerOrigin)
            {
                foreach (var item in dodgeLines)
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
                    slashLine = Instantiate(dodgePrefab).GetComponent<LineRenderer>();
                    dodgeLines.Add(slashLine);
                }
            }
            else
            {
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
            }
            slashLine.positionCount = 1;
            slashLine.SetPosition(slashLine.positionCount - 1, new Vector3(touchWorldPoint.x, touchWorldPoint.y, 0));
            slashLine.gameObject.SetActive(true);
            slashOn = false;
            slashStart = false;

            // tap start
            if (!playerOrigin)
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
                    // speed control
                    var borderX = extraX / 2;
                    var borderY = extraY / 2;
                    if (touchPos.x > borderX && touchPos.x < borderX + Camera.main.pixelWidth && touchPos.y > borderY && touchPos.y < borderY + Camera.main.pixelHeight)
                    {
                        Debug.Log(touchPos);
                        lastStop = touchPos;
                    }
                    else
                    {
                        slashOn = false;
                    }
                    stopTimeTrack = 0;
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
        foreach (var item in dodgeLines)
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
        var width = Camera.main.pixelWidth;
        var height = Camera.main.pixelHeight;
        var xBorder = extraX / 2;
        var yBorder = extraY / 2;

        var pos = new Vector3(xBorder, (playerEnemyRatio * height) + yBorder, 1);
        ratioLine.SetPosition(0, ScreenToWorld(pos));
        pos.x = width + xBorder;
        ratioLine.SetPosition(1, ScreenToWorld(pos));

        /*pos.x = width * sideDodgeRatio + xBorder;
        sideDodgeLineLeft.SetPosition(0, ScreenToWorld(pos));
        pos.y = backDodgeRatio * height + yBorder;
        sideDodgeLineLeft.SetPosition(1, ScreenToWorld(pos));

        pos.x = width - (width * sideDodgeRatio) + xBorder;
        sideDodgeLineRight.SetPosition(0, ScreenToWorld(pos));
        pos.y = playerEnemyRatio * height + yBorder;
        sideDodgeLineRight.SetPosition(1, ScreenToWorld(pos));

        pos.x = xBorder;
        pos.y = backDodgeRatio * height + yBorder;
        backDodgeLine.SetPosition(0, ScreenToWorld(pos));
        pos.x = width + xBorder;
        backDodgeLine.SetPosition(1, ScreenToWorld(pos));*/
    }

    public Vector3 ScreenToWorld(Vector3 pos)
    {
        var z = pos.z;
        var worldPos = Camera.main.ScreenToWorldPoint(pos);
        worldPos.z = z;
        return worldPos;
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
