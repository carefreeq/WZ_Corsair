﻿Shader "QQ/Outline"
{
	Properties
	{
		_Color("color",Color) = (1,1,1,1)
		_Outline("ouline",Range(0,1)) = 0.1
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
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
			float3 normal: NORMAL;
		};

		struct v2f
		{
			UNITY_FOG_COORDS(0)
			float4 vertex : SV_POSITION;
		};

		fixed4 _Color;
		float _Outline;

		v2f vert(appdata v)
		{
			v2f o;
			v.vertex.xyz += v.normal*_Outline;
			o.vertex = UnityObjectToClipPos(v.vertex);
			UNITY_TRANSFER_FOG(o,o.vertex);
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			// sample the texture
			fixed4 col = _Color;
		// apply fog
		UNITY_APPLY_FOG(i.fogCoord, col);
		return col;
	}
	ENDCG
}
	}
}
