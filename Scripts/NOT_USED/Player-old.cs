#if tools
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{   
    [Header("Physics Values")]
    public float jumpForce = 10f;
    public float walkSpeed = 10f;
    public float airSpeed = 8f; // (airStrafeMultiplyer) used to adjust player control in the air
    public float maxSpeed = 6f;
    public float dashSpeed = 6f;
    public int dashTime = 15;

    // used to control drag, player has less drag when attempting to move
    public float baseDrag = 5f;
    public float altDrag = 1f;
    
    // used for controlling falling speed based on player input
    public float baseGravityScale = 4f; // default gravity scale
    public float altGravityScale = 2f; // increases jump height

    // threshold of speed to trigger/end turnaround state when changing direction
    public float turnThreshold = 4f;
    public float turnEndThreshold = 1f;

    [Header("Attack Info")]
    public string playerStateDebug = "";
    public Action currentAction;
    public int attackSequence;
    public bool hitboxActive;
    public bool jumpCancel; // true if an attack connects, allows jump cancel from attacks
    public bool hasJumpKick; // used to limit air actions
    public bool hasAirdash; // used to limit air actions

    public int dashStock; // resource player uses for dashes and dash attacks
    public int dashStockCap = 3;
    public int dashRestock; // timer for regaining dash stocks
    public int dashRestockCap = 100; // when dashMeter reaches cap, stock is gained

    [Header("Other Stuffs")]
    public bool launched;
    public bool animating; // if player animations are currently playing
    public bool facingRight = true; // used to flip sprite renderer
    public bool dashingRight; // used for dash state
    public bool grounded;
    public int fallTimer; // tracks how long player has been airborne (in frames)
    public bool inHitstop; // used to pause animator and extend attack duration
    public bool atDoor;
    public bool spawning; // used for initiating spawn sequence on level start
    public float GCR = 0.1f; // (groundCheckRange) used to adjust range of boxcast to check for ground
    public float GCRreset = 0.1f;
    public int GCRdelay = 0; // used to remove ground check for a brief moment when player is attemping to to air moves close to ground
    public int attackForceDelay;

    int altInt = 1; // alternating int for hitshake
    public float hitshakeIntensity = 30.0f;

    // Statemachine and States
    [HideInInspector] public StateMachine playerSM;
    [HideInInspector] public Spawn spawn;
    [HideInInspector] public Idle idle;
    [HideInInspector] public Walking walking;
    [HideInInspector] public Airborne airborne;
    [HideInInspector] public Turnaround turnaround;
    [HideInInspector] public PlayerAction action;
    [HideInInspector] public Dash dash;
    [HideInInspector] public Stun stun;
    [HideInInspector] public Victory victory;
    
    // Components
    [HideInInspector] public PlayerInput pi;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public BoxCollider2D bc;
    [HideInInspector] public AudioSource au;

    [HideInInspector] public GameObject sprite;
    [HideInInspector] public SpriteRenderer sr;
    [HideInInspector] public Animator an;

    [HideInInspector] public HitHandler hh;

    LayerMask groundCheckLayerMask;

    #region // animation names
    // Animation States
    public string currentAnimationState;
    [HideInInspector] const string IDLE1 = "pawnch_idle1";
    [HideInInspector] const string IDLE2 = "pawnch_idle2";
    [HideInInspector] const string IDLE3 = "pawnch_idle3";
    string[] idles = new string[3];
    [HideInInspector] const string RUN = "pawnch_run";
    [HideInInspector] const string JUMP = "pawnch_jump";
    [HideInInspector] const string LAND = "pawnch_land";
    [HideInInspector] const string RUNSTART = "pawnch_runstart";
    [HideInInspector] const string TURN = "pawnch_turn";
    [HideInInspector] const string FALL = "pawnch_fall";
    [HideInInspector] const string LANDRUN = "pawnch_landrun";
    [HideInInspector] const string RUNSTOP = "pawnch_runstop";
    [HideInInspector] const string DASHF = "pawnch_dashf";
    [HideInInspector] const string AIRDASHF = "pawnch_airdashf";
    [HideInInspector] const string DASHB = "pawnch_dashb";
    [HideInInspector] const string AIRDASHB = "pawnch_airdashb";

    // Animation Attack States
    [HideInInspector] const string PUNCH1 = "pawnch_punch1";
    [HideInInspector] const string PUNCH2 = "pawnch_punch2";
    [HideInInspector] const string KICK = "pawnch_kick";
    [HideInInspector] const string JUMPKICK = "pawnch_jumpkick";
    [HideInInspector] const string FLIP = "pawnch_flip";
    [HideInInspector] const string TURNPUNCH = "pawnch_turnpunch";
    [HideInInspector] const string STOMP = "pawnch_stomp";
    [HideInInspector] const string STOMPLAND = "pawnch_stompland";
    
    #endregion

    void Start()
    {
        pi = gameObject.GetComponent<PlayerInput>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        bc = gameObject.GetComponent<BoxCollider2D>();
        au = gameObject.GetComponent<AudioSource>();
        // sprite components
        sprite = this.transform.GetChild(0).gameObject;
        sr = sprite.GetComponent<SpriteRenderer>();
        an = sprite.GetComponent<Animator>();
        
        hh = gameObject.GetComponent<HitHandler>();

        groundCheckLayerMask = LayerMask.GetMask("Ground") | LayerMask.GetMask("Stage");

        // setup state machine
        playerSM = gameObject.AddComponent<StateMachine>();
        spawn = new Spawn(this, playerSM);
        idle = new Idle(this, playerSM);
        walking = new Walking(this, playerSM);
        airborne = new Airborne(this, playerSM);
        turnaround = new Turnaround(this, playerSM);
        action = new PlayerAction(this, playerSM);
        dash = new Dash(this, playerSM);
        stun = new Stun(this, playerSM);

        victory = new Victory(this, playerSM);

        // store idles in array for randomness
        idles[0] = IDLE1;
        idles[1] = IDLE2;
        idles[2] = IDLE3;

        playerSM.Initialize(spawn);
    }

    void Update()
    {
        // pause check
        if (!Oracle.Instance.paused){
            HandleAnimation(); // HandleAnimation needs to be BEFORE state logic to prevent auto transitions overriding current action
            
            playerSM.currentState.HandleInput();
            playerSM.currentState.UpdateLogic();

            #region // conditionals for each state
            if (playerSM.currentState == spawn) {
                sr.color = Color.black;
                playerStateDebug = "spawn";
                }
            else if (playerSM.currentState == idle) {
                sr.color = Color.white;
                playerStateDebug = "idle";
                }
            else if (playerSM.currentState == walking) {
                sr.color = Color.white;
                playerStateDebug = "walk";
                }
            else if (playerSM.currentState == airborne) {
                sr.color = Color.white;
                playerStateDebug = "air";
                }
            else if (playerSM.currentState == turnaround) {
                sr.color = Color.white;
                playerStateDebug = "turnaround";
                }
            else if (playerSM.currentState == action) {
                sr.color = Color.white;
                playerStateDebug = "attack";
                }
            else if (playerSM.currentState == dash) {
                sr.color = Color.yellow;
                playerStateDebug = "dash";
                }
            else if (playerSM.currentState == victory) {
                sr.color = Color.black;
                playerStateDebug = "victory";
                }
            else if (playerSM.currentState == stun) {
                sr.color = Color.red;
                playerStateDebug = "stun";
            }
            #endregion
            
            // sprite flipping
            sr.flipX = !facingRight;
            hh.facingRight = facingRight;

            // ground check
            grounded = GroundCheck();
            

            HandleHitstop();
            HandleHitstun();

            GetAirTime();

            HandleGround();

            HandleGCR();

            UpdateMeter();
        }
    }

    void FixedUpdate()
    {
        playerSM.currentState.UpdatePhysics();    

        // ground check
        // if (GCRdelay == 0){
        //     grounded = GroundCheck();
        // }
        
    }

    void UpdateMeter(){
        // update restock every frame
        if (dashStock == dashStockCap){
            dashRestock = 0;
        } else {
            dashRestock++;
        }
        
        // if stock exceeds cap, reset to 1
        if (dashRestock > dashRestockCap){
            dashRestock = 1;
            
            if (dashStock < dashStockCap){
                dashStock++;
            }
        }
    }

    // function to manually switch animation states
    public void ChangeAnimationState(string newState){
        //Debug.Log("new player ani state: " + newState);

        // stop self interruption
        if (currentAnimationState == newState){
            
            // allow certain animations to cancel themselves
            if (an.GetCurrentAnimatorStateInfo(0).IsName(DASHF) ||
                an.GetCurrentAnimatorStateInfo(0).IsName(DASHB) ||
                an.GetCurrentAnimatorStateInfo(0).IsName(STOMP)){
                var nameHash = an.GetCurrentAnimatorStateInfo(0).fullPathHash;
                an.Play(nameHash,0,0);
            }
            return;
        } 
        an.StopPlayback();

        // play animation
        an.Play(newState);

        // assign state
        currentAnimationState = newState;
    }

    // function to automatically transition animations
    void HandleAnimation(){

        AnimatorStateInfo anStateInfo = an.GetCurrentAnimatorStateInfo(0);
        //Debug.Log(anStateInfo.normalizedTime);
        // if animation reaches end and isn't a loop
        if ((anStateInfo.normalizedTime > 1.0f) && !an.GetCurrentAnimatorStateInfo(0).loop){
            //Debug.Log("animation ended");
            animating = false;

            switch(currentAnimationState){
                case IDLE1:
                    int randIdle = Random.Range(0, 3); // 0-2
                    ChangeAnimationState(idles[randIdle]);
                    break;
                case IDLE2:
                    ChangeAnimationState(IDLE1);
                    break;
                case IDLE3:
                    ChangeAnimationState(IDLE1);
                    break;
                case LAND:
                    if (grounded){ ChangeAnimationState(IDLE1); }
                    break;
                case LANDRUN:
                    ChangeAnimationState(RUN);
                    break;
                case RUNSTART:
                    ChangeAnimationState(RUN);
                    break;
                case TURN:
                    if (playerSM.currentState.dpad.x != 0) { ChangeAnimationState(RUNSTART); }
                    else { ChangeAnimationState(IDLE1); }
                    break;
                case RUNSTOP:
                    ChangeAnimationState(IDLE1);
                    break;
                case DASHB:
                    ChangeAnimationState(IDLE1);
                    break;
                case DASHF:
                    ChangeAnimationState(IDLE1);
                    break;
                case AIRDASHB:
                    if (grounded) ChangeAnimationState(LAND);
                    break;
                case AIRDASHF:
                    if (grounded) ChangeAnimationState(LAND);
                    break;
                default:
                    break;
            }

        } else {
            animating = true;
        }

        // cancel looping animation
        switch (currentAnimationState){
            case RUN:
                if ((playerSM.currentState == airborne) && (rb.velocity.y < 1)){ ChangeAnimationState(FALL); }
                break;
            case IDLE1:
                if (!grounded){ ChangeAnimationState(FALL); }
                break;
            case IDLE2:
                if (!grounded){ ChangeAnimationState(FALL); }
                break;
            case IDLE3:
                if (!grounded){ ChangeAnimationState(FALL); }
                break;
            default:
                break;
        }

    }

    // door checks
    void OnTriggerStay2D(Collider2D other){
        if (other.gameObject.tag == "Door"){
            if (other.gameObject.GetComponent<Door>().open){
                atDoor = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other){
        if (other.gameObject.tag == "Door"){
            atDoor = false;
        }
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(bc.bounds.center - new Vector3(0,bc.bounds.extents.y + (GCR/2),0), new Vector3(bc.size.x,GCR));
    }

    bool GroundCheck(){
        RaycastHit2D boxCastHit = Physics2D.BoxCast(bc.bounds.center, bc.bounds.size, 0f, Vector2.down, GCR, groundCheckLayerMask);
        
        // RaycastHit2D boxCastHit = Physics2D.BoxCast(bc.bounds.center - new Vector3(0,bc.bounds.extents.y + (GCR/2f),0),
        // new Vector3(bc.size.x,GCR),
        // 0f, Vector2.down, 0, groundCheckLayerMask);
        

        return boxCastHit.collider != null;
    }

    public void Jump(){
        //rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        rb.velocity = new Vector2(rb.velocity.x,jumpForce);
        SoundManager.Instance.PlayPlayerSound(Database.Instance.jump);
        playerSM.ChangeState(airborne);
        grounded = false; // removes edge cases where update is not in sync with unity physics
        ChangeAnimationState(JUMP);
        GCROffset(5);

        //SpawnDustCloud();
    }

    public void SpawnDustCloud(){
        // spawn dust cloud
        GameObject.Instantiate(Database.Instance.dust,
        transform.position + new Vector3(rb.velocity.x/8,-1, 0), // position
        Quaternion.identity); // rotation
    }

    public void Punch(){
        // grounded punch combo
        currentAction = Database.Instance.punch1;
        playerSM.ChangeState(action);
        ChangeAnimationState(PUNCH1);
    }

    public void JumpKick(){
        currentAction = Database.Instance.jumpKick;
        playerSM.ChangeState(action);
        ChangeAnimationState(JUMPKICK);
        GCROffset(5);
        
    }

    public void Flip(){
        currentAction = Database.Instance.flip;
        playerSM.ChangeState(action);
        grounded = false; // removes edge cases where update is not in sync with unity physics
        ChangeAnimationState(FLIP);
        GCROffset(5);
        facingRight = !facingRight;
    }

    public void TurnPunch(){
        currentAction = Database.Instance.turnpunch;
        playerSM.ChangeState(action);
        ChangeAnimationState(TURNPUNCH);
    }

    public void LowFlip(){
        if (playerSM.currentState == dash){
            dashStock++;
        }

        if (dashStock > 0){
            dashStock--;
            currentAction = Database.Instance.lowflip;
            playerSM.ChangeState(action);
            grounded = false; // removes edge cases where update is not in sync with unity physics
            ChangeAnimationState(FLIP);
            GCROffset(5);
        } else {
            // play backfire sound
            SoundManager.Instance.PlayPlayerSound(Database.Instance.blip);
        }
    }

    // function to remove ground check from player for a few frames for air actions to perform smoothly close to ground
    public void GCROffset(int delay){
        GCR = 0;
        GCRdelay = delay;
    }

    public void Stomp(){
        if (playerSM.currentState == dash){
            dashStock++;
        }

        if (dashStock > 0){    
            dashStock--;
            currentAction = Database.Instance.stomp;
            playerSM.ChangeState(action);
            ChangeAnimationState(STOMP);
        } else {
            // play backfire sound
            SoundManager.Instance.PlayPlayerSound(Database.Instance.blip);
        }
    }

    public void Dash(bool right){
        // check if there are dash stocks remaining
        if (dashStock > 0){
            SpawnAfterImage();

            dashingRight = right;
            playerSM.ChangeState(dash);
            // forward dash
            if((dashingRight && facingRight) || (!dashingRight && !facingRight)){
                if (grounded){ ChangeAnimationState(DASHF); }
                else { ChangeAnimationState(AIRDASHF); }
            } else // backward dash 
            {
                if (grounded){ ChangeAnimationState(DASHB); }
                else { ChangeAnimationState(AIRDASHB); }
            }

            dashStock--;
        } else {
            // play backfire sound
            SoundManager.Instance.PlayPlayerSound(Database.Instance.blip);
        }
    }

    void SpawnAfterImage(){
        // instantiate afterImage
        GameObject afterImage = GameObject.Instantiate(Database.Instance.afterImage,
        transform.position, // position
        Quaternion.identity); // rotation

        // set sprite of newly created afterImage
        Sprite curSprite = sr.sprite;
        afterImage.GetComponent<SpriteRenderer>().sprite = curSprite;
        afterImage.GetComponent<SpriteRenderer>().flipX = !facingRight;
        
        // set size of sprite to match parent
        Transform afterImageT = afterImage.transform;
        afterImageT.localScale = new Vector3(sprite.transform.localScale.x, sprite.transform.localScale.y, sprite.transform.localScale.z);
        afterImageT.position = new Vector3(afterImageT.position.x, afterImageT.position.y + sprite.transform.localPosition.y, afterImageT.position.z);
    }

    public void EnterDoor(){
        // start scene transition
        //Oracle.Instance.ResetScene();
        StartCoroutine(Oracle.Instance.ResetSceneWithDelay(1f));
        SoundManager.Instance.PlayPlayerSound(Database.Instance.exit);
        // enter victory state
        playerSM.ChangeState(victory);
    }

    public void ExitHitstop(){
        inHitstop = false;
        rb.gravityScale = baseGravityScale;
    }

    // polls HitHandler (hh) for ticks on hitstop and hitstun, and stored velocity
    void HandleHitstop(){

        inHitstop = (hh.inHitstop) ? true : false;

        // hitstop logic, if player is inHitstop, pause animator and attack tickdown
        // when entering hitstop, player velocity is stored, then reapplied when hitstop ends
        // (attack state handles tickdown pause)
        if (inHitstop){
            
            if (hh.inHitstun){
                sprite.transform.localPosition = new Vector3((hh.hitstop/hitshakeIntensity)*altInt, sprite.transform.localPosition.y);
                altInt = altInt*(-1);
            }

            // animator stuff
            
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            an.enabled = false;
        } else {
            an.enabled = true;
            // SPECIAL CASE WHEN ENEMY IS HIT WHILE IN HITSTOP FROM ANOTHER ENEMY
            // this ALWAYS fires
            if ((rb.gravityScale == 0) && (playerSM.currentState == action)){
                //Debug.Log("fire");
                rb.gravityScale = baseGravityScale;
                rb.velocity = hh.backupVel;
                hh.backupVel = Vector2.zero;
            }

            sprite.transform.localPosition = new Vector3(0,sprite.transform.localPosition.y);

            // if there is stored vel, release it and set it back to zero
            if (hh.storedVel != Vector2.zero){
                rb.velocity = hh.storedVel;
                hh.storedVel = Vector2.zero;
                rb.gravityScale = baseGravityScale;
            }
        }
    }

    void HandleHitstun(){
        if (hh.inHitstun && (playerSM.currentState != stun)){
            playerSM.ChangeState(stun);
        }
    }

    // function to count frames when player is falling (changes volume of landing sound)
    void GetAirTime(){
        // if falling, count frames
        if (rb.velocity.y < -1){
            fallTimer++;
        } else if ((rb.velocity.y > 0) && (fallTimer > 0)) {
            fallTimer--;
        }
    }

    void HandleGCR(){
        // reset GCR after delay
        if ((GCRdelay > 0)){
            if (!inHitstop) GCRdelay--;
        } else {
            GCR = GCRreset;
        }
    }

    void HandleGround(){
        // refresh air actions when grounded
        if (grounded){
            hasAirdash = true;
            hasJumpKick = true;

            // for fall sound to be dynamic, needs a seperate audio source
            // play landing sound if fallTimer >= 0, reset
            // landing sound is louder if fall is greater
            if (fallTimer > 1){
                //print(fallTimer + " / " + 60);
                //float landVolume = (fallTimer/20f);
                // (fallTimer/60f)
                SoundManager.Instance.PlayPlayerSound(Database.Instance.land);
                fallTimer = 0;
                //an.SetTrigger("land");
                SpawnDustCloud();
            }
            //an.SetBool("airborne", false);
        } else {
            //an.SetBool("airborne", true);
        }
    }
}
#endif