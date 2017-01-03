using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ShowPath : MonoBehaviour 
{
	public Transform[] points;

	public IEnumerator<Transform> GetPathsEnumerator()
	{
		if (points == null || points.Length < 1) {
			yield break;
				}

		var direction = 1;
		var index = 0;

		while (true) {
			yield return points[index];

			if(points.Length == 2)
				continue;

			if(index <= 1)
				direction = 1;
			else if (index >= points.Length - 1)
				direction = -1;

			index = index + direction;
				}

	}

	public void OnDrawGizmos()
	{
		points = this.GetComponentsInChildren<Transform> ();

		if (points.Length < 2)
			return;

		for (var i = 2; i < points.Length; i++)
		{
			Gizmos.DrawLine(points[i-1].position,points[i].position);
		}
	}
}