Shader "QQ/Font"
{
	Properties
	{
		_Color("Color",color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("Ztest",float) = 0
	}
		SubShader
		{
			Tags{ "Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent" }
			Cull [_Cull]
			Lighting Off
			ZWrite Off
			Ztest[_Ztest]
			Fog{ Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha
			Pass
			{
				SetTexture[_MainTex]{ combine primary, texture * primary }
			}
		}
			SubShader
		{
			Tags { "RenderType" = "Overlay"
			"Queue" = "Overlay"
			}
			LOD 100

			Pass
			{
				Ztest[_Ztest]
				Zwrite Off
				Blend SrcAlpha OneMinusSrcAlpha
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#include "UnityCG.cginc"

				struct a2v
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 color : TEXCOORD1;
				};
				fixed4 _Color;
				sampler2D _MainTex;
				float4 _MainTex_ST;

				v2f vert(a2v v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.color = v.color;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv);
					col.rgb = col.rgb * i.color.rgb*_Color;
					col.a = min(min(col.a,i.color.a),_Color.a);
					return _Color;
				}
				ENDCG
			}
		}
}
