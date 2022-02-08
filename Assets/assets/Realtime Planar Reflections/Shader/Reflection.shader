Shader "Realtime Planar Reflections/Reflection" {
	Properties {
		[Header(Standard)]
		[NoScaleOffset]_MainTex ("Albedo", 2D) = "white" {}
		_MainTexScaleOffset     ("Albedo Scale Offset", Vector) = (1, 1, 0, 0)
		[NoScaleOffset]_BumpMap ("Bump", 2D) = "bump" {}
		_BumpMapScaleOffset     ("Bump Scale Offset", Vector) = (1, 1, 0, 0)
		_Glossiness             ("Smoothness", Range(0, 1)) = 0
		_Metallic               ("Metallic", Range(0, 1)) = 0
		_Color                  ("Tint", Color) = (1, 1, 1, 1)
		_EmissionColor          ("Emission", Color) = (0, 0, 0, 0)
		[Header(Reflection)]
		[NoScaleOffset]_ReflectionTex  ("Reflection", 2D) = "black" {}
		_ReflectionTint                ("Reflection Tint", Color) = (1, 1, 1, 1)
		_ReflectionStrength            ("Reflection Strength", Range(0,1)) = 1
		[NoScaleOffset]_ReflBumpMap    ("Reflection Bump", 2D) = "bump" {}
		_ReflBumpMapScaleOffset        ("Bump Scale Offset", Vector) = (1, 1, 0, 0)
		_ReflBumpStrength              ("Bump Strength", Float) = 0.5
		[NoScaleOffset]_MaskTex        ("Mask", 2D) = "black" {}
		_MaskTexScaleOffset            ("Mask Scale Offset", Vector) = (1, 1, 0, 0)
		[NoScaleOffset]_HeightAttenTex ("Height Atten", 2D) = "black" {}
		_HeightFade                    ("Height Fade", Float) = 0
		[HideInInspector]_texcoord     ("", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }

		CGPROGRAM
		#pragma surface surf Standard addshadow fullforwardshadows
		#pragma target 3.0
		#pragma multi_compile _ RPR_HEIGHT_ATTEN
		#pragma multi_compile _ RPR_BUMP_REFLECTION

		sampler2D _MainTex, _BumpMap, _ReflectionTex, _MaskTex, _ReflBumpMap, _HeightAttenTex;
		fixed4 _MainTexScaleOffset, _BumpMapScaleOffset, _ReflBumpMapScaleOffset, _MaskTexScaleOffset;
		fixed4 _ReflectionTint, _Color, _EmissionColor;
		fixed _ReflBumpStrength, _ReflectionStrength, _HeightFade, _Metallic, _Glossiness;

		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float3 worldNormal;
			INTERNAL_DATA
		};
		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			float2 uv = IN.uv_texcoord * _MainTexScaleOffset.xy + _MainTexScaleOffset.zw;
			fixed4 tc = tex2D(_MainTex, uv) * _Color;

			float4 scrpos = IN.screenPos;
#ifdef RPR_BUMP_REFLECTION
			uv = IN.uv_texcoord * _ReflBumpMapScaleOffset.xy + _ReflBumpMapScaleOffset.zw;
			float3 offset = UnpackNormal(tex2D(_ReflBumpMap, uv)).xyz * _ReflBumpStrength;
			scrpos.xyz += offset.xyz;
#endif

			uv = IN.uv_texcoord * _MaskTexScaleOffset.xy + _MaskTexScaleOffset.zw;
			half4 mask = tex2D(_MaskTex, uv);

			float2 scruv = scrpos.xy / max(scrpos.w, 0.001);
			half4 refl = tex2D(_ReflectionTex, scruv) * _ReflectionTint;

			// height based reflection blend use different method
#ifdef RPR_HEIGHT_ATTEN
			half h = tex2D(_HeightAttenTex, scruv).g;
			h = saturate(_HeightFade - h);
			fixed3 c = tc.rgb + refl.rgb * (1.0 - _ReflectionStrength) * h;
#else
			fixed3 c = lerp(refl.rgb, tc.rgb, _ReflectionStrength);
			c = lerp(c, refl.rgb, mask.r);
#endif
			o.Albedo = c;
			o.Alpha = tc.a;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Emission = _EmissionColor;

#ifndef RPR_HEIGHT_ATTEN
			// blend surface normal based on mask, reflection part use vertex normal
			float3 n1 = WorldNormalVector(IN, float3(0, 0, 1));
			uv = IN.uv_texcoord * _BumpMapScaleOffset.xy + _BumpMapScaleOffset.zw;
			float3 n2 = UnpackNormal(tex2D(_BumpMap, uv));
			o.Normal = lerp(n2, n1, step(0.5, mask.r));
#endif
		}
		ENDCG
	}
	FallBack "Diffuse"
}