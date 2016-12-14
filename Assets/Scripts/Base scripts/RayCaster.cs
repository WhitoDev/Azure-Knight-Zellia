using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class RayCaster : MonoBehaviour 
{
    private Collider2D _collider;
    internal Collider2D collider
    {
        get
        {
            if (_collider == null)
                _collider = GetComponent<Collider2D>();
            return _collider;
        }
    }
    internal float skinWidth = .015f;
    public LayerMask collisionMask;
    public List<string> ignoreColliderWithTag;
    internal const int rayAmount = 5;
    internal float horizontalRaySpacing;
    internal float verticalRaySpacing;

    public void CalculateColliderCoordinates()
    {
        cs.bounds = collider.bounds;

        cs.width = cs.bounds.size.x;
        cs.height = cs.bounds.size.y;

        cs.Right = collider.bounds.max.x;
        cs.Left = collider.bounds.min.x;
        cs.Top = collider.bounds.max.y;
        cs.Bottom = collider.bounds.min.y;
        cs.TopRight = (Vector2)collider.bounds.max - Vector2.one * skinWidth;
        cs.BottomLeft = (Vector2)collider.bounds.min + Vector2.one * skinWidth;
        cs.BottomRight = new Vector2(cs.Right - skinWidth, cs.Bottom + skinWidth);
        cs.TopLeft = new Vector2(cs.Left + skinWidth, cs.Top - skinWidth);
    }

    public struct ColliderStatus
    {
        public Bounds bounds;

        public bool collidingLeft;
        public bool collidingRight;
        public bool collidingUp;
        public bool collidingDown;
        public bool climbingSlope;
        public bool descendingSlope;

        public bool wasCollidingLeft;
        public bool wasCollidingRight;
        public bool wasCollidingUp;
        public bool wasCollidingDown;
        public bool wasClimbingSlope;
        public bool wasDescendingSlope;
        //public bool stay;

        public float width;
        public float height;

        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
        public Vector2 BottomLeft;
        public Vector2 BottomRight;
        public Vector2 TopLeft;
        public Vector2 TopRight;
        public Vector2 initialMoveVec;

        public float slopeAngle;
        public float slopeAngleOld;

        public Transform activePlatform;
        public Vector2 activePlatformPos;

        public void Reset()
        {
            wasCollidingLeft = collidingLeft;
            wasCollidingRight = collidingRight;
            wasCollidingUp = collidingUp;
            wasCollidingDown = collidingDown;
            wasClimbingSlope = climbingSlope;
            wasDescendingSlope = descendingSlope;

            collidingDown =
            collidingLeft =
            collidingRight =
            collidingUp =
            climbingSlope =
            descendingSlope = false;

            initialMoveVec = Vector2.zero;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }

    public ColliderStatus cs = new ColliderStatus();

    void Awake()
    {
        CalculateColliderCoordinates();
        verticalRaySpacing = (cs.height - (2 * skinWidth)) / (rayAmount - 1);
        horizontalRaySpacing = (cs.width - (2 * skinWidth)) / (rayAmount - 1);
    }
}
