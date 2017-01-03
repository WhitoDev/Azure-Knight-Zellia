using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlatformController))]
public class FollowPath : MonoBehaviour {

	public enum FollowType
	{
		MoveTowards,
		Lerp,
	}

	public FollowType Type = FollowType.MoveTowards;
	public ShowPath Path;
	public float Speed = 1;
	public float MaxDistanceToGoal = .1f;

    Vector2 previousPoint;
    Vector2 currentPoint;

	private IEnumerator<Transform> _currentPoint;

	public void Start()
	{
		if (Path == null) {
			Debug.LogError("Path cannot be null", gameObject);
				return;
				}
		_currentPoint = Path.GetPathsEnumerator ();
		_currentPoint.MoveNext ();

		if (_currentPoint.Current == null)
						return;

		transform.position = _currentPoint.Current.position;

	}

	public Vector2 CalculateMoveVec()
	{
        Vector2 moveVec = Vector2.zero;

		if (_currentPoint == null || _currentPoint.Current == null)
			return moveVec;

        if (Type == FollowType.MoveTowards)
        {
            previousPoint = transform.position;
            moveVec = Vector2.MoveTowards(transform.position, _currentPoint.Current.position, Time.deltaTime * Speed) - previousPoint;
            currentPoint = moveVec;
        }

        if (Type == FollowType.Lerp)
        {
            previousPoint = transform.position;
            moveVec = Vector2.Lerp(transform.position, _currentPoint.Current.position, Time.deltaTime * Speed) - previousPoint;
            currentPoint = moveVec;
        }

		var distanceSquared = (transform.position - _currentPoint.Current.position).sqrMagnitude;
		if (distanceSquared < MaxDistanceToGoal * MaxDistanceToGoal)
						_currentPoint.MoveNext ();

        return moveVec;
	}
    
}