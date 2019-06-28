Shader "QQ/HintPoint"
{
	Properties
	{
		_Color("Color",Color) = (1,0,0,1)
		_MainTex("Texture", 2D) = "white" {}
		_Flash("flash",float) = 1.0
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" }
		//LOD 100

		Pass
		{
			Cull off
			ZTest Always
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
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};

		fixed4 _Color;
		float _Flash;

		sampler2D _MainTex;
		float4 _MainTex_ST;
		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw * _Time.y;
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, i.uv)*_Color;
			col.a = col.a * (cos(_Time.y*_Flash)+1.0f)/2.0f;
			return col;
		}
		ENDCG
	}
	}
}
