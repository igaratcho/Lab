using UnityEngine;
using System.Collections;

public class ShipMoveController : MonoBehaviour 
{
	#region For Test.
	[SerializeField]Vector3 m_test_target_pos = Vector3.zero;
	[SerializeField]Vector3 m_test_damage_vel = Vector3.zero;
	[SerializeField]bool	m_test_player = true;
	
	const float DYNAMIC_FRICTION		= 0.98f;
	const float DYNAMIC_FRICTION_ROT	= 0.8f;
	const float STATIC_FRICTION			= 0.95f;
	const float STATIC_FRICTION_ROT		= 0.8f;
	#endregion.	

	enum MoveState
	{
		Stop,
		Move,
	}

	const float	FOWARD_ANGLE			=-90.0f;
	const float	ARIVE_RANGE				= 3.0f;
	const float	INCREMENT_ROT_T			= 0.02f;
	const float LOCK_TIMER				= 2.0f;
	const int	MAX_COURSE_POINT		= 10;

	public float m_mass;
	public float m_speed;
	public float m_curve;
	
	delegate void Move();
	Move ExecMove;

	MoveState	m_state;
	Vector3		m_target_pos;
	Vector3		m_next_pos;
	Vector3		m_side_pos;
	Quaternion	m_start_rot;		// 開始時の傾き.
	float		m_rot_t;			// 傾きの補完率.
	
	Vector3[]	m_course_point;
	int 		m_course_idx;
	float		m_next_dist;
	float		m_wait_timer;
	float		m_lock_timer;

	// Use this for initialization
	void Start () 
	{
		Load ();

		Init ();

		if(m_test_player)
		{
			MoveToTargetPos (m_test_target_pos);
		}
	}

	// Update is called once per frame
	void FixedUpdate () 
	{
		Exec ();
	}

	void OnCollisionEnter(Collision collision) 
	{
		OnCollisionStay (collision);
	}

	void OnCollisionStay(Collision collision) 
	{
		if(IsStop == false)
		{
			ShipMoveController ship = collision.gameObject.GetComponent<ShipMoveController>();
			if(ship != null)
			{
				CalcShipHitCollision(this, ship);
			}
		}

/*
		if(m_side_pos == Vector3.zero)
		{
			ShipMoveController opposite_ship = collision.gameObject.GetComponent<ShipMoveController> ();
			if(opposite_ship)
			{
				if(m_lock_timer <= 0.0f)
				{
					this.m_lock_timer = LOCK_TIMER;

					Vector3 opposite_pos = opposite_ship.MyPos;
					float diff_z = opposite_pos.z - MyPos.z;
					
//				if(Mathf.Abs(diff_z) < 1.0f)
//				{
					m_wait_timer = 0.2f;
					m_side_pos = opposite_pos;
					
					if(diff_z <= 0.0f)
					{
						m_side_pos.z += 7.0f;
					} 
					else 
					{
						m_side_pos.z -= 7.0f;
					}
					this.ExecMove = MoveSide;
//				}
				} else {
					m_lock_timer -= Time.deltaTime;
				}
			}
		}
*/
	}

	void CalcShipHitCollision(ShipMoveController my_ship, ShipMoveController opposite_ship)
	{
		Vector3 diff = opposite_ship.transform.position - my_ship.transform.position;

		float dist = diff.magnitude;

		float sin = diff.x / dist ;
		float cos = diff.z / dist ;

		Vector3 n_diff = diff.normalized;

		Vector3 cross = Vector3.zero;
		cross.x = n_diff.z;
		cross.z = n_diff.x;

		Vector3 n_dist = Vector3.zero;
		n_dist.x = Mathf.Abs(sin);
		n_dist.z = Mathf.Abs(cos);

		float dot = Vector3.Dot (cross, n_dist);

		float limit_low = 1.0f;
		if(Mathf.Abs(dot) < limit_low)
		{
			float sign = Mathf.Sign(dot);
			n_dist.x += sign * cross.x * limit_low;
			n_dist.z += sign * cross.z * limit_low;

			diff.x += sign * cross.x * limit_low;
			diff.z += sign * cross.z * limit_low;
		}

		float power = 1.0f;

		Vector3 my_force = Vector3.zero;
		my_force.x = diff.x * -n_dist.x * power;
		my_force.z = diff.z * -n_dist.z * power;

		Vector3 opposite_force = Vector3.zero;
		opposite_force.x = diff.x * n_dist.x * power;
		opposite_force.z = diff.z * n_dist.z * power;

		my_ship.AddForce (my_force, ForceMode.Acceleration);
		opposite_ship.AddForce (opposite_force, ForceMode.Acceleration);

		Debug.Log(diff + " : " +  my_force + " : " + opposite_force);

	}

	public bool		IsStop	{ get { return this.m_state == MoveState.Stop; } }
	public Vector3	MyPos	{ get { return this.transform.position; } }

	/// <summary>
	/// ロード.
	/// </summary>
	public void Load()
	{
		AddRigidbody ();
	}
	
	/// <summary>
	/// 初期化.
	/// </summary>
	public void Init()
	{
		this.m_state		= MoveState.Stop;
		this.m_target_pos	= Vector3.zero;
		this.m_next_pos		= Vector3.zero;
		this.m_start_rot	= Quaternion.identity;
		this.m_rot_t		= 0.0f;
		this.m_course_point = new Vector3[MAX_COURSE_POINT];
		this.m_course_idx	= 0;
		this.m_next_dist	= 0.0f;
		this.m_wait_timer	= 0.0f;
		this.m_lock_timer	= LOCK_TIMER;
		this.ExecMove		= MoveStraight;
	}

	/// <summary>
	/// 実行.
	/// </summary>
	public void Exec() 
	{ 
		if(IsStop == false ) 
		{ 
			ExecMove(); 
		}
		ExecFriction ();
	}

	/// <summary>
	/// 移動（直進）.
	/// </summary>
	void MoveStraight()
	{
		Vector3 dir = this.m_target_pos - this.MyPos;
		
		if(IsArrived(dir))
		{
			Stop();
		} 
		else 
		{
			AddForce ( dir.normalized * this.m_speed );
			
			RotateSmothly (dir);
		}
	}

	/// <summary>
	/// 移動（コースに沿って）..
	/// </summary>
	void MoveAlongCourse()
	{
		if(m_wait_timer > 0.0f)
		{
			m_wait_timer -= Time.deltaTime;

			if(m_wait_timer < 0.0f)
			{
				m_wait_timer = 0.0f;

				MoveToTargetPos(m_target_pos);
			}
			return;
		}

		Vector3 dir = this.m_next_pos - this.MyPos;

		if (this.m_course_idx < MAX_COURSE_POINT) 
		{
			if( IsArrived(dir) )
			{
				this.m_course_idx++;

				if (this.m_course_idx < MAX_COURSE_POINT) 
				{
					this.m_next_pos = this.m_course_point [ this.m_course_idx ];
				} 
				else 
				{
					this.m_next_pos = this.m_target_pos;
				}

				dir = m_next_pos - this.MyPos;

				this.m_next_dist = dir.sqrMagnitude;

				ClearStartRot();
			}
			else if(IsCourseOut(dir))
			{
				this.m_wait_timer = 1.0f;
			}
		}
		else
		{
			if(IsArrived(dir))
			{
				Stop();
			} 
		}

		AddForce ( dir.normalized * this.m_speed );

		RotateSmothly (dir);
	}

	/// <summary>
	/// 移動(脇道をすり抜ける).
	/// </summary>
	void MoveSide()
	{
		Vector3 dir = this.m_side_pos - this.MyPos;
		
		if(IsArrived(dir))
		{
			ExecMove = MoveAlongCourse;
			m_side_pos = Vector3.zero;
		} 
		else 
		{
			AddForce ( dir.normalized * this.m_speed );
			
			RotateSmothly (dir);
		}
	}

	/// <summary>
	/// 摩擦.
	/// </summary>
	public void ExecFriction()
	{
		if(this.rigidbody.velocity.sqrMagnitude > 0.0f)
		{
			float friction = IsStop ? STATIC_FRICTION : DYNAMIC_FRICTION;
			this.rigidbody.velocity *= friction;
		}

		if(this.rigidbody.angularVelocity.sqrMagnitude > 0.0f)
		{
			float friction_rot = IsStop ? STATIC_FRICTION_ROT : DYNAMIC_FRICTION_ROT; 
			this.rigidbody.angularVelocity *= friction_rot;	
		}
	}

	/// <summary>
	/// ターゲット位置に移動.
	/// </summary>
	/// <param name="target_pos">ターゲット位置.</param>
	public void MoveToTargetPos(Vector3 target_pos)
	{
		this.m_target_pos = target_pos;

		ClearStartRot ();

		CreateCoursePoint ();

		this.m_next_pos = m_course_point [ 0 ];

		this.m_state = MoveState.Move;
	}

	/// <summary>
	/// 停止.
	/// </summary>
	public void Stop()
	{
		this.m_state = MoveState.Stop;
	}

	/// <summary>
	/// 速度を追加.
	/// </summary>
	/// <param name="velocity">速度.</param>
	public void AddVelocity(Vector3 velocity)
	{
		this.rigidbody.velocity += velocity;
	}

	/// <summary>
	/// 外力を追加.
	/// </summary>
	/// <param name="force">外力.</param>
	/// <param name="mode">外力の加え方.</param>
	public void AddForce(Vector3 force, ForceMode mode=ForceMode.Acceleration)
	{
		this.rigidbody.AddForce (force, mode);
	}

	/// <summary>
	/// 剛体を追加.
	/// </summary>
	void AddRigidbody()
	{
		this.gameObject.AddComponent<Rigidbody>();
		
		this.rigidbody.useGravity = false;
		this.rigidbody.mass = m_mass;
		this.rigidbody.drag = 0.1f;
		this.rigidbody.angularDrag = 0.1f;
//		this.rigidbody.centerOfMass = new Vector3(-3.0f, 1.0f, 0);
		this.rigidbody.interpolation=RigidbodyInterpolation.Interpolate;
		this.rigidbody.constraints = RigidbodyConstraints.FreezePositionY | 
									 RigidbodyConstraints.FreezeRotationX | 
									 RigidbodyConstraints.FreezeRotationZ ;
	}
	
	/// <summary>
	///開始時の傾きをクリア.
	/// </summary>
	void ClearStartRot()
	{
		this.m_start_rot = this.transform.rotation;
		this.m_rot_t = 0.0f;
	}

	//// <summary>
	/// 到着したか.
	/// </summary>
	/// <returns>True:到着した, Flase:到着していない.</returns>
	/// <param name="dir">向き.</param>
	bool IsArrived(Vector3 dir)
	{
		return (dir.sqrMagnitude < ARIVE_RANGE * ARIVE_RANGE);
	}

	bool IsCourseOut(Vector3 dir)
	{
		return (dir.sqrMagnitude > m_next_dist);
	}

	/// <summary>
	/// 回転.
	/// </summary>
	/// <param name="dir">向き.</param>
	void Rotate(Vector3 dir)
	{
		float angle = Mathf.Atan2 (dir.x, dir.z) * Mathf.Rad2Deg;
		this.transform.rotation = Quaternion.Euler (0.0f, FOWARD_ANGLE + angle, 0.0f);
	}

	// <summary>
	/// 回転（スムーズに）.
	/// </summary>
	/// <param name="dir">向き.</param>
	void RotateSmothly(Vector3 dir)
	{
		float angle = Mathf.Atan2 (dir.x, dir.z) * Mathf.Rad2Deg;
		this.transform.rotation = Quaternion.Slerp(this.m_start_rot, Quaternion.Euler(0.0f, FOWARD_ANGLE + angle, 0.0f), this.m_rot_t);
		this.m_rot_t += INCREMENT_ROT_T;
	}

	Vector3 CalcCourseCenterPos()
	{
		Vector3 center_pos = Vector3.Lerp (this.MyPos, this.m_target_pos, 0.5f);
		center_pos.z += 10.0f * this.m_curve;
		return center_pos;
	}

	void CreateCoursePoint()
	{
		float unit = 1.0f / (float)MAX_COURSE_POINT;

		Vector3 center_pos = CalcCourseCenterPos ();
		
		for(int i=0; i<MAX_COURSE_POINT; i++)
		{
			this.m_course_point[i] = BezierCurve (this.MyPos, center_pos, this.m_target_pos, unit*i);
		}
	}

	public static Vector3 BezierCurve(Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		return BezierCurve (p1, p2, p2, p3, t);
	}
	public static Vector3 BezierCurve(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
	{
		return new Vector3 (
			BezierCurve(p1.x, p2.x, p3.x, p4.x, t),
			0.0f,
			BezierCurve(p1.z, p2.z, p3.z, p4.z, t)
			);
	}
	static float BezierCurve(float x1, float x2, float x3, float x4, float t)
	{
		return Mathf.Pow(1 - t, 3) * x1 + 3 * Mathf.Pow(1 - t, 2) * t * x2 + 3 * (1 - t) * Mathf.Pow(t, 2) * x3 + Mathf.Pow(t, 3) * x4;
	}

	void OnGUI()
	{
		if(m_test_player)
		{
			if(GUI.Button(new Rect(50.0f, 50.0f,100.0f, 50.0f), "Damage" ))
			{
				m_wait_timer = 1.0f;

//				Quaternion q = transform.rotation;
//				transform.rotation = Quaternion.Euler(0.0f, q.eulerAngles.y+5.0f, 0.0f);
				AddVelocity(m_test_damage_vel);
//				AddForce(m_test_damage_vel, ForceMode.VelocityChange);
			}
		}
	}
}
