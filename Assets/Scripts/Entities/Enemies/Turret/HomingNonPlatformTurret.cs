using System;
using UnityEngine;

public class HomingNonPlatformTurret : Turret
{
    private Transform player;
    private float baseAngle;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        baseAngle = transform.rotation.eulerAngles.z;
        shootingAngle = calculateShootingAngle();

        SetUpTurret();
    }

    private void Update()
    {
        shootingAngle = calculateShootingAngle();
        transform.eulerAngles = new Vector3(0f, 0f, shootingAngle);
    }

    private float calculateShootingAngle()
    {
        Vector2 playerRelativePosition = (Vector2)(player.transform.position - transform.position);
        float offsetAngle = Mathf.Clamp(Vector2.SignedAngle(new Vector2(Mathf.Cos(Mathf.Deg2Rad * baseAngle), Mathf.Sin(Mathf.Deg2Rad * baseAngle)), playerRelativePosition), -45f, 45f);
        return baseAngle + offsetAngle;
    }
}
