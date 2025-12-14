using UnityEngine;
using UnityEngine.UI;
using System;


public class VerticalMovementScript : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D playerRigidBody;
    public PlankScript handlePlanks;
    public Image remainingFuelImage;
    public GameObject remainingFuelParent;

    [Header("Scripts")]
    public PlayerAnimationManagerScript playerAnimationManagerScript;
    public PlayerPhysicsScript playerPhysicsScript;
    public PlayerScript playerScript;

    [Header("Variables")]
    private float maxYSpeed = 20f;

    [Header("Timers")]
    private float remainingFuelTimer = 0;
    private float remainingFuelTimeToDisappear = .5f;

    [Header("Ground Checks")]
    public bool onGround = false;
    public bool overlappingGround = false;
    public bool trulyOnGround = false;
    public bool nearGround = false;
    private float groundLength = .9f;
    private float nearGroundLength = 1.25f;
    private float legLength = .78f;
    private Vector3 distanceToLeg = new Vector3(.42f, 0, 0);
    public LayerMask groundLayer;

    [Header("Jumping")]
    public bool jumpPressed = false;
    public float jumpTimer;
    private float jumpDelay = .15f;
    public float maxYSpeedTimer;
    private float maxYSpeedDelay = .7f;
    private float jumpForce = 7f;

    [Header("Jetpack")]
    private float jetpackTotalTime = 1.2f;
    public float jetpackCurrentTime;
    private float jetPackForce = 12f;
    private float maxJetSpeed = 19f;
    private float jetpackRecoveryTimer = 0f;
    private float jetpackRecoveryTime = 0.25f;
    private float jetPackTimeRecoveryMultiplier = .85f;
    private float slowSpeedMultiplyer = 1.4f;
    private bool jetpackOn;
    public AudioSource jetpackAudio;

    [Header("Jetpack Components")]
    public GameObject jetpackLower;
    public GameObject jetpackBackwards;
    public bool jetpackBackwardsOn;

    void Awake()
    {
        groundLayer = LayerMask.GetMask("Ground", "Plank Ground");
        jetpackCurrentTime = jetpackTotalTime;
    }

    public bool handleVerticalUpdates(float verticalDirection, bool playerAlive, bool jump)
    {
        jumpPressed = jump;
        if (verticalDirection <= -.25f && handlePlanks != null)
        {
            groundLayer = LayerMask.GetMask("Ground");
            handlePlanks.disablePlanks();
        }
        else
        {
            {
                groundLayer = LayerMask.GetMask("Ground", "Plank Ground");
            }
        }
        onGround = (Physics2D.Raycast(transform.position - distanceToLeg, Vector2.down, groundLength, groundLayer) || Physics2D.Raycast(transform.position + distanceToLeg, Vector2.down, groundLength, groundLayer));
        overlappingGround = (Physics2D.Raycast(transform.position - distanceToLeg, Vector2.down, legLength, groundLayer) || Physics2D.Raycast(transform.position + distanceToLeg, Vector2.down, legLength, groundLayer));
        trulyOnGround = onGround && !overlappingGround;
        nearGround = Physics2D.Raycast(transform.position - distanceToLeg, Vector2.down, nearGroundLength, groundLayer) || Physics2D.Raycast(transform.position + distanceToLeg, Vector2.down, nearGroundLength, groundLayer) && !overlappingGround;
        if (playerAlive)
        {
            //playerAnimationManagerScript.setLanding(trulyOnGround, onGround, myRigidbody2D.linearVelocity.y);
            playerAnimationManagerScript.setLanding(nearGround);
        }
        //Method One Recover on Ground
        if (trulyOnGround)
        {
            jetpackCurrentTime = jetpackTotalTime;
            handleJetPackTime();
        }
        jetpackOn = jumpPressed && (!trulyOnGround);
        if (jetpackOn)
        {
            if (jetpackCurrentTime <= 0)
            {
                jetpackOn = false;
            }
            else
            {
                jetpackCurrentTime -= Time.deltaTime;
                jetpackRecoveryTimer = 0f;
                handleJetPackTime();
            }
        }
        //Method Two Recover whenever jetpack isn't in use
        if (!jetpackOn && jetpackCurrentTime < jetpackTotalTime)
        {
            jetpackRecoveryTimer += Time.deltaTime;
            if (jetpackRecoveryTimer > jetpackRecoveryTime)
            {
                jetpackCurrentTime += Time.deltaTime * 1.4f;
            }
            handleJetPackTime();
        }
        if (jumpPressed)
        {
            jumpTimer = Time.time + jumpDelay;
            if (trulyOnGround)
            {
                maxYSpeedTimer = Time.time + maxYSpeedDelay;
            }
        }
        if (jetpackAudio != null)
        {
            if (jetpackOn)
            {
                if (!jetpackAudio.isPlaying) jetpackAudio.Play();
            }
            else
            {
                jetpackAudio.Stop();
            }
        }
        return jetpackOn;
    }

    public void handleJetPackTime()
    {
        int remainingFuelPercent = (int)(100 * jetpackCurrentTime / jetpackTotalTime);
        if (remainingFuelPercent < 0)
        {
            remainingFuelPercent = 0;
        }
        remainingFuelImage.fillAmount = remainingFuelPercent / 100.0f;
    }
    public void handleRemainingFuelBar()
    {
        if (jetpackCurrentTime >= jetpackTotalTime)
        {
            remainingFuelTimer += Time.deltaTime * jetPackTimeRecoveryMultiplier;
            if (remainingFuelTimer > remainingFuelTimeToDisappear)
            {
                remainingFuelParent.SetActive(false);
            }
        }
        else
        {
            remainingFuelParent.SetActive(true);
            remainingFuelTimer = 0;
        }
    }

    public void SetJetpackSprites(float direction)
    {
        jetpackLower.GetComponent<JetpackScript>().setJetpack(jetpackOn);
        //jetpackLowerRight.GetComponent<JetpackScript>().setJetpack(jetpackOn);
        jetpackBackwardsOn = !trulyOnGround && Mathf.Abs(direction) > 0;
        jetpackBackwards.GetComponent<JetpackScript>().setJetpack(jetpackBackwardsOn);
    }
    public void handleVerticalMovement()
    {
        //handles checks and related to vertical movement once every Update cycle
        if (trulyOnGround && jumpTimer > Time.time)
        {
            jump();
        }
        if (!trulyOnGround && jetpackOn)
        {
            float currentJetPackForce = jetPackForce;
            if (playerRigidBody.linearVelocity.y < 5)
            {
                currentJetPackForce *= slowSpeedMultiplyer;
            }
            playerRigidBody.AddForce(new Vector2(0, currentJetPackForce));

            if (playerRigidBody.linearVelocity.y > maxJetSpeed && maxYSpeedTimer < Time.time)
            {
                if (playerScript.repelOn || playerScript.attractOn)
                {
                    return;
                }
                playerPhysicsScript.ApplyMaxJetpackSpeed();
                //myRigidbody2D.velocity = new Vector2(myRigidbody2D.velocity.x, maxJetSpeed);
            }
        }
        adjustMaxYSpeed();
    }
    void adjustMaxYSpeed()
    {
        //turns on damping if the player is above a certain y speed
        float currentMaxSpeed = playerScript.getMagnetMaxYSpeed(maxYSpeed);
        playerPhysicsScript.ApplyMaxVerticalSpeedDrag(currentMaxSpeed);
    }

    private void jump()
    {
        playerRigidBody.linearVelocity = new Vector2(playerRigidBody.linearVelocity.x, 0);
        playerRigidBody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        //jumpSound.Play();
        //Instantiate(JumpDust, new Vector3(transform.position.x, transform.position.y - groundLength * 3 / 4, transform.position.z), transform.rotation);
        jumpTimer = 0;
    }

    public bool IsOnMovingPlatform(out Rigidbody2D platformRb)
    {
        platformRb = null;
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position - distanceToLeg, Vector2.down, groundLength, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position + distanceToLeg, Vector2.down, groundLength, groundLayer);
        RaycastHit2D[] hits = { hitLeft, hitRight };
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("MovingPlatform"))
            {
                platformRb = hit.collider.attachedRigidbody;
                return true;
            }
        }
        return false;
    }

    public void PlayerKilled()
    {
        if (jetpackAudio != null && jetpackAudio.isPlaying) jetpackAudio.Stop();
        jetpackLower.GetComponent<JetpackScript>().setJetpack(false);
        //jetpackLowerRight.GetComponent<JetpackScript>().setJetpack(false);
        jetpackBackwards.GetComponent<JetpackScript>().setJetpack(false);
    }
    
    public bool returnTrulyOnGround()
    {
        return trulyOnGround;
    }
    public bool returnJetpackOn()
    {
        return jetpackOn;
    }
    public Vector3 getDistanceToLeg()
    {
        return distanceToLeg;
    }
    public float getGroundLength()
    {
        return groundLength;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position - distanceToLeg, transform.position - distanceToLeg + Vector3.down * legLength);
        Gizmos.DrawLine(transform.position + distanceToLeg, transform.position + distanceToLeg + Vector3.down * groundLength);
    }
}
