using UnityEngine;
using System.Collections;

public class CharacterController2D : RayCaster 
{
    float slopeClimbMaxAngle = 60;

    void Start()
    {
        if (collisionMask == null)
            Debug.LogError("Please set a layer for the collision mask on the CharacterController2D script for " + this.gameObject.name);        
    }

    public void Move(ref Vector2 moveVec, bool standingOnPlatform = false)
    {
        cs.Reset();

        CalculateColliderCoordinates();
        cs.initialMoveVec = moveVec;        

        FallingDownSlope(ref moveVec);

        if(moveVec.x != 0)
            HandleHorizontal(ref moveVec);

        HandleVertical(ref moveVec);

        Debug.DrawRay(cs.bounds.center, moveVec, Color.green);
        
        if (Mathf.Abs(moveVec.x) < 0.00001f)
            moveVec.x = 0;

        if (Mathf.Abs(moveVec.y) < 0.00001f)
            moveVec.y = 0;

        this.transform.position += (Vector3)moveVec;

        if (standingOnPlatform)
            cs.collidingDown = true;
    }

    void FallingDownSlope(ref Vector2 moveVec)
    {
        //falling down slope if it's too steep
        if (moveVec.y < 0)
        {
            SlideDownSlope(cs.BottomLeft, ref moveVec);
            SlideDownSlope(cs.BottomRight, ref moveVec);
        }
    }

    void SlideDownSlope(Vector2 rayOrigin, ref Vector2 moveVec)
    {
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle > slopeClimbMaxAngle && hit.distance < Mathf.Abs(moveVec.y))
            {
                float directionXNormal = Mathf.Sign(hit.normal.x);
                float moveAmount = Mathf.Abs(moveVec.y);
                moveVec.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveAmount * directionXNormal;
                moveVec.y = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveAmount * -1;
                cs.slopeAngle = slopeAngle;                
            }
        }
    }

    void HandleHorizontal(ref Vector2 moveVec)
    {
        float directionX = Mathf.Sign(moveVec.x);
        float rayLength = Mathf.Abs(moveVec.x) + skinWidth;

        //SI SE JODE TODO DESCOMENTAR ESTO!
        //if (rayLength == skinWidth)
        //    rayLength = skinWidth * 2;

        //moving down slope
        Vector2 rayOrigin = directionX == 1 ? cs.BottomLeft : cs.BottomRight;
        RaycastHit2D downSlopeRay = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);
        if(downSlopeRay)
        {
            float slopeAngle = Vector2.Angle(downSlopeRay.normal, Vector2.up);            
            if(slopeAngle != 0 && slopeAngle <= slopeClimbMaxAngle && downSlopeRay.distance < Mathf.Abs(moveVec.x))
            {
                float slopeNormalSign = Mathf.Sign(downSlopeRay.normal.x);
                if(slopeNormalSign == directionX)
                {
                    float moveAmountX = Mathf.Abs(moveVec.x);
                    float moveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveAmountX;
                    if ((downSlopeRay.distance - skinWidth) < moveAmountY && moveVec.y < moveAmountY)
                    {
                        moveVec.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveAmountX * directionX;                          
                        moveVec.y = moveAmountY * -1;
                        cs.slopeAngle = slopeAngle;
                        cs.collidingDown = true;
                        cs.descendingSlope = true;
                    }
                }
            }            
        }        

        //moving up slope or horizontally
        rayOrigin = directionX == 1 ? cs.BottomRight : cs.BottomLeft;
        for(int i = 0; i<rayAmount; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin + Vector2.up * i * verticalRaySpacing, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin + Vector2.up * i * verticalRaySpacing, Vector2.right * directionX * rayLength, Color.yellow);
            if(hit)
            {
                if (ignoreColliderWithTag.Contains(hit.transform.tag) && hit.distance == 0)
                    continue;

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if(i == 0 && slopeAngle <= slopeClimbMaxAngle)
                {
                    float distanceToSlopeStart = 0;
                    if (cs.slopeAngleOld != slopeAngle)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        moveVec.x = moveVec.x - distanceToSlopeStart * directionX;
                    }

                    float moveAmountX = Mathf.Abs(moveVec.x);                    
                    float moveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveAmountX;

                    if(cs.initialMoveVec.y < moveAmountY)
                    {
                        moveVec.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveAmountX * directionX;
                        moveVec.y = moveAmountY;
                        cs.climbingSlope = true;
                        cs.collidingDown = true;
                        moveVec.x += distanceToSlopeStart * directionX;
                        cs.slopeAngle = slopeAngle;
                    }                    
                }

                if(!cs.climbingSlope || slopeAngle > slopeClimbMaxAngle)
                {
                    moveVec.x = (hit.distance - skinWidth) * directionX;                    

                    if(cs.climbingSlope || cs.descendingSlope)
                    {
                        float directionY = Mathf.Sign(moveVec.y);
                        moveVec.y = Mathf.Tan(cs.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveVec.x) * directionY;
                    }
                }
                if(i != 0 || !cs.climbingSlope)
                    rayLength = hit.distance;
                cs.collidingLeft = directionX == -1;
                cs.collidingRight = directionX == 1;
            }
        }       

        return;
    }

    void HandleVertical(ref Vector2 moveVec)
    {        
        float directionY = moveVec.y == 0 ? -1 : Mathf.Sign(moveVec.y);
        float direcctionX = Mathf.Sign(moveVec.x);
        float moveAmountX = Mathf.Abs(moveVec.x);
        Vector2 offset = Vector2.right * moveAmountX * direcctionX;
        float rayLength;
        Vector2 rayOrigin;

        if(cs.climbingSlope)
        {
            rayOrigin = (direcctionX == 1 ? cs.BottomRight : cs.BottomLeft) + moveVec;
            rayLength = skinWidth;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.blue);
            if(hit)
            {
                moveVec.y += (skinWidth - hit.distance);
            }            
        }
        
        rayLength = Mathf.Abs(moveVec.y) + skinWidth;
        if (rayLength == skinWidth)
            rayLength = skinWidth * 2;
        
        
        rayOrigin = (directionY == 1 ? cs.TopLeft : cs.BottomLeft);
        
        float spacingDirection = 1;
        if (directionY == 1)
        {
            rayOrigin = direcctionX == 1 ? cs.TopRight : cs.TopLeft;
            spacingDirection = direcctionX == 1 ? -1 : 1;
        }

        for(int i = 0; i<rayAmount; i++)
        {
            if (i == 4 && directionY == 1)
                offset = Vector2.zero;

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin + offset + Vector2.right * i * horizontalRaySpacing * spacingDirection, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin + offset + Vector2.right * i * horizontalRaySpacing * spacingDirection, Vector2.up * directionY * rayLength, Color.yellow);
            if(hit)
            {
                if (ignoreColliderWithTag.Contains(hit.transform.tag) && directionY == 1)
                    continue;

                float angle = Vector2.Angle(hit.normal, Vector2.up);

                float distance = (hit.distance - skinWidth) * directionY;
                
                moveVec.y = distance;

                if(cs.climbingSlope)
                {
                    if (distance < skinWidth)
                        moveVec.y = hit.distance - skinWidth;
                    moveVec.x = (moveVec.y / (Mathf.Tan(Mathf.Deg2Rad * cs.slopeAngle))) * direcctionX;                    
                }

                cs.collidingUp = cs.collidingUp || directionY == 1;
                cs.collidingDown = cs.collidingDown || directionY == -1 && angle <= slopeClimbMaxAngle;
                
                rayLength = hit.distance;
            }
        }

        if(cs.descendingSlope)
        {
            rayOrigin = (direcctionX == 1 ? cs.TopRight : cs.TopLeft) + moveVec;
            rayLength = skinWidth * 2;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.blue);
            if (hit)
            {       
                float angle = Vector2.Angle(hit.normal, Vector2.down);
                if (angle != 0)
                {
                    moveAmountX = (skinWidth / Mathf.Tan(angle * Mathf.Deg2Rad));
                    moveVec.x -= moveAmountX * direcctionX;
                    moveVec.y = Mathf.Tan(cs.slopeAngle * Mathf.Deg2Rad) * moveVec.x;
                }
                cs.collidingUp = true;
            }            
        }

        if(cs.climbingSlope && Mathf.Abs(moveVec.x) > 0)
        {
            rayOrigin = (direcctionX == 1 ? cs.BottomRight : cs.BottomLeft) + Vector2.up * moveVec.y;
            rayLength = Mathf.Abs(moveVec.x) + skinWidth;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * direcctionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * rayLength * direcctionX, Color.black);
            if(hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(slopeAngle != cs.slopeAngle)
                {
                    moveVec.x = (hit.distance - skinWidth) * direcctionX;
                }
            }
        }
    }
}
