using UnityEngine;
using System.Collections;

public class TargetTrackProyectile : MonoBehaviour 
{
    public Transform target;
    public float dot;
    public float angle;
    public float moveSpeed = 5f;

    void Start()
    {
        if(target == null)
        {
            target = FindObjectOfType<Player>().transform;
        }
    }
    void Update()
    {
        if (target != null)
        {
            Vector2 newRot = -(target.position - transform.position);

            newRot = Vector2.Lerp(transform.right, newRot, Time.deltaTime * 10);

            this.transform.right = newRot;

            this.transform.Translate(-transform.right * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}
