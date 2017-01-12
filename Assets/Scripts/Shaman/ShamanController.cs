using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController2D))]
public class ShamanController : MonoBehaviour 
{
    public float timeJumpApex;
    public float maxJumpDistance;
    public float minJumpDistance;
    public float minVerticalVelocity;

    CharacterController2D charController;
    float verticalVelocity;
    float gravity;
    float maxJumpForce;
    float minJumpForce;

    void CalculateJumpForceAndGravity()
    {
        gravity = -(2 * maxJumpDistance) / Mathf.Pow(timeJumpApex, 2);
        maxJumpForce = Mathf.Abs(gravity) * timeJumpApex;
        minJumpForce = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpDistance);
    }

    void Start()
    {
        charController = GetComponent<CharacterController2D>();
        CalculateJumpForceAndGravity();
    }


    void Update()
    {
        if (!GameManager.instance.gamePaused)
        {
            Vector2 moveVec = Vector2.zero;
            ApplyGravity();

            moveVec = new Vector2(0, verticalVelocity);
            moveVec = moveVec * Time.deltaTime;
            charController.Move(ref moveVec);
        }
    }

    void ApplyGravity()
    {
        if(charController.cs.collidingDown)
        {
            verticalVelocity = 0;
        }

        verticalVelocity += gravity;

        verticalVelocity = Mathf.Clamp(verticalVelocity, -Mathf.Abs(minVerticalVelocity), Mathf.Infinity);
    }
}
