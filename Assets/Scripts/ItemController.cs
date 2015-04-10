using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemController : MonoBehaviour 
{
	[SerializeField] int maxItemCount = 3;
	[SerializeField] float udpateIntervalTime = 5.0f;
	[SerializeField] int addItemCount = 1;
	[SerializeField] UIScrollView scrollView;
	[SerializeField] UIGrid grid;
	[SerializeField] GameObject itemPrefab;
	
	int index;
	List<Item> itemList;
	float prevTime;
	
	void Start () 
	{
		this.index = 1;
		this.itemList = new List<Item> ();
		this.prevTime = Time.realtimeSinceStartup;
	}

	void Update () 
	{
		if (IsUpdate ()) 
		{
			for (int i=0; i<addItemCount; i++) {
				this.AddItem();
			}

			this.RemoveItem();
			this.ResetPostion();
			this.prevTime = Time.realtimeSinceStartup;
		}
	}

	bool IsUpdate()
	{
		return (Time.realtimeSinceStartup - this.prevTime) > this.udpateIntervalTime;
	}

	void AddItem()
	{
		GameObject obj = (GameObject)Instantiate (itemPrefab);
		this.grid.AddChild (obj.transform);
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale    = Vector3.one;
		Item item = obj.GetComponent<Item> ();
		item.Setup (this.index);
		itemList.Add (item);
		this.index++;
	}

	void RemoveItem()
	{
		if(this.itemList.Count > maxItemCount)
		{
			int removeCount = this.itemList.Count - maxItemCount;

			for(int i=0; i<removeCount; i++)
			{
				Destroy(itemList[i].gameObject);
				itemList.RemoveAt(i);
			}
		}
	}

	void ResetPostion()
	{
		this.grid.Reposition ();
		this.grid.repositionNow = true;

//		StopCoroutine  (Move ());
//		StartCoroutine (Move ());

		if (this.scrollView.shouldMoveVertically) {
			this.scrollView.verticalScrollBar.value = 1.0f;
		}


//		this.scrollView.RestrictWithinBounds(true);
	}

	IEnumerator Move()
	{
		yield return null;

		if(this.scrollView.verticalScrollBar.alpha > 0.0f)
		{
			//			this.scrollView.ResetPosition ();
			//			this.scrollView.verticalScrollBar.value = 0.95f;
			
			this.scrollView.MoveRelative(Vector3.up*100f);

			yield return null;

			this.scrollView.RestrictWithinBounds(true);
		}

	}



}
