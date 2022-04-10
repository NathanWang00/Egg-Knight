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
    [SerializeField] protected float stabTime = 0.5f;
    [SerializeField] protected float slashTime = 0.5f;
    [SerializeField] [Range(0f, 1f)] protected float playerEnemyRatio = 0.5f;
    //[SerializeField] [Range(0f, 1f)] protected float backDodgeRatio = 0.1f;
    //[SerializeField] [Range(0f, 0.5f)] protected float sideDodgeRatio = 0.33f;
    protected float initialAngle, currentAngle, stopTimeTrack, holdTimeTrack, attackTimeTrack;
    protected Vector2 initialPos, initialWorldPoint, touchPos, touchWorldPoint, lastTouch, currentVelo, lastStop;
    protected bool slashOn = false, slashStart = false, playerOrigin = false, tapConditions = false, stabAnimating = false, slashAnimating = false;

    [Header("Slash Graphics")]
    [SerializeField] protected float minLineChange = 0.1f;
    [SerializeField] protected float slashDelayFade = 0.5f;
    [SerializeField] protected float fadeSpeed = 5;
    [SerializeField] protected GameObject slashPrefab;
    [SerializeField] protected GameObject dodgePrefab;
    protected float linePosition = 0;
    protected LineRenderer slashLine;
    protected List<LineRenderer> slashLines = new List<LineRenderer>();
    //protected LineRenderer dodgeLine;
    protected List<LineRenderer> dodgeLines = new List<LineRenderer>();
    protected LineRenderer preslashLine;

    [Header("Player Controls")]
    [SerializeField] protected float sideDodgeAngle = 90;
    [SerializeField] protected float dodgeStopTime = 3;
    [SerializeField] protected float guardTime = 0.5f;
    [SerializeField] protected float minDodgeTime = 1;
    [SerializeField] protected float doubleTapGuard = 0.3f;
    protected float dodgeTimeTrack = 0, doubleTapTrack = 0;
    protected bool dodgeOn = false, guardOn = false, doubleTapConditions = false;
    protected Player player;

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
            else if (line.CompareTag("Preslash"))
            {
                preslashLine = line;
            }
        }
        tapManager = GetComponentInChildren<TapManager>(true);
        player = FindObjectOfType<Player>();
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
                extraY = 0;
            }
            if (camRatio < 0.56)
            {
                extraY = camDimensions.y - (camDimensions.x * 16 / 9);
                extraX = 0;
                canvasScaler.matchWidthOrHeight = 0;
            }
            else
            {
                canvasScaler.matchWidthOrHeight = 1;
                extraY = 0;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            // position tracking
            initialPos = GetTouchPos();
            initialWorldPoint = Camera.main.ScreenToWorldPoint(initialPos);
            initialAngle = 0;
            lastTouch = touchPos;
            stopTimeTrack = 0;
            lastStop = touchPos;

            playerOrigin = touchPos.y < Camera.main.pixelHeight * playerEnemyRatio;

            // reset animations
            attackTimeTrack = 0;
            slashAnimating = false;
            stabAnimating = false;

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
                slashLine.positionCount = 1;
                slashLine.SetPosition(0, new Vector3(touchWorldPoint.x, touchWorldPoint.y, 0));
                slashLine.gameObject.SetActive(true);
            }
            else
            {
                player.Windup();
                preslashLine.positionCount = 1;
                preslashLine.SetPosition(0, new Vector3(touchWorldPoint.x, touchWorldPoint.y, 0));
                preslashLine.gameObject.SetActive(true);
            }
            /*else
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
                player.Windup();
            }
            slashLine.positionCount = 1;
            slashLine.SetPosition(0, new Vector3(touchWorldPoint.x, touchWorldPoint.y, 0));
            slashLine.gameObject.SetActive(true);*/
            slashOn = false;
            slashStart = false;

            holdTimeTrack = -Time.deltaTime;
            tapConditions = true;

            if (dodgeOn)
            {
                dodgeOn = false;
                player.Move(Player.Area.Center);
            }

            // activate double tap
            if (doubleTapConditions)
            {
                guardOn = true;
                player.Guard();
            }
            doubleTapConditions = false;

            // start double tap conditions
            if (playerOrigin)
            {
                doubleTapConditions = true;
                doubleTapTrack = -Time.deltaTime;
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
                    // where slash is turned on
                    currentVelo = touchPos - lastTouch;
                    currentAngle = Mathf.Rad2Deg * Mathf.Atan2(currentVelo.y, currentVelo.x);
                    initialAngle = currentAngle;
                    slashStart = true;
                    slashOn = true;
                    tapConditions = false;
                    if (playerOrigin)
                    {
                        dodgeOn = true;
                        dodgeTimeTrack = 0;
                    }
                }
                else
                {
                    holdTimeTrack += Time.deltaTime;
                    if (holdTimeTrack > tapTime)
                    {
                        tapConditions = false;
                    }
                    if (holdTimeTrack > guardTime && playerOrigin && !slashStart)
                    {
                        if (dodgeOn)
                        {
                            dodgeOn = false;
                            player.Move(Player.Area.Center);
                        }
                        guardOn = true;
                        player.Guard();
                    }
                }
            }
            else
            {
                if (playerOrigin)
                {
                    if (slashLine.positionCount > 0)
                    {
                        // check min distance
                        Vector2 lastNode = slashLine.GetPosition(slashLine.positionCount - 1);
                        if ((lastNode - touchWorldPoint).magnitude > minLineChange)
                        {
                            // compare to original angle
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
                else
                {
                    preslashLine.positionCount = 2;
                    preslashLine.SetPosition(1, new Vector3(touchWorldPoint.x, touchWorldPoint.y, 0));
                }

                // dodge directions
                if (playerOrigin)
                {
                    if (Mathf.Abs(Mathf.DeltaAngle(0, initialAngle)) <= sideDodgeAngle || (initialAngle > sideDodgeAngle && initialAngle <= 90))
                    {
                        player.Move(Player.Area.Right);
                    }
                    else if (Mathf.Abs(Mathf.DeltaAngle(180, initialAngle)) <= sideDodgeAngle || (initialAngle <= 180 && initialAngle >= 90))
                    {
                        player.Move(Player.Area.Left);
                    } 
                    else if (initialAngle < 0)
                    {
                        player.Move(Player.Area.Back);
                    }
                    else
                    {
                        Debug.Log("Unknown dodge angle " + initialAngle);
                    }
                    dodgeTimeTrack += Time.deltaTime;
                }

                // stop conditions
                if (playerOrigin)
                {
                    if ((touchPos - lastStop).magnitude < stopDistance)
                    {
                        // tracking if too slow
                        stopTimeTrack += Time.deltaTime;
                        if ((!playerOrigin && stopTimeTrack > stopTime) || (playerOrigin && stopTimeTrack > dodgeStopTime))
                        {
                            slashOn = false;
                            stopTimeTrack = 0;
                        }
                    }
                    else
                    {
                        // out of bounds
                        var borderX = extraX / 2;
                        var borderY = extraY / 2;
                        if (touchPos.x > borderX && touchPos.x < borderX + Camera.main.pixelWidth && touchPos.y > borderY && touchPos.y < borderY + Camera.main.pixelHeight)
                        {
                            lastStop = touchPos;
                        }
                        else
                        {
                            slashOn = false;
                        }
                        stopTimeTrack = 0;
                    }
                }

                if (playerOrigin && !slashOn && dodgeTimeTrack >= minDodgeTime)
                {
                    player.Move(Player.Area.Center);
                    dodgeOn = false;
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
            if (tapConditions && !playerOrigin)
            {
                // Stab stuff
                tapManager.StartTap(touchWorldPoint);

                // animation stuff
                player.Stab();
                stabAnimating = true;
                attackTimeTrack = -Time.deltaTime;

                // detection stuff
                LayerMask mask = LayerMask.GetMask("EnemyHurtbox", "WeakpointHurtbox");
                float radius = 1;
                Debug.DrawRay(touchWorldPoint - Vector2.right * radius, Vector2.right * 2 * radius, Color.green, 0.7f);
                Debug.Log(Physics2D.OverlapCircleAll(touchWorldPoint, radius, mask).Length);
            }
            else if (!playerOrigin)
            {
                if (slashOn)
                {
                    // animation stuff
                    player.Slash();
                    slashAnimating = true;
                    attackTimeTrack = -Time.deltaTime;

                    // create line
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
                    slashLine.positionCount = 2;
                    slashLine.SetPosition(0, preslashLine.GetPosition(0));
                    slashLine.SetPosition(1, preslashLine.GetPosition(1));
                    slashLine.gameObject.SetActive(true);

                    LayerMask mask = LayerMask.GetMask("EnemyHurtbox", "WeakpointHurtbox");

                    // detection stuff
                    RaycastHit2D[] hit = Physics2D.LinecastAll(initialWorldPoint, touchWorldPoint, mask);
                    Debug.Log(hit.Length);

                    mask = LayerMask.GetMask("EnemyHurtbox");
                    RaycastHit2D inside = Physics2D.Raycast(touchWorldPoint, Vector2.zero, 20.0f, mask);
                    if (inside)
                    {
                        Debug.Log(inside.collider.name);
                    }
                }
                preslashLine.positionCount = 0;
            }
            slashOn = false;

            if (slashStart || guardOn)
            {
                doubleTapConditions = false;
            }

            if (guardOn)
            {
                guardOn = false;
                player.Move(Player.Area.Center);
            }
        }

        if (dodgeOn && !slashOn)
        {
            if (dodgeTimeTrack >= minDodgeTime)
            {
                dodgeOn = false;
                player.Move(Player.Area.Center);
            } 
            else
            {
                dodgeTimeTrack += Time.deltaTime;
            }
        }

        if (stabAnimating && player.GetState() == Player.States.Stab)
        {
            attackTimeTrack += Time.deltaTime;
            if (attackTimeTrack >= stabTime)
            {
                player.Move(Player.Area.Center);
            }
        }

        if (slashAnimating && player.GetState() == Player.States.Slash)
        {
            attackTimeTrack += Time.deltaTime;
            if (attackTimeTrack >= slashTime)
            {
                player.Move(Player.Area.Center);
            }
        }

        if (doubleTapConditions && !Input.GetMouseButtonDown(0))
        {
            doubleTapTrack += Time.deltaTime;
            if (doubleTapTrack >= doubleTapGuard)
            {
                doubleTapConditions = false;
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

    private void FixedUpdate()
    {
        // moved fade to fixedupdate since it was acting weird
        foreach (var item in slashLines)
        {
            if (item.gameObject.activeSelf)
            {
                var color = item.endColor;
                var startColor = item.startColor;
                if (color.a > 0.95)
                {
                    color.a -= 0.05f * slashDelayFade;
                    startColor.a -= 0.05f * slashDelayFade;
                }
                else
                {
                    color.a -= fadeSpeed;
                    startColor.a -= fadeSpeed;
                }
                if (color.a <= 0)
                {
                    item.gameObject.SetActive(false);
                    color.a = 1;
                    startColor.a = 0.5f;
                    item.endColor = color;
                    item.startColor = startColor;
                }
                else
                {
                    item.endColor = color;
                    item.startColor = startColor;
                }
            }
        }
        foreach (var item in dodgeLines)
        {
            if (item.gameObject.activeSelf && (item != slashLine || (!slashOn && slashStart)))
            {
                var color = item.endColor;
                color.a -= fadeSpeed;
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
