using UnityEngine;
using System.Collections;

public class PhisicCircleController : MonoBehaviour {

	[SerializeField] CircleController[] m_circles;
	
	bool m_movable = false;

	void Awake()
	{
		Application.targetFrameRate = 30;
		Time.captureFramerate = 30;
	}
	
	void Update () 
	{
		ExecCollision ();
		ExecMove ();
	}
	
	void ExecCollision()
	{
		for (int i=0; i<m_circles.Length; i++) 
		{
			for(int j=0; j<m_circles.Length; j++)
			{
				if(i == j) continue;
				
				if(m_circles[i].Bounds.Intersects(m_circles[j].Bounds))
				{
					m_circles[i].OnCollison(m_circles[j]);
				}
			}
		}
		
		for (int i=0; i<m_circles.Length; i++) 
		{
			m_circles[i].ApplyCollision();
		}
	}
	
	void ExecMove()
	{
		foreach(var move_ctrl in m_circles)
		{
			move_ctrl.Move();
		}
	}
	
	void OnGUI()
	{
		string btn_name = m_movable ? "Stop" : "Move" ;
		if (GUILayout.Button (btn_name)) 
		{
			foreach(var circle in m_circles)
			{
				circle.SendMessage("On"+btn_name, SendMessageOptions.DontRequireReceiver);
			}
			m_movable = !m_movable;
		}

		if (GUILayout.Button ("AddForce")) 
		{
			foreach(var circle in m_circles)
			{
				circle.SendMessage("AddForce", SendMessageOptions.DontRequireReceiver);
			}
		}
		
		if (GUILayout.Button ("AddTorque")) 
		{
			foreach(var circle in m_circles)
			{
				circle.SendMessage("AddTorque", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
