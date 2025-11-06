using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CeilingManager : MonoBehaviour
{
    [SerializeField] private Tilemap[] tilemaps;
    private Queue<Tilemap> tilemapQueue;
    private Dictionary<Tilemap, int> lengths;
    private Tilemap lastMap;
    private Camera mainCamera;
    private bool currentlyMoving;
    public float distanceBuffer = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        tilemapQueue = new Queue<Tilemap>();
        lengths = new Dictionary<Tilemap, int>();
        foreach (Tilemap map in tilemaps)
        {
            map.CompressBounds();
            tilemapQueue.Enqueue(map);
            lengths.Add(map, map.cellBounds.size.x);
        }
        lastMap = tilemaps[tilemaps.Length - 1];
        currentlyMoving = false;
    }

    // Update is called once per frame
    void Update()
    {
        float cameraRightBound = mainCamera.transform.position.x + (mainCamera.orthographicSize * mainCamera.aspect);
        float lastMapRightBound = lastMap.transform.position.x + (lengths[lastMap] / 2);
        if (!currentlyMoving)
        {
            if (cameraRightBound + distanceBuffer >= lastMapRightBound)
            {
                currentlyMoving = true;

                Tilemap firstMap = tilemapQueue.Dequeue();
                float newPosition = lastMap.transform.position.x + (lengths[lastMap] / 2) + (lengths[firstMap] / 2);
                firstMap.transform.position = new Vector3(newPosition, firstMap.transform.position.y, firstMap.transform.position.z);
                tilemapQueue.Enqueue(firstMap);
                lastMap = firstMap;

                currentlyMoving = false;
            }
        }
    }
}
