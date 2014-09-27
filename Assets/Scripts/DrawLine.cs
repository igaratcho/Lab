using UnityEngine;
using System.Collections;

public class DrawLine : MonoBehaviour 
{
	const string VERSION = "5.0";

	public Vector3	m_start = Vector3.zero;
	public Vector3	m_end = Vector3.zero;
	public bool		m_show=false;
	public float	m_width=5.0f;

	public Material m_lineMaterial;
	Color m_color = Color.magenta;
	
	void Start () 
	{
		// ライン描画用のマテリアルを生成.
//		m_lineMaterial = new Material(
//			"Shader \"myShader\" {" +
//			"  SubShader {" +
//			"    Tags {" +
//			"      \"Queue\"=\"Transparent+3\"" +
//			"      \"RenderType\"=\"Transparent\"" +
//			"       }"+
//			"    LOD 200"+
//			"    Pass {" +
//			"       Lighting Off" +
//			"       ZWrite Off" +
//			"       ZTest Less" + 
//			"       Fog { Mode Off }" +
//			"       Cull Off" + 
//			"       Blend SrcAlpha OneMinusSrcAlpha" +
//			"       BindChannels {" +
//			"         Bind \"vertex\", vertex" + 
//			"         Bind \"color\", color" +
//			"       }" +
//			"    }" +
//			"  }" +
//			"}"
//		);
//		m_lineMaterial = new Material (Shader.Find ("Custom/OldLine"));

		m_color.a = 0.3f;
		m_lineMaterial = CreateMaterial (m_color);

		m_lineMaterial.hideFlags = HideFlags.HideAndDontSave;
		m_lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
	}

	Material CreateMaterial(Color color) {
		return new Material("Shader \"Lines/Background\" { Properties { _Color (\"Main Color\", Color) = ("+color.r+","+color.g+","+color.b+","+color.a+") } SubShader { Pass { Cull off ZWrite on  Blend SrcAlpha OneMinusSrcAlpha Colormask RGBA Lighting Off Offset 1, 1 Color[_Color] }}}");
	}

	void DrawSquare(Vector3 v0, Vector3 v1) 
	{
		Vector3 n = ((new Vector3(v1.z, 0.0f, v0.x)) - (new Vector3(v0.z, 0.0f, v1.x))).normalized * m_width;
		GL.Vertex3(v0.x - n.x, 0.0f, v0.z - n.z);
		GL.Vertex3(v0.x + n.x, 0.0f, v0.z + n.z);
		GL.Vertex3(v1.x + n.x, 0.0f, v1.z + n.z);
		GL.Vertex3(v1.x - n.x, 0.0f, v1.z - n.z);
	}
	
	void OnRenderObject() 
	{
		if(m_show == false) return;

		if (m_lineMaterial != null) 
		{
			m_lineMaterial.SetPass(0);

			GL.PushMatrix();
//			GL.LoadOrtho();
			GL.Begin (GL.QUADS);
			GL.Color (m_color);
			DrawSquare(m_start, m_end);

			GL.End();
			GL.PopMatrix();
		}
	}

	void OnGUI()
	{
		GUI.Label(new Rect(10.0f, 10.0f, 50.0f, 50.0f), VERSION);

		if(GUI.Button(new Rect(10.0f, 50.0f, 200.0f, 200.0f), "SWITCH"))
		{
			m_show = !m_show;
		}
	}


}
