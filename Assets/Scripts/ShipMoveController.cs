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

	// 船の移動状態.
	enum MoveState
	{
		Stop,	// 停止中.
		Move, 	// 移動中.
	}

	const float	FOWARD_ANGLE			=-90.0f;	// 前進方向の角度.
	const float	ARIVE_RANGE				= 3.0f;		// 到着範囲.
	const float	INCREMENT_ROT_T			= 0.02f;	// 角度補正値（1フレーム毎） .
	const int	MAX_COURSE_POINT		= 10;		// 最大コースポイント数.

	public float m_mass;
	public float m_speed;
	public float m_curve;
	
	delegate void Move();
	Move ExecMove;

	MoveState	m_state;			// 船の移動状態.
	Vector3		m_target_pos;		// 船の移動状態.
	Quaternion	m_start_rot;		// 開始時の傾き.
	float		m_rot_t;			// 傾きの補完率.

	// --- コース移動時用の変数 --- //.
	Vector3[]	m_course_point;		// コースポイント.
	int 		m_course_idx;		// コースポイントのインデックス.
	Vector3		m_next_pos;			// 次のコースポイント.
	float		m_next_dist;		// 次のコースまでの距離.
	float		m_wait_timer;		// 待ちタイマー.

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
		if(IsMove)
		{
			ShipMoveController clash_ship = collision.gameObject.GetComponent<ShipMoveController>();
			if(clash_ship != null)
			{
				ApplyClashForce(this, clash_ship);
			}
		}
	}

	/// <summary>
	/// 船衝突時の力を適用.
	/// </summary>
	/// <param name="ship1">船1.</param>
	/// <param name="ship2">船2.</param>
	/// <param name="power">力.</param>
	void ApplyClashForce(ShipMoveController ship1, ShipMoveController ship2, float power=1.0f)
	{
		Vector3 pos = ship2.transform.position - ship1.transform.position;

		Vector3 n_pos = pos.normalized;

		Vector3 r_angle = Vector3.zero;					// 進行方向に対して直角方向のベクトル.
		r_angle.x = n_pos.z;
		r_angle.z = n_pos.x;

		n_pos.x = Mathf.Abs (n_pos.x);
		n_pos.z = Mathf.Abs (n_pos.z);

		float dot = Vector3.Dot (r_angle, n_pos);

		float limit_low = 1.0f;
		if(Mathf.Abs(dot) < limit_low)					// 内積が一定値未満の場合、衝突時に船がずれないため、直角方向のベクトルを加算する.
		{
			float sign = Mathf.Sign(dot);
			n_pos.x += sign * r_angle.x * limit_low;
			n_pos.z += sign * r_angle.z * limit_low;
			pos.x   += sign * r_angle.x * limit_low;
			pos.z   += sign * r_angle.z * limit_low;
		}

		Vector3 force1 = Vector3.zero;
		force1.x = pos.x * -n_pos.x * power;
		force1.z = pos.z * -n_pos.z * power;

		Vector3 force2 = Vector3.zero;
		force2.x = pos.x * n_pos.x * power;
		force2.z = pos.z * n_pos.z * power;

		ship1.AddForce (force1, ForceMode.Acceleration);
		ship2.AddForce (force2, ForceMode.Acceleration);
	}

	/// <summary>
	/// 停止中か.
	/// </summary>
	public bool		IsStop	{ get { return this.m_state == MoveState.Stop; } }

	/// <summary>
	/// 移動中か.
	/// </summary>
	public bool		IsMove	{ get { return this.m_state == MoveState.Move; } }

	/// <summary>
	/// 船の位置.
	/// </summary>
	/// <value>My position.</value>
	public Vector3	ShipPos	{ get { return this.transform.position; } }

	/// <summary>
	/// ロード.
	/// </summary>
	public void Load()
	{
		AddRigidbody ();

		AddCupsuleCollider ();
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
		this.ExecMove		= MoveStraight;
	}

	/// <summary>
	/// 実行.
	/// </summary>
	public void Exec() 
	{ 
		if( IsMove ) 
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
		Vector3 dir = this.m_target_pos - this.ShipPos;
		
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

		Vector3 dir = this.m_next_pos - this.ShipPos;

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

				dir = m_next_pos - this.ShipPos;

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
		this.rigidbody.interpolation=RigidbodyInterpolation.Interpolate;
		this.rigidbody.constraints = RigidbodyConstraints.FreezePositionY | 
									 RigidbodyConstraints.FreezeRotationX | 
									 RigidbodyConstraints.FreezeRotationZ ;
	}

	/// <summary>
	/// カプセルコライダー追加.
	/// </summary>
	void AddCupsuleCollider()
	{
		MeshRenderer mesh = this.gameObject.GetComponentInChildren<MeshRenderer> ();
		if(mesh != null)
		{
			mesh.gameObject.AddComponent<CapsuleCollider>();
		}
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
		Vector3 center_pos = Vector3.Lerp (this.ShipPos, this.m_target_pos, 0.5f);
		center_pos.z += 10.0f * this.m_curve;
		return center_pos;
	}

	void CreateCoursePoint()
	{
		float unit = 1.0f / (float)MAX_COURSE_POINT;

		Vector3 center_pos = CalcCourseCenterPos ();
		
		for(int i=0; i<MAX_COURSE_POINT; i++)
		{
			this.m_course_point[i] = BezierCurve (this.ShipPos, center_pos, this.m_target_pos, unit*i);
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
				AddVelocity(m_test_damage_vel);
			}
		}
	}
}
