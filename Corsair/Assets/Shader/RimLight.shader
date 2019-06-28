Shader "QQ/RimLight"
{
	Properties
	{
		_Color("Color",Color) = (1,1,1,1)
	}
		SubShader
	{
		//Tags { "RenderType" = "Transparent"
		//		"Queue" = "Transparent"}
		Tags{"RenderType" = "Opaque"}
		LOD 100
		Pass
		{
		//ZWrite Off
		//Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal:NORMAL;
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float rim : TEXCOORD0;
		};
		uniform fixed4 _Color;
		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			float3 n = mul((float3x3)UNITY_MATRIX_V, UnityObjectToWorldNormal(v.normal));
			o.rim = 1 - max(0,dot(n,float3(0,0,1))) + _Color.a;
			o.rim = saturate(o.rim * o.rim * _Color.a);
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			return fixed4(_Color.rgb * i.rim,i.rim);
		}
		ENDCG
	}
	}
}
