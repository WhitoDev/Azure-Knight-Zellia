using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerAnimatorController : AnimationController2D 
{
    private Player playerController;
    private CharacterController2D characterController;

    #region Animation hashes
    public struct AnimationHashes
    {
        public int hs_Current;
        public int hs_Last;

        public int hs_Airborne_Attack;
        public int hs_Airborne_Down;
        public int hs_Airborne_Up;
        public int hs_Damaged;
        public int hs_Grounded_Slash_1;
        public int hs_Grounded_Slash_2;
        public int hs_Idle;
        public int hs_Run;
        public int hs_Dash;
        public int hs_Air_Dash;
    }

    public AnimationHashes animationHashes = new AnimationHashes();
    #endregion

    void Start()
    {
        playerController = GetComponentInParent<Player>();
        characterController = GetComponentInParent<CharacterController2D>();
        
        HashAnimations();        
    }

    void HashAnimations()
    {
        animationHashes.hs_Airborne_Attack = Animator.StringToHash("Base Layer.Airborne.Airborne_Attack");
        animationHashes.hs_Airborne_Down = Animator.StringToHash("Base Layer.Airborne.Airborne_Down");
        animationHashes.hs_Airborne_Up = Animator.StringToHash("Base Layer.Airborne.Airborne_Up");
        animationHashes.hs_Damaged = Animator.StringToHash("Base Layer.Damaged");
        animationHashes.hs_Grounded_Slash_1 = Animator.StringToHash("Base Layer.Grounded.Grounded_Slash_1");
        animationHashes.hs_Grounded_Slash_2 = Animator.StringToHash("Base Layer.Grounded.Grounded_Slash_2");
        animationHashes.hs_Idle = Animator.StringToHash("Base Layer.Grounded.Idle");
        animationHashes.hs_Run = Animator.StringToHash("Base Layer.Grounded.Run");
        animationHashes.hs_Dash = Animator.StringToHash("Base Layer.Dash");
        animationHashes.hs_Air_Dash = Animator.StringToHash("Base Layer.Air_Dash");
    }

    public void SetAnimationParameters()
    {
        animator.SetFloat("velocityX", Mathf.Abs(playerController.moveVec.x));
        animator.SetFloat("velocityY", playerController.moveVec.y);
        animator.SetBool("collidingDown", characterController.cs.collidingDown);
        animator.SetBool("collidingUp", characterController.cs.collidingUp);
        animator.SetBool("collidingLeft", characterController.cs.collidingLeft);
        animator.SetBool("collidingRight", characterController.cs.collidingRight);
        animator.SetBool("pressedAttackButton", playerController.playerStatus.pressedAttackButton);
        animator.SetBool("pressedJumpButton", playerController.playerStatus.pressedJumpButton);
        animator.SetBool("pressedDashButton", playerController.playerStatus.pressedDashButton);
        animator.SetBool("holdAttackButton", playerController.playerStatus.holdAttackButton);
        animator.SetBool("holdJumpButton", playerController.playerStatus.holdJumpButton);
        animator.SetBool("holdDashButton", playerController.playerStatus.holdDashButton);
        animator.SetBool("takingDamage", playerController.playerStatus.takingDamage);
        animator.SetBool("dynamodeActive", playerController.playerStatus.dynamodeActive);
        animator.SetBool("canAttack", playerController.playerStatus.canAttack);
    }

    public void GetCurrentState()
    {
        animationHashes.hs_Last = animationHashes.hs_Current;
        animationHashes.hs_Current = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
    }

    private GameObject InstantiateObjectBase(GameObject obj)
    {
        var objPos = obj.transform.position;
        var objPositionOffset = new Vector3(objPos.x * (playerController.playerStatus.facingRight ? 1 : -1), objPos.y, objPos.z);

        Vector3 position = this.transform.position + objPositionOffset;

        var objLocalScale = obj.transform.localScale;
        Vector3 localScale = new Vector3(objLocalScale.x * (playerController.playerStatus.facingRight ? 1 : -1), objLocalScale.y, objLocalScale.z);

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
        playerController.playerStatus.readHorizontalInput = flag;
    }

    public void MakeASmallHop()
    {
        playerController.verticalVelocity = playerController.minJumpForce;
    }

    public void SetActiveComboFlag(int value)
    {
        bool flag = value == -1 ? false : true;

        if (!playerController.playerStatus.dynamodeActive)
        {
            playerController.playerStatus.canAttack = flag;
        }
    }

    public void StartAfterImage(float seconds)
    {
        playerController.StartAfterImage(seconds);
    }

    public void StopAfterImage()
    {
        playerController.StopAfterImage();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerAnimatorController))]
public class PlayerAnimatorEditor : AnimationController2DEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }    
} 
#endif