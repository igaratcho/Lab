using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SSvChatMessage
{
	public long uid;
	public int user_id;
	public string user;
	public string cat;
	public string msg;
}

public class ChatTagProcessor 
{
	public enum ChatTagType
	{
		Normal,
		Friend,
		Hero,
		Quest,
	}

	public class ChatTagData
	{
		public ChatTagType tag_type;
		public int id;
		public string message;
	}

	const string Pattern = @"\${(?<tag>.*?)}\$";

	static bool is_inied=false;
	static Regex regex;
	static List<ChatTagData> parse_data;
	static Queue<ChatTagData> tag_work;


	static void Initialize()
	{
		regex = new Regex (Pattern);
		parse_data = new List<ChatTagData> ();
		tag_work = new Queue<ChatTagData>();
		is_inied = true;
	}

	static public void Process(SSvChatMessage chat_message)
	{
	}

	static public ChatTagData[] Parse(string message)
	{
		if(!is_inied)
			Initialize ();

		parse_data.Clear ();
		tag_work.Clear ();

		MatchCollection match_collection = regex.Matches(message);
		foreach (Match match in match_collection) 
		{
			string tag = match.Groups["tag"].Value;
			tag_work.Enqueue(_Parse(tag));
			message = message.Replace(match.Value, ",${tag}$,");
		}

		string[] split = message.Split(',');

		foreach (string text in split) {

			if(string.IsNullOrEmpty(text)) continue;

			if(text.StartsWith("${"))
			{
				parse_data.Add(tag_work.Dequeue());
			}
			else {
				ChatTagData data = new ChatTagData ();
				data.tag_type = ChatTagType.Normal;
				data.message = text;
				parse_data.Add(data);
			}
		}
		return parse_data.ToArray();
	}

	static ChatTagData _Parse(string tag)
	{
		ChatTagData data = new ChatTagData ();

		string[] split = tag.Split(':');
		if (split [0] == "u") {
			data.tag_type = ChatTagType.Friend;
			data.id = int.Parse(split[1]);
			data.message = split[2];
		} 
		else if(split[0] == "h" )
		{
			data.tag_type = ChatTagType.Friend;
			data.id = int.Parse(split[1]);
			data.message = "Valter";
		}
		return data;
	}


	static public void Build(ChatTagData tag_datas)
	{
	}
}
