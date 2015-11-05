using UnityEngine;
using System.Collections;

public class MoveController : MonoBehaviour 
{
	const int	FPS				= 30;
	const float	FOWARD_ANGLE	=-90.0f;
	const float	ARIVE_RANGE		= 3.0f;
	const float	INCREMENT_ROT_T	= 0.02f;
	const float CORRECT_COEF	= 0.1f;

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

	[SerializeField] float		m_mass;
	[SerializeField] Vector3	m_correct;
	[SerializeField] Vector3	m_reflect;
	[SerializeField] float		m_radius;

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

	public float Radius
	{
		get
		{
//			return Bounds.extents.x;
			return m_radius;
		}
	}
	
	public Vector3 Force
	{
		get
		{
//			return Forward;
			return m_force;
		}
	}
	
	public void ExecCollision(MoveController opposite)
	{
		if (opposite.IsStop) 
		{
			Stop ();
		}

		Vector3 dir = opposite.transform.position - transform.position;

		// ------------------- 位置補正 ------------------- //.
		{
			// 相手との最短距離を取得.
			float r = Radius + opposite.Radius;

			// めり込んだ値を算出.
			float distance = r - dir.magnitude;

			// めり込んだ値の半分を補正値とする.
			distance *= 0.5f;

			// 補正方向.
			Vector3 correct = dir.normalized * distance;

			// 調整用係数.
			correct *= CORRECT_COEF;

			// 値を反映.
			m_correct -= correct;
			opposite.m_correct += correct;
		}

		// ------------------- 反射 ------------------- //.
		{
			// 相手への向きを正規化.
			Vector3 n_dir = dir.normalized;

			// 力の向き.
			Vector3 force_a = Force;
			Vector3 force_b = opposite.Force; 

			// 相手への向きと、力の向きとの内積を求める.
			float dot_a = Vector3.Dot (force_a, n_dir);
			float dot_b = Vector3.Dot (force_b, n_dir);

			// 新たな力の向き = 力の向き - (相手への向き * 内積値) を求める (※力と相手へ向きが同じ場合は0になる).
			Vector3 new_force_a = force_a - (n_dir * dot_a);
			Vector3 new_force_b = force_b - (n_dir * dot_b);

			// 質量.
			float am = m_mass;
			float bm = opposite.m_mass;

			// 弾性係数,
			float ae = m_bounciness;
			float be = opposite.m_bounciness;

			// 反発力を求める
			float reflect_power_a = (am * dot_a + bm * dot_b - bm * ae * (dot_a - dot_b) ) / (am + bm);
			float reflect_power_b = (am * dot_a + bm * dot_b + am * be * (dot_a - dot_b) ) / (am + bm);

			// 値を反映.
			m_reflect = new_force_a + (n_dir * reflect_power_a);
			opposite.m_reflect = new_force_b + (n_dir * reflect_power_b);
		}
	}

	public void ApplyCollision()
	{
		if (m_correct.magnitude > 0.0f) 
		{
			transform.position += m_correct;
			m_correct = Vector3.zero;
		}

		if (m_reflect.magnitude > 0.0f) 
		{
			m_force = m_reflect;
			m_reflect = Vector3.zero;
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

		Vector3 dir = m_tgt_pos - transform.position;
		m_force += dir.normalized * Speed.CalcSpeed (m_speed_m_s, Meter.M, Time.SEC);

		m_start_rot = transform.rotation;
		m_rot_t = 0.0f;

		m_state = MoveState.Move;
	}
	
	public void Stop()
	{
//		m_force *= 0.5f;
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

		Vector3 extents = Bounds.extents;
		m_radius = Mathf.Sqrt (extents.x * extents.x + extents.z * extents.z);
	}

	void Move()
	{
		Vector3 dir = m_tgt_pos - transform.position;

		if (IsArrived (dir)) 
		{
			Stop();
		}

		if (IsMove) 
		{
			RotateSmothly (dir);
			m_force += (m_accel * dir.normalized);	// @note 加速度は可変フレームへの対応が必要.
		}
		
		if (m_force.sqrMagnitude > 0.0f) 
		{
			m_force *= IsMove ? m_friction : 0.8f ;	// @note 摩擦力は可変フレームへの対応が必要.
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
