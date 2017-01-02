using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class PlayerAnimatorController : AnimationController2D 
{
    private Player playerController;
    private CharacterController2D characterController;

    #region Animations Hashes
    int hs_Current;
    int hs_Last;

    int hs_Airborne_Attack;
    int hs_Airborne_Down;
    int hs_Airborne_Up;
    int hs_Damaged;
    int hs_Grounded_Slash_1;
    int hs_Grounded_Slash_2;
    int hs_Idle;
    int hs_Run;
    #endregion

    void Start()
    {
        playerController = GetComponentInParent<Player>();
        characterController = GetComponentInParent<CharacterController2D>();
        
        HashAnimations();
    }

    void HashAnimations()
    {
        hs_Airborne_Attack = Animator.StringToHash("Base Layer.Airborne.Airborne_Attack");
        hs_Airborne_Down = Animator.StringToHash("Base Layer.Airborne.Airborne_Down");
        hs_Airborne_Up = Animator.StringToHash("Base Layer.Airborne.Airborne_Up");
        hs_Damaged = Animator.StringToHash("Base Layer.Damaged");
        hs_Grounded_Slash_1 = Animator.StringToHash("Base Layer.Grounded.Grounded_Slash_1");
        hs_Grounded_Slash_2 = Animator.StringToHash("Base Layer.Grounded.Grounded_Slash_2");
        hs_Idle = Animator.StringToHash("Base Layer.Grounded.Idle");
        hs_Run = Animator.StringToHash("Base Layer.Grounded.Run");
    }

    public void SetAnimationParameters()
    {
        animator.SetFloat("velocityX", Mathf.Abs(playerController.moveVec.x));
        animator.SetFloat("velocityY", playerController.moveVec.y);
        animator.SetBool("collidingDown", characterController.cs.collidingDown);
        animator.SetBool("collidingUp", characterController.cs.collidingUp);
        animator.SetBool("collidingLeft", characterController.cs.collidingLeft);
        animator.SetBool("collidingRight", characterController.cs.collidingRight);
        animator.SetBool("pressedAttackButton", playerController.flags.pressedAttackButton);
        animator.SetBool("pressedJumpButton", playerController.flags.pressedJumpButton);
        animator.SetBool("pressedDashButton", playerController.flags.pressedDashButton);
        animator.SetBool("holdAttackButton", playerController.flags.holdAttackButton);
        animator.SetBool("holdJumpButton", playerController.flags.holdJumpButton);
        animator.SetBool("holdDashButton", playerController.flags.holdDashButton);
        animator.SetBool("takingDamage", playerController.flags.takingDamage);        
    }

    public void GetCurrentState()
    {
        hs_Last = hs_Current;
        hs_Current = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
    }

    private GameObject InstantiateObjectBase(GameObject obj)
    {
        var objPos = obj.transform.position;
        var objPositionOffset = new Vector3(objPos.x * (playerController.flags.facingRight ? 1 : -1), objPos.y, objPos.z);

        Vector3 position = this.transform.position + objPositionOffset;

        var objLocalScale = obj.transform.localScale;
        Vector3 localScale = new Vector3(objLocalScale.x * (playerController.flags.facingRight ? 1 : -1), objLocalScale.y, objLocalScale.z);

        var newObj = Instantiate(obj, position, Quaternion.identity) as GameObject;
        newObj.transform.localScale = localScale;

        return newObj;
    }

    public void InstantiateObject(GameObject obj)
    {
        InstantiateObjectBase(obj);        
    }

    public void InstantiateObjectAsChild(GameObject obj)
    {
        var newObj = InstantiateObjectBase(obj);
        newObj.transform.parent = this.transform.parent;
    }

    public void SetReadHorizontalInputFlag(int value)
    {
        bool flag = value == -1 ? false : true;
        playerController.flags.readHorizontalInput = flag;
    }

    public void MakeASmallHop()
    {
        playerController.verticalVelocity = playerController.minJumpForce;
    }
}


[CustomEditor(typeof(PlayerAnimatorController))]
public class PlayerAnimatorEditor : AnimationController2DEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }    
} 