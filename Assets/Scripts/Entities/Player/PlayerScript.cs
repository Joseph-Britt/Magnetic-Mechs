using System;
using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{

    //main script for managing the player
    [Header("Components")]
    public Rigidbody2D myRigidbody2D;
    public CapsuleCollider2D myCapsuleCollider2D;
    public PlayerAnimationManagerScript playerAnimationManagerScript;
    public PlayerHealthScript healthScript;
    public AudioSource jumpSound;
    public CutsceneManager cutsceneManagerScript;
    public MultiSceneVariables savedVariables;
    public PlayerInput myInput;
    public GameObject pointerArrow;
    public LogicScript logic;

    public MagnetVisualEffectScript magnetVisualEffectScript;
    public GameObject DeathAnimation;
    [Header("Scripts")]
    public PlayerPhysicsScript myPlayerPhysicsScript;
    public VerticalMovementScript myVerticalMovementScript;

    [Header("Logic")]
    private bool playerAlive = true;
    public bool gamePadNotMouse = false;


    [Header("Inputs")]
    public bool jumpPressed;
    private bool jetpackOn;


    [Header("Horizontal Movement")]
    public float direction;
    private float baseSpeed = 15f;
    private float maxSpeed = 11f;
    public float horizontalSpeed;
    public bool facingRight = true;
    public bool movementDisabled;

    [Header("Vertical Movement")]
    public float verticalDirection;

    [Header("To Health")]
    private float knockbackTime = 0.25f;
    const float invincibilityTimeDefault = .5f;
    private float inGroundTimer = 0f;
    private float inGroundKillTime = .15f;
    public LayerMask inGroundLayer;
    private Vector3 inGroundOffset = new Vector3(0, .25f, 0);



    [Header("Charging")]
    public bool chargePressed = false;
    public bool isCharging = false;
    private float chargeTime = 1f;
    private float chargeTimer = 0f;
    private float chargeCooldown = 3f;
    private float chargeCooldownTimer = 0f;
    private float chargeSpeed = 21f;
    public Sprite chargeIndicatorImage;
    public Sprite invincibilityIndicatorImage;
    public Sprite chargeCooldownIndicatorImage;
    public SpriteRenderer chargeIndicator;




    [Header("Physics")]
    public bool repelOn = false;
    public bool attractButtonHeld = false;
    public bool attractOn = false;
    public bool holdToAttract = false;

    [Header("Orientation")]
    public Camera virtualCamera;
    public Vector2 mousePosition;
    public Vector2 mouseRelativePosition;

    [Header("Input")]
    public GameObject BulletSpawner;
    public BulletSpawnerScript bulletSpawnerScript;
    public bool shootingInput = false;
    public Vector2 rightJoystick = Vector2.left;

    [Header("Magnet")]
    public GameObject MagnetSpawner;
    public MagnetSpawnerScript magnetSpawnerScript;
    private bool launchMagnetHeld = false;
    private bool launchMagnet = false;
    private GameObject myMagnet;
    private Vector2 magnetRelativePosition;
    private float magnetDistance;

    [Header("Magnet")]
    private float magnetDistanceMultiplyingForceRepulsion = 98;
    private float magnetDistanceMultiplyingForceAttraction = -87;
    private float magnetBaseForceRepulsion = 7f;
    private float magnetBaseForceAttraction = -7f;
    private float maximumMagnetDistance = 30;  
    public AudioSource magnetAttractionAudio;
    public AudioSource magnetRepulsionAudio;

    [Header("Platform Friction")]
    public PhysicsMaterial2D lowFrictionMaterial;
    public PhysicsMaterial2D highFrictionMaterial;
    [Range(0f,1000f)] public float frictionLerpSpeed = 10f;
    public float inputMemoryDuration = 0.1f;

    [Header("Recent Input Timers")]
    public float lastMoveInputTime = -10f;
    public float lastJumpInputTime = -10f;
    public float lastRepelInputTime = -10f;
    public float lastAttractInputTime = -10f;
    public bool checkMovementInput = false;
    public bool checkJumpInput = false;

    public Vector3 lastPlatformPosition;
    private void Awake()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        //animator = GetComponent<Animator>();
        //sprite = GetComponent<SpriteRenderer>();
        myInput = GetComponent<PlayerInput>();
        healthScript = GameObject.FindGameObjectWithTag("PlayerHealth").GetComponent<PlayerHealthScript>();
        virtualCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        GameObject savedVariablesObject = GameObject.FindGameObjectWithTag("MultiSceneVariables");
        if(savedVariablesObject != null)
        {
            gamePadNotMouse = savedVariablesObject.GetComponent<MultiSceneVariables>().gamePadNotMouse;
        }
        if (gamePadNotMouse)
        {
            myInput.defaultControlScheme = "Gamepad";
            pointerArrow.GetComponent<SpriteRenderer>().enabled = true;
        }
        else myInput.defaultControlScheme = "KeyboardMouse";
        BulletSpawner = GameObject.FindGameObjectWithTag("BulletSpawner");
        bulletSpawnerScript = BulletSpawner.GetComponent<BulletSpawnerScript>();
        MagnetSpawner = GameObject.FindGameObjectWithTag("MagnetSpawner");
        magnetSpawnerScript = MagnetSpawner.GetComponent<MagnetSpawnerScript>();
        inGroundLayer = LayerMask.GetMask("Ground");
        
    }

    private void Start() {
        holdToAttract = InputRebinding.Instance.GetHoldToAttract();
        InputRebinding.Instance.OnHoldToAttractChanged += InputRebinding_OnHoldToAttractChanged;
    }

    private void OnDestroy() {
        InputRebinding.Instance.OnHoldToAttractChanged -= InputRebinding_OnHoldToAttractChanged;
    }

    private void InputRebinding_OnHoldToAttractChanged(object sender, EventArgs e) {
        holdToAttract = InputRebinding.Instance.GetHoldToAttract();
    }

    // Update is called once per frame
    void Update()
    {
        attractOn = attractButtonHeld || (holdToAttract && launchMagnetHeld);
        if (!playerAlive || logic.IsPaused)
        {
            return;
        }
        mousePosition = virtualCamera.ScreenToWorldPoint(Input.mousePosition);//orientation
        //Vertical
        jetpackOn = myVerticalMovementScript.handleVerticalUpdates(verticalDirection, playerAlive, jumpPressed);
        myVerticalMovementScript.SetJetpackSprites(direction);
        //Magnet
        if (magnetAttractionAudio != null && magnetRepulsionAudio != null)
        {
            if (repelOn ^ attractOn)
            {
                if (repelOn)
                {
                    if (!magnetRepulsionAudio.isPlaying) magnetRepulsionAudio.Play();
                }
                if (attractOn)
                {
                    if (!magnetAttractionAudio.isPlaying) magnetAttractionAudio.Play();
                }
            }
            else
            {
                magnetAttractionAudio.Stop();
                magnetRepulsionAudio.Stop();
            }
        }
        if (shootingInput)
        {
            bulletSpawnerScript.Shoot();
        }
        if (launchMagnet)
        {
            magnetSpawnerScript.Launch();
            launchMagnet = false;
        }

        //Experiment
        if (checkMovementInput) {
            lastMoveInputTime = Time.time;
        }
        if (checkJumpInput) {
            lastJumpInputTime = Time.time;
        }
    }
    private void FixedUpdate()
    {
        handleGunOrientation();
        if (!playerAlive || movementDisabled)
        {
            //TODO set up dying stuff
            //animator.SetBool("hasDied", false);
            return;
        }
        UpdatePlatformFriction();
        myPlayerPhysicsScript.modifyPhysics(jetpackOn, direction, myVerticalMovementScript.returnTrulyOnGround());
        handleHorizontalMovement();
        myVerticalMovementScript.handleVerticalMovement();
        myVerticalMovementScript.handleRemainingFuelBar();
        handleMagneticRepulsion();
        handleCharging();
        CheckIfStuckInGround();
    }

    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext ctx) { 
        Vector2 v = ctx.ReadValue<Vector2>();

        if (ctx.performed) checkMovementInput = true;
        if (ctx.canceled) checkMovementInput = false;
        //Debug.Log("OnMove Activate");

    }
    public void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext ctx) { 
        if (ctx.performed) {
            jumpPressed = true;
            checkJumpInput = true;

        }
        if (ctx.canceled)
        {
            jumpPressed = false;
            checkJumpInput = false;
        }
        //Debug.Log("OnJump Activate");
    }

    public void OnRepel(UnityEngine.InputSystem.InputAction.CallbackContext ctx) { 
        if (ctx.ReadValueAsButton()) {
           
            lastRepelInputTime = Time.time;
        }
        repelOn = ctx.ReadValueAsButton();
    }
    public void OnAttract(UnityEngine.InputSystem.InputAction.CallbackContext ctx) { 
        if (ctx.ReadValueAsButton()) {
           
            lastAttractInputTime = Time.time;
        }
        attractButtonHeld = ctx.ReadValueAsButton();
    }
    private bool HasRecentInput()
    {
        //Debug.Log(lastMoveInputTime);
        float now = Time.time;
        
        //Debug.Log($"RecentMove={now - lastMoveInputTime < 0.1f}, " +
        //  $"RecentJump={now - lastJumpInputTime < 0.1f}, " +
        //  $"RecentMagnet={now - lastRepelInputTime < 0.1f || now - lastAttractInputTime < 0.1f}");
        //Debug.Log($"lastMoveInputTime={lastMoveInputTime}, lastJumpInputTime={lastJumpInputTime}, lastRepelInputTime={lastRepelInputTime}, lastAttractInputTime={lastAttractInputTime}, now={now}");
        bool recentMove = now - lastMoveInputTime < inputMemoryDuration;
        bool recentJump = now - lastJumpInputTime < inputMemoryDuration;
        bool recentMagnet = (now - lastRepelInputTime < inputMemoryDuration && repelOn && myMagnet != null) ||
            (now - lastAttractInputTime < inputMemoryDuration && attractOn && myMagnet != null);

        return recentMove || recentJump || recentMagnet;
    }

    private float currentFriction = 0f;
    private void UpdatePlatformFriction() {
        if (myRigidbody2D == null) return;
        Rigidbody2D platformRb;
        bool onMovingPlatform = myVerticalMovementScript.IsOnMovingPlatform(out platformRb);
        
        
        bool playerInteracting = HasRecentInput();
        //playerInteracting = false;
        //onMovingPlatform = true;
        //Debug.Log($"onMovingPlatform={onMovingPlatform}, playerInteracting={playerInteracting}");
        float targetFriction = (onMovingPlatform && !playerInteracting) ? highFrictionMaterial.friction : lowFrictionMaterial.friction;
        currentFriction = Mathf.Lerp(currentFriction, targetFriction, frictionLerpSpeed * Time.deltaTime);
        if (myRigidbody2D.sharedMaterial == null) {
            var mat = new PhysicsMaterial2D("runtimeMat") {friction = currentFriction, bounciness = 0f};
            myRigidbody2D.sharedMaterial = mat;
        }
        else
        {
            myRigidbody2D.sharedMaterial.friction = currentFriction;
        }
        //Debug.Log(onMovingPlatform);
        if (onMovingPlatform && platformRb!=null) {
            Vector3 platformDelta;
            if (lastPlatformPosition == Vector3.zero) {
                lastPlatformPosition = platformRb.transform.position;
            }

            Vector3 tempVec = platformRb.transform.position - lastPlatformPosition;
            platformDelta = tempVec.magnitude > 0.5f ? Vector3.zero:tempVec;
            //Debug.Log($"{platformDelta}, {platformRb.transform.position}, {lastPlatformPosition}");


            if (!playerInteracting && currentFriction >= highFrictionMaterial.friction * 0.5f)
            {
                transform.position += platformDelta;
            }
            
            else {
                //The goal is to conserve player momentum when moving on the platform
                transform.position += platformDelta;
            }
            
            
                lastPlatformPosition = platformRb.transform.position;
        }
        else
        {
           lastPlatformPosition = Vector3.zero;
        }

        //Debug.Log($"currentFriction={currentFriction}");

    }
    void handleHorizontalMovement()
    {
        playerAnimationManagerScript.setHorizontalSpeed(Mathf.Abs(direction));
        //animator.SetFloat("HorizontalInput", Mathf.Abs(direction));
        horizontalSpeed = baseSpeed * direction;
        myRigidbody2D.AddForce(Vector2.right * horizontalSpeed);
        if ((horizontalSpeed > 0 && !facingRight) || (horizontalSpeed < 0 && facingRight))
        {
            Flip();
        }
        adjustMaxXSpeed();
    }
    void adjustMaxXSpeed()
    {
        //turns on damping if the player is above a certain x speed
        float currentMaxSpeed = maxSpeed;
        if (repelOn ^ attractOn)
        {
            if (magnetSpawnerScript != null && magnetSpawnerScript.magnetActive)
            {
                Vector2 magnetRelativePosition = transform.position - myMagnet.transform.position;
                float magnetDistance = magnetRelativePosition.magnitude;
                if (magnetDistance < (maximumMagnetDistance))
                {
                    float angle = Mathf.Atan2(magnetRelativePosition.y, magnetRelativePosition.x);
                    float cosAngle = Mathf.Cos(angle);
                    float sign = MathF.Abs(myRigidbody2D.linearVelocity.x) / myRigidbody2D.linearVelocity.x;
                    if (attractOn) sign *= -1;//if attract is on then we want the + in the next step to be a minus
                    currentMaxSpeed = maxSpeed * MathF.Abs(sign + 4 * cosAngle * MathF.Sqrt((maximumMagnetDistance - magnetDistance) / maximumMagnetDistance));
                }
            }
        }
        myPlayerPhysicsScript.ApplyMaxHorizontalSpeedDrag(currentMaxSpeed);
    }
    
    private void Flip()
    {
        facingRight = !facingRight;
        playerAnimationManagerScript.flipLegs(facingRight);
        //transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
        myVerticalMovementScript.remainingFuelImage.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    public void DisableMovement()
    {
        movementDisabled = true;
        myRigidbody2D.linearVelocity = Vector3.zero;
        playerAnimationManagerScript.setHorizontalSpeed(0);
        //animator.SetFloat("HorizontalInput", 0);
    }
    public void EnableMovement()
    {
        movementDisabled = false;
    }
    private void handleGunOrientation()
    {
        if (gamePadNotMouse)
        {
            BulletSpawner.transform.right = rightJoystick;
            MagnetSpawner.transform.right = rightJoystick;
            //playerAnimationManagerScript.setFiringAngle(Vector2.Angle(myRigidbody2D.position, mousePosition));
        }
        else
        {
            mouseRelativePosition = mousePosition - myRigidbody2D.position;
            BulletSpawner.transform.right = mouseRelativePosition;
            MagnetSpawner.transform.right = mouseRelativePosition;
            playerAnimationManagerScript.setFiringAngle(Mathf.Atan2(mouseRelativePosition.y, mouseRelativePosition.x) * Mathf.Rad2Deg);
        }
    }
    

    private void handleCharging()
    {
        // check if we have hit charging speed
        if (myRigidbody2D.linearVelocity.magnitude >= chargeSpeed && !healthScript.invincible)
        {
            // if charging cooldown is over and we aren't charging
            if (!isCharging && chargeCooldownTimer < 0f)
            {
                //chargeIndicator.sprite = chargeIndicatorImage;

                // start charging if we press the button
                if (chargePressed)
                {
                    chargeTimer = chargeTime;

                    isCharging = true;
                    chargeIndicator.sprite = invincibilityIndicatorImage;
                }
            }
        }
        else
        {
            // too slow for charge speed and not charging
            if (!isCharging)
            {
                chargeIndicator.sprite = null;
            }
        }
        
        // if we stop charging
        if (chargeTimer <= 0f && isCharging)
        {
            isCharging = false;
            chargeCooldownTimer = chargeCooldown;
        }

        // if cooldown is happening
        if (chargeCooldownTimer > 0f)
        {
            chargeIndicator.sprite = chargeCooldownIndicatorImage;
        }

        chargeTimer -= Time.deltaTime;
        chargeCooldownTimer -= Time.deltaTime;
    }
    public void DamagePlayer(float Damage, Vector2 knockbackDirection, float knockback = 0, float invincibilityTime = invincibilityTimeDefault)
    {
        healthScript.takeDamage(Damage, knockbackDirection, knockback,invincibilityTime);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("collision happened");
        if (!isCharging)
        {
            if (collision.gameObject.layer == 7) // enemy
            {
                Vector2 relativePosition = transform.position - collision.transform.position;
                float knockbackVal = 1;
                if (collision.gameObject.tag == "RobotSpiderQueen")
                {
                    knockbackVal = 1.5f;
                    if (relativePosition.y > Math.Abs(relativePosition.x) / .9f)
                    {
                        knockbackVal = 3.25f;
                    }
                }
                DamagePlayer(1, relativePosition.normalized, knockbackVal);
            }
            if (collision.gameObject.layer == 12) // death pit
            {
                //Vector2 relativePosition = transform.position - collision.transform.position;
                DamagePlayer(16, new Vector2(0, 0));
            }
            if (collision.gameObject.layer == 19) // spike
            {
                Vector2 relativePosition = transform.position - collision.transform.position;
                float knockbackVal = .5f;
                if (relativePosition.y > Math.Abs(relativePosition.x) / .9f)
                {
                    knockbackVal = 1.25f;
                }
                DamagePlayer(1, relativePosition.normalized, knockbackVal);
            }
        }
        else
        {
            if (collision.gameObject.layer == 7) // enemy
            {
                Vector2 relativePosition = transform.position - collision.transform.position;
                float knockbackVal = 1;
                if (collision.gameObject.tag == "RobotSpiderQueen")
                {
                    knockbackVal = 1.5f;
                    if (relativePosition.y > Math.Abs(relativePosition.x) / .9f)
                    {
                        knockbackVal = 3.25f;
                    }
                }

                StartCoroutine(handleKnockback(knockbackVal, relativePosition.normalized));
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isCharging)
        {
            if (collision.gameObject.layer == 13) //enemy bullet
            {
                Vector2 relativePosition = transform.position - collision.transform.position;
                DamagePlayer(1, relativePosition.normalized, .5f);
            }
            if (collision.gameObject.layer == 16) // rock
            {
                Vector2 relativePosition = transform.position - collision.transform.position;
                DamagePlayer(1, relativePosition.normalized, 0f);
            }
            if (collision.gameObject.layer == 7) // enemy
            {
                Vector2 relativePosition = transform.position - collision.transform.position;
                DamagePlayer(1, relativePosition.normalized, .5f);
            }
        }
    }
    public IEnumerator handleKnockback(float knockback, Vector2 knockbackDirection)
    {
        //movementEnabled = false;
        myRigidbody2D.AddForce(knockbackDirection * knockback * 10, ForceMode2D.Impulse);
        playerAnimationManagerScript.setAllSpritesColor(Color.red);
        yield return new WaitForSeconds(knockbackTime);
        //movementEnabled = true;
        playerAnimationManagerScript.setAllSpritesColor(Color.white);
    }

    private void handleMagneticRepulsion()
    {
        if (myMagnet == null || !(repelOn ^ attractOn) || !magnetSpawnerScript.magnetActive) return;
        magnetRelativePosition = transform.position - myMagnet.transform.position;
        magnetDistance = magnetRelativePosition.magnitude;
        if (magnetDistance < 1.1f)
        {
            magnetDistance = 1.1f;
        }
        if (magnetDistance < (maximumMagnetDistance))
        {
            float size = 1 - 1.5f * (magnetDistance - 1.1f) / maximumMagnetDistance;
            size = Mathf.Max(size, 0f);
            magnetVisualEffectScript.StartMagnetEffect(repelOn, size);
            applyMagnetism(magnetRelativePosition.normalized, magnetDistance);
        }
    }
    void applyMagnetism(Vector2 forceDirection, float magnetDistance)
    {
        float forceMagnitude = 1 / (float)Math.Sqrt(magnetDistance);
        if (attractOn)
        {
            forceMagnitude *= magnetDistanceMultiplyingForceAttraction;
            forceMagnitude += magnetBaseForceAttraction;
        }
        else
        {
            forceMagnitude *= magnetDistanceMultiplyingForceRepulsion;
            forceMagnitude += magnetBaseForceRepulsion;
        }
        myRigidbody2D.AddForce(forceDirection * forceMagnitude, ForceMode2D.Force);
    }
    public float getMagnetMaxYSpeed(float maxYSpeed)
    {
        float currentMaxSpeed = maxYSpeed;
        if (repelOn ^ attractOn)
        {
            if (myMagnet != null && magnetSpawnerScript.magnetActive)
            {
                Vector2 magnetRelativePosition = transform.position - myMagnet.transform.position;
                float magnetDistance = magnetRelativePosition.magnitude;
                if (magnetDistance < (maximumMagnetDistance))
                {
                    float angle = Mathf.Atan2(magnetRelativePosition.y, magnetRelativePosition.x);
                    float sinAngle = Mathf.Sin(angle);
                    float sign = MathF.Abs(myRigidbody2D.linearVelocity.y) / myRigidbody2D.linearVelocity.y;
                    if (attractOn) sign *= -1;//if attract is on then we want the + in the next step to be a minus
                    currentMaxSpeed = maxYSpeed * MathF.Abs(sign + 4 * sinAngle * MathF.Sqrt((maximumMagnetDistance - magnetDistance) / maximumMagnetDistance));
                }
            }
        }
        return currentMaxSpeed;
    }
    public void KillPlayer()
    {
        if (!playerAlive)
        {
            return;
        }
        playerAlive = false;
        //TODO dying stuff
        //animator.SetBool("hasDied", true);
        playerAnimationManagerScript.startDeath();
        DeathAnimation.SetActive(true);
        myRigidbody2D.linearVelocity = new Vector3(0, 0, 0);
        myRigidbody2D.gravityScale = 1.5f;
        chargeIndicator.sprite = null;
        if (magnetAttractionAudio != null && magnetAttractionAudio.isPlaying) magnetAttractionAudio.Stop();
        if (magnetRepulsionAudio != null && magnetRepulsionAudio.isPlaying) magnetRepulsionAudio.Stop();
        myVerticalMovementScript.PlayerKilled();
        StartCoroutine(HandleDeath());
    }
    IEnumerator HandleDeath()
    {
        yield return new WaitUntil(() => DeathAnimation.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Dead"));
        gameObject.SetActive(false);
    }
    public void setMagnet(GameObject magnet)
    {
        myMagnet = magnet;
        magnetVisualEffectScript.Magnet = magnet;
    }
    public void Move(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        direction = input.x;
        verticalDirection = input.y;
        if (context.performed && Mathf.Abs(input.x) > 0.1f) { 
            lastMoveInputTime = Time.time;
        }
    }
    public void Aim(InputAction.CallbackContext context)
    {
        if(Mathf.Abs(context.ReadValue<Vector2>().x) > .1 || Mathf.Abs(context.ReadValue<Vector2>().y) > .1)
        {
            rightJoystick = context.ReadValue<Vector2>();
        }
    }
    public void JumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpPressed = true;
            lastJumpInputTime = Time.time;
            if (cutsceneManagerScript != null)
            {
                cutsceneManagerScript.SkipCutscene();
            }
        }
        if (context.canceled)
        {
            jumpPressed = false;
        }
    }

    public void ChargeInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            chargePressed = true;
        }
        if (context.canceled)
        {
            chargePressed = false;
        }
    }

    public void ShootingInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            shootingInput = true;
        }
        if (context.canceled)
        {
            shootingInput = false;
        }
    }
    public void LaunchMagnet(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            launchMagnetHeld = true;
            launchMagnet = true;
        }
        if (context.canceled)
        {
            launchMagnetHeld = false;
            launchMagnet = false;
        }
    }
    public void MagnetRepel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            repelOn = true;
        }
        if (context.canceled)
        {
            repelOn = false;
        }
        if (context.performed && myMagnet != null){
            lastRepelInputTime= Time.time;
        }
    }
    public void MagnetAttract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            attractButtonHeld = true;
        }
        if (context.canceled)
        {
            attractButtonHeld = false;
        }
        if (context.performed && myMagnet != null){
            lastAttractInputTime= Time.time;
        }
    }
    public void Pause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            logic.SetPausePressed();
        }
    }
    public void CheckIfStuckInGround()
    {
        bool inGround= (Physics2D.Raycast(transform.position - myVerticalMovementScript.getDistanceToLeg()/2 + inGroundOffset, Vector2.down, myVerticalMovementScript.getGroundLength(), inGroundLayer) || Physics2D.Raycast(transform.position + myVerticalMovementScript.getDistanceToLeg() / 2 + inGroundOffset, Vector2.down, myVerticalMovementScript.getGroundLength(), inGroundLayer));
        if (inGround)
        {
            inGroundTimer += Time.deltaTime;
            if(inGroundTimer >= inGroundKillTime) healthScript.HandlePlayerDeath();
        }
        else inGroundTimer = 0;
    }
}
