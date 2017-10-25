using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
	[SerializeField] float x = 0.3f;
	[SerializeField] float y = 0.3f;
	[SerializeField] float t = 1.0f;

	Transform tranCache;
	Vector3 originPos;
	float timer = 0.0f;

	void Awake()
	{
		tranCache = this.transform;
	}

	void Update()
	{
		if (timer <= 0.0f)
			return;

		float ratio = timer / t;
		Vector2 ran = Random.insideUnitCircle;	
	
		Vector3 pos = originPos;	
		pos.x = pos.x + ran.x * x * ratio;
		pos.y = pos.y + ran.y * y * ratio;
		tranCache.localPosition = pos;

		timer -= Time.deltaTime;

		if (timer <= 0.0f)
		{
			tranCache.localPosition = originPos;
		}
	}

	void Play()
	{
		originPos = tranCache.localPosition;
		timer = t;
	}

	void OnGUI()
	{
		if(GUILayout.Button("Shake"))
		{
			Play();
		}
	}
}
