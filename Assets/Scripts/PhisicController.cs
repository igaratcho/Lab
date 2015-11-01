using UnityEngine;
using System.Collections;

public class PhisicController : MonoBehaviour 
{
	[SerializeField] MoveController[] m_move_ctrls;

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
		for (int i=0; i<m_move_ctrls.Length; i++) 
		{
			for(int j=0; j<m_move_ctrls.Length; j++)
			{
				if(i == j) continue;

				if(m_move_ctrls[i].Bounds.Intersects(m_move_ctrls[j].Bounds))
				{
					m_move_ctrls[i].ExecCollision(m_move_ctrls[j]);
				}
			}
		}
	}

	void ExecMove()
	{
		foreach(var move_ctrl in m_move_ctrls)
		{
			move_ctrl.ExexMove();
		}
	}
	
	void OnGUI()
	{
		string btn_name = m_movable ? "Stop" : "Move" ;
		if (GUILayout.Button (btn_name)) 
		{
			foreach(var move_ctrl in m_move_ctrls)
			{
				move_ctrl.SendMessage("On"+btn_name, SendMessageOptions.DontRequireReceiver);
			}
			m_movable = !m_movable;
		}
		
		if (GUILayout.Button ("AddForce")) 
		{
			foreach(var move_ctrl in m_move_ctrls)
			{
				move_ctrl.SendMessage("OnAddForce", SendMessageOptions.DontRequireReceiver);
			}
		}
		
		if (GUILayout.Button ("AddTorque")) 
		{
			foreach(var move_ctrl in m_move_ctrls)
			{
				move_ctrl.SendMessage("OnAddTorque", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
