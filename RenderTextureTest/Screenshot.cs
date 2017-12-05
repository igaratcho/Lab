using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Screenshot : MonoBehaviour
{
	Camera mCamera;

	public Camera Camera
	{
		get
		{
			return this.mCamera;
		}
	}

	public RenderTexture RenderTexture
	{
		private set;
		get;
	}

	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		this.mCamera = this.GetComponent<Camera>();
		this.gameObject.SetActive(false);
	}

	public void SetRenderTexture(int width, int height, int depth = 24, RenderTextureFormat format = RenderTextureFormat.Default)
	{
		SetRenderTexture(new RenderTexture(width, height, depth, format));
	}

	public void SetRenderTexture(RenderTexture renderTexture)
	{
		this.RenderTexture = renderTexture;
		this.mCamera.targetTexture = RenderTexture;
	}

	public void Take(System.Action<Texture> doneCb = null)
	{
		this.gameObject.SetActive(true);
		StartCoroutine(TakeCorutine(doneCb));
	}
	
	IEnumerator TakeCorutine(System.Action<Texture> doneCb)
	{
		yield return new WaitForEndOfFrame();

		if (doneCb != null)
		{
			doneCb(RenderTexture);
		}

		this.gameObject.SetActive(false);
	}

	public Texture2D ToTex2D(TextureFormat format = TextureFormat.ARGB32, bool mipmap = false)
	{
		if (RenderTexture == null) return null;

		var tex2d = new Texture2D(RenderTexture.width, RenderTexture.height, format, mipmap);
		RenderTexture.active = RenderTexture;
		tex2d.ReadPixels(new Rect(0, 0, RenderTexture.width, RenderTexture.height), 0, 0);
		tex2d.Apply();
		return tex2d;
	}

	public byte[] ToPNG(TextureFormat format = TextureFormat.ARGB32, bool mipmap = false)
	{
		if (RenderTexture == null) return null;

		return ToTex2D(format, mipmap).EncodeToPNG();
	}
}