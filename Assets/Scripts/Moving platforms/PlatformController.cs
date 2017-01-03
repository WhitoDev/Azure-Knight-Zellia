using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RayCaster 
{
    public Vector2 move;
    public bool oneWayPlatform;
    private Vector2 moveVec;
    private FollowPath pathController;

    public class PassangerMovement
    {
        public Transform transform;
        public Vector2 pushVec;
        public bool pushPassangerBeforePlatform;
        public bool standingOnPlatform;

        public PassangerMovement(Transform transform, Vector2 pushVec, bool pushPassangerBeforePlatform, bool standingOnPlatform)
        {
            this.transform = transform;
            this.pushVec = pushVec;
            this.pushPassangerBeforePlatform = pushPassangerBeforePlatform;
            this.standingOnPlatform = standingOnPlatform;
        }
    }

    public List<PassangerMovement> passangerList = new List<PassangerMovement>();

    void Start()
    {
        rayAmount = 10;
        pathController = GetComponent<FollowPath>();
    }

    void Update()
    {
        if (pathController == null)
            moveVec = move * Time.deltaTime;
        else
            moveVec = pathController.CalculateMoveVec();

        MovePassanger(false);
        CalculatePassangerPush();
        MovePassanger(true);

        this.transform.Translate(moveVec);
    }

    void MovePassanger(bool pushPassangerBeforePlatform)
    {
        foreach(PassangerMovement passanger in passangerList)
        {
            if (passanger.pushPassangerBeforePlatform == pushPassangerBeforePlatform)
            {
                Vector2 pushVec = new Vector2(passanger.pushVec.x, passanger.pushVec.y);
                passanger.transform.GetComponent<CharacterController2D>().Move(ref pushVec, passanger.standingOnPlatform);
            }
        }
    }

    void CalculatePassangerPush()
    {
        cs.Reset();
        CalculateColliderCoordinates();        

        float direcctionX = Mathf.Sign(moveVec.x);
        float direcctionY = Mathf.Sign(moveVec.y);

        HashSet<Transform> movedPassangers = new HashSet<Transform>();
        passangerList = new List<PassangerMovement>();

        //if the platform is moving vertically
        if (moveVec.y != 0)
        {
            Vector2 rayOrigin = direcctionY == 1 ? cs.TopLeft : cs.BottomLeft;
            float rayLenght = Mathf.Abs(moveVec.y) + skinWidth;
            for (int i = 0; i < rayAmount; i++)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin + Vector2.right * horizontalRaySpacing * i, Vector2.up * direcctionY, rayLenght, collisionMask);
                Debug.DrawRay(rayOrigin + Vector2.right * horizontalRaySpacing * i, Vector2.up * rayLenght * direcctionY, Color.red);
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit)
                    {
                        if (oneWayPlatform)
                        {
                            if (hit.distance == 0 && direcctionY == 1)
                                continue;
                        }

                        if (!movedPassangers.Contains(hit.transform))
                        {
                            movedPassangers.Add(hit.transform);
                            float pushX = direcctionY == 1 ? moveVec.x : 0;
                            float pushY = moveVec.y;

                            passangerList.Add(new PassangerMovement(hit.transform, new Vector2(pushX, pushY), direcctionY == 1, direcctionY == 1));
                        }
                    }
                }                
            }
        }

        //if the platform is moving horizontally        
        if (moveVec.x != 0 && !oneWayPlatform)
        {
            Vector2 rayOrigin = direcctionX == 1 ? cs.BottomRight : cs.BottomLeft;
            float rayLenght = Mathf.Abs(moveVec.x) + skinWidth;
            for (int i = 0; i < rayAmount; i++)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin + Vector2.up * verticalRaySpacing * i, Vector2.right * direcctionX, rayLenght, collisionMask);
                Debug.DrawRay(rayOrigin + Vector2.up * verticalRaySpacing * i, Vector2.right * direcctionX * rayLenght, Color.red);
                foreach(RaycastHit2D hit in hits)
                {
                    if (hit)
                    {
                        if (!movedPassangers.Contains(hit.transform))
                        {
                            movedPassangers.Add(hit.transform);
                            float pushX = moveVec.x;
                            float pushY = 0;

                            passangerList.Add(new PassangerMovement(hit.transform, new Vector2(pushX, pushY), true, false));
                        }
                    }
                }                
            }
        }

        //if the platform is moving downwards or horizontally, account for the passangers on top of it
        if (direcctionY == -1 || moveVec.x != 0 && moveVec.y <= 0)
        {
            Vector2 rayOrigin = cs.TopLeft;
            float rayLenght = skinWidth * 2;
            for (int i = 0; i < rayAmount; i++)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin + Vector2.right * horizontalRaySpacing * i, Vector2.up, rayLenght, collisionMask);
                Debug.DrawRay(rayOrigin + Vector2.right * horizontalRaySpacing * i, Vector2.up * rayLenght, Color.magenta);
                foreach(RaycastHit2D hit in hits)
                {
                    if (hit)
                    {
                        if (!movedPassangers.Contains(hit.transform))
                        {
                            movedPassangers.Add(hit.transform);
                            float pushX = moveVec.x;
                            float pushY = moveVec.y;

                            passangerList.Add(new PassangerMovement(hit.transform, new Vector2(pushX, pushY), false, true));
                        }
                    }
                }                
            }
        }        
    }
}
