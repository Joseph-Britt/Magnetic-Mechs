using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserScriptStationary : MonoBehaviour
{
    //script for managing the robot spider queen's sweeping laser attack
    [Header("Components")]
    public LayerMask blockLaserLayers;
    public GameObject laserSpawnMiddle;
    public GameObject laserSpawnLeft;
    public GameObject laserSpawnRight;
    public LineRenderer lineRendererMiddle;
    public LineRenderer lineRendererLeft;
    public LineRenderer lineRendererRight;
    public GameObject ObjectHit;
    public Transform playerTransform;
    public PlayerScript playerScript;
    public LineRenderer laserPointer;
    [Header("variables")]
    public float laserAngle;
    public float startingAngle = 0;
    private float rotationSpeed;
    private float defaultDamage = .25f;
    private float defaultInvincibleTime = .1f;
    public bool laserEnabled = false;
    [Header("Explosion")]
    public float explosionTime = .2f;
    private float explosionTimer;
    public float explosionOffset = .05f;
    [Header("Stages")]
    public float rotationSpeedStage1 = 23f;
    void Awake()
    {
        laserAngle = startingAngle;
        explosionTimer = 0;
        //rotationSpeed = 30f;
        blockLaserLayers = LayerMask.GetMask("Ground", "Player", "Wall");
        lineRendererMiddle = laserSpawnMiddle.GetComponent<LineRenderer>();
        lineRendererLeft = laserSpawnLeft.GetComponent<LineRenderer>();
        lineRendererRight = laserSpawnRight.GetComponent<LineRenderer>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.GetComponent<Transform>();
        else Debug.Log("Player could not be found");
        TriggerStage1();
        enableLaserRenderer();
    }
    private void TriggerStage1()
    {
        rotationSpeed = rotationSpeedStage1;
    }
    private void FixedUpdate()
    {
        if (!laserEnabled)
        {
            return;
        }
        laserAngle -= rotationSpeed * Time.fixedDeltaTime;
        transform.rotation = Quaternion.Euler(Vector3.forward * laserAngle);
        explosionTimer += Time.fixedDeltaTime;
        ShootLaser(laserSpawnMiddle.GetComponent<Transform>(), lineRendererMiddle);
        ShootLaser(laserSpawnLeft.GetComponent<Transform>(), lineRendererLeft);
        ShootLaser(laserSpawnRight.GetComponent<Transform>(), lineRendererRight);
    }
    //External Functions
    private void ShootLaser(Transform laserSpawnPoint, LineRenderer lineRenderer)
    {
        RaycastHit2D hit = Physics2D.Raycast(laserSpawnPoint.position, transform.right, 100, blockLaserLayers);
        if (hit.point == null)
        {
            return;
        }
        Draw2DRay(laserSpawnPoint.position, hit.point, lineRenderer);
        if (hit.collider == null) return;
        ObjectHit = hit.collider.gameObject;
        if (ObjectHit.layer == 3)
        {
            DealDamage(ObjectHit);
        }
    }
    void Draw2DRay(Vector2 startPosition, Vector2 endPosition, LineRenderer lineRenderer)
    {
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
    }
    void DealDamage(GameObject ObjectHit)
    {
        playerScript = ObjectHit.GetComponent<PlayerScript>();
        if (playerScript != null)
        {
            playerScript.DamagePlayer(defaultDamage, new Vector2(0, 0), 0, defaultInvincibleTime);
        }
    }
    void enableLaserRenderer()
    {
        lineRendererMiddle.enabled = true;
        lineRendererLeft.enabled = true;
        lineRendererRight.enabled = true;
        laserEnabled = true;
    }
    void disableLaserRenderer()
    {
        lineRendererMiddle.enabled = false;
        lineRendererLeft.enabled = false;
        lineRendererRight.enabled = false;
        laserEnabled = false;
    }
}
