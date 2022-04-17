using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] protected GameObject[] spawnArray;
    [SerializeField] protected float waitTime;
    protected float waitTracker = 0;
    protected bool waiting = false;

    protected int spawnIndex = 0;

    private static SpawnManager _instance;
    public static SpawnManager Instance //Singleton Stuff
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("SpawnManager is Null");
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
        Spawn();
    }

    private void Update()
    {
        if (waitTracker <= 0 && waiting)
        {
            waiting = false;
            Spawn();
        }
        else if (waiting)
        {
            waitTracker -= Time.deltaTime;
        }
    }

    protected void Spawn()
    {
        if (spawnArray.Length > spawnIndex)
        {
            Instantiate(spawnArray[spawnIndex]);
        }
        else
        {
            Debug.Log("Out of enemies");
        }
        spawnIndex++;
    }

    public void NextWave(float time)
    {
        waitTracker = time;
        waiting = true;
    }

    public void NextWave()
    {
        waitTracker = waitTime;
        waiting = true;
    }
}
