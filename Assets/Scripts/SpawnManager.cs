using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Spawn { public GameObject spawn; public Vector3 pos; }

public class SpawnManager : MonoBehaviour
{
    [SerializeField] protected Spawn[] spawnArray;
    [SerializeField] protected float waitTime;
    protected float waitTracker = 0;
    protected bool waiting = true;

    protected int spawnIndex = 0, loopAmount = 0;

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

    private void Update()
    {
        if (waitTracker <= 0 && waiting && GameManager.Instance.GetStarted())
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
        Enemy enemy;
        if (spawnArray.Length > spawnIndex)
        {
            enemy = Instantiate(spawnArray[spawnIndex].spawn, spawnArray[spawnIndex].pos, Quaternion.identity).GetComponent<Enemy>();
        }
        else
        {
            spawnIndex = 0;
            loopAmount++;
            enemy = Instantiate(spawnArray[spawnIndex].spawn, spawnArray[spawnIndex].pos, Quaternion.identity).GetComponent<Enemy>();
            Debug.Log("Out of enemies");
        }
        for (int i = 0; i < loopAmount; i++)
        {
            enemy.DoubleHealth();
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
