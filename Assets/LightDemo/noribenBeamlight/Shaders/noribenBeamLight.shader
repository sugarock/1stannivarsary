Shader "Noriben/noribenBeamLight"
{
	Properties
	{
		[Header(Texture)]
		_NoiseTex ("NoiseTex", 2D) = "white" {}

		[Header(Color)]
		_Color ("Color" , Color) = (1.0, 1.0, 1.0, 1.0)
		_Intensity ("Light Intensity", Range(0, 10)) = 1 

		[Space]
		[Header(Size)]
		_ConeWidth ("Width", Range(-0.07, 0.5)) = 1
		_ConeLength ("Length", Range(0.01, 3)) = 1

		[Space]
		[Header(Noise)]
		_TexPower ("Noise power", Range(0, 1)) = 1
		_NoieMoveSpeed ("Noise move speed", Range(0, 4)) = 1
		_LightIndex ("Noise seed", float) = 0
		[Toggle] _WorldPosNoiseRandom ("WorldPosition Noise Random", float) = 0

		[Space]
		[Header(Soft)]
		_RimPower ("Edge soft", Range(0.01, 10.0)) = 3

		[Space]
		[Header(Gradation Height)]
		[Toggle] _GradOn ("Gradation Height ON", float) = 0
		_GradHeight ("Gradation Height", float) = 1
		_GradPower ("Gradation Power", Range(0, 5)) = 0.3

		[Space]
		[Header(Divide)]
		_Divide ("Divide", Range(0, 30)) = 0
		_DivideScroll("Divide Scroll", Range(-10, 10)) = 0
		_DividePower ("Divide Power", Range(0, 1)) = 0

		[Space(40)]
		[Header(AudioLink for VRChat)]
		[Toggle(AudioLinkOn)] _AudioLinkOn ("AudioLink On", Float) = 0
		
		//_AudioLink ("AudioLink Texture", 2D) = "black" {}
		_AudioLinkIntensity ("AudioLink Intensity", Range(0,1)) = 1
		[Enum(Bass,0,Low mid,1,High mid,2,Treble,3)]
		_AudioLinkType ("AudioLink BandType", int) = 0
		_AudioLinkFiltering ("AudioLink Smooth Filtering", Range(0, 1)) = 0


	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" "DisableBatching" = "True"}
		Cull Front
		Blend One One
		Zwrite Off
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature SliderOn
			// make fog work
			#pragma multi_compile_fog
			#pragma multi_compile_instancing

			#pragma shader_feature_local _ AudioLinkOn
			
			#include "UnityCG.cginc"
			#include "UnityInstancing.cginc"

			#ifdef AudioLinkOn
			#include "../../../AudioLink/Shaders/AudioLink.cginc"
			#endif

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD3;
				float3 normal : NORMAL;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0; //フレネル用
				UNITY_FOG_COORDS(1) //内部でTEXCOORD1を使っている
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float2 uv2 : TEXCOORD4; //テクスチャ用
				float3 objPos: TEXCOORD5;

				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			//float4 _Color;
			//float _Intensity;
			//float _RimPower;
			//float _TexPower;
			//float _GradHeight;
			//float _GradPower;
			//float _ConeWidth;
			//float _ConeLength;
			//float _Divide;
			//float _DivideScroll;
			//float _DividePower;
			//float _LightIndex;		
			//float _GradOn;
			//float _WorldPosNoiseRandom;
			//float _NoieMoveSpeed;
			//float _AudioLinkIntensity;
			//int _AudioLinkType;
			//float _AudioLinkFiltering;


			// GPU instancing parameters
			UNITY_INSTANCING_BUFFER_START(Props)
            	UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
				UNITY_DEFINE_INSTANCED_PROP(float, _Intensity)
				UNITY_DEFINE_INSTANCED_PROP(float, _RimPower)
				UNITY_DEFINE_INSTANCED_PROP(float, _TexPower)
				UNITY_DEFINE_INSTANCED_PROP(float, _GradHeight)
				UNITY_DEFINE_INSTANCED_PROP(float, _GradPower)
				UNITY_DEFINE_INSTANCED_PROP(float, _ConeWidth)
				UNITY_DEFINE_INSTANCED_PROP(float, _ConeLength)
				UNITY_DEFINE_INSTANCED_PROP(float, _Divide)
				UNITY_DEFINE_INSTANCED_PROP(float, _DivideScroll)
				UNITY_DEFINE_INSTANCED_PROP(float, _DividePower)
				UNITY_DEFINE_INSTANCED_PROP(float, _LightIndex)
				UNITY_DEFINE_INSTANCED_PROP(float, _GradOn)
				UNITY_DEFINE_INSTANCED_PROP(float, _WorldPosNoiseRandom)
				UNITY_DEFINE_INSTANCED_PROP(float, _NoieMoveSpeed)
				UNITY_DEFINE_INSTANCED_PROP(float, _AudioLinkIntensity)
				UNITY_DEFINE_INSTANCED_PROP(int, _AudioLinkType)
				UNITY_DEFINE_INSTANCED_PROP(float, _AudioLinkFiltering)
        	UNITY_INSTANCING_BUFFER_END(Props)
			

			//リマップ
			//InMinMax.xからInMinMax.yの範囲で入力された値Inが、OutMinMax.xからOutMinMax.yのスケールにリマップして出力される
			float remap(float In, float2 InMinMax, float2 OutMinMax)
			{
				return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
			}

			//ランダム
			float random1D (float p)
			{ 
            return frac(sin(p) * 10000);
        	}

			float random2D (float2 p)
			{ 
            return frac(sin(dot(p, fixed2(12.9898,78.233))) * 43758.5453);
        	}


			v2f vert (appdata v)
			{

				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				// GPU instansing parameter
				float ConeWidth = UNITY_ACCESS_INSTANCED_PROP(Props, _ConeWidth);
				float ConeLength = UNITY_ACCESS_INSTANCED_PROP(Props, _ConeLength);


				o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;

				//コーンの直径を大きくする
				//0から1にグラデーション状に大きくする
				float vertexHeight = (1 - v.uv.y) * ConeWidth * 100; 
				//normal方向に拡大
				v.vertex.xz = v.vertex.xz + v.normal.xz * 1 * vertexHeight;
				v.vertex.y *= ConeLength;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;//UnityObjectToWorldNormal(v.normal);
				
				o.uv = v.uv;
				o.uv2 = TRANSFORM_TEX(v.uv2, _NoiseTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				
				float3 fresnelviewDir = normalize(ObjSpaceViewDir(v.vertex));
				UNITY_TRANSFER_FOG(o,o.vertex);
				
				return o;
			}

			
			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				// GPU instansing parameter
				float4 Color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				float Intensity = UNITY_ACCESS_INSTANCED_PROP(Props, _Intensity);
				float RimPower = UNITY_ACCESS_INSTANCED_PROP(Props, _RimPower);
				float TexPower = UNITY_ACCESS_INSTANCED_PROP(Props, _TexPower);
				float GradHeight = UNITY_ACCESS_INSTANCED_PROP(Props, _GradHeight);
				float GradPower = UNITY_ACCESS_INSTANCED_PROP(Props, _GradPower);
				float ConeWidth = UNITY_ACCESS_INSTANCED_PROP(Props, _ConeWidth);
				float Divide = UNITY_ACCESS_INSTANCED_PROP(Props, _Divide);
				float DivideScroll = UNITY_ACCESS_INSTANCED_PROP(Props, _DivideScroll);
				float DividePower = UNITY_ACCESS_INSTANCED_PROP(Props, _DividePower);
				float LightIndex = UNITY_ACCESS_INSTANCED_PROP(Props, _LightIndex);
				float GradOn = UNITY_ACCESS_INSTANCED_PROP(Props, _GradOn);
				float WorldPosNoiseRandom = UNITY_ACCESS_INSTANCED_PROP(Props, _WorldPosNoiseRandom);
				float NoieMoveSpeed = UNITY_ACCESS_INSTANCED_PROP(Props, _NoieMoveSpeed);
				float AudioLinkIntensity = UNITY_ACCESS_INSTANCED_PROP(Props, _AudioLinkIntensity);
				int AudioLinkType = UNITY_ACCESS_INSTANCED_PROP(Props, _AudioLinkType);
				float AudioLinkFiltering = UNITY_ACCESS_INSTANCED_PROP(Props, _AudioLinkFiltering);


				
				i.normal = normalize(i.normal);
				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				viewDir = normalize(mul(unity_WorldToObject,float4(viewDir,0.0)));

				//_AudioLink
				#ifdef AudioLinkOn
					float AudioLink = AudioLinkData(ALPASS_AUDIOLINK + int2(0, AudioLinkType));
					float AudioLinkLowFiltered = AudioLinkData(ALPASS_FILTEREDAUDIOLINK + int2(0, AudioLinkType));

					AudioLink = lerp(AudioLink, AudioLinkLowFiltered, AudioLinkFiltering);
					AudioLink = lerp(1, AudioLink, AudioLinkIntensity);
				#endif
				
				//光のグラデーション
				float grad = 0.1 / distance(i.uv.y, float2(1.0, 1.0));
				//マイナス値をなくす
				grad = clamp(grad, 0, 100000);

				//端のだんだん消えていく感じのマスク
				float mask = smoothstep(0.1, 1.0, distance(i.uv.y, 0.0));
				//リムライト的処理エッジをやわらかく消す
				float rim = (pow(max(0, dot(i.normal, -viewDir)), RimPower));

				//ビームの分割
				float pi = 3.14159265;
				float divide = sin(i.uv.x * pi * floor(Divide) * 2 + (_Time.w * DivideScroll)) + 1.0;
				divide = lerp(1.0, divide, DividePower);
				//合成と明るさ調整・ビームが太くなるほど暗くする
				float baseCol = grad * 20 * Intensity  * mask * rim * divide * pow(1.5, - ConeWidth * 20);

				

				//空気中のもやもや用ノイズテクスチャ
				float2 texUV = i.uv2;
				texUV.x = texUV.x + _Time * .5 * NoieMoveSpeed;
				texUV.y = texUV.y + _Time * .1 * NoieMoveSpeed;
				//ノイズテクスチャを位置によって適当にずらす（ほんとに適当）
				float randVal = random1D(LightIndex + 1); // 適当なランダム値				
				float2 worldPosRandom = float2(frac(i.objPos.x + i.objPos.y + i.objPos.z + randVal), frac(i.objPos.x + i.objPos.y + i.objPos.z + randVal));
				texUV += lerp(float2(0, 0), worldPosRandom, WorldPosNoiseRandom);

				// 2つ目の逆回転のノイズ
				float2 texUV2 = i.uv2;
				texUV2.x = texUV2.x - _Time * .7 * NoieMoveSpeed;
				texUV2.y = texUV2.y - _Time * .2 * NoieMoveSpeed;
				//ノイズテクスチャを位置によって適当にずらす（ほんとに適当）			
				texUV2 += lerp(float2(0, 0), worldPosRandom, WorldPosNoiseRandom);

				// noise multiply
				float4 tex = tex2D(_NoiseTex, texUV);
				float4 tex2 = tex2D(_NoiseTex, texUV2);
				tex *= tex2;
				tex = lerp(fixed4(1, 1, 1, 1), tex, TexPower);
				float4 col = float4(baseCol, baseCol, baseCol, 1) * Color * tex;
				#ifdef AudioLinkOn
					col = float4(baseCol, baseCol, baseCol, 1) * Color * tex * AudioLink;
				#endif

				//レンズ部分の色
				float baselensColor = grad * 20 * Intensity  * mask * divide * pow(1.5, - ConeWidth * 20);
				float3 lensColor = float3(baselensColor, baselensColor,baselensColor) * Color * tex;
				#ifdef AudioLinkOn
					lensColor = float3(baselensColor, baselensColor,baselensColor) * Color * tex * AudioLink;
				#endif


				//高さが0になるにつれてグラデーションで消える
				float worldHeight = saturate(i.worldPos.y * GradPower - GradHeight) ;
				worldHeight = lerp(1, worldHeight, GradOn); //トグル
				col *= float4(worldHeight, worldHeight, worldHeight, 1);

				//いい感じの減衰
				float lm = length(_WorldSpaceCameraPos - i.worldPos);
				lm = clamp(lm * 0.3, 0.0, 1.0) - 0.0;
				col *= float4(lm, lm, lm, 1.0);
				
				//正面向いたときだけフラッシュ
				float xzlen = length(mul(unity_WorldToObject,float4(_WorldSpaceCameraPos,1.0)).xz);
				col *= clamp(0.6/xzlen,1.0,3.0)  - 0.0; //-0.0が明るさ調整
				
				//レンズ部分の色
				float lgrad= smoothstep(0.9999,.99999, i.uv.y);
				lensColor *= float3(lgrad,lgrad,lgrad);
				lensColor *= .0003;
				col += float4(float3(lensColor),1);

				//bloom爆発対策
				col = clamp(col,0,3);

				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
