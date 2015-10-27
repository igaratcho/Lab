using UnityEngine;
using System.Collections;

public class MoveController : MonoBehaviour 
{
	const int FPS = 30;

	static class Time
	{
		public static readonly float SEC = 1.0f  * FPS;
		public static readonly float MIN = 60.0f * SEC;
		public static readonly float HOUR= 60.0f * MIN;
	}
	
	static class Meter
	{
		public static readonly float MM = CM * 0.1f;
		public static readonly float CM = M  * 0.01f;
		public static readonly float M  = 1.0f;
		public static readonly float KM = M * 1000.0f;
	}
	
	static class Speed
	{
		public static float CalcSpeed(float speed, float meter, float time)
		{
			return (speed * meter) / time;
		}

		public static float CalcAccel(float speed, float meter, float time)
		{
			return (speed * meter) / (time * time);
		}
	}

	#region InspectorProperty
	[SerializeField] Vector3	m_tgt_pos;
	[SerializeField] float		m_speed_m_s;
	[SerializeField] float		m_accel_m_s;
	[SerializeField] float		m_friction;

	[SerializeField] Vector3	m_add_force;
	[SerializeField] Vector3	m_add_torque;
	#endregion
	
	[SerializeField] float		m_speed;
	[SerializeField] float		m_accel;
	[SerializeField] Vector3	m_force; 
	[SerializeField] Vector3	m_torque;

	bool m_started = false;
	bool m_movable = false;
	
	void Init () 
	{
		m_speed = Speed.CalcSpeed (m_speed_m_s, Meter.M, Time.SEC);
		m_accel = Speed.CalcAccel (m_accel_m_s, Meter.M, Time.SEC);
	}

	void Update () 
	{
		Move ();
		Rotate ();
	}

	void Move()
	{
		if (m_movable) 
		{
			m_speed += m_accel;
		}

//		if (m_speed <= 0.0f)
//			return;

		m_speed *= m_friction;
		m_force *= m_friction;

//		if (m_speed <= 0.0f) m_speed = 0.0f;

		Vector3 dir = (m_tgt_pos - transform.position).normalized;
		transform.position += (dir * m_speed);
		transform.position += m_force;
	}

	void Rotate()
	{
		if (m_torque.sqrMagnitude < 0.1f)
			return;

		m_torque *= m_friction;
		transform.Rotate (m_torque);
	}

	void AddForce(Vector3 force)
	{
		m_force += force;
	}

	void AddTorque(Vector3 truqe)
	{
		m_torque += truqe;
	}

	void OnGUI()
	{
		if (m_started) 
		{
			string btn_name = m_movable ? "Stop" : "Move" ;
			if (GUILayout.Button (btn_name)) 
			{
				m_movable = !m_movable;
			}

			if (GUILayout.Button ("AddForce")) 
			{
				AddForce(m_add_force);
			}

			if (GUILayout.Button ("AddTorque")) 
			{
				AddTorque(m_add_torque);
			}
		} 
		else 
		{
			if (GUILayout.Button ("Start")) 
			{
				Init ();
				m_started = true;
				m_movable = true;
			}
		}
	}
}
