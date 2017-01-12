using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController2D))]
public class Player : MonoBehaviour 
{
    public struct PlayerFlags
    {
        public bool dynamodeActive;
        public bool canAttack;        
        public float attackBufferTimer;
        public bool pressedAttackButton;
        public bool pressedJumpButton;
        public bool pressedDashButton;
        public bool holdAttackButton;
        public bool holdJumpButton;
        public bool holdDashButton;
        public bool takingDamage;
        public bool facingRight;
        public bool readHorizontalInput;
        public int extraJumps;
        public int airDashes;

        public void Reset()
        {
            pressedAttackButton = 
            pressedJumpButton = 
            pressedDashButton = 
            holdAttackButton = 
            holdJumpButton = 
            holdDashButton = 
            takingDamage = false;
        }
    }

    public bool holdRight;
    public bool holdLeft;
    public bool jump;

    public GameObject AfterImagePrefab;

    private Coroutine AfterImageCorutine;
    private CharacterController2D controller;
    private PlayerAnimatorController animationController;
    public PlayerFlags playerStatus = new PlayerFlags();
    public float attackButtonBufferTime = 0.2f;
    public float moveSpeed = 10f;
    public float maxFallingSpeed = -20f;

    public float maxJumpDistance = 4f;
    public float minJumpDistance = .5f;
    public float timeJumpApex = .5f;

    private float dashForce;
    public float dashTimeLenght = 1f;
    public float dashMaxDistance = 4f;

    public float gravity;
    public float maxJumpForce;
    public float minJumpForce;
    
    public float verticalVelocity;

    Vector2 inputVec;
    public Vector2 moveVec;
    Vector2 moveVecOld;

    void Start()
    {
        controller = GetComponent<CharacterController2D>();
        animationController = GetComponentInChildren<PlayerAnimatorController>();
        CalculateJumpForceAndGravity();
        CalculateDashForce();

        playerStatus.readHorizontalInput = true;
        playerStatus.canAttack = true;
    }

    void CalculateJumpForceAndGravity()
    {
        gravity = -(2 * maxJumpDistance) / Mathf.Pow(timeJumpApex, 2);
        maxJumpForce = Mathf.Abs(gravity) * timeJumpApex;
        minJumpForce = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpDistance);
    }

    void CalculateDashForce()
    {
        dashForce = (dashMaxDistance / dashTimeLenght);
    }

    void ReadInput()
    {
        inputVec = Vector2.zero;

        if ((Input.GetKey(KeyCode.LeftArrow) || holdLeft) && playerStatus.readHorizontalInput)
        {
            inputVec.x += -1;
        }

        if ((Input.GetKey(KeyCode.RightArrow) || holdRight) && playerStatus.readHorizontalInput)
        {
            inputVec.x += 1;
        }

        if ((Input.GetKeyDown(KeyCode.X) || jump) && (controller.cs.collidingDown || playerStatus.extraJumps > 0) && ! controller.cs.collidingUp)
        {
            verticalVelocity = maxJumpForce;
            playerStatus.pressedJumpButton = true;
            if(!controller.cs.collidingDown)
                playerStatus.extraJumps--;
        }

        if (Input.GetKeyUp(KeyCode.X) && verticalVelocity > minJumpForce)
        {
            verticalVelocity = minJumpForce;
        }

        if(Input.GetKeyDown(KeyCode.C) || (Input.GetKey(KeyCode.C) && !playerStatus.dynamodeActive && playerStatus.attackBufferTimer > 0))
        {
            playerStatus.pressedAttackButton = true;
        }

        if(Input.GetKeyDown(KeyCode.Z) && (controller.cs.collidingDown || playerStatus.airDashes > 0))
        {            
            playerStatus.pressedDashButton = true;

            if (!controller.cs.collidingDown && playerStatus.dynamodeActive)
                playerStatus.airDashes--;
        }

        if(Input.GetKey(KeyCode.Z))
        {
            playerStatus.holdDashButton = true;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            ToggleDynamode();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            GameManager.instance.SleepGame(.02f, true);
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            if(GameManager.instance.gamePaused)
            {
                GameManager.instance.UnpauseGame();
            }
            else
            {
                GameManager.instance.PauseGame();
            }
        }

    }    

    void SetSpriteDirecction()
    {
        if(moveVec.x != 0)
        {
            this.transform.localScale = new Vector3(Mathf.Abs(this.transform.localScale.x) * Mathf.Sign(moveVec.x), this.transform.localScale.y, this.transform.localScale.z);
        }

        playerStatus.facingRight = Mathf.Sign(this.transform.localScale.x) == 1 ? true : false;
    }

    void InputToVelocity()
    {
        moveVec = Vector2.zero;

        float currentMoveSpeed;

        animationController.GetCurrentState();
        if (animationController.animationHashes.hs_Current == animationController.animationHashes.hs_Dash || animationController.animationHashes.hs_Current == animationController.animationHashes.hs_Air_Dash)
        {
            currentMoveSpeed = dashForce;
            moveVec.x = (inputVec.x == 0 ? Mathf.Sign(transform.localScale.x) : inputVec.x) * currentMoveSpeed;            
        }
        else
        {
            currentMoveSpeed = moveSpeed;
            moveVec.x = inputVec.x * currentMoveSpeed;            
        }

        if (animationController.animationHashes.hs_Current == animationController.animationHashes.hs_Air_Dash)
        {
            verticalVelocity = 0;
        }

        moveVec.y = verticalVelocity;

        moveVec = moveVec * Time.deltaTime;
    }

    public void ToggleDynamode()
    {
        playerStatus.dynamodeActive = !playerStatus.dynamodeActive;

        if(AfterImageCorutine != null)
        {
            StopCoroutine(AfterImageCorutine);
            AfterImageCorutine = null;
        }

        if(playerStatus.dynamodeActive)
        {
            AfterImageCorutine = StartCoroutine(RunAfterImage(.02f));
        }
    }

    public void StartAfterImage(float seconds)
    {
        AfterImageCorutine = StartCoroutine(RunAfterImage(seconds));
    }

    public void StopAfterImage()
    {
        StopCoroutine(AfterImageCorutine);
        AfterImageCorutine = null;
    }

    void HandleInputBuffer()
    {
        if (playerStatus.attackBufferTimer > 0)
            playerStatus.attackBufferTimer -= Time.deltaTime;
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            playerStatus.attackBufferTimer = attackButtonBufferTime;
        }
    }

    IEnumerator RunAfterImage(float interval)
    {
        while(true)
        {
            yield return new WaitForSeconds(interval);
            var obj = Instantiate(AfterImagePrefab, AfterImagePrefab.transform.position + this.transform.position, Quaternion.identity) as GameObject;
            obj.transform.localScale = new Vector3(obj.transform.localScale.x * Mathf.Sign(transform.localScale.x), obj.transform.localScale.y, obj.transform.localScale.z);
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetComponentInChildren<SpriteRenderer>().sprite;
            Destroy(obj, obj.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length);            
        }
    }

    void Update()
    {
        animationController.GetCurrentState();

        if (animationController.animationHashes.hs_Current != animationController.animationHashes.hs_Last 
            && (animationController.animationHashes.hs_Last == animationController.animationHashes.hs_Dash || animationController.animationHashes.hs_Last == animationController.animationHashes.hs_Air_Dash) 
            && AfterImageCorutine != null)
        {
            StopAfterImage();
        }

        ReadInput();
        if (!GameManager.instance.gamePaused)
        {
            InputToVelocity();
            SetSpriteDirecction();
            controller.Move(ref moveVec);

            if (animationController.animationHashes.hs_Current != animationController.animationHashes.hs_Air_Dash)
            {
                verticalVelocity += gravity * Time.deltaTime;
                if (controller.cs.collidingUp)
                    verticalVelocity = gravity * Time.deltaTime;
                verticalVelocity = Mathf.Max(verticalVelocity, maxFallingSpeed);
            }

            CalculateJumpForceAndGravity();
            CalculateDashForce();

            if(!playerStatus.pressedJumpButton)
                GameManager.instance.PlayerMoved();

            animationController.SetAnimationParameters();

            if (controller.cs.collidingDown)
            {
                playerStatus.extraJumps = 1;
                verticalVelocity = 0;
                playerStatus.airDashes = 1;
            }

            playerStatus.Reset();

            moveVecOld = moveVec;            
        }

        HandleInputBuffer();
    }    
}
