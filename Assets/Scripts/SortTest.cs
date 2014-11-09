using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SortTest : MonoBehaviour 
{
	const int MAX_LV = 10;

	class TestData
	{
		public int id;
		public int lv;

		public TestData(int id, int lv)
		{
			this.id = id;
			this.lv = lv;
		}
	}

	class TestElements
	{
		public int lv;
		public int index;

		public TestElements(int lv, int index)
		{
			this.lv = lv;
			this.index = index;
		}
	}

	
	// Use this for initialization
	void Start () 
	{
		TestData[] datas = CreateTestData ();
		Dictionary<int, List<TestElements>> dict = new Dictionary<int, List<TestElements>> ();

		for(int i=0; i<datas.Length; i++)
		{
			TestData data = datas[i];
			int id = data.id;

			if(dict.ContainsKey(id))
			{
				dict[id].Add(new TestElements(data.lv, i));
			} 
			else 
			{
				dict[id] = new List<TestElements>();
				dict[id].Add(new TestElements(data.lv, i));
			}
		}

		foreach(int key in dict.Keys)
		{
			List<TestElements> lv_list = dict[key];
			lv_list = lv_list.OrderBy(n => n.lv).ToList();

			int current_index = 0;
			TestElements current_elem = lv_list[current_index];
			int next_lv = GetNextLv(current_index, lv_list);

			for(int lv=1; lv<=MAX_LV; lv++)
			{
				if(lv != 1)
				{
					if(lv == next_lv)
					{
						current_index++;
						current_elem = lv_list[current_index];
						next_lv = GetNextLv(current_index, lv_list);
					}
				}
				Debug.Log(string.Format("ID:{0} LV:{1} Index{2}", key, lv, current_elem.index) );
			}
		}
	}

	int GetNextLv(int index, List<TestElements> lv_list)
	{
		if (index < (lv_list.Count-1)) 
		{
			index++;
		}
		return lv_list[index].lv;
	}

	TestData[] CreateTestData()
	{
		return new TestData[]
		{
			new TestData(1,1),
			new TestData(2,1),
			new TestData(3,1),
			new TestData(3,5),
			new TestData(4,1),
			new TestData(4,3),
			new TestData(5,1),
		};
	}
}
