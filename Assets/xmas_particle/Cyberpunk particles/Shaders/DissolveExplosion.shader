// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DissolveExplosion"
{
	Properties
	{
		_Texture0("Texture 0", 2D) = "white" {}
		_Distort("Distort", Range( 0 , 0.2)) = 0.1675351
		[HDR]_ColorFlame("ColorFlame", Color) = (0.7132784,3.780375,6.062866,0)
		[HDR]_ColorFlame2("ColorFlame2", Color) = (0.003921568,0.07093469,1,0)
		_FlameExponent("FlameExponent", Range( 0 , 16)) = 5.082353
		_AlphaRemap("AlphaRemap", Vector) = (0,0.6,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float4 _ColorFlame2;
		uniform float4 _ColorFlame;
		uniform sampler2D _Texture0;
		uniform float _Distort;
		uniform float _FlameExponent;
		uniform float2 _AlphaRemap;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 panner19 = ( 1.0 * _Time.y * float2( 0.15,-0.15 ) + i.uv_texcoord);
			float2 panner10 = ( 1.0 * _Time.y * float2( 0.3,0.3 ) + i.uv_texcoord);
			float2 panner13 = ( 1.0 * _Time.y * float2( -0.2,0.2 ) + i.uv_texcoord);
			float4 tex2DNode9 = tex2D( _Texture0, panner13 );
			float2 appendResult20 = (float2(( i.uv_texcoord.x * tex2D( _Texture0, panner10 ).r ) , ( i.uv_texcoord.y * tex2DNode9.r )));
			float2 lerpResult16 = lerp( panner19 , appendResult20 , _Distort);
			float4 tex2DNode17 = tex2D( _Texture0, lerpResult16 );
			float4 lerpResult26 = lerp( _ColorFlame2 , _ColorFlame , pow( tex2DNode17.r , _FlameExponent ));
			float4 temp_output_33_0 = ( ( lerpResult26 * tex2DNode17.r ) * i.vertexColor );
			o.Albedo = temp_output_33_0.rgb;
			o.Emission = temp_output_33_0.rgb;
			float clampResult38 = clamp( (0.0 + (tex2DNode17.r - _AlphaRemap.x) * (1.0 - 0.0) / (_AlphaRemap.y - _AlphaRemap.x)) , 0.0 , 1.0 );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV40 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode40 = ( 0.33 + 1.0 * pow( 1.0 - fresnelNdotV40, 1.91 ) );
			float clampResult42 = clamp( (0.0 + (( 1.0 - fresnelNode40 ) - 0.38) * (1.0 - 0.0) / (0.76 - 0.38)) , 0.0 , 1.0 );
			o.Alpha = ( i.vertexColor.a * clampResult38 * clampResult42 );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15401
-48;986;1509;568;-1668.819;1171.427;1.948618;True;True
Node;AmplifyShaderEditor.CommentaryNode;12;-602.0154,-129.1414;Float;False;321;303;Comment;1;10;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-784.7288,-426.8572;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;3;41.40615,-757.2473;Float;True;Property;_Texture0;Texture 0;0;0;Create;True;0;0;False;0;e292e5b965c1a6647bf1d9982029f5ac;e292e5b965c1a6647bf1d9982029f5ac;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.PannerNode;10;-552.0154,-79.14137;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.3,0.3;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;13;-219.1159,-3.241361;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.2,0.2;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;9;40.78455,-42.74135;Float;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-320.3966,-512.2915;Float;True;Property;_Mask;Mask;0;0;Create;True;0;0;False;0;e292e5b965c1a6647bf1d9982029f5ac;e292e5b965c1a6647bf1d9982029f5ac;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;67.2229,-488.1938;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;9.666138,-251.5717;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;336.0822,-546.0734;Float;False;Property;_Distort;Distort;1;0;Create;True;0;0;False;0;0.1675351;0.2;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;19;11.00197,-995.9141;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.15,-0.15;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;20;271.5428,-227.7877;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;16;789.0883,-489.5019;Float;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FresnelNode;40;2332.239,-813.934;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0.33;False;2;FLOAT;1;False;3;FLOAT;1.91;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;1010.132,-974.3248;Float;False;Property;_FlameExponent;FlameExponent;4;0;Create;True;0;0;False;0;5.082353;3.69;0;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;1528.091,-515.8798;Float;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;41;2689.506,-516.4413;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;37;1822.864,-213.4435;Float;False;Property;_AlphaRemap;AlphaRemap;5;0;Create;True;0;0;False;0;0,0.6;0,0.3;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ColorNode;23;1152.591,-1055.173;Float;False;Property;_ColorFlame;ColorFlame;2;1;[HDR];Create;True;0;0;False;0;0.7132784,3.780375,6.062866,0;110.1309,25.37046,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;24;1471.591,-892.2595;Float;False;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;30;1149.697,-1237.186;Float;False;Property;_ColorFlame2;ColorFlame2;3;1;[HDR];Create;True;0;0;False;0;0.003921568,0.07093469,1,0;220.239,8.712502,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;26;1465.603,-1212.967;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;36;2010.864,-473.4435;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;45;3309.177,-675.5825;Float;False;5;0;FLOAT;0;False;1;FLOAT;0.38;False;2;FLOAT;0.76;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;1786.772,-1187.694;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;32;1955.391,-995.2391;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;38;1980.272,-764.0975;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;42;3633.206,-633.839;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;2340.051,-1158.089;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;3064.41,-1036.25;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;15;486.0854,-28.74738;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;1;-513.364,209.6339;Float;False;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3604.091,-1285.148;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;DissolveExplosion;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;11;0
WireConnection;13;0;11;0
WireConnection;9;0;3;0
WireConnection;9;1;13;0
WireConnection;2;0;3;0
WireConnection;2;1;10;0
WireConnection;21;0;11;1
WireConnection;21;1;2;1
WireConnection;22;0;11;2
WireConnection;22;1;9;1
WireConnection;19;0;11;0
WireConnection;20;0;21;0
WireConnection;20;1;22;0
WireConnection;16;0;19;0
WireConnection;16;1;20;0
WireConnection;16;2;18;0
WireConnection;17;0;3;0
WireConnection;17;1;16;0
WireConnection;41;0;40;0
WireConnection;24;0;17;1
WireConnection;24;1;31;0
WireConnection;26;0;30;0
WireConnection;26;1;23;0
WireConnection;26;2;24;0
WireConnection;36;0;17;1
WireConnection;36;1;37;1
WireConnection;36;2;37;2
WireConnection;45;0;41;0
WireConnection;25;0;26;0
WireConnection;25;1;17;1
WireConnection;38;0;36;0
WireConnection;42;0;45;0
WireConnection;33;0;25;0
WireConnection;33;1;32;0
WireConnection;39;0;32;4
WireConnection;39;1;38;0
WireConnection;39;2;42;0
WireConnection;15;0;9;1
WireConnection;0;0;33;0
WireConnection;0;2;33;0
WireConnection;0;9;39;0
ASEEND*/
//CHKSM=E3855D0254AA2B1E3926248E773AF31ED0730F50