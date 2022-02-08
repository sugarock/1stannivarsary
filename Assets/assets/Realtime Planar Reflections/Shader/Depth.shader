Shader "Realtime Planar Reflections/Depth" {
	Properties {
		_PlaneOrigin ("Plane Origin", Vector) = (0, 0, 0, 0)
		_PlaneNormal ("Plane Normal", Vector) = (0, -1, 0, 0)
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : POSITION;
				float4 color : COLOR;
			};
			float4 _PlaneNormal, _PlaneOrigin;

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = _PlaneOrigin * _PlaneNormal - mul(unity_ObjectToWorld, v.vertex) * _PlaneNormal;
				return o;
			}
			float4 frag (v2f i) : COLOR
			{
				return float4(i.color.rgb, 1.0);
			}
			ENDCG
		}
	}
	FallBack Off
}