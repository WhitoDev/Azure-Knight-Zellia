using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraController2D : PixelPerfectCamera 
{
    public Vector2 cameraBoxSize;
    public Collider2D target;   

    public struct BoundSides
    {
        public float left;
        public float right;
        public float up;
        public float down;
    }

    private BoundSides cameraBoundSides;
    private BoundSides targetBoundSides;

    public override void LateUpdate()
    {
        CalculateBounds(target.bounds, ref targetBoundSides);
        CalculateBounds(new Bounds(this.transform.position, cameraBoxSize), ref cameraBoundSides);

        if(targetBoundSides.left < cameraBoundSides.left)
        {
            transform.Translate(targetBoundSides.left - cameraBoundSides.left,0,0);
        }

        if (targetBoundSides.right > cameraBoundSides.right)
        {
            transform.Translate(targetBoundSides.right - cameraBoundSides.right, 0, 0);
        }

        if (targetBoundSides.down < cameraBoundSides.down)
        {
            transform.Translate(0, targetBoundSides.down - cameraBoundSides.down, 0);
        }

        if (targetBoundSides.up > cameraBoundSides.up)
        {
            transform.Translate(0, targetBoundSides.up - cameraBoundSides.up, 0);
        }
        base.LateUpdate();
    }

    void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = new Color(1, 0, 0, .5f);
        Gizmos.DrawCube(transform.position, cameraBoxSize);
    }

    void CalculateBounds(Bounds bounds, ref BoundSides boundSides)
    {
        boundSides.left = bounds.min.x;
        boundSides.right = bounds.max.x;
        boundSides.up = bounds.max.y;
        boundSides.down = bounds.min.y;
    }
}
