using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class InputTextValidator 
{
	static public bool Validate(string text)
	{
		return ContainsBBCode (text) || ContainsChatTag(text);
	}

	static public string Escape(string text)
	{
		text = EscapeBBCode (text);
		text = EscapeChatTag(text);
		return text;
	}

	static public bool ContainsBBCode(string text)
	{
		int index = 0;
		return NGUIText.ParseSymbol (text, ref index);
	}

	const string CHAT_TAG_PATTERN = @"\${(?<tag>.*?)}\$";
	static readonly Regex s_chat_regex = new Regex (CHAT_TAG_PATTERN);
	static public bool ContainsChatTag(string text)
	{
		return s_chat_regex.IsMatch (text);
	}

	const string ESCAPE_CD = @"\";
	const string BBCODE_PATTERN = @"\[(?<tag>.*?)\]";
	static readonly Regex s_bbcode_regex = new Regex (BBCODE_PATTERN);
	static public string EscapeBBCode(string text)
	{
		return s_bbcode_regex.Replace(text, ESCAPE_CD + "[" + "$1" + ESCAPE_CD + "]");
	}

	static public string EscapeChatTag(string text)
	{
		return s_chat_regex.Replace(text, "$" + ESCAPE_CD + "{$1}" + ESCAPE_CD +  "$");
	}
}
