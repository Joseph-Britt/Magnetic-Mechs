using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class WallGoomechScript : GoomechScript
{
    private void Awake() 
    {
        base.Awake();
        myRigidBody2D.gravityScale = 0;
        Debug.Log("I'm a wall goomech"); 
    }
    private void Update()
    {
        if (startingBehavior)
        {
            SpawnBehavior();
            return;
        }
        approachingWall = Physics2D.Raycast(transform.position, facingRight ? Vector2.right : Vector2.left, horizontalCheckLength, groundLayer);
        approachingSpike = Physics2D.Raycast(transform.position, facingRight ? Vector2.right : Vector2.left, horizontalCheckLength *1.65f, spikeLayer);
        approachingEnemy = Physics2D.Raycast(transform.position + Vector3.right * (horizontalCheckLength - .01f) * (facingRight ? 1 : -1), facingRight ? Vector2.right : Vector2.left, horizontalCheckLength, enemyLayer);
        if (approachingWall || approachingEnemy || approachingSpike)
        {
            Flip();
        }
        if (movementEnabled)
        {
            myRigidBody2D.linearVelocity = new Vector3(speed * (facingRight ? 1 : -1), myRigidBody2D.linearVelocity.y, 0);
        }
        if (includePrompt) handleTargetingReticle();
        // whether this enemy is on ground, regardless of which direction that ground faces
        groundBeneath = Physics2D.Raycast(transform.position, Vector2.down, groundCheckHeight, groundLayer);
        if (!groundBeneath)
        {
            Debug.Log("No ground beneath");
        }
    }
}