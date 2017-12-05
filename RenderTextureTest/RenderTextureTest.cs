using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class RenderTextureTest : MonoBehaviour
{
	[SerializeField]
	Screenshot mScreenshot;
	
	[SerializeField]
	RawImage mRawImage;

	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		mScreenshot.SetRenderTexture(500, 500);
	}

	/// <summary>
	/// OnGUI is called for rendering and handling GUI events.
	/// This function can be called multiple times per frame (one call per event).
	/// </summary>
	void OnGUI()
	{
		if(GUILayout.Button("Take"))
		{
			mScreenshot.Take((texture) => { mRawImage.texture = texture; });
		}
		if(GUILayout.Button("Tex2d"))
		{
			mRawImage.texture = mScreenshot.ToTex2D();
		}
		if(GUILayout.Button("ToPNG"))
		{
			byte[] png = mScreenshot.ToPNG();
			File.WriteAllBytes("hoge.png", png);
		}	
	}
}
