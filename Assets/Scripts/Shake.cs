using UnityEngine;
using System.Collections;

public class Shake : MonoBehaviour
{
	[SerializeField] float m_range = 1.0f;
	[SerializeField] float m_speed = 1.0f;
	[SerializeField] float m_time  = 1.0f;

	float t = 0.0f;

	void Update ()
	{
		t += GetFrameTime () * m_speed;

		var y = Mathf.Clamp(Mathf.Sin (t), 0.0f, 1.0f) * m_range;
		this.transform.position = new Vector3 (0.0f, y, 0.0f);
	}

	float GetFrameTime()
	{
		return 1.0f / 30.0f * m_time; 
	}

	void Rough()
	{
		m_speed = 10.0f;
	}

	void OnGUI()
	{
		if (GUILayout.Button ("Rough"))
		{
			Rough ();
		}
	}
}
