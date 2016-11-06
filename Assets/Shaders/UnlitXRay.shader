Shader "Custom/UnlitXRay"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("_Color", Color) = (0,1,0,1)
		_Inside("_Inside", Range(0,1) ) = 0
		_Rim("_Rim", Range(0,2) ) = 1.2
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "Transparent"   }
		LOD 100

		Pass
		{
//			Cull off
//			Zwrite off
			Blend oneminusdstcolor one

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
//			#include "HLSLSupport.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _Color;
			half _Rim;
			half _Inside;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = v.normal;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
//				// sample the texture
//				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				half3 uv = mul( (float3x3)UNITY_MATRIX_IT_MV, i.normal );
				uv = normalize(uv);
				fixed4 col = lerp(half4(0,0,0,0),_Color, saturate(max(1- pow (uv.z,_Rim), _Inside)));

				return col;
			}
			ENDCG
		}
	}
}
