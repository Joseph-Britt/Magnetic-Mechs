using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class WallGoomechScript : GoomechScript
{
    // must be a factor of 90, otherwise the goomech will not reach a walkable angle (0, 90, 180, 270) once it starts turning
    private static readonly float TURN_SPEED = 10.0f;

    // the smallest value I could pick that consistently gets the goomech to turn is 0.6, even then doesn't work sometimes
    private static readonly float CHECK_LENGTH = 0.7f;

    private void Awake()
    {
        base.Awake();
        myRigidBody2D.gravityScale = 0;
        Debug.Log("I'm a wall goomech");
    }
    private void Update()
    {
        // edge case: starting behavior
        if (startingBehavior)
        {
            SpawnBehavior();
            return;
        }

        // targeting reticle
        if (includePrompt) handleTargetingReticle();




        // raycasts
        Vector2 facingDirection = transform.right.normalized * (facingRight ? 1 : -1);
        approachingWall = Physics2D.Raycast(transform.position, facingDirection, CHECK_LENGTH, groundLayer);
        approachingSpike = Physics2D.Raycast(transform.position, facingDirection, CHECK_LENGTH * 1.65f, spikeLayer);
        approachingEnemy = Physics2D.Raycast((Vector2)transform.position + facingDirection * 4, facingDirection, CHECK_LENGTH, enemyLayer);
        onGround = Physics2D.Raycast(transform.position, -1 * transform.up, groundCheckHeight, groundLayer);
        bool groundInFront = Physics2D.Raycast(transform.position + (0.1f * transform.right), -1 * transform.up, groundCheckHeight, groundLayer);

        // free fall, gravity and movement
        if (onGround)
        {
            myRigidBody2D.gravityScale = 0;
            this.movementEnabled = true;
        }
        else
        {
            Debug.Log("Free fall");
            myRigidBody2D.gravityScale = 1;
            this.movementEnabled = false;
            return;
        }

        // enemy or spike: turn around
        if (approachingEnemy || approachingSpike)
        {
            if (approachingEnemy)
            {
                Debug.Log("Enemy: flipped");
            }
            else
            {
                Debug.Log("Spike: flipped");
            }
            Flip();
        } 




        if (!movementEnabled) return;

        // ==== movement ====

        // concave turns
        if (approachingWall || ((int) transform.eulerAngles.z) % 90 != 0)
        {
            Debug.Log("Concave turn" + (approachingWall ? " wall" : ""));
            myRigidBody2D.linearVelocity = Vector3.zero;
            myRigidBody2D.constraints = RigidbodyConstraints2D.FreezePosition;
            myRigidBody2D.bodyType = RigidbodyType2D.Kinematic;
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.z +=
                ((this.facingRight) ? 1 : -1)
                * TURN_SPEED;
            // round to nearest multiple of TURN_SPEED to avoid float imprecision
            newRotation.z = (float) System.Math.Round(newRotation.z / TURN_SPEED) * TURN_SPEED;
            transform.rotation = Quaternion.Euler(newRotation);
        }

        // walk
        else if (groundInFront)
        {
            Debug.Log("Walk");
            // freeze x or y
            if (transform.rotation.eulerAngles.z % 180 == 0)
            {
                myRigidBody2D.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            }
            else if ((transform.rotation.eulerAngles.z + 90) % 180 == 0)
            {
                myRigidBody2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }
            myRigidBody2D.bodyType = RigidbodyType2D.Dynamic;
            float direction = transform.rotation.eulerAngles.z;
            myRigidBody2D.linearVelocity = speed * new Vector3((float)System.Math.Cos(direction), (float)System.Math.Sin(direction), 0);
        }

        else
        {
            Debug.Log("Convex turn");
        }

        // else: turn convex


    }
}