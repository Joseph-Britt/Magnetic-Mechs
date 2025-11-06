using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class WallGoomechScript : GoomechScript
{
    // must be a factor of 90, otherwise the goomech will not reach a walkable angle (0, 90, 180, 270) once it starts turning
    private static readonly float TURN_SPEED = 10.0f;
    [SerializeField]
    private static readonly float WALK_SPEED = 0.08f;

    private static readonly float CHECK_LENGTH = 0.5f;
    // the distance in front of the goomech at which to check whether to start a convex turn
    private static readonly float GROUND_IN_FRONT_CHECK_LENGTH = 0.4f;
    // distance units per frame to walk while turning
    private static readonly float TURN_WALK_SPEED = 0.12f;


    private bool turning = false;
    // whether currently turning concave; only relevant when turning == true
    private bool turningConcave = true;
    // stores last position at which the goomech was not turning; useful for correcting anomalies after a turn
    private Vector3 lastWalkingPosition;



    new private void Awake()
    {
        base.Awake();
        myRigidBody2D.gravityScale = 0;
        Debug.Log("I'm a wall goomech");
        base.Flip();
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
        bool groundInFront = Physics2D.Raycast(transform.position + (GROUND_IN_FRONT_CHECK_LENGTH * transform.right), -1 * transform.up, groundCheckHeight, groundLayer);

        // free fall, gravity and movement
        if (onGround || turning)
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

        // // enemy or spike: turn around
        // if (approachingEnemy || approachingSpike)
        // {
        //     if (approachingEnemy)
        //     {
        //         Debug.Log("Enemy: flipped");
        //     }
        //     else
        //     {
        //         Debug.Log("Spike: flipped");
        //     }
        //     Flip();
        // } 




        if (!movementEnabled) return;

        // ==== movement ====

        // concave turns
        if ((turning && turningConcave) || approachingWall)
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

            turning = newRotation.z % 90 != 0;
            turningConcave = true;

            // correct position anomalies
            if (!turning)
            {
                transform.position = lastWalkingPosition;
            }
        }

        // turn convex
        else if ((turning && !turningConcave) || !groundInFront)
        {
            Debug.Log("Convex turn");
            myRigidBody2D.linearVelocity = Vector3.zero;
            myRigidBody2D.constraints = RigidbodyConstraints2D.FreezePosition;
            myRigidBody2D.bodyType = RigidbodyType2D.Kinematic;
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.z +=
                ((this.facingRight) ? -1 : 1)
                * TURN_SPEED;
            // round to nearest multiple of TURN_SPEED to avoid float imprecision
            newRotation.z = (float) System.Math.Round(newRotation.z / TURN_SPEED) * TURN_SPEED;
            transform.rotation = Quaternion.Euler(newRotation);
            turning = newRotation.z % 90 != 0;
            turningConcave = false;

            // correct position anomalies after finished turning
            // if (!turning)
            // {
            //     Vector3 newPosition = lastWalkingPosition;
            //     // get direction in range [0, 360)
            //     int direction = (int) System.Math.Round(transform.rotation.eulerAngles.z);
            //     while (direction < 0)
            //     {
            //         direction += 360;
            //     }
            //     while (direction >= 360)
            //     {
            //         direction -= 360;
            //     }
            //     // facing right (on ground)
            //     if (direction == 0)
            //     {
            //         newPosition.x += NET_TURN_POSITION_CHANGE;
            //         newPosition.y += NET_TURN_POSITION_CHANGE;
            //     }
            //     // facing up (climbing wall)
            //     else if (direction == 90)
            //     {
            //         newPosition.x -= NET_TURN_POSITION_CHANGE;
            //         newPosition.y += NET_TURN_POSITION_CHANGE;
            //     }
            //     // facing left (on ceiling)
            //     else if (direction == 180)
            //     {
            //         newPosition.x -= NET_TURN_POSITION_CHANGE;
            //         newPosition.y -= NET_TURN_POSITION_CHANGE;
            //     }
            //     // facing down (descending wall)
            //     else if (direction == 270)
            //     {
            //         newPosition.x += NET_TURN_POSITION_CHANGE;
            //         newPosition.y -= NET_TURN_POSITION_CHANGE;
            //     }

            //     transform.position = newPosition;
            //     lastWalkingPosition = transform.position;
            // }
            if (!turning)
            {
                Debug.Log($"Recommended NET_TURN_POSITION_CHANGE: {(System.Math.Abs(transform.position.y - lastWalkingPosition.y) + System.Math.Abs(transform.position.x - lastWalkingPosition.x)) / 2.0f}");
            }
        }

        // walk
        if ((!turning && groundInFront) || (turning && !turningConcave))
        {
            Debug.Log($"Walk; turning: {turning}, groundInFront: {groundInFront}, turningConcave: {turningConcave}");
            // freeze x or y
            if (transform.rotation.eulerAngles.z % 180 == 0)
            {
                myRigidBody2D.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            }
            else if ((transform.rotation.eulerAngles.z + 90) % 180 == 0)
            {
                myRigidBody2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }
            // myRigidBody2D.bodyType = RigidbodyType2D.Dynamic;
            myRigidBody2D.bodyType = RigidbodyType2D.Kinematic;
            float directionDeg = transform.rotation.eulerAngles.z;
            float directionRad = directionDeg * (float) System.Math.PI / 180;
            // trying manually editing position instead of using speed
            // myRigidBody2D.linearVelocity = speed * new Vector3((float) System.Math.Cos(direction), (float) System.Math.Sin(direction), 0);
            myRigidBody2D.transform.position += (facingRight ? 1 : -1) * (turning ? TURN_WALK_SPEED : WALK_SPEED) * new Vector3((float) System.Math.Cos(directionRad), (float) System.Math.Sin(directionRad), 0);

            lastWalkingPosition = myRigidBody2D.transform.position;
        }

        



    }
}