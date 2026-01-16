using UnityEngine;

public class HomingNonPlatformTurret : Turret
{
    private Transform player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

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
        return Mathf.Clamp(Vector2.SignedAngle(Vector2.right, playerRelativePosition), -45f, 45f);
    }
}
