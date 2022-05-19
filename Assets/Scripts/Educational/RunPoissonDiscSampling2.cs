using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunPoissonDiscSampling2 : MonoBehaviour {

	public float radius = 1;
	public Vector3 regionSize = Vector3.one;
	public int rejectionSamples = 30;
	public float displayRadius =1;

	List<Vector3> points;


	void OnValidate() {
		float yPos = 1.0f;
		points = PoissonDiscSampling2.GeneratePoints(radius, regionSize, yPos, rejectionSamples);
	}

	void OnDrawGizmos() {
		Gizmos.DrawWireCube(regionSize/2,regionSize);
		if (points != null) {
			foreach (Vector3 point in points) {
				Gizmos.DrawSphere(point, displayRadius);
			}
		}
	}
}
