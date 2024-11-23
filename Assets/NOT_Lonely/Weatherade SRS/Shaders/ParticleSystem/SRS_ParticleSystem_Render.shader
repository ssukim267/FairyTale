// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/NOT_Lonely/Weatherade/SRS_ParticleSystem_Render"
{
	Properties
	{
		_NearBlurFalloff("_NearBlurFalloff", Float) = 0.8
		_MainTex("Texture", 2D) = "white" {}
		_OpacityFadeStartDistance("_OpacityFadeStartDistance", Float) = 1
		_Normal("Normal", 2D) = "bump" {}
		_sizeMinMax("_sizeMinMax", Vector) = (0.004,0.008,0,0)
		_OpacityFadeFalloff("_OpacityFadeFalloff", Int) = 8
		_NearBlurDistance("_NearBlurDistance", Int) = 8
		_lightsCount("_lightsCount", Int) = 5
		[HideInInspector]_startRotationMinMax("_startRotationMinMax", Vector) = (0,0,0,0)
		[HideInInspector]_rotationSpeedMinMax("_rotationSpeedMinMax", Vector) = (-5,5,0,0)
		_SRS_particles("_SRS_particles", 2D) = "white" {}
		_gradientTexOLT("gradientTexOLT", 2D) = "white" {}
		[HDR]_color("color", Color) = (1,1,1,1)
		[HideInInspector]_gradientsRatio("_gradientsRatio", Float) = 0.5
		_sunMaskSize("Sun Mask Size", Float) = 0
		_sunMaskSharpness("Sun Mask Sharpness", Range( 0 , 1)) = 0
		_sparklesStartDistance("Sparkles Start Distance", Float) = 1
		_lightDirection("lightDirection", Vector) = (0,0,0,0)
		[HDR]_lightColor("lightColor", Color) = (1,1,1,1)
		_stretchingMultiplier("_stretchingMultiplier", Float) = 1
		_pointLightsIntensity("_pointLightsIntensity", Float) = 1
		_spotLightsIntensity("_spotLightsIntensity", Float) = 1
		_ColorMultiplier("ColorMultiplier", Color) = (1,1,1,1)
		_SimTexSize("SimTexSize", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
		Cull Front
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		
		
		GrabPass{ }

		Pass
		{
			Name "Unlit"

			CGPROGRAM

			#define ASE_ABSOLUTE_VERTEX_POS 1
			#pragma multi_compile_fwdbase
			#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
			#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
			#else
			#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
			#endif
			#define ASE_USING_SAMPLING_MACROS 1


			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#include "Lighting.cginc"
			#include "UnityStandardUtils.cginc"
			#define ASE_NEEDS_VERT_COLOR
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _REFRACTION
			#pragma shader_feature_local _COLOR_GRADIENT
			#pragma shader_feature_local _RAND_GRADIENT_OLT
			#pragma shader_feature_local _RAND_GRADIENT
			#pragma shader_feature_local _SPARKLES
			#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
			#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
			#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex.SampleLevel(samplerTex,coord, lod)
			#define SAMPLE_TEXTURE2D_BIAS(tex,samplerTex,coord,bias) tex.SampleBias(samplerTex,coord,bias)
			#define SAMPLE_TEXTURE2D_GRAD(tex,samplerTex,coord,ddx,ddy) tex.SampleGrad(samplerTex,coord,ddx,ddy)
			#else//ASE Sampling Macros
			#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
			#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex2Dlod(tex,float4(coord,0,lod))
			#define SAMPLE_TEXTURE2D_BIAS(tex,samplerTex,coord,bias) tex2Dbias(tex,float4(coord,0,bias))
			#define SAMPLE_TEXTURE2D_GRAD(tex,samplerTex,coord,ddx,ddy) tex2Dgrad(tex,coord,ddx,ddy)
			#endif//ASE Sampling Macros
			


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				uint ase_vertexID : SV_VertexID;
				float4 ase_texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float3 ase_normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_lmap : TEXCOORD1;
				float4 ase_sh : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_color : COLOR;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float2 _SimTexSize;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_SRS_particles);
			uniform float _stretchingMultiplier;
			uniform float2 _sizeMinMax;
			uniform float2 _rotationSpeedMinMax;
			uniform float2 _startRotationMinMax;
			uniform float4 _color;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_gradientTexOLT);
			uniform float _gradientsRatio;
			uniform float4 _ColorMultiplier;
			ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
			UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal);
			uniform float4 _Normal_ST;
			SamplerState sampler_Normal;
			uniform float4 _srs_lightsPositions[(int)16.0];
			uniform float4 _srs_lightsColors[(int)16.0];
			uniform int _lightsCount;
			uniform float _pointLightsIntensity;
			uniform float _spotLightsIntensity;
			uniform float4 _srs_spotsPosRange[(int)16.0];
			uniform float4 _srs_spotsDirAngle[(int)16.0];
			uniform float4 _srs_spotsColors[(int)16.0];
			UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
			uniform float4 _CameraDepthTexture_TexelSize;
			uniform float _sunMaskSize;
			uniform float _sunMaskSharpness;
			uniform float3 _lightDirection;
			uniform float4 _lightColor;
			uniform float _sparklesStartDistance;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_MainTex);
			uniform float4 _MainTex_ST;
			SamplerState sampler_MainTex;
			uniform int _NearBlurDistance;
			uniform float _NearBlurFalloff;
			uniform float _OpacityFadeStartDistance;
			uniform int _OpacityFadeFalloff;
			float4 ReadPixels1_g13( int u, int v, Texture2D tex )
			{
				return tex.Load(int3(u, v, 0));
			}
			
			float4 ReadPixels1_g20( int u, int v, Texture2D tex )
			{
				return tex.Load(int3(u, v, 0));
			}
			
			float4 ReadPixels1_g22( int u, int v, Texture2D tex )
			{
				return tex.Load(int3(u, v, 0));
			}
			
			float4 ReadPixels1_g21( int u, int v, Texture2D tex )
			{
				return tex.Load(int3(u, v, 0));
			}
			
			float4 ReadPixels1_g19( int u, int v, Texture2D tex )
			{
				return tex.Load(int3(u, v, 0));
			}
			
			inline float4 ASE_ComputeGrabScreenPos( float4 pos )
			{
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				float4 o = pos;
				o.y = pos.w * 0.5f;
				o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
				return o;
			}
			
			float4 PointsMask7_g4( float positionsArray, float colorsArray, float4 screenPosN, float3 worldPos, int lightsCount )
			{
				float4 mask = 0;
				for(int i = 0; i < lightsCount; i++)
				{
					if(_srs_lightsPositions[i].w > 0)
					{
						float4 worldToClip = mul(UNITY_MATRIX_VP, float4(_srs_lightsPositions[i].xyz, 1.0));
						float3 worldToClipN = worldToClip.xyz / worldToClip.w;
						float linearDepth = Linear01Depth(worldToClipN.z);
						float linearScreenDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenPosN.xy));
						float dist = distance(worldPos, _srs_lightsPositions[i].xyz);
						//float circle = 1/dist * _srs_lightsPositions[i].w * 0.5;
						float attn = 1.0 / (10.0 + _srs_lightsPositions[i].w * 0.5 * dist + 1 * dist * dist) * _srs_lightsPositions[i].w * 10;
						mask += attn.xxxx * _srs_lightsColors[i] * step(linearDepth, linearScreenDepth);
						//mask = max(mask, 1-(distance(worldPos, (_srs_lightsPositions[i]).xyz)  / _srs_lightsPositions[i].w)) * step(linearDepth, sceneDepth);
						//mask = max(mask, 1-(distance(worldPos, (_srs_lightsPositions[i]).xyz)  / _srs_lightsPositions[i].w)) * step(linearDepth, sceneDepth) * step(linearDepth, linearScreenDepth);
					}	
				}
				return mask;
			}
			
			float3 SpotsMask18_g4( int spotsCount, float3 worldPos, float4 screenPosN, float spotsPosArray, float spotsDirArray, float spotsColorArray )
			{
				float3 mask = 0;
				for(int i = 0; i < spotsCount; i++)
				{
					if(_srs_spotsPosRange[i].w > 0)
					{
						float4 worldToClip = mul(UNITY_MATRIX_VP, float4(_srs_spotsPosRange[i].xyz, 1.0));
						float3 worldToClipN = worldToClip.xyz / worldToClip.w;
						float linearDepth = Linear01Depth(worldToClipN.z);
						float linearScreenDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenPosN.xy));
						float dist = distance(worldPos, _srs_spotsPosRange[i].xyz);
						float attn = 1 / (10 + _srs_spotsPosRange[i].w * 0.5 * dist + 1 * dist * dist) * _srs_spotsPosRange[i].w * 50;
						float3 dir = normalize(worldPos - _srs_spotsPosRange[i].xyz);
						float angle = acos(clamp(dot(dir, _srs_spotsDirAngle[i].xyz), 0.00001, 1.0));
						float spotAngleRad = radians(_srs_spotsDirAngle[i].w * 0.5);
						float3 cone = saturate(1 - angle/spotAngleRad) * attn * _srs_spotsColors[i].xyz *  _srs_spotsColors[i].w;
						//cone = dist < 0.2 ? 0 : cone;
						//mask += (cone + pow(attn, 3) * _srs_spotsColors[i].xyz * _srs_spotsColors[i].w * haloIntensity) * step(linearDepth, linearScreenDepth);
						mask += cone * step(linearDepth, linearScreenDepth);
					}
				}
				return mask;
			}
			
			float4 ReadPixels1_g18( int u, int v, Texture2D tex )
			{
				return tex.Load(int3(u, v, 0));
			}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float temp_output_1165_0 = ( v.ase_vertexID * 0.25 );
				int u1_g13 = (int)( temp_output_1165_0 % _SimTexSize.x );
				int v1_g13 = (int)( temp_output_1165_0 / _SimTexSize.y );
				Texture2D tex1_g13 =(Texture2D)_SRS_particles;
				float4 localReadPixels1_g13 = ReadPixels1_g13( u1_g13 , v1_g13 , tex1_g13 );
				float4 temp_output_1206_0 = localReadPixels1_g13;
				float3 temp_output_1_0_g6 = (temp_output_1206_0).xyz;
				float3 temp_output_6_0_g6 = frac( temp_output_1_0_g6 );
				float3 particlePos952 = ( ( temp_output_1_0_g6 - temp_output_6_0_g6 ) / 1000.0 );
				float3 temp_cast_2 = (1.0).xxx;
				float3 direction975 = ( ( temp_output_6_0_g6 * 2.0 ) - temp_cast_2 );
				float3 temp_output_977_0 = ( direction975 * _stretchingMultiplier );
				float2 temp_cast_3 = (1.0).xx;
				float mulTime588 = _Time.y * (_rotationSpeedMinMax.x + (v.color.a - 0.0) * (_rotationSpeedMinMax.y - _rotationSpeedMinMax.x) / (1.0 - 0.0));
				float cos587 = cos( ( mulTime588 + ( (_startRotationMinMax.x + (v.color.a - 0.0) * (_startRotationMinMax.y - _startRotationMinMax.x) / (1.0 - 0.0)) * UNITY_PI * 0.005555556 ) ) );
				float sin587 = sin( ( mulTime588 + ( (_startRotationMinMax.x + (v.color.a - 0.0) * (_startRotationMinMax.y - _startRotationMinMax.x) / (1.0 - 0.0)) * UNITY_PI * 0.005555556 ) ) );
				float2 rotator587 = mul( ( ( ( v.ase_texcoord.xy * 2.0 ) - temp_cast_3 ) * (_sizeMinMax.x + (v.color.a - 0.0) * (_sizeMinMax.y - _sizeMinMax.x) / (1.0 - 0.0)) ) - float2( 0,0 ) , float2x2( cos587 , -sin587 , sin587 , cos587 )) + float2( 0,0 );
				float2 break252 = rotator587;
				float flakeOffset_x231 = break252.x;
				float flakeOffset_y253 = break252.y;
				float3 normalizeResult956 = normalize( ( particlePos952 - _WorldSpaceCameraPos ) );
				float3 normalizeResult961 = normalize( cross( temp_output_977_0 , normalizeResult956 ) );
				float3 billboard239 = ( ( temp_output_977_0 * flakeOffset_x231 ) + ( flakeOffset_y253 * normalizeResult961 ) );
				float3 temp_output_261_0 = ( particlePos952 + billboard239 );
				float3 worldToObj558 = mul( unity_WorldToObject, float4( temp_output_261_0, 1 ) ).xyz;
				float3 vPos633 = worldToObj558;
				
				#ifdef DYNAMICLIGHTMAP_ON //dynlm
				o.ase_lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif //dynlm
				#ifdef LIGHTMAP_ON //stalm
				o.ase_lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif //stalm
				float3 ase_worldPos = mul(unity_ObjectToWorld, float4( (v.vertex).xyz, 1 )).xyz;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				#ifndef LIGHTMAP_ON //nstalm
				#if UNITY_SHOULD_SAMPLE_SH //sh
				o.ase_sh.xyz = 0;
				#ifdef VERTEXLIGHT_ON //vl
				o.ase_sh.xyz += Shade4PointLights (
				unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
				unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
				unity_4LightAtten0, ase_worldPos, ase_worldNormal);
				#endif //vl
				o.ase_sh.xyz = ShadeSHPerVertex (ase_worldNormal, o.ase_sh.xyz);
				#endif //sh
				#endif //nstalm
				o.ase_texcoord3.xyz = ase_worldNormal;
				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord4 = screenPos;
				float3 objectToViewPos = UnityObjectToViewPos(v.vertex.xyz);
				float eyeDepth = -objectToViewPos.z;
				o.ase_texcoord3.w = eyeDepth;
				
				o.ase_sh.w = v.ase_vertexID;
				o.ase_color = v.color;
				o.ase_texcoord5.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord5.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vPos633;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float3 ase_worldNormal = i.ase_texcoord3.xyz;
				UnityGIInput data1072;
				UNITY_INITIALIZE_OUTPUT( UnityGIInput, data1072 );
				#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON) //dylm1072
				data1072.lightmapUV = i.ase_lmap;
				#endif //dylm1072
				#if UNITY_SHOULD_SAMPLE_SH //fsh1072
				data1072.ambient = i.ase_sh;
				#endif //fsh1072
				UnityGI gi1072 = UnityGI_Base(data1072, 1, ase_worldNormal);
				float3 lighting1152 = gi1072.indirect.diffuse;
				float temp_output_1165_0 = ( i.ase_sh.w * 0.25 );
				int u1_g13 = (int)( temp_output_1165_0 % _SimTexSize.x );
				int v1_g13 = (int)( temp_output_1165_0 / _SimTexSize.y );
				Texture2D tex1_g13 =(Texture2D)_SRS_particles;
				float4 localReadPixels1_g13 = ReadPixels1_g13( u1_g13 , v1_g13 , tex1_g13 );
				float4 temp_output_1206_0 = localReadPixels1_g13;
				float temp_output_2_0_g5 = (temp_output_1206_0).w;
				float temp_output_3_0_g5 = frac( temp_output_2_0_g5 );
				float lifetime662 = ( ( temp_output_2_0_g5 - temp_output_3_0_g5 ) / 1000.0 );
				float maxLifetime669 = ( 1000.0 * temp_output_3_0_g5 );
				float lerpResult1232 = lerp( (float)256 , 0.0 , (0.0 + (lifetime662 - 0.0) * (1.0 - 0.0) / (maxLifetime669 - 0.0)));
				float lifetimeUVx683 = lerpResult1232;
				int u1_g20 = (int)lifetimeUVx683;
				int v1_g20 = (int)0.0;
				Texture2D tex1_g20 =(Texture2D)_gradientTexOLT;
				float4 localReadPixels1_g20 = ReadPixels1_g20( u1_g20 , v1_g20 , tex1_g20 );
				float4 colorOLT_A673 = localReadPixels1_g20;
				int u1_g22 = (int)( i.ase_color.a * 256.0 );
				int v1_g22 = (int)0.0;
				Texture2D tex1_g22 =(Texture2D)_gradientTexOLT;
				float4 localReadPixels1_g22 = ReadPixels1_g22( u1_g22 , v1_g22 , tex1_g22 );
				float4 gradient_A750 = localReadPixels1_g22;
				int u1_g21 = (int)( i.ase_color.a * 256.0 );
				int v1_g21 = (int)1.0;
				Texture2D tex1_g21 =(Texture2D)_gradientTexOLT;
				float4 localReadPixels1_g21 = ReadPixels1_g21( u1_g21 , v1_g21 , tex1_g21 );
				float4 gradient_B762 = localReadPixels1_g21;
				float blend767 = step( i.ase_color.a , _gradientsRatio );
				float4 lerpResult765 = lerp( gradient_A750 , gradient_B762 , blend767);
				#ifdef _RAND_GRADIENT
				float4 staticSwitch743 = lerpResult765;
				#else
				float4 staticSwitch743 = colorOLT_A673;
				#endif
				int u1_g19 = (int)lifetimeUVx683;
				int v1_g19 = (int)1.0;
				Texture2D tex1_g19 =(Texture2D)_gradientTexOLT;
				float4 localReadPixels1_g19 = ReadPixels1_g19( u1_g19 , v1_g19 , tex1_g19 );
				float4 colorOLT_B727 = localReadPixels1_g19;
				float4 lerpResult735 = lerp( colorOLT_A673 , colorOLT_B727 , blend767);
				#ifdef _RAND_GRADIENT_OLT
				float4 staticSwitch736 = lerpResult735;
				#else
				float4 staticSwitch736 = staticSwitch743;
				#endif
				#ifdef _COLOR_GRADIENT
				float4 staticSwitch720 = staticSwitch736;
				#else
				float4 staticSwitch720 = _color;
				#endif
				float4 color740 = ( staticSwitch720 * _ColorMultiplier );
				float4 screenPos = i.ase_texcoord4;
				float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( screenPos );
				float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
				float2 uv_Normal = i.ase_texcoord5.xy * _Normal_ST.xy + _Normal_ST.zw;
				float4 screenColor990 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( ase_grabScreenPosNorm + float4( UnpackScaleNormal( SAMPLE_TEXTURE2D( _Normal, sampler_Normal, uv_Normal ), 0.5 ) , 0.0 ) ).xy);
				#ifdef _REFRACTION
				float4 staticSwitch1156 = ( color740 * saturate( screenColor990 ) );
				#else
				float4 staticSwitch1156 = color740;
				#endif
				float4 temp_cast_14 = (0.0).xxxx;
				float positionsArray7_g4 = _srs_lightsPositions[0].x;
				float colorsArray7_g4 = _srs_lightsColors[0].r;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float4 screenPosN7_g4 = ase_screenPosNorm;
				float3 worldPos7_g4 = WorldPosition;
				int temp_output_9_0_g4 = _lightsCount;
				int lightsCount7_g4 = temp_output_9_0_g4;
				float4 localPointsMask7_g4 = PointsMask7_g4( positionsArray7_g4 , colorsArray7_g4 , screenPosN7_g4 , worldPos7_g4 , lightsCount7_g4 );
				int spotsCount18_g4 = temp_output_9_0_g4;
				float3 worldPos18_g4 = WorldPosition;
				float4 screenPosN18_g4 = ase_screenPosNorm;
				float spotsPosArray18_g4 = _srs_spotsPosRange[0].x;
				float spotsDirArray18_g4 = _srs_spotsDirAngle[0].x;
				float spotsColorArray18_g4 = _srs_spotsColors[0].r;
				float3 localSpotsMask18_g4 = SpotsMask18_g4( spotsCount18_g4 , worldPos18_g4 , screenPosN18_g4 , spotsPosArray18_g4 , spotsDirArray18_g4 , spotsColorArray18_g4 );
				float clampDepth1138 = Linear01Depth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
				float temp_output_909_0 = ( 1.0 - _sunMaskSize );
				float temp_output_914_0 = ( temp_output_909_0 * ( 1.0 - ( 0.99 + (-0.5 + (_sunMaskSharpness - 0.0) * (0.01 - -0.5) / (1.0 - 0.0)) ) ) );
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(WorldPosition);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float dotResult910 = dot( _lightDirection , ase_worldViewDir );
				float temp_output_912_0 = saturate( dotResult910 );
				float dotResult913 = dot( temp_output_912_0 , temp_output_912_0 );
				float smoothstepResult915 = smoothstep( ( temp_output_909_0 - temp_output_914_0 ) , ( temp_output_909_0 + temp_output_914_0 ) , dotResult913);
				float dotResult924 = dot( WorldPosition.y , 1.0 );
				float eyeDepth = i.ase_texcoord3.w;
				float cameraDepthFade892 = (( eyeDepth -_ProjectionParams.y - 0.0 ) / _sparklesStartDistance);
				int u1_g18 = (int)lifetimeUVx683;
				int v1_g18 = (int)2.0;
				Texture2D tex1_g18 =(Texture2D)_gradientTexOLT;
				float4 localReadPixels1_g18 = ReadPixels1_g18( u1_g18 , v1_g18 , tex1_g18 );
				float4 sparkles928 = ( ( ( ( localPointsMask7_g4 * _pointLightsIntensity ) + float4( ( _spotLightsIntensity * localSpotsMask18_g4 ) , 0.0 ) ) + ( saturate( ( saturate( sign( ( clampDepth1138 - 0.99 ) ) ) * ( smoothstepResult915 * saturate( dotResult924 ) ) ) ) * _lightColor * saturate( cameraDepthFade892 ) ) ) * (localReadPixels1_g18).w );
				#ifdef _SPARKLES
				float4 staticSwitch1144 = sparkles928;
				#else
				float4 staticSwitch1144 = temp_cast_14;
				#endif
				float temp_output_677_0 = (color740).a;
				float2 uv_MainTex = i.ase_texcoord5.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode561 = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv_MainTex );
				float3 temp_output_1_0_g6 = (temp_output_1206_0).xyz;
				float3 temp_output_6_0_g6 = frac( temp_output_1_0_g6 );
				float3 particlePos952 = ( ( temp_output_1_0_g6 - temp_output_6_0_g6 ) / 1000.0 );
				float3 temp_cast_30 = (1.0).xxx;
				float3 direction975 = ( ( temp_output_6_0_g6 * 2.0 ) - temp_cast_30 );
				float3 temp_output_977_0 = ( direction975 * _stretchingMultiplier );
				float2 temp_cast_31 = (1.0).xx;
				float mulTime588 = _Time.y * (_rotationSpeedMinMax.x + (i.ase_color.a - 0.0) * (_rotationSpeedMinMax.y - _rotationSpeedMinMax.x) / (1.0 - 0.0));
				float cos587 = cos( ( mulTime588 + ( (_startRotationMinMax.x + (i.ase_color.a - 0.0) * (_startRotationMinMax.y - _startRotationMinMax.x) / (1.0 - 0.0)) * UNITY_PI * 0.005555556 ) ) );
				float sin587 = sin( ( mulTime588 + ( (_startRotationMinMax.x + (i.ase_color.a - 0.0) * (_startRotationMinMax.y - _startRotationMinMax.x) / (1.0 - 0.0)) * UNITY_PI * 0.005555556 ) ) );
				float2 rotator587 = mul( ( ( ( i.ase_texcoord5.xy * 2.0 ) - temp_cast_31 ) * (_sizeMinMax.x + (i.ase_color.a - 0.0) * (_sizeMinMax.y - _sizeMinMax.x) / (1.0 - 0.0)) ) - float2( 0,0 ) , float2x2( cos587 , -sin587 , sin587 , cos587 )) + float2( 0,0 );
				float2 break252 = rotator587;
				float flakeOffset_x231 = break252.x;
				float flakeOffset_y253 = break252.y;
				float3 normalizeResult956 = normalize( ( particlePos952 - _WorldSpaceCameraPos ) );
				float3 normalizeResult961 = normalize( cross( temp_output_977_0 , normalizeResult956 ) );
				float3 billboard239 = ( ( temp_output_977_0 * flakeOffset_x231 ) + ( flakeOffset_y253 * normalizeResult961 ) );
				float3 temp_output_261_0 = ( particlePos952 + billboard239 );
				float3 vPosWS1250 = temp_output_261_0;
				float pDist1253 = distance( _WorldSpaceCameraPos , vPosWS1250 );
				float temp_output_1_0_g30 = ( _NearBlurDistance + _NearBlurFalloff + 0.001 );
				float texDistBlend635 = saturate( ( ( pDist1253 - temp_output_1_0_g30 ) / ( (float)_NearBlurDistance - temp_output_1_0_g30 ) ) );
				float lerpResult607 = lerp( ( temp_output_677_0 * tex2DNode561.r ) , ( temp_output_677_0 * tex2DNode561.g ) , texDistBlend635);
				float temp_output_1_0_g29 = ( 0.001 + _OpacityFadeStartDistance + _OpacityFadeFalloff );
				float distFade638 = ( 1.0 - saturate( ( ( pDist1253 - temp_output_1_0_g29 ) / ( _OpacityFadeStartDistance - temp_output_1_0_g29 ) ) ) );
				float opacity1149 = saturate( ( lerpResult607 * distFade638 ) );
				float4 appendResult1183 = (float4(( lighting1152 + (( staticSwitch1156 + staticSwitch1144 )).rgb ) , opacity1149));
				
				
				finalColor = appendResult1183;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.CommentaryNode;1151;8352.28,-1193.832;Inherit;False;604.2294;184.058;;2;1072;1152;Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1148;4549.294,-1768.024;Inherit;False;1735.416;503.3545;;11;1034;1149;644;741;636;561;677;607;642;614;566;Opacity;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;966;3097.668,888.2849;Inherit;False;1664.285;696.488;;15;976;239;948;947;974;954;953;955;977;961;957;963;962;956;949;Stretched Billboard;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;897;3177.825,-4226.904;Inherit;False;2948.069;1343.103;;43;1146;1138;1137;1136;1135;1134;924;1024;1025;1026;1028;1015;1045;893;895;892;906;928;933;1039;919;902;886;921;918;926;922;852;851;925;917;916;915;914;913;912;910;909;908;1225;1226;1227;1231;Sparkles;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;739;3196.739,-2757.343;Inherit;False;2788.118;862.2819;;4;740;1162;1163;768;Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;766;3216.497,-2687.677;Inherit;False;540;353.3416;;4;738;733;767;734;Blend;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;703;7165.692,1837.129;Inherit;False;1069.661;408.0276;;0;Size and rotation over lifetime;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;684;6141.033,1378.722;Inherit;False;1071.926;359.5641;;7;670;663;665;683;1201;1232;1233;Calculate UV.x from lifetime;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;674;6135.311,393.8713;Inherit;False;883.272;350.9368;;5;673;1211;685;1209;1210;Color over lifetime A;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;637;2560.104,-1874.365;Inherit;False;1939.808;665.4979;;21;1253;1242;1239;1238;1256;1254;635;1236;1245;1244;638;1249;1248;1235;1246;1247;1255;1251;1237;1252;1243;Particles distance blend;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;632;3098.332,-1035.785;Inherit;False;2888.927;547.5844;;21;262;633;261;558;952;975;669;662;1179;529;1160;813;1198;1178;543;542;1166;1165;532;1206;1250;Vertex Position;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;270;3101.714,-63.81917;Inherit;False;2351.489;877.0695;;22;593;774;233;587;588;590;777;778;776;775;581;238;236;659;583;773;231;253;252;234;237;235;Calculate Particle Size And Rotation;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;721;6134.96,850.7137;Inherit;False;885.272;345.9368;;5;727;1203;1208;726;1207;Color over lifetime B;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;699;7978.755,2152.557;Inherit;False;rotMaxOLT;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;694;7973.94,1887.129;Inherit;False;sizeMinOLT;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;744;6145.145,-772.1112;Inherit;False;983.017;515.2935;;7;750;1224;1223;1222;1221;1220;1219;Gradient A;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;754;6141.349,-170.7516;Inherit;False;947.6141;495.0339;;7;1213;1214;1212;1217;1218;762;1216;Gradient B;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;765;4031.919,-2585.306;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;764;3799.629,-2592.853;Inherit;False;762;gradient_B;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;769;3803.326,-2506.983;Inherit;False;767;blend;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;729;3806.593,-2247.998;Inherit;False;727;colorOLT_B;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;728;3804.593,-2375.998;Inherit;False;673;colorOLT_A;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;735;4041.229,-2268.221;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;691;7598.034,1922.189;Inherit;True;Property;_colorOverLifetime1;sizeAndRotOverLife;14;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;698;7978.654,1974.857;Inherit;False;sizeMaxOLT;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;700;7981.656,2070.457;Inherit;False;rotMinOLT;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;693;7432.158,1950.787;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;692;7254.103,2034.147;Inherit;False;Constant;_Float7;Float 6;9;0;Create;True;0;0;0;False;0;False;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;689;7215.692,1934.206;Inherit;False;683;lifetimeUVx;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;763;3800.547,-2682.866;Inherit;False;750;gradient_A;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OneMinusNode;908;3884.611,-3676.69;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;909;3694.546,-3821.934;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;910;3623.357,-3923.004;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;912;3819.23,-3929.997;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;913;4026.683,-3953.378;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;914;4069.684,-3703.677;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;915;4432.004,-3904.079;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;916;4249.542,-3761.045;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;917;4236.526,-3864.435;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;925;3498.529,-3699.835;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.5;False;4;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;851;3495.851,-3803.169;Inherit;False;Property;_sunMaskSize;Sun Mask Size;17;0;Create;False;0;0;0;False;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;852;3197.669,-3706.109;Inherit;False;Property;_sunMaskSharpness;Sun Mask Sharpness;18;0;Create;False;0;0;0;False;0;False;0;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;922;3712.576,-3721.191;Inherit;False;2;2;0;FLOAT;0.99;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;926;3347.262,-3941.619;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SaturateNode;918;4416.511,-3649.931;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;921;4066.365,-3583.221;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;902;3335.941,-4106.386;Inherit;False;Property;_lightDirection;lightDirection;20;0;Create;False;0;0;0;False;0;False;0,0,0;-0.2816645,-0.5045274,0.81616;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;919;4600.217,-3735.004;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1039;5080.465,-3757.71;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;933;5488.115,-3579.519;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;906;4747.86,-3663.133;Inherit;False;Property;_lightColor;lightColor;21;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;47.93726,47.93726,47.93726,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CameraDepthFade;892;4585.634,-3452.282;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;895;4297.634,-3436.282;Inherit;False;Property;_sparklesStartDistance;Sparkles Start Distance;19;0;Create;False;0;0;0;False;0;False;1;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;893;4856.334,-3455.282;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;1015;4726.435,-4174.552;Inherit;False;Property;_lightsCount;_lightsCount;9;0;Create;True;0;0;0;False;0;False;5;16;False;0;1;INT;0
Node;AmplifyShaderEditor.RangedFloatNode;1028;4695.69,-4067.663;Inherit;False;Property;_pointLightsIntensity;_pointLightsIntensity;23;0;Create;True;0;0;0;False;0;False;1;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1024;4759.819,-3812.311;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;924;4271.429,-3574.846;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1134;4081.347,-4129.084;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1135;3902.348,-4045.086;Inherit;False;Constant;_Float14;Float 10;28;0;Create;True;0;0;0;False;0;False;0.99;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SignOpNode;1136;4243.349,-4127.084;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1137;4389.471,-4047.409;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;1138;3841.347,-4134.776;Inherit;False;1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;566;5107.049,-1718.024;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;614;5109.558,-1614.009;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;642;5555.762,-1553.185;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;607;5325.875,-1615.079;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;677;4788.424,-1680.316;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;561;4689.368,-1586.707;Inherit;True;Property;_MainTex;Texture;1;0;Create;False;0;0;0;False;0;False;-1;None;d8fe714c5c4ed5c40bee4dd67a209895;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;636;5117.87,-1507.035;Inherit;False;635;texDistBlend;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;720;4842.18,-2463.631;Inherit;False;Property;_COLOR_GRADIENT;COLOR_GRADIENT;18;0;Create;True;0;0;0;True;0;False;0;0;0;False;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;736;4531.531,-2357.767;Inherit;False;Property;_RAND_GRADIENT_OLT;RAND_GRADIENT_OLT;21;0;Create;True;0;0;0;True;0;False;0;0;0;False;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;743;4238.905,-2435.344;Inherit;False;Property;_RAND_GRADIENT;RAND_GRADIENT;24;0;Create;True;0;0;0;True;0;False;0;0;0;False;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;719;4592.045,-2568.888;Inherit;False;Property;_color;color;15;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1145;9144.652,-1667.377;Inherit;False;Constant;_Float10;Float 10;27;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;850;9136.461,-1567.913;Inherit;False;928;sparkles;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1154;9586.885,-1713.405;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;1156;9061.296,-1950.535;Inherit;False;Property;_REFRACTION;_REFRACTION;26;0;Create;True;0;0;0;True;0;False;0;1;1;False;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;1144;9331.06,-1673.977;Inherit;False;Property;_SPARKLES;SPARKLES;27;0;Create;True;0;0;0;True;0;False;0;0;0;False;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;928;5693.443,-3581.905;Inherit;False;sparkles;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;1146;4968.666,-4092.797;Inherit;False;GetPointLightsMask;2;;4;5fc1436ebbdcf85419c854243b4ddabf;0;3;9;INT;5;False;23;FLOAT;5;False;26;FLOAT;5;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;1045;4735,-3983.935;Inherit;False;Property;_spotLightsIntensity;_spotLightsIntensity;24;0;Create;True;0;0;0;False;0;False;1;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1025;5277.609,-3745.493;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;1026;4924.417,-3770.963;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;740;5434.06,-2463.588;Inherit;False;color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;1162;4866.369,-2347.337;Inherit;False;Property;_ColorMultiplier;ColorMultiplier;25;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1163;5200.911,-2466.159;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;644;5325.822,-1464.895;Inherit;False;638;distFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1149;5986.338,-1552.631;Inherit;False;opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1034;5808.867,-1553.324;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;235;3360.225,134.9178;Inherit;False;Constant;_Float4;Float 4;5;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;237;3520.225,134.9178;Inherit;False;Constant;_Float5;Float 5;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;234;3568.714,4.179867;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;252;4973.319,18.61098;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RegisterLocalVarNode;253;5205.821,69.71091;Inherit;False;flakeOffset_y;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;231;5203.832,-11.11861;Inherit;False;flakeOffset_x;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;773;4645.765,78.35391;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;583;3439.091,337.2854;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;659;3169.873,147.9715;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;236;3726.715,10.17986;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;238;3947.699,9.768453;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;949;4176.834,958.9292;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;962;4182.892,1109.17;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;963;4358.985,1016.408;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CrossProductOpNode;957;3776.034,1185.485;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;961;3971.794,1190.366;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;947;3937.873,1105.396;Inherit;False;253;flakeOffset_y;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;581;3176.219,349.9035;Inherit;False;Property;_sizeMinMax;_sizeMinMax;6;0;Create;True;0;0;0;False;0;False;0.004,0.008;0.007,0.01;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TFHCRemapNode;775;4028.793,461.2805;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;776;4042.795,636.2798;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;778;4067.437,715.8223;Inherit;False;Constant;_Float3;Float 3;17;0;Create;True;0;0;0;False;0;False;0.005555556;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;777;4343.32,430.9386;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;590;4038.663,232.3907;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;588;4235.647,235.8981;Inherit;False;1;0;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;955;3417.355,1351.229;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;953;3210.306,1313.579;Inherit;False;952;particlePos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;977;3628.889,1053.464;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;976;3344.889,996.4633;Inherit;False;975;direction;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;974;3359.82,1157.694;Inherit;False;Property;_stretchingMultiplier;_stretchingMultiplier;22;0;Create;True;0;0;0;False;0;False;1;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;948;3935.506,1019.263;Inherit;False;231;flakeOffset_x;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;239;4536.808,1019.594;Inherit;False;billboard;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector2Node;774;3732.79,595.28;Inherit;False;Property;_startRotationMinMax;_startRotationMinMax;10;1;[HideInInspector];Create;True;0;0;0;False;0;False;0,0;-180,180;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;593;3772.412,294.7991;Inherit;False;Property;_rotationSpeedMinMax;_rotationSpeedMinMax;11;1;[HideInInspector];Create;True;0;0;0;False;0;False;-5,5;-10,10;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TexCoordVertexDataNode;233;3151.715,-13.81873;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldSpaceCameraPos;954;3155.355,1407.229;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;956;3594.355,1326.228;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RotatorNode;587;4778.946,15.66256;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;768;3813.719,-2159.568;Inherit;False;767;blend;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;1182;9757.291,-1692.493;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;741;4599.294,-1682.174;Inherit;False;740;color;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1180;10810.47,-1657.888;Float;False;True;-1;2;ASEMaterialInspector;100;5;Hidden/NOT_Lonely/Weatherade/SRS_ParticleSystem_Render;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;;10;False;;0;5;False;;10;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;True;True;1;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;2;False;;True;3;False;;True;False;0;False;;0;False;;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;0;638436776112113720;0;1;True;False;;True;0
Node;AmplifyShaderEditor.GetLocalVarNode;634;10536.94,-1452.054;Inherit;False;633;vPos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;1183;10635.65,-1676.264;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;1187;9798.91,-1796.939;Inherit;False;1152;lighting;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1188;10268.91,-1717.939;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;813;4599.677,-693.6192;Inherit;False;UnpackFloats;-1;;5;fb7d2f1b3bb77824887f04cb05937452;0;1;2;FLOAT;0;False;2;FLOAT;0;FLOAT;1
Node;AmplifyShaderEditor.FunctionNode;1160;4655.843,-952.5403;Inherit;False;Unpack3dVectors;-1;;6;b7fd13cda2d9947418e24c4aad49bb03;0;1;1;FLOAT3;0,0,0;False;2;FLOAT3;4;FLOAT3;3
Node;AmplifyShaderEditor.ComponentMaskNode;529;4348.551,-917.8436;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;1179;4347.21,-741.9963;Inherit;False;False;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;662;4870.401,-700.607;Inherit;False;lifetime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;669;4878.215,-614.6395;Inherit;False;maxLifetime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;975;4924.608,-868.3993;Inherit;False;direction;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;952;4953.946,-982.0891;Inherit;False;particlePos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformPositionNode;558;5490.977,-986.3579;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;261;5369.739,-979.4252;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;633;5716.255,-987.9651;Inherit;False;vPos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;262;5176.041,-864.6547;Inherit;False;239;billboard;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;1072;8424.486,-1134.033;Inherit;False;World;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;993;8305.646,-1819.399;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GrabScreenPosition;992;7966.65,-1941.398;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;988;7999.685,-1665.975;Inherit;True;Property;_Normal;Normal;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;989;7779.65,-1616.399;Inherit;False;Constant;_NormalIntensity;Normal Intensity;4;0;Create;True;0;0;0;False;0;False;0.5;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;990;8441.125,-1816.141;Inherit;False;Global;_GrabScreen0;Grab Screen 0;28;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;742;8470.56,-2115.235;Inherit;False;740;color;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;1150;10431.15,-1579.164;Inherit;False;1149;opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;738;3233.711,-2643.172;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;734;3237.167,-2440.83;Inherit;False;Property;_gradientsRatio;_gradientsRatio;16;1;[HideInInspector];Create;True;0;0;0;False;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;733;3428.167,-2550.83;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;767;3553.729,-2550.422;Inherit;False;blend;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;686;5656.661,717.8879;Inherit;True;Property;_gradientTexOLT;gradientTexOLT;13;0;Create;True;0;0;0;True;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.VertexIdVariableNode;532;3401.293,-1002.518;Inherit;False;0;1;INT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1165;3582.778,-972.954;Inherit;False;2;2;0;INT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1166;3379.778,-899.9541;Inherit;False;Constant;_Float13;Float 13;26;0;Create;True;0;0;0;False;0;False;0.25;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleRemainderNode;543;3816.097,-984.6882;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;1178;3838.529,-805.1607;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;1198;3554.711,-801.5508;Inherit;False;Property;_SimTexSize;SimTexSize;26;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;1206;4080.202,-825.8301;Inherit;False;TexLoad;-1;;13;24997ab8bd7822d44bede2b16c924318;0;3;2;INT;0;False;3;INT;0;False;4;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;1227;4685.025,-3182.373;Inherit;False;TexLoad;-1;;18;24997ab8bd7822d44bede2b16c924318;0;3;2;INT;0;False;3;INT;0;False;4;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode;1231;4890.856,-3184.707;Inherit;False;False;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1226;4484.025,-3147.373;Inherit;False;Constant;_Float20;Float 2;25;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1225;4434.521,-3058.874;Inherit;False;716;gradientTexOLT;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;886;4453.513,-3255.011;Inherit;False;683;lifetimeUVx;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1232;6791.424,1466.707;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;663;6171.714,1496.53;Inherit;False;662;lifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;670;6173.033,1567.815;Inherit;False;669;maxLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;665;6392.971,1516.286;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;4;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;1201;6582.908,1415.587;Inherit;False;Constant;_NoiseTexWidth;NoiseTexWidth;25;0;Create;True;0;0;0;False;0;False;256;0;False;0;1;INT;0
Node;AmplifyShaderEditor.RangedFloatNode;1233;6603.424,1492.707;Inherit;False;Constant;_Float12;Float 12;25;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;683;6983.958,1467.391;Inherit;False;lifetimeUVx;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;1207;6486.091,920.651;Inherit;False;TexLoad;-1;;19;24997ab8bd7822d44bede2b16c924318;0;3;2;INT;0;False;3;INT;0;False;4;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;726;6218.08,910.3963;Inherit;False;683;lifetimeUVx;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1208;6173.587,1085.15;Inherit;False;716;gradientTexOLT;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;1203;6228.091,998.651;Inherit;False;Constant;_Float2;Float 2;25;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;727;6725.105,919.5098;Inherit;False;colorOLT_B;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;1210;6202.251,638.1489;Inherit;False;716;gradientTexOLT;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.FunctionNode;1209;6470.755,500.6499;Inherit;False;TexLoad;-1;;20;24997ab8bd7822d44bede2b16c924318;0;3;2;INT;0;False;3;INT;0;False;4;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;685;6221.431,453.5538;Inherit;False;683;lifetimeUVx;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1211;6256.755,547.6499;Inherit;False;Constant;_Float15;Float 2;25;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;673;6714.456,494.6674;Inherit;False;colorOLT_A;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;1214;6237.803,145.1754;Inherit;False;Constant;_Float16;Float 2;25;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1217;6428.635,31.67527;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1218;6241.635,66.67521;Inherit;False;Constant;_Float17;Float 17;25;0;Create;True;0;0;0;False;0;False;256;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;1212;6612.804,42.17551;Inherit;False;TexLoad;-1;;21;24997ab8bd7822d44bede2b16c924318;0;3;2;INT;0;False;3;INT;0;False;4;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;1213;6190.299,227.6743;Inherit;False;716;gradientTexOLT;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.VertexColorNode;1216;6209.65,-110.1404;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;762;6823.921,38.73969;Inherit;False;gradient_B;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;1219;6180.302,-363.3012;Inherit;False;716;gradientTexOLT;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;1220;6234.806,-449.8008;Inherit;False;Constant;_Float18;Float 2;25;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;1221;6614.806,-496.801;Inherit;False;TexLoad;-1;;22;24997ab8bd7822d44bede2b16c924318;0;3;2;INT;0;False;3;INT;0;False;4;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1222;6425.638,-563.301;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1223;6238.638,-528.3011;Inherit;False;Constant;_Float19;Float 17;25;0;Create;True;0;0;0;False;0;False;256;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;1224;6181.638,-707.3011;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;750;6882.903,-495.5097;Inherit;False;gradient_A;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;716;5896.564,717.942;Inherit;False;gradientTexOLT;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;542;3751.198,-688.7853;Inherit;True;Property;_SRS_particles;_SRS_particles;12;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1157;8837.097,-1897.239;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;1234;8645.986,-1847.229;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1152;8714.122,-1134.513;Inherit;False;lighting;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1250;5514.158,-796.2869;Inherit;False;vPosWS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;638;4267.959,-1500.018;Inherit;False;distFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;635;4131.962,-1747.705;Inherit;False;texDistBlend;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;1243;3730.42,-1495.583;Inherit;False;Inverse Lerp;-1;;29;09cbe79402f023141a4dc1fddd4c9511;0;3;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1252;3559.657,-1543.954;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;INT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1255;3534.825,-1358.495;Inherit;False;1253;pDist;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1235;3240.748,-1605.075;Inherit;False;Constant;_Float21;Float 13;25;0;Create;True;0;0;0;False;0;False;0.001;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1248;3930.867,-1497.703;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1249;4099.997,-1498.1;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;1244;3737.212,-1752.797;Inherit;False;Inverse Lerp;-1;;30;09cbe79402f023141a4dc1fddd4c9511;0;3;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1245;3955.213,-1746.797;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1236;3575.018,-1816.668;Inherit;False;3;3;0;INT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1254;3523.54,-1644.355;Inherit;False;1253;pDist;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;1256;3466.938,-1707.53;Inherit;False;1;0;INT;0;False;1;INT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;1238;2591.939,-1650.645;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;1239;2651.223,-1498.98;Inherit;False;1250;vPosWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DistanceOpNode;1242;2846.94,-1598.645;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1253;2999.777,-1595.39;Inherit;False;pDist;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;1246;3205.724,-1834.227;Inherit;False;Property;_NearBlurDistance;_NearBlurDistance;8;0;Create;True;0;0;0;False;0;False;8;10;False;0;1;INT;0
Node;AmplifyShaderEditor.RangedFloatNode;1247;3230.566,-1747.628;Inherit;False;Property;_NearBlurFalloff;_NearBlurFalloff;0;0;Create;True;0;0;0;False;0;False;0.8;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1237;3210.208,-1433.859;Inherit;False;Property;_OpacityFadeStartDistance;_OpacityFadeStartDistance;4;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;1251;3278.255,-1330.985;Inherit;False;Property;_OpacityFadeFalloff;_OpacityFadeFalloff;7;0;Create;True;0;0;0;False;0;False;8;10;False;0;1;INT;0
WireConnection;699;0;691;4
WireConnection;694;0;691;1
WireConnection;765;0;763;0
WireConnection;765;1;764;0
WireConnection;765;2;769;0
WireConnection;735;0;728;0
WireConnection;735;1;729;0
WireConnection;735;2;768;0
WireConnection;691;1;693;0
WireConnection;698;0;691;2
WireConnection;700;0;691;3
WireConnection;693;0;689;0
WireConnection;693;1;692;0
WireConnection;908;0;922;0
WireConnection;909;0;851;0
WireConnection;910;0;902;0
WireConnection;910;1;926;0
WireConnection;912;0;910;0
WireConnection;913;0;912;0
WireConnection;913;1;912;0
WireConnection;914;0;909;0
WireConnection;914;1;908;0
WireConnection;915;0;913;0
WireConnection;915;1;917;0
WireConnection;915;2;916;0
WireConnection;916;0;909;0
WireConnection;916;1;914;0
WireConnection;917;0;909;0
WireConnection;917;1;914;0
WireConnection;925;0;852;0
WireConnection;922;1;925;0
WireConnection;918;0;924;0
WireConnection;919;0;915;0
WireConnection;919;1;918;0
WireConnection;1039;0;1026;0
WireConnection;1039;1;906;0
WireConnection;1039;2;893;0
WireConnection;933;0;1025;0
WireConnection;933;1;1231;0
WireConnection;892;0;895;0
WireConnection;893;0;892;0
WireConnection;1024;0;1137;0
WireConnection;1024;1;919;0
WireConnection;924;0;921;2
WireConnection;1134;0;1138;0
WireConnection;1134;1;1135;0
WireConnection;1136;0;1134;0
WireConnection;1137;0;1136;0
WireConnection;566;0;677;0
WireConnection;566;1;561;1
WireConnection;614;0;677;0
WireConnection;614;1;561;2
WireConnection;642;0;607;0
WireConnection;642;1;644;0
WireConnection;607;0;566;0
WireConnection;607;1;614;0
WireConnection;607;2;636;0
WireConnection;677;0;741;0
WireConnection;720;1;719;0
WireConnection;720;0;736;0
WireConnection;736;1;743;0
WireConnection;736;0;735;0
WireConnection;743;1;728;0
WireConnection;743;0;765;0
WireConnection;1154;0;1156;0
WireConnection;1154;1;1144;0
WireConnection;1156;1;742;0
WireConnection;1156;0;1157;0
WireConnection;1144;1;1145;0
WireConnection;1144;0;850;0
WireConnection;928;0;933;0
WireConnection;1146;9;1015;0
WireConnection;1146;23;1028;0
WireConnection;1146;26;1045;0
WireConnection;1025;0;1146;0
WireConnection;1025;1;1039;0
WireConnection;1026;0;1024;0
WireConnection;740;0;1163;0
WireConnection;1163;0;720;0
WireConnection;1163;1;1162;0
WireConnection;1149;0;1034;0
WireConnection;1034;0;642;0
WireConnection;234;0;233;0
WireConnection;234;1;235;0
WireConnection;252;0;587;0
WireConnection;253;0;252;1
WireConnection;231;0;252;0
WireConnection;773;0;588;0
WireConnection;773;1;777;0
WireConnection;583;0;659;4
WireConnection;583;3;581;1
WireConnection;583;4;581;2
WireConnection;236;0;234;0
WireConnection;236;1;237;0
WireConnection;238;0;236;0
WireConnection;238;1;583;0
WireConnection;949;0;977;0
WireConnection;949;1;948;0
WireConnection;962;0;947;0
WireConnection;962;1;961;0
WireConnection;963;0;949;0
WireConnection;963;1;962;0
WireConnection;957;0;977;0
WireConnection;957;1;956;0
WireConnection;961;0;957;0
WireConnection;775;0;659;4
WireConnection;775;3;774;1
WireConnection;775;4;774;2
WireConnection;777;0;775;0
WireConnection;777;1;776;0
WireConnection;777;2;778;0
WireConnection;590;0;659;4
WireConnection;590;3;593;1
WireConnection;590;4;593;2
WireConnection;588;0;590;0
WireConnection;955;0;953;0
WireConnection;955;1;954;0
WireConnection;977;0;976;0
WireConnection;977;1;974;0
WireConnection;239;0;963;0
WireConnection;956;0;955;0
WireConnection;587;0;238;0
WireConnection;587;2;773;0
WireConnection;1182;0;1154;0
WireConnection;1180;0;1183;0
WireConnection;1180;1;634;0
WireConnection;1183;0;1188;0
WireConnection;1183;3;1150;0
WireConnection;1188;0;1187;0
WireConnection;1188;1;1182;0
WireConnection;813;2;1179;0
WireConnection;1160;1;529;0
WireConnection;529;0;1206;0
WireConnection;1179;0;1206;0
WireConnection;662;0;813;0
WireConnection;669;0;813;1
WireConnection;975;0;1160;3
WireConnection;952;0;1160;4
WireConnection;558;0;261;0
WireConnection;261;0;952;0
WireConnection;261;1;262;0
WireConnection;633;0;558;0
WireConnection;993;0;992;0
WireConnection;993;1;988;0
WireConnection;988;5;989;0
WireConnection;990;0;993;0
WireConnection;733;0;738;4
WireConnection;733;1;734;0
WireConnection;767;0;733;0
WireConnection;1165;0;532;0
WireConnection;1165;1;1166;0
WireConnection;543;0;1165;0
WireConnection;543;1;1198;1
WireConnection;1178;0;1165;0
WireConnection;1178;1;1198;2
WireConnection;1206;2;543;0
WireConnection;1206;3;1178;0
WireConnection;1206;4;542;0
WireConnection;1227;2;886;0
WireConnection;1227;3;1226;0
WireConnection;1227;4;1225;0
WireConnection;1231;0;1227;0
WireConnection;1232;0;1201;0
WireConnection;1232;1;1233;0
WireConnection;1232;2;665;0
WireConnection;665;0;663;0
WireConnection;665;2;670;0
WireConnection;683;0;1232;0
WireConnection;1207;2;726;0
WireConnection;1207;3;1203;0
WireConnection;1207;4;1208;0
WireConnection;727;0;1207;0
WireConnection;1209;2;685;0
WireConnection;1209;3;1211;0
WireConnection;1209;4;1210;0
WireConnection;673;0;1209;0
WireConnection;1217;0;1216;4
WireConnection;1217;1;1218;0
WireConnection;1212;2;1217;0
WireConnection;1212;3;1214;0
WireConnection;1212;4;1213;0
WireConnection;762;0;1212;0
WireConnection;1221;2;1222;0
WireConnection;1221;3;1220;0
WireConnection;1221;4;1219;0
WireConnection;1222;0;1224;4
WireConnection;1222;1;1223;0
WireConnection;750;0;1221;0
WireConnection;716;0;686;0
WireConnection;1157;0;742;0
WireConnection;1157;1;1234;0
WireConnection;1234;0;990;0
WireConnection;1152;0;1072;0
WireConnection;1250;0;261;0
WireConnection;638;0;1249;0
WireConnection;635;0;1245;0
WireConnection;1243;1;1252;0
WireConnection;1243;2;1237;0
WireConnection;1243;3;1255;0
WireConnection;1252;0;1235;0
WireConnection;1252;1;1237;0
WireConnection;1252;2;1251;0
WireConnection;1248;0;1243;0
WireConnection;1249;0;1248;0
WireConnection;1244;1;1236;0
WireConnection;1244;2;1256;0
WireConnection;1244;3;1254;0
WireConnection;1245;0;1244;0
WireConnection;1236;0;1246;0
WireConnection;1236;1;1247;0
WireConnection;1236;2;1235;0
WireConnection;1256;0;1246;0
WireConnection;1242;0;1238;0
WireConnection;1242;1;1239;0
WireConnection;1253;0;1242;0
ASEEND*/
//CHKSM=67548FA17407B89744EBF976B53863788AFD9145