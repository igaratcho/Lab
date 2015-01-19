using UnityEngine;
using System.Collections;

public class NodeChecker : MonoBehaviour 
{
	class Node
	{
		protected string name;
		protected bool is_need;
		protected Node[] children;
		protected Checker[] checkers;

		public Node(string name, bool is_need, Node[] children) : this(name, is_need, children, null)
		{

		}

		public Node(string name, bool is_need, Node[] children, params Checker[] checkers)
		{
			this.children = children;
			this.checkers = checkers;
		}

		public virtual bool Check(Transform t)
		{
			Transform current = t.FindChild (name);

			if(current)
			{
				foreach(var checker in checkers)
				{
					checker.Check(current, this);
				}
				foreach(var child in children)
				{
					child.Check(current);
				}
			} 
			else 
			{
				if(is_need)
				{
					// error.
				}
			}
			return true;
		}
	}

	class RootNode : Node
	{
		public RootNode(Node[] children) : base(null, true, children, null)
		{	
		}
		public RootNode(Node[] children, params Checker[] checkers) : base(null, true, children, checkers)
		{
		}
		public override bool Check(Transform t)
		{
			foreach(var checker in base.checkers)
			{
				checker.Check(t, this);
			}
			foreach(var child in children)
			{
				child.Check(t);
			}
			return true;
		}
	}

	abstract class Checker
	{
		public abstract bool Check (Transform t, Node node);
	}
	

	class AnimationChecker : Checker
	{
		public override bool Check(Transform t, Node node)
		{
			return true;
		}
	}

	Node STARNDARD_HIERARCHY = new RootNode
	(
		null
	);
	
	void Start () 
	{
		GameObject obj = new GameObject("ROOT");
		STARNDARD_HIERARCHY.Check (obj.transform);
	}
}
