using UnityEngine;
using System.Collections;

public class Counter : MonoBehaviour 
{
	[SerializeField] UILabel m_label;
	[SerializeField] int from;
	[SerializeField] int to;
	[SerializeField] int endFrame;

	[SerializeField] AnimationCurve curve;

	int frame = 0;

	// Use this for initialization
	void Start () 
	{
		m_label.text = from.ToString ();
		StartCoroutine (StartCounter());
	}

	IEnumerator StartCounter()
	{
		while (frame <= endFrame) 
		{ 
			float t = curve.Evaluate(1.0f*frame/endFrame);
			int count = Mathf.CeilToInt(Mathf.Lerp (from, to, t));
			m_label.text = count.ToString ();
			
			frame++;

			yield return null;
		}
	}
}
