using UnityEngine;
using System.Collections;

public class CircleController : MonoBehaviour 
{
	const int	FPS				= 30;
	const float	FOWARD_ANGLE	=-90.0f;
	const float	ARIVE_RANGE		= 3.0f;
	const float	INCREMENT_ROT_T	= 0.02f;
	
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

	enum MoveState
	{
		Stop,	// 停止中.
		Move, 	// 移動中.
	}

	#region Inspector Property.
	[SerializeField] Vector3	m_tgt_pos;
	[SerializeField] float		m_speed_m_s;
	[SerializeField] float		m_accel_m_s;
	[SerializeField] float		m_mass;
	[SerializeField] float		m_friction;
	[SerializeField] float		m_bounciness;
	
	[SerializeField] Vector3	m_add_force;
	[SerializeField] Vector3	m_add_torque;
	[SerializeField] Collider	m_collider;
	#endregion

	[SerializeField] MoveState	m_state;
	[SerializeField] float		m_speed;
	[SerializeField] float		m_accel;
	[SerializeField] Vector3	m_force; 
	[SerializeField] Vector3	m_torque;
	[SerializeField] Vector3	m_collect;
	[SerializeField] Vector3	m_reflect;

	[SerializeField] Quaternion	m_start_rot;
	[SerializeField] float		m_rot_t;

	public bool IsStop { get { return m_state == MoveState.Stop; } }
	
	public bool IsMove { get { return m_state == MoveState.Move; } }

	public Bounds Bounds
	{
		get
		{
			return m_collider.bounds;
		}
	}

	public float Radius
	{
		get
		{
			return Bounds.extents.x;
		}
	}

	public Vector3 Power
	{
		get
		{
//			Vector3 dir = m_tgt_pos - transform.position;
//			return dir.normalized ;
//			return transform.right;
			return m_force;
		}
	}
	
	public void OnCollison(CircleController opposite)
	{
		// 1.collect
		{
			float r = Radius + opposite.Radius;
			float vx  = transform.position.x - opposite.transform.position.x;
			float vz  = transform.position.z - opposite.transform.position.z;
			float len = Mathf.Sqrt(vx * vx + vz * vz);
			float distance = r - len;
			
			if(len > 0.0f) len = 1.0f / len;
			vx *= len;
			vz *= len;
			
			distance /= 2.0f;
			m_collect.x += vx * distance;
			m_collect.z += vz * distance;
			opposite.m_collect.x -= vx * distance;
			opposite.m_collect.z -= vz * distance;
		}

		// 2.Reflect.
		{
			Vector3 v = opposite.transform.position - transform.position;

			float t1 = -(v.x * Power.x + v.z * Power.z) / (v.x * v.x + v.z * v.z);
			float arx = Power.x + v.x * t1;
			float arz = Power.z + v.z * t1;
			
			float t2 = -(-v.z * Power.x + v.x * Power.z) / (v.z * v.z + v.x * v.x);
			float amx = Power.x - v.z * t2;
			float amz = Power.z + v.x * t2;
			
			float t3 = -(v.x * opposite.Power.x + v.z * opposite.Power.z) / (v.x * v.x + v.z * v.z);
			float brx = opposite.Power.x + v.x * t3;
			float brz = opposite.Power.z + v.z * t3;
			
			float t4 = -(-v.z * opposite.Power.x + v.x * opposite.Power.z) / (v.z * v.z + v.x * v.x);
			float bmx = opposite.Power.x - v.z * t4;
			float bmz = opposite.Power.z + v.x * t4;

			float e = 1.0f;
			float am = m_mass;
			float bm = opposite.m_mass;

			float adx = (am * amx + bm * bmx + bmx * e * bm - amx * e * bm) / (am + bm);
			float bdx = - e * (bmx - amx) + adx;
			float adz = (am * amz + bm * bmz + bmz * e * bm - amz * e * bm) / (am + bm);
			float bdz = - e * (bmz - amz) + adz;

			m_reflect.x += adx + arx;
			m_reflect.z += adz + arz;
			opposite.m_reflect.x += bdx + brx;
			opposite.m_reflect.z += bdz + brz;
		}
	}

	public void ApplyCollision()
	{
		if (m_collect.magnitude > 0.0f) 
		{
//			transform.position += m_collect;
			m_collect = Vector3.zero;
		}

		if (m_reflect.magnitude > 0.0f) 
		{
//			transform.position += m_reflect;
			m_force = m_reflect;
			m_speed = 0.0f;
			m_reflect = Vector3.zero;
		}
	}

	public void Move(Vector3 tgt_pos)
	{
		m_tgt_pos = tgt_pos;

		Vector3 dir = m_tgt_pos - transform.position;
		m_force += dir.normalized*Speed.CalcSpeed (m_speed_m_s, Meter.M, Time.SEC);

		m_start_rot = transform.rotation;
		m_rot_t = 0.0f;

		m_state = MoveState.Move;
	}

	public void Move()
	{
		Vector3 dir = m_tgt_pos - transform.position;

		if (IsArrived (dir)) 
		{
			Stop();
		}

		if (IsMove) 
		{
			RotateSmothly (dir);
//			transform.position += (m_speed * dir.normalized);
			m_force += (m_accel * dir.normalized);
		}

		if (m_force.sqrMagnitude > 0.0f) 
		{
			m_force *= m_friction;
			transform.position += m_force;
		}
	}

	public void Stop()
	{
		m_speed *= 0.0f;
		m_state = MoveState.Stop;
	}

	public void AddForce()
	{
		m_force += m_add_force;
	}

	public void AddTorque()
	{
		m_torque += m_add_torque;
	}

	void Awake()
	{
		m_accel = Speed.CalcAccel (m_accel_m_s, Meter.M, Time.SEC);
		m_state = MoveState.Stop;
	}

	bool IsArrived(Vector3 dir)
	{
		return (dir.sqrMagnitude < ARIVE_RANGE * ARIVE_RANGE);
	}

	void RotateSmothly(Vector3 dir)
	{
		float angle = Mathf.Atan2 (dir.x, dir.z) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Slerp(m_start_rot, Quaternion.Euler(0.0f, FOWARD_ANGLE + angle, 0.0f), m_rot_t);
		m_rot_t += INCREMENT_ROT_T;
	}

	#region debug property.
	void OnMove()
	{
		if (m_tgt_pos.sqrMagnitude <= 0.0f)
			return;
		
		Move (m_tgt_pos);
	}
	
	void OnStop()
	{
		Stop ();
	}
	#endregion
}
