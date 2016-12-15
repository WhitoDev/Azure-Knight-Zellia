using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController2D))]
public class Player : MonoBehaviour 
{
    private struct PlayerFlags
    {
        public bool pressedAttackButton;
        public bool pressedJumpButton;
        public bool pressedDashButton;
        public bool holdAttackButton;
        public bool holdJumpButton;
        public bool holdDashButton;
        public bool takingDamage;
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
    private Animator animator;
    private PlayerFlags flags = new PlayerFlags();
    public float moveSpeed = 10f;
    public float maxFallingSpeed = -20f;

    public float maxJumpDistance = 4f;
    public float minJumpDistance = .5f;
    public float timeJumpApex = .5f;
        
    private float gravity;
    private float maxJumpForce;
    private float minJumpForce;
    
    private float verticalVelocity;

    Vector2 inputVec;
    Vector2 moveVec;
    Vector2 moveVecOld;

    void Start()
    {
        controller = GetComponent<CharacterController2D>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.Log("Please set an animator controller to this gameobject!");
        CalculateJumpForceAndGravity();
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

        if (Input.GetKey(KeyCode.A) || holdLeft)
        {
            inputVec.x += -1;            
        }

        if (Input.GetKey(KeyCode.D) || holdRight)
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


    }

    void SetSpriteDirecction()
    {
        if(moveVec.x != 0)
        {
            this.transform.localScale = new Vector3(Mathf.Abs(this.transform.localScale.x) * Mathf.Sign(moveVec.x), this.transform.localScale.y, this.transform.localScale.z);
        }
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
        ReadInput();        
        InputToVelocity();
        SetSpriteDirecction();
        controller.Move(ref moveVec);

        verticalVelocity += gravity * Time.deltaTime;
        if(controller.cs.collidingUp)
            verticalVelocity = gravity * Time.deltaTime;
        verticalVelocity = Mathf.Max(verticalVelocity, maxFallingSpeed);

        CalculateJumpForceAndGravity();
        SetAnimationParameters();

        if(controller.cs.collidingDown)
        {
            flags.extraJumps = 1;
            verticalVelocity = 0;
        }

        flags.Reset();

        moveVecOld = moveVec;
    }

    void SetAnimationParameters()
    {
        animator.SetFloat("velocityX", Mathf.Abs(moveVec.x));
        animator.SetFloat("velocityY", moveVec.y);
        animator.SetBool("collidingDown", controller.cs.collidingDown);
        animator.SetBool("collidingUp", controller.cs.collidingUp);
        animator.SetBool("collidingLeft", controller.cs.collidingLeft);
        animator.SetBool("collidingRight", controller.cs.collidingRight);
        animator.SetBool("pressedAttackButton", flags.pressedAttackButton);
        animator.SetBool("pressedJumpButton", flags.pressedJumpButton);
        animator.SetBool("pressedDashButton", flags.pressedDashButton);
        animator.SetBool("holdAttackButton", flags.holdAttackButton);
        animator.SetBool("holdJumpButton", flags.holdJumpButton);
        animator.SetBool("holdDashButton", flags.holdDashButton);
        animator.SetBool("takingDamage", flags.takingDamage);
    }
}
