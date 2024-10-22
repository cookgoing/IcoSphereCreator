using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAtan : MonoBehaviour
{
	public Transform target;

    void Update()
    {
		if (Input.GetKeyDown(KeyCode.A))
		{
			Vector3 relative = transform.InverseTransformPoint(target.position);
        	float angle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
			float angle2 = Mathf.Atan(relative.x / relative.z) * Mathf.Rad2Deg;
			print($"angle: {angle}; angle2: {angle2}; color: {new Color()}");
        	// transform.Rotate(0, angle, 0);
		}
    }

	[ContextMenu("TestTangle")]
	void TestTangle()
	{
		Vector3 normal = new Vector3(0.0f, 1.0f, 1.0f);  // 法线
		Vector3 tangent = new Vector3(1.0f, 0.0f, 0.0f); // 切线

		Vector3.OrthoNormalize(ref normal, ref tangent);

		print($"[TestTangle]. normal: {normal}; tangent: {tangent}");
	}
}
