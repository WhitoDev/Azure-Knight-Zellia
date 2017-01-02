using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController2D))]
public class Player : MonoBehaviour 
{
    public struct PlayerFlags
    {
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

    private CharacterController2D controller;
    private PlayerAnimatorController animationController;
    public PlayerFlags flags = new PlayerFlags();
    public float moveSpeed = 10f;
    public float maxFallingSpeed = -20f;

    public float maxJumpDistance = 4f;
    public float minJumpDistance = .5f;
    public float timeJumpApex = .5f;
        
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

        flags.readHorizontalInput = true;
    }

    void CalculateJumpForceAndGravity()
    {
        gravity = -(2 * maxJumpDistance) / Mathf.Pow(timeJumpApex, 2);
        maxJumpForce = Mathf.Abs(gravity) * timeJumpApex;
        minJumpForce = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpDistance);
    }

    void ReadInput()
    {
        inputVec = Vector2.zero;

        if ((Input.GetKey(KeyCode.A) || holdLeft) && flags.readHorizontalInput)
        {
            inputVec.x += -1;
        }

        if ((Input.GetKey(KeyCode.D) || holdRight) && flags.readHorizontalInput)
        {
            inputVec.x += 1;
        }

        if ((Input.GetKeyDown(KeyCode.Space) || jump) && (controller.cs.collidingDown || flags.extraJumps > 0) && ! controller.cs.collidingUp)
        {
            verticalVelocity = maxJumpForce;
            flags.pressedJumpButton = true;
            if(!controller.cs.collidingDown)
                flags.extraJumps--;
        }

        if (Input.GetKeyUp(KeyCode.Space) && verticalVelocity > minJumpForce)
        {
            verticalVelocity = minJumpForce;
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            flags.pressedAttackButton = true;
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            if(GameManager.gamePaused)
            {
                GameManager.UnpauseGame();
            }
            else
            {
                GameManager.PauseGame();
            }
        }

    }

    void SetSpriteDirecction()
    {
        if(moveVec.x != 0)
        {
            this.transform.localScale = new Vector3(Mathf.Abs(this.transform.localScale.x) * Mathf.Sign(moveVec.x), this.transform.localScale.y, this.transform.localScale.z);
        }

        flags.facingRight = Mathf.Sign(this.transform.localScale.x) == 1 ? true : false;
    }

    void InputToVelocity()
    {
        moveVec = Vector2.zero;

        moveVec.x = inputVec.x * moveSpeed;
        moveVec.y = verticalVelocity;

        moveVec = moveVec * Time.deltaTime;
    }

    void Update()
    {
        animationController.GetCurrentState();
        ReadInput();
        if (!GameManager.gamePaused)
        {
            InputToVelocity();
            SetSpriteDirecction();
            controller.Move(ref moveVec);

            verticalVelocity += gravity * Time.deltaTime;
            if (controller.cs.collidingUp)
                verticalVelocity = gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, maxFallingSpeed);

            CalculateJumpForceAndGravity();

            animationController.SetAnimationParameters();

            if (controller.cs.collidingDown)
            {
                flags.extraJumps = 1;
                verticalVelocity = 0;
            }

            flags.Reset();

            moveVecOld = moveVec;
        }
    }

    
}
