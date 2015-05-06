using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class SSvChatMessage
{
	public long uid;
	public int user_id;
	public string user;
	public string cat;
	public string msg;
}

public class ChatMsgProcessor 
{
	public enum ChatTagType
	{
		None,
		User,
		Hero,
		Quest,
	}
	
	public class ChatTagData
	{
		public ChatTagType tag_type;
		public int id;
		public string message;
	}

	public class ChatTagParser
	{
		abstract class AbstractTagParser
		{
			string[] msgs = null;

			public virtual bool Parse (SSvChatMessage chat_msg, string[] texts, out ChatTagData[] tag_data)
			{
				tag_data = null;
				
				if (IsElemCountOk (texts)) {
					
					if(msgs == null) msgs = GetMessages();
					
					tag_data = new ChatTagData[msgs.Length];
					
					int txt_index = 1;
					for(int i=0; i<tag_data.Length; i++)
					{
						tag_data[i] = new ChatTagData();
						
						string msg = msgs[i];
					
						if(ContainsFormtaTag(msg)) {
							GetParseTag(txt_index)(chat_msg, texts[txt_index], ref tag_data[i]);
							txt_index++;
						} else {
							tag_data[i].message = msg;
							
						}
					}
					return true;
				}
				return false;
			}

			protected delegate void ParseTag(SSvChatMessage chat_msg, string text, ref ChatTagData tag_data);

			protected abstract int GetElemCount ();
			protected abstract int GetMsgID ();
			protected abstract ParseTag GetParseTag (int index);

			protected bool IsElemCountOk(string[] texts)
			{
				return texts.Length == GetElemCount ();
			}

			protected string[] GetMessages()
			{
				string text = "{0}が{1}を手に入れました";
				text = text.Replace("{",",{");
				text = text.Replace("}","},");
				return text.Split(',').Where(n => !string.IsNullOrEmpty(n)).ToArray();
			}

			protected bool ContainsFormtaTag(string text)
			{
				return text.Contains("{");
			}

			protected static void ParseUserTag(SSvChatMessage chat_msg, string text, ref ChatTagData tag_data)
			{
				tag_data.tag_type = ChatTagType.User;
				tag_data.id = chat_msg.user_id;
				tag_data.message = "USER_" + chat_msg.user_id;
			}
		}

		class HeroTagParser : AbstractTagParser
		{
			static readonly ParseTag[] parsers = new ParseTag[]
			{
				null,
				ParseUserTag,
				ParseHeroTag
			};

			protected override int GetElemCount ()
			{
				return 3;
			}

			protected override int GetMsgID ()
			{
				return 1001;
			}

			protected override ParseTag GetParseTag (int index)
			{
				return parsers[index];
			}

			static void ParseHeroTag(SSvChatMessage chat_msg, string text, ref ChatTagData tag_data)
			{
				tag_data.tag_type = ChatTagType.Hero;
				tag_data.id = int.Parse (text);
				tag_data.message = text;
			}
		}

		class QuestTagParser : AbstractTagParser
		{
			static readonly ParseTag[] parsers = new ParseTag[]
			{
				null,
				ParseUserTag,
				ParseQuestTag
			};

			protected override int GetElemCount ()
			{
				return 3;
			}

			protected override int GetMsgID ()
			{
				return 1002;
			}

			protected override ParseTag GetParseTag (int index)
			{
				return parsers[index];
			}

			static void ParseQuestTag(SSvChatMessage chat_msg, string text, ref ChatTagData tag_data)
			{
				tag_data.tag_type = ChatTagType.Quest;
				tag_data.id = int.Parse (text);
				tag_data.message = text;
			}
		}

		static Dictionary<string, AbstractTagParser> tag_parsers = new Dictionary<string, AbstractTagParser>()
		{
			{"h", new HeroTagParser () },
			{"q", new QuestTagParser() },
		};

		const string Pattern = @"\${(?<tag>.*?)}\$";

		Regex regex;

		public ChatTagParser()
		{
			regex = new Regex (Pattern);
		}

		public ChatTagData[] Parse(SSvChatMessage chat_msg)
		{
			ChatTagData[] tag_data = null;

			foreach (Match match in regex.Matches(chat_msg.msg)) 
			{
				string tag_text = match.Groups["tag"].Value;

				if(string.IsNullOrEmpty(tag_text)) continue;

				string[] split = tag_text.Split(':');

				if(split.Length < 1) continue;

				string tag = split[0];

				if(!tag_parsers.ContainsKey(tag)) continue;

				if(tag_parsers[tag].Parse(chat_msg, split, out tag_data)) {
					return tag_data;
				}
			}
			return tag_data;
		}
	}
	static ChatTagParser parser = new ChatTagParser();

	static public ChatTagData[] Parse(SSvChatMessage chat_msg)
	{
		return parser.Parse (chat_msg);
	}
}
