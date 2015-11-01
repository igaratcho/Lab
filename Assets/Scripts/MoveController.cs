using UnityEngine;
using System.Collections;

public class MoveController : MonoBehaviour 
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

	#region InspectorProperty
	[SerializeField] Vector3	m_tgt_pos;
	[SerializeField] float		m_speed_m_s;
	[SerializeField] float		m_accel_m_s;
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

	[SerializeField] Quaternion	m_start_rot;
	[SerializeField] float		m_rot_t;

	public Vector3	m_resolver;
	
	public bool		IsStop	{ get { return this.m_state == MoveState.Stop; } }

	public bool		IsMove	{ get { return this.m_state == MoveState.Move; } }

	public Vector3	ShipPos	{ get { return this.transform.position; } }

	public Vector3 Forward
	{
		get
		{
			return transform.right;
		}
	}

	public Bounds Bounds
	{
		get
		{
			return m_collider.bounds;
		}
	}

	public void ExecCollision(MoveController oppsite)
	{
		Vector3 power = Vector3.zero;

		if (m_speed > 0.0f) 
		{
			power += (m_speed * Forward.normalized);
		}
		
		if (m_force.sqrMagnitude > 0.0f) 
		{
			power += (m_force);
		}

		if (power.sqrMagnitude > 0.0f) 
		{
			Vector3 reflect = (oppsite.transform.position - transform.position).normalized;
			float dot = Vector3.Dot(power.normalized, reflect);
			power *= dot;

			power += reflect * m_bounciness;

//			Debug.LogWarning(string.Format("{0} power:{1} reflect:{2} dot:{3}",this.name, power, reflect, dot));

//			transform.position += -power;
//			oppsite.transform.position += power;

			m_resolver += -power;
			oppsite.m_resolver += power;
		}
	}

	public void ExexMove () 
	{
		Move ();
		Torque ();
	}
	
	public void Move(Vector3 tgt_pos)
	{
		m_tgt_pos = tgt_pos;
		m_speed = Speed.CalcSpeed (m_speed_m_s, Meter.M, Time.SEC);
		m_start_rot = transform.rotation;
		m_rot_t = 0.0f;

		m_state = MoveState.Move;
	}
	
	public void Stop()
	{
		m_speed *= 0.1f;
		m_state = MoveState.Stop;
	}

	public void AddForce(Vector3 force)
	{
		m_force += force;
	}
	
	public void AddTorque(Vector3 truqe)
	{
		m_torque += truqe;
	}

	void Awake () 
	{
		m_accel		= Speed.CalcAccel (m_accel_m_s, Meter.M, Time.SEC);
		m_start_rot	= Quaternion.identity;
		m_rot_t		= 0.0f;
		m_state		= MoveState.Stop;
	}

	void Move()
	{
		if (m_resolver.sqrMagnitude > 0.0f) 
		{
			Debug.LogWarning(string.Format("{0} power:{1}",this.name, m_resolver));

			transform.position += m_resolver;
			m_resolver = Vector3.zero;

			Stop ();
		}

		Vector3 dir = (m_tgt_pos - ShipPos);

		if (IsArrived (dir)) 
		{
			Stop();
		}

		if (IsMove) 
		{
			m_speed += m_accel;
			RotateSmothly (dir);
		}

		if (m_speed > 0.0f) 
		{
			m_speed *= m_friction;
			transform.position += (m_speed * dir.normalized);
		}
		
		if (m_force.sqrMagnitude > 0.0f) 
		{
			m_force *= m_friction;
			transform.position += m_force;
		}
	}

	void Torque()
	{
		if (m_torque.sqrMagnitude < 0.1f)
			return;

		m_torque *= m_friction;
		transform.Rotate (m_torque);
	}

	void RotateSmothly(Vector3 dir)
	{
		float angle = Mathf.Atan2 (dir.x, dir.z) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Slerp(m_start_rot, Quaternion.Euler(0.0f, FOWARD_ANGLE + angle, 0.0f), m_rot_t);
		m_rot_t += INCREMENT_ROT_T;
	}

	bool IsArrived(Vector3 dir)
	{
		return (dir.sqrMagnitude < ARIVE_RANGE * ARIVE_RANGE);
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

	void OnAddForce()
	{
		AddForce(m_add_force);
	}

	void OnAddTorque()
	{
		AddTorque (m_add_torque);
	}
	#endregion.
}
