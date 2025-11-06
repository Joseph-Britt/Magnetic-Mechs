using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal.Filters;
using UnityEngine;

public class DroneSpawnerScript : MonoBehaviour
{
    //script responsible for spawning drones
    [Header("Number of Drones")]
    private GameObject[] DronesArray;
    private Queue<int> DronesAvailableQueue;
    private int maxDrones = 2;
    [Header("Manage Spawn")]
    private float timeToSpawn;
    private float timer;
    public bool active = false;
    [Header("Components")]
    public GameObject dronePrefab;
    public Transform playerTransform;
    public Transform myTransform;
    [Header("Spawn Points")]
    public Vector3 spawnPosition;
    public bool spawnRight;
    private float spawnRightChance = .5f;
    private float spawnRightChanceModifier = .1f;
    [Header("Variables")]
    public bool uniformSpeed = false;
    public bool followX = false;
    public bool followY = false;
    private float heightSpread = 2f;
    public float XSpawnDistance = 10f;
    public float MaxXDistance = 15f;
    public float MaxYDistance = 15f;
    private void Start()
    {
        timeToSpawn = 2;
        myTransform = transform;
        SetUpArrays();
    }
    private void SetUpArrays()
    {
        DronesArray = new GameObject[maxDrones];
        for (int i = 0; i < maxDrones; i++)
        {
            GameObject tempDrone = Instantiate(dronePrefab, transform.position, Quaternion.Euler(0, 0, 0));
            DronesArray[i] = tempDrone;
            if (uniformSpeed) tempDrone.GetComponent<DroneHorizontalScript>().speedModifier = 0f;
            tempDrone.SetActive(false);
            DroneHorizontalScript tempDroneScript = tempDrone.GetComponent<DroneHorizontalScript>();
            tempDroneScript.DroneSpawnerScript = this;
            tempDroneScript.DroneSpawnerTransform = transform;
            tempDroneScript.MaxXDistance = MaxXDistance;
            tempDroneScript.MaxYDistance = MaxYDistance;
        }
        DronesAvailableQueue = new Queue<int>();
        for (int i = 0; i < maxDrones; i++)
        {
            DronesAvailableQueue.Enqueue(i);
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (playerTransform != null)
        {
            float newXPos = followX ? playerTransform.position.x : myTransform.position.x;
            float newYPos = followY ? playerTransform.position.y : myTransform.position.y;
            myTransform.position = new Vector3(newXPos, newYPos, myTransform.position.z);
        }
        if (!active) return;
        if (DronesAvailableQueue.Count == 0)
        {
            timer = 0;
        }
        if (timer < timeToSpawn)
        {
            timer = timer + Time.fixedDeltaTime;
        }
        else
        {
            Vector3 height = new Vector3(0f, Random.Range(-1.0f, 1.0f) * heightSpread, 0f);
            Vector3 xDistance = new Vector3(XSpawnDistance, 0, 0);
            spawnRight = UnityEngine.Random.Range(0f, 1f) < spawnRightChance;
            if (spawnRight)
            {
                spawnPosition = transform.position + xDistance + height;
                if (spawnRightChance < .5f)
                {
                    spawnRightChance = .5f - spawnRightChanceModifier;
                }
                else
                {
                    spawnRightChance -= spawnRightChanceModifier;
                }
            }
            else
            {
                spawnPosition = transform.position - xDistance + height;
                if (spawnRightChance > .5f)
                {
                    spawnRightChance = .5f + spawnRightChanceModifier;
                }
                else
                {
                    spawnRightChance += spawnRightChanceModifier;
                }
            }
            spawnDrone();
            timer = 0;
        }
    }
    public void Activate()
    {
        active = true;
    }
    public void Deactivate()
    {
        active = false;
    }
    void spawnDrone()
    {
        int currentIndex = DronesAvailableQueue.Dequeue();
        GameObject tempDrone = DronesArray[currentIndex];
        tempDrone.SetActive(true);
        tempDrone.transform.position = spawnPosition;
        DroneHorizontalScript tempDroneScript = tempDrone.GetComponent<DroneHorizontalScript>();
        tempDroneScript.index = currentIndex;
        tempDroneScript.restartDrone();
        if (spawnRight ^ !tempDroneScript.facingRight)
        {
            tempDroneScript.Flip();
        }
    }

    public void DroneKilled(int index)
    {
        DronesAvailableQueue.Enqueue(index);
    }
}
