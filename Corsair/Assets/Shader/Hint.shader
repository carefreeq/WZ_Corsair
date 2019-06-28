Shader "QQ/Hint"
{
	Properties
	{
		_Color("Color",Color) = (1,0,0,1)
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" }
		//LOD 100

		Pass
		{
			ZTest Always
			Cull off
			Zwrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		// make fog work
		#pragma multi_compile_fog

		#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float4 color:color;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 color:TEXCOORD1;
			float4 vertex : SV_POSITION;
		};

		fixed4 _Color;
		sampler2D _MainTex;
		float4 _MainTex_ST;

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw * _Time.y;
			o.color = v.color;
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			return tex2D(_MainTex, i.uv)*i.color;
		}
		ENDCG
	}
	}
}
