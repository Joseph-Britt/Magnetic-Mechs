using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

/*
 * This is the script for the WallGoomech prefab, a variant of goomech that crawls along walls and ceilings.
 * A few things to note:
 * They will never be deflected by walls or cliffs; they always respond to these situations by climbing the wall/cliff.
 * They will turn around on reaching another enemy
 * They walk through spikes
 * If you give a camera to the serialized field "cam", then they will only move when visible in that camera
 * If they are spawned in midair, they will fall straight down and latch onto the ground that they land on
 * Occasionally, if they are very dense in one area, they might begin to interact strangely (ex. rapid turns instead of flips). This isn't a problem unless they are closely packed.
 */



public class WallGoomechScript : GoomechScript
{
    // must be a factor of 90, otherwise the goomech will not reach a walkable angle (0, 90, 180, 270) once it starts turning
    private static readonly float TURN_SPEED = 10.0f;
    private static readonly float WALK_SPEED = 0.08f;
    private static readonly float CHECK_LENGTH = 0.5f;
    // the distance in front of the goomech at which to check whether to start a convex turn
    private static readonly float GROUND_IN_FRONT_CHECK_LENGTH = 0.4f;
    // distance units per frame to walk while turning
    private static readonly float TURN_WALK_SPEED = 0.12f;
    // amount of distance to ignore when raycasting for other enemies to avoid reacting to self
    private static readonly float ENEMY_RAYCAST_BUFFER_DIST = 0.55f;

    private bool justFlipped = false;
    private bool turning = false;
    // whether currently turning concave; only relevant when turning == true
    private bool turningConcave = true;
    // stores last position at which the goomech was not turning; useful for correcting anomalies after a turn
    private Vector3 facingDirection;
    // use this in Unity editor to set which direction to face initially
    [SerializeField]
    private Camera cam;





    new private void Awake()
    {
        base.Awake();
        myRigidBody2D.bodyType = RigidbodyType2D.Kinematic;
        if (!initialFaceRight)
        {
            Flip();
        }
    }
    private void Update()
    {
        // do nothing if dead or out of camera
        if (!isAlive || !InCamera())
        {
            return;
        }

        // edge case: starting behavior
        if (startingBehavior)
        {
            SpawnBehavior();
            return;
        }

        // targeting reticle
        if (includePrompt) handleTargetingReticle();

        // raycasts
        facingDirection = transform.right.normalized;
        approachingWall = Physics2D.Raycast(transform.position, facingDirection, CHECK_LENGTH, groundLayer);
        approachingSpike = Physics2D.Raycast(transform.position, facingDirection, CHECK_LENGTH * 1.65f, spikeLayer);
        approachingEnemy = Physics2D.Raycast(transform.position + ENEMY_RAYCAST_BUFFER_DIST * facingDirection, facingDirection, CHECK_LENGTH, enemyLayer);
        onGround = Physics2D.Raycast(transform.position, -1 * transform.up, groundCheckHeight, groundLayer);
        bool groundInFront = Physics2D.Raycast(transform.position + (GROUND_IN_FRONT_CHECK_LENGTH * facingDirection), -1 * transform.up, groundCheckHeight, groundLayer);

        // free fall, gravity and movement
        if (onGround || turning)
        {
            myRigidBody2D.bodyType = RigidbodyType2D.Kinematic;
            this.movementEnabled = true;
        }
        // if falling
        else
        {
            myRigidBody2D.bodyType = RigidbodyType2D.Dynamic;
            this.movementEnabled = false;

            // point downwards; looks cool, helps land correctly
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));

            // land on feet if ground directly underneath
            if (Physics2D.Raycast(transform.position, new Vector3(0, -1, 0), CHECK_LENGTH, groundLayer))
            {
                this.movementEnabled = true;
            }
        }

        // enemy or spike: turn around
        if (approachingEnemy)
        {
            Flip();
        }

        if (!movementEnabled) return;

        // ==== movement ====

        if (justFlipped)
        {
            justFlipped = false;
        }
        else
        {
            // concave turns
            if ((turning && turningConcave) || approachingWall)
            {
                Turn(true);
            }

            // turn convex
            else if ((turning && !turningConcave) || !groundInFront)
            {
                Turn(false);
            }
        }

        // walk
        if ((!turning && groundInFront) || (turning && !turningConcave))
        {
            Walk();
        }
    }

    private void Walk()
    {
        // freeze x or y
        if (transform.rotation.eulerAngles.z % 180 == 0)
        {
            myRigidBody2D.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        }
        else if ((transform.rotation.eulerAngles.z + 90) % 180 == 0)
        {
            myRigidBody2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        myRigidBody2D.transform.position += facingDirection * (turning ? TURN_WALK_SPEED : WALK_SPEED);
    }

    private void Turn(bool concave)
    {
        if (concave)
        {
            myRigidBody2D.linearVelocity = Vector3.zero;
            myRigidBody2D.constraints = RigidbodyConstraints2D.FreezePosition;
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.z += TURN_SPEED;
            // round to nearest multiple of TURN_SPEED to avoid float imprecision
            newRotation.z = (float)System.Math.Round(newRotation.z / TURN_SPEED) * TURN_SPEED;
            transform.rotation = Quaternion.Euler(newRotation);

            turning = newRotation.z % 90 != 0;
            turningConcave = true;
        }
        else
        {
            myRigidBody2D.linearVelocity = Vector3.zero;
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.z -= TURN_SPEED;
            // round to nearest multiple of TURN_SPEED to avoid float imprecision
            newRotation.z = (float)System.Math.Round(newRotation.z / TURN_SPEED) * TURN_SPEED;
            transform.rotation = Quaternion.Euler(newRotation);
            turning = newRotation.z % 90 != 0;
            turningConcave = false;
        }
    }

    private bool InCamera()
    {
        if (cam == null) return true;
        Vector3 pos = cam.WorldToViewportPoint(transform.position);
        return pos.x >= 0 && pos.x <= 1 && pos.y >= 0 && pos.y <= 1;
    }

    new public void Flip()
    {
        bool horizontal = transform.rotation.eulerAngles.z % 180 == 0;
        Vector3 newRotation = transform.rotation.eulerAngles;
        if (horizontal)
        {
            newRotation.y += 180;
        }
        else
        {
            newRotation.x += 180;
        }
        transform.rotation = Quaternion.Euler(newRotation);
        justFlipped = true;
    }
}