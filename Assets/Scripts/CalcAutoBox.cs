using UnityEngine;
using System.Collections;

public class CalcAutoBox : MonoBehaviour 
{
	[SerializeField]Vector3 start;
	[SerializeField]Vector3 end;

	void Start()
	{
		DoTest ();
	}

	void DoTest()
	{
		float height = 3.0f;

		Vector3 center = Vector3.Lerp(start, end, 0.5f);
		center.y = height * 0.5f;

		BoxCollider box = this.gameObject.AddComponent<BoxCollider> ();
		box.center = center;
		box.size = new Vector3(end.x-start.x, height, end.z-start.z);
	}

	void OnDrawGizmos()
	{
		BoxCollider box = this.gameObject.GetComponent<BoxCollider> ();
		if(box) {
			Gizmos.DrawCube (box.center, box.size);
		}
	}

}
