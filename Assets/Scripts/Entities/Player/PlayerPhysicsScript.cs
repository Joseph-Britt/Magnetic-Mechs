using UnityEngine;
using System;

public class PlayerPhysicsScript : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D playerRigidBody;
    [Header("Dust Effects")]
    public ParticleSystem changeDirectionDust;
    [Header("Gravity")]
    private float linearDrag = 3f;
    public float gravity = 1f;
    private float fallMultiplier = 3f;
    [Header("Drag Values")]
    private float defaultDrag = .05f;
    private float clampXDrag = 2.5f;
    private float clampYDrag = 3.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyMaxJetpackSpeed()
    {
        if (playerRigidBody.linearDamping < clampYDrag) playerRigidBody.linearDamping = clampYDrag / 2;
    }
    public void ApplyMaxHorizontalSpeedDrag(float currentMaxSpeed)
    {
        if (MathF.Abs(playerRigidBody.linearVelocity.x) > currentMaxSpeed && playerRigidBody.linearDamping < clampXDrag)
        {
            //myRigidbody2D.velocity = new Vector2(MathF.Sign(myRigidbody2D.velocity.x) * currentMaxSpeed, myRigidbody2D.velocity.y);
            playerRigidBody.linearDamping = clampXDrag;
        }
    }
    public void ApplyMaxVerticalSpeedDrag(float currentMaxSpeed)
    {
        if (MathF.Abs(playerRigidBody.linearVelocity.y) > currentMaxSpeed && playerRigidBody.linearDamping < clampYDrag)
        {
            //myRigidbody2D.velocity = new Vector2(myRigidbody2D.velocity.x, MathF.Sign(myRigidbody2D.velocity.y) * currentMaxSpeed);
            playerRigidBody.linearDamping = clampYDrag;
        }
    }
    public void modifyPhysics(bool jetpackOn, float direction, bool trulyOnGround)
    {
        bool changingDirection = (direction > 0 && playerRigidBody.linearVelocity.x < 0) || (direction < 0 && playerRigidBody.linearVelocity.x > 0);
        if (trulyOnGround)
        {
            playerRigidBody.gravityScale = 0;
            if (MathF.Abs(direction) == 0f || changingDirection)
            {
                playerRigidBody.linearDamping = linearDrag * 2.5f;
                if (changingDirection)
                {
                    CreateDust();
                }
            }
            else
            {
                playerRigidBody.linearDamping = defaultDrag;
            }
        }
        else
        {
            playerRigidBody.gravityScale = gravity;
            playerRigidBody.linearDamping = linearDrag * .15f;
            if (jetpackOn)
            {
                playerRigidBody.gravityScale = gravity / 2;
            }
            if (playerRigidBody.linearVelocity.y < 0f)
            {
                if (jetpackOn)
                {
                    playerRigidBody.linearDamping = linearDrag;
                }
                else
                {
                    playerRigidBody.gravityScale = gravity * fallMultiplier;
                }
            }
            else if (playerRigidBody.linearVelocity.y > 0f && !jetpackOn)
            {
                playerRigidBody.gravityScale = gravity * fallMultiplier / 2;
            }

        }
    }
    private void CreateDust()
    {
        changeDirectionDust.Play();
    }
}
