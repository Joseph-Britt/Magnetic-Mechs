using UnityEngine;

public class HomingNonPlatformTurret : Turret
{
    [SerializeField] private bool facingRight = true;
    private Transform player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        shootingAngle = calculateShootingAngle();

        SetUpTurret(facingRight);
    }

    private void Update()
    {
        shootingAngle = calculateShootingAngle();
        transform.eulerAngles = new Vector3(0f, 0f, shootingAngle);
    }

    private float calculateShootingAngle()
    {
        Vector2 playerRelativePosition = (Vector2)(player.transform.position - transform.position);
        return Mathf.Clamp(Vector2.SignedAngle(facingRight ? Vector2.right : Vector2.left, playerRelativePosition), -45f, 45f);
    }
}
