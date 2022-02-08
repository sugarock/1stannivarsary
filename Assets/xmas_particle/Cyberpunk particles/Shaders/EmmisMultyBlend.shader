// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ppp"
{
	Properties
	{
		_Dephault2("Dephault2", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_BaseColor("BaseColor", Color) = (0.6603774,0.6603774,0.6603774,0)
		[HDR]_Color0("Color 0", Color) = (10.77512,48.07835,55.71524,0)
		_Exponent("Exponent", Range( 0 , 256)) = 142
		_WS_size("WS_size", Range( 0 , 1)) = 0.2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float4 _BaseColor;
		uniform float4 _Color0;
		uniform sampler2D _Dephault2;
		uniform float4 _Dephault2_ST;
		uniform sampler2D _Mask;
		uniform float _WS_size;
		uniform float _Exponent;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = ( i.vertexColor * _BaseColor ).rgb;
			float2 uv_Dephault2 = i.uv_texcoord * _Dephault2_ST.xy + _Dephault2_ST.zw;
			float3 ase_worldPos = i.worldPos;
			float simplePerlin2D38 = snoise( ( ( _Time.x * 0.51 ) + ( ase_worldPos * _WS_size ) ).xy );
			float lerpResult39 = lerp( ( _Time.x * 0.9 ) , _Time.x , (0.0 + (simplePerlin2D38 - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)));
			float2 panner4 = ( lerpResult39 * float2( 0.1,0.3 ) + i.uv_texcoord);
			float2 panner10 = ( lerpResult39 * float2( 0.1,-0.15 ) + ( i.uv_texcoord * 0.77 ));
			float blendOpSrc3 = tex2D( _Dephault2, uv_Dephault2 ).r;
			float blendOpDest3 = ( ( tex2D( _Mask, panner4 ).g + tex2D( _Mask, panner10 ).r ) * 0.5 );
			float temp_output_6_0 = ( 1.0 - ( saturate( ( blendOpDest3 / blendOpSrc3 ) )) );
			float clampResult19 = clamp( pow( temp_output_6_0 , _Exponent ) , 0.0 , 1.0 );
			float3 hsvTorgb27 = RGBToHSV( i.vertexColor.rgb );
			float4 lerpResult30 = lerp( float4( 0,0,0,0 ) , ( _Color0 * clampResult19 * i.vertexColor ) , (0.0 + (hsvTorgb27.z - 0.5) * (1.0 - 0.0) / (1.0 - 0.5)));
			o.Emission = lerpResult30.rgb;
			float clampResult17 = clamp( temp_output_6_0 , 0.0 , 1.0 );
			float clampResult42 = clamp( ( i.vertexColor.a * clampResult17 ) , 0.0 , 1.0 );
			o.Alpha = clampResult42;
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
245;215;1509;550;-1341.699;761.0535;1.629637;True;True
Node;AmplifyShaderEditor.RangedFloatNode;34;-1166.201,1044.172;Float;False;Property;_WS_size;WS_size;5;0;Create;True;0;0;False;0;0.2;0.025;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;35;-1845.352,454.6416;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;32;-1052.688,769.3299;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-492.0367,650.6768;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.51;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-750.3041,833.21;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;37;-256.0196,867.5698;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;38;-104.999,591.3282;Float;False;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-1474.857,-43.81483;Float;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;False;0;0.77;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;40;-1480.08,638.6663;Float;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-1584.989,252.5619;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-1263.84,104.2692;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-987.9769,282.0934;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;39;-1308.574,420.7732;Float;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;10;-749.1921,370.7058;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.1,-0.15;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;-765.2297,183.137;Float;True;Property;_Mask;Mask;1;0;Create;True;0;0;False;0;12777bae487c37241903c8083f51c243;12777bae487c37241903c8083f51c243;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.PannerNode;4;-783.8884,-22.4935;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.1,0.3;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;2;-379.1674,133.7;Float;True;Property;_cl_mask;cl_mask;1;0;Create;True;0;0;False;0;12777bae487c37241903c8083f51c243;12777bae487c37241903c8083f51c243;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;9;-356.9348,351.144;Float;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;12;0.8716269,203.8818;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-565.6999,-101.3;Float;True;Property;_Dephault2;Dephault2;0;0;Create;True;0;0;False;0;54f1eee3b5b254a4384f089affbe2eec;54f1eee3b5b254a4384f089affbe2eec;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;281.7183,164.3405;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;3;-81.86639,-139.9036;Float;False;Divide;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;6;243.4353,-137.7026;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;214.5494,-498.3612;Float;False;Property;_Exponent;Exponent;4;0;Create;True;0;0;False;0;142;142;0;256;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;25;455.4532,58.98167;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;18;813.8242,-662.7089;Float;False;2;0;FLOAT;0;False;1;FLOAT;32;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;17;1256.454,175.7743;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;19;1127.105,-551.5786;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;27;1416.52,-452.8709;Float;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;22;1086.648,-712.9172;Float;False;Property;_Color0;Color 0;3;1;[HDR];Create;True;0;0;False;0;10.77512,48.07835,55.71524,0;51.05598,26.59089,225.4908,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;31;1850.742,-408.7866;Float;False;5;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;20;1372.299,-189.5815;Float;False;Property;_BaseColor;BaseColor;2;0;Create;True;0;0;False;0;0.6603774,0.6603774,0.6603774,0;0.6603774,0.6603774,0.6603774,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;1454.39,-749.7151;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;2262.719,-171.8004;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;1628.934,-248.8395;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;30;1913.812,-695.3798;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;42;2532.963,-306.3848;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2727.092,-689.0729;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;ppp;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;36;0;35;1
WireConnection;33;0;32;0
WireConnection;33;1;34;0
WireConnection;37;0;36;0
WireConnection;37;1;33;0
WireConnection;38;0;37;0
WireConnection;40;0;38;0
WireConnection;41;0;35;1
WireConnection;13;0;5;0
WireConnection;13;1;14;0
WireConnection;39;0;41;0
WireConnection;39;1;35;1
WireConnection;39;2;40;0
WireConnection;10;0;13;0
WireConnection;10;1;39;0
WireConnection;4;0;5;0
WireConnection;4;1;39;0
WireConnection;2;0;7;0
WireConnection;2;1;4;0
WireConnection;9;0;7;0
WireConnection;9;1;10;0
WireConnection;12;0;2;2
WireConnection;12;1;9;1
WireConnection;16;0;12;0
WireConnection;3;0;1;1
WireConnection;3;1;16;0
WireConnection;6;0;3;0
WireConnection;18;0;6;0
WireConnection;18;1;24;0
WireConnection;17;0;6;0
WireConnection;19;0;18;0
WireConnection;27;0;25;0
WireConnection;31;0;27;3
WireConnection;23;0;22;0
WireConnection;23;1;19;0
WireConnection;23;2;25;0
WireConnection;26;0;25;4
WireConnection;26;1;17;0
WireConnection;29;0;25;0
WireConnection;29;1;20;0
WireConnection;30;1;23;0
WireConnection;30;2;31;0
WireConnection;42;0;26;0
WireConnection;0;0;29;0
WireConnection;0;2;30;0
WireConnection;0;9;42;0
ASEEND*/
//CHKSM=034513885F0707FFFD31A997DD8BE6F2F5D43F26