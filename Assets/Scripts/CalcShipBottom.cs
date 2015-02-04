using UnityEngine;
using System.Collections;

public class CalcShipBottom : MonoBehaviour 
{
	[SerializeField] Vector3 target_pos;

	void Start () 
	{
	}

	void OnDrawGizmos()
	{
		DoTest ();
	}

	void DoTest()
	{
		float Width		= target_pos.x;
		float Height	= target_pos.y;	// 甲板 - 高さ.

		float rot_z = GetRotation ();

		float sn = Mathf.Sin  ( GetDegToRad( rot_z ) );
		float cs = Mathf.Cos  ( GetDegToRad( rot_z ) );
		
		Vector3 v1 = this.transform.position;
		v1.x += ( Height * sn + Width * cs );
		v1.y -= ( Height * cs - Width * sn );

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(this.transform.position, 0.5f);

		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(v1, 0.5f);
	}

	float GetRotation() 
	{
		return this.transform.rotation.eulerAngles.z;
	}

	float GetDegToRad(float deg )
	{
		return deg * 2.0f * 3.141592f / 360f;
	}

}
