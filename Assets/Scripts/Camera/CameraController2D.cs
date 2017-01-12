using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Camera), typeof(CharacterController2D))]
public class CameraController2D : PixelPerfectCamera 
{
    public Vector2 cameraBoxSize;
    public Collider2D target;
    private CharacterController2D charController;

    public struct BoundSides
    {
        public float left;
        public float right;
        public float up;
        public float down;
    }

    private BoundSides cameraBoundSides;
    private BoundSides targetBoundSides;

    void Start()
    {
        charController = GetComponent<CharacterController2D>();
    }

    public override void LateUpdate()
    {
        CalculateBounds(target.bounds, ref targetBoundSides);
        CalculateBounds(new Bounds(this.transform.position, cameraBoxSize), ref cameraBoundSides);

        Vector2 moveVec = Vector2.zero;
        float moveX = 0;
        float moveY = 0;

        if(targetBoundSides.left < cameraBoundSides.left)
        {
            moveX = targetBoundSides.left - cameraBoundSides.left;
        }

        if (targetBoundSides.right > cameraBoundSides.right)
        {
            moveX = targetBoundSides.right - cameraBoundSides.right;
        }

        if (targetBoundSides.down < cameraBoundSides.down)
        {
            moveY = targetBoundSides.down - cameraBoundSides.down;
        }

        if (targetBoundSides.up > cameraBoundSides.up)
        {
            moveY = targetBoundSides.up - cameraBoundSides.up;
        }

        if(moveX != 0 || moveY != 0)
        {
            moveVec = new Vector2(moveX, moveY);
            charController.Move(ref moveVec);
        }
        

        base.LateUpdate();
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = new Color(1, 0, 0, .5f);
        Gizmos.DrawCube(transform.position, cameraBoxSize);
    }
    #endif

    void CalculateBounds(Bounds bounds, ref BoundSides boundSides)
    {
        boundSides.left = bounds.min.x;
        boundSides.right = bounds.max.x;
        boundSides.up = bounds.max.y;
        boundSides.down = bounds.min.y;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraController2D))]
public class CameraControllerEditor: PixelPerfectCameraEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CameraController2D controller = (CameraController2D)target;
        controller.target = EditorGUILayout.ObjectField("Target to follow", controller.target,typeof(Collider2D)) as Collider2D;
        controller.cameraBoxSize = EditorGUILayout.Vector2Field("Camera bounds size", controller.cameraBoxSize);
    }
}
#endif