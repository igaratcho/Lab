using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour 
{
	[SerializeField] float m_range = 1.0f;
	[SerializeField] float m_speed = 1.0f;
	[SerializeField] float m_time  = 1.0f;

	void Update ()
	{
		float t = GetFrameTime () * m_speed;
		this.transform.Rotate (Vector3.up * t);
	}

	float GetFrameTime()
	{
		return 1.0f / 30.0f * m_time; 
	}
}
