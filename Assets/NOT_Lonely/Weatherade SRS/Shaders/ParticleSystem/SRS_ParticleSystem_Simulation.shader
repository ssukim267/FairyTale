// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/NOT_Lonely/Weatherade/SRS_ParticleSystem_Simulation"
{
	Properties
	{
		_SRS_particles("_SRS_particles", 2D) = "white" {}
		_velocityMax("_velocityMax", Vector) = (0,1,0,0)
		_velocityMin("_velocityMin", Vector) = (0,0,0,0)
		_lifetimeMinMax("_lifetimeMinMax", Vector) = (0.5,1,0,0)
		_SRS_localDepth("_SRS_localDepth", 2D) = "white" {}
		_swayingFrequency("_swayingFrequency", Vector) = (0,0,0,0)
		_swayingAmplitude("_swayingAmplitude", Vector) = (0,0,0,0)
		_gradientTexOLT("gradientTexOLT", 2D) = "black" {}
		_emissionFlag("_emissionFlag", Float) = 1
		_particlesPercentage("_particlesPercentage", Float) = 0
		[Toggle]_velCurvesRand("velCurvesRand", Float) = 0
		_noiseTex("_noiseTex", 2D) = "white" {}
		_velocityMinMultiplier("_velocityMinMultiplier", Vector) = (0,0,0,0)
		_velocityMaxMultiplier("_velocityMaxMultiplier", Vector) = (0,0,0,0)
		[Toggle]_isPrewarming("isPrewarming", Float) = 0
		_emitterSize("_emitterSize", Vector) = (0,0,0,0)
		_uvOffset("_uvOffset", Vector) = (0,0,0,0)
		_Teleporting("Teleporting", Float) = 0
		_AddPos("AddPos", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.5
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"

			CGPROGRAM

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
			#pragma shader_feature_local _VEL_CURVES
			#pragma shader_feature_local _WARP_POSITIONS
			#pragma shader_feature_local _SRS_COLLISION
			#pragma shader_feature_local _SRS_LOCALDEPTH
			#pragma warning(disable : 4008)
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
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			UNITY_DECLARE_TEX2D_NOSAMPLER(_SRS_particles);
			uniform float4 _SRS_particles_ST;
			SamplerState sampler_SRS_particles;
			uniform float _emissionFlag;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_noiseTex);
			uniform float4 _noiseTex_ST;
			SamplerState sampler_noiseTex;
			uniform float _particlesPercentage;
			uniform float4x4 _emitterMatrix;
			uniform float3 _emitterSize;
			uniform float2 _srs_deltaTime;
			uniform float3 _swayingFrequency;
			uniform float3 _swayingAmplitude;
			uniform float3 _velocityMin;
			uniform float3 _velocityMax;
			uniform float _velCurvesRand;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_gradientTexOLT);
			uniform float3 _velocityMinMultiplier;
			uniform float3 _velocityMaxMultiplier;
			uniform float _Teleporting;
			uniform float3 _AddPos;
			uniform float2 _lifetimeMinMax;
			uniform float4x4 _depthCamMatrix;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_SRS_depth);
			SamplerState sampler_SRS_depth;
			uniform float4x4 _snowfallDepthCamMatrix;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_SRS_localDepth);
			uniform float2 _uvOffset;
			SamplerState sampler_SRS_localDepth;
			uniform float _isPrewarming;
			float4 ReadPixels1_g26( int u, int v, Texture2D tex )
			{
				return tex.Load(int3(u, v, 0));
			}
			
			float4 ReadPixels1_g27( int u, int v, Texture2D tex )
			{
				return tex.Load(int3(u, v, 0));
			}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
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
				float2 uv_SRS_particles = i.ase_texcoord1.xy * _SRS_particles_ST.xy + _SRS_particles_ST.zw;
				float4 tex2DNode3 = SAMPLE_TEXTURE2D( _SRS_particles, sampler_SRS_particles, uv_SRS_particles );
				float temp_output_2_0_g22 = tex2DNode3.a;
				float temp_output_3_0_g22 = frac( temp_output_2_0_g22 );
				half remainingLifetime90 = ( ( temp_output_2_0_g22 - temp_output_3_0_g22 ) / 1000.0 );
				float3 temp_cast_0 = (0.0).xxx;
				float3 temp_output_4_0_g24 = temp_cast_0;
				float2 uv_noiseTex = i.ase_texcoord1.xy * _noiseTex_ST.xy + _noiseTex_ST.zw;
				float4 tex2DNode587 = SAMPLE_TEXTURE2D( _noiseTex, sampler_noiseTex, uv_noiseTex );
				float noiseStaticC611 = tex2DNode587.b;
				float2 texCoord46 = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_cast_1 = (0.5).xx;
				float2 break147 = ( ( texCoord46 - temp_cast_1 ) * (_emitterSize).xz );
				float mulTime597 = _Time.y * 0.1;
				float2 texCoord598 = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult599 = (float2(( frac( mulTime597 ) + texCoord598.x ) , texCoord598.y));
				float4 tex2DNode593 = SAMPLE_TEXTURE2D( _noiseTex, sampler_noiseTex, appendResult599 );
				float noiseDynamicA600 = tex2DNode593.r;
				float2 _randomStartPosY = float2(-2,0);
				float randomStartPosY570 = (_randomStartPosY.x + (noiseDynamicA600 - 0.0) * (_randomStartPosY.y - _randomStartPosY.x) / (1.0 - 0.0));
				float4 appendResult70 = (float4(break147.x , randomStartPosY570 , break147.y , 1.0));
				float3 initPos413 = (mul( _emitterMatrix, appendResult70 )).xyz;
				float3 hidePos424 = float3(0,-9999,0);
				float3 visibleParticles428 = ( step( noiseStaticC611 , (0.0 + (( _particlesPercentage > 0.0 ? pow( 2.0 , ( ( _particlesPercentage * 10.0 ) - 10.0 ) ) : 0.0 ) - 0.0001) * (1.0 - 0.0) / (1.0 - 0.0001)) ) > 0.0 ? initPos413 : hidePos424 );
				float3 appendResult65 = (float3(tex2DNode3.r , tex2DNode3.g , tex2DNode3.b));
				float3 temp_output_1_0_g25 = appendResult65;
				float3 temp_output_6_0_g25 = frac( temp_output_1_0_g25 );
				float3 temp_cast_2 = (1.0).xxx;
				float3 lastDir827 = ( ( temp_output_6_0_g25 * 2.0 ) - temp_cast_2 );
				float noiseStaticA591 = tex2DNode587.r;
				float temp_output_13_0_g10 = noiseStaticA591;
				float swayingX211 = ( sin( ( ( remainingLifetime90 + temp_output_13_0_g10 ) * ( temp_output_13_0_g10 * _swayingFrequency.x ) ) ) * ( temp_output_13_0_g10 * _swayingAmplitude.x ) );
				float noiseStaticB592 = tex2DNode587.g;
				float3 appendResult60 = (float3(noiseStaticA591 , noiseStaticB592 , noiseStaticC611));
				float3 appendResult562 = (float3(noiseStaticA591 , noiseStaticB592 , noiseStaticC611));
				half previousStartLifetime515 = ( 1000.0 * temp_output_3_0_g22 );
				float lifetimeUVx533 = (0.0 + (remainingLifetime90 - 0.0) * ((float)255 - 0.0) / (previousStartLifetime515 - 0.0));
				int u1_g26 = (int)lifetimeUVx533;
				int v1_g26 = 2;
				Texture2D tex1_g26 =(Texture2D)_gradientTexOLT;
				float4 localReadPixels1_g26 = ReadPixels1_g26( u1_g26 , v1_g26 , tex1_g26 );
				float3 velMinOLT534 = ( (localReadPixels1_g26).xyz * _velocityMinMultiplier );
				int u1_g27 = (int)lifetimeUVx533;
				int v1_g27 = 3;
				Texture2D tex1_g27 =(Texture2D)_gradientTexOLT;
				float4 localReadPixels1_g27 = ReadPixels1_g27( u1_g27 , v1_g27 , tex1_g27 );
				float3 velMaxOLT547 = ( (localReadPixels1_g27).xyz * _velocityMaxMultiplier );
				#ifdef _VEL_CURVES
				float3 staticSwitch556 = (( _velCurvesRand )?( velMinOLT534 ):( (velMinOLT534 + (appendResult562 - float3( 0,0,0 )) * (velMaxOLT547 - velMinOLT534) / (float3( 1,1,1 ) - float3( 0,0,0 ))) ));
				#else
				float3 staticSwitch556 = (_velocityMin + (appendResult60 - float3( 0,0,0 )) * (_velocityMax - _velocityMin) / (float3( 1,1,1 ) - float3( 0,0,0 )));
				#endif
				float3 finalVelocity693 = staticSwitch556;
				float3 break179 = finalVelocity693;
				float temp_output_13_0_g8 = noiseStaticB592;
				float swayingY212 = ( sin( ( ( remainingLifetime90 + temp_output_13_0_g8 ) * ( temp_output_13_0_g8 * _swayingFrequency.y ) ) ) * ( temp_output_13_0_g8 * _swayingAmplitude.y ) );
				float temp_output_13_0_g9 = noiseStaticC611;
				float swayingZ213 = ( sin( ( ( ( 1.0 - remainingLifetime90 ) + temp_output_13_0_g9 ) * ( temp_output_13_0_g9 * _swayingFrequency.z ) ) ) * ( temp_output_13_0_g9 * _swayingAmplitude.z ) );
				float3 appendResult180 = (float3(( swayingX211 + break179.x ) , ( break179.y + swayingY212 ) , ( break179.z + swayingZ213 )));
				float3 particlesLastPos123 = ( ( temp_output_1_0_g25 - temp_output_6_0_g25 ) / 1000.0 );
				float3 curParticlePos777 = ( ( appendResult180 * _srs_deltaTime.x ) + particlesLastPos123 );
				float3 temp_output_789_0 = ( curParticlePos777 - particlesLastPos123 );
				float3 temp_cast_6 = (1.0).xxx;
				float3 normalizeResult890 = normalize( ( _srs_deltaTime.x == 0.0 ? lastDir827 : ( sign( temp_output_789_0 ) * min( abs( temp_output_789_0 ) , temp_cast_6 ) ) ) );
				float3 temp_output_4_0_g23 = normalizeResult890;
				float4 appendResult715 = (float4(0.0 , 0.0 , 0.0 , 1.0));
				float3 emitterCenter716 = (mul( _emitterMatrix, appendResult715 )).xyz;
				float3 break751 = emitterCenter716;
				float3 emitterSize721 = _emitterSize;
				float3 appendResult752 = (float3(break751.x , ( break751.y - ( 0.5 * (emitterSize721).y ) ) , break751.z));
				float3 temp_output_717_0 = ( curParticlePos777 - appendResult752 );
				float3 break757 = emitterSize721;
				float3 appendResult756 = (float3(( 0.5 * break757.x ) , ( break757.y * 0.5 ) , ( 0.5 * break757.z )));
				float3 warpPos782 = ( _Teleporting == 1.0 ? ( _AddPos + curParticlePos777 ) : ( abs( temp_output_717_0 ) > appendResult756 ? ( ( ( temp_output_717_0 - -temp_output_717_0 ) * 0.01 ) + ( -temp_output_717_0 + appendResult752 ) ) : curParticlePos777 ) );
				#ifdef _WARP_POSITIONS
				float3 staticSwitch776 = warpPos782;
				#else
				float3 staticSwitch776 = curParticlePos777;
				#endif
				float3 simulatedParticlePos373 = staticSwitch776;
				float3 temp_output_2_0_g23 = simulatedParticlePos373;
				float noiseDynamicB601 = tex2DNode593.g;
				float inititalLifetime99 = (_lifetimeMinMax.x + (noiseDynamicB601 - 0.0) * (_lifetimeMinMax.y - _lifetimeMinMax.x) / (1.0 - 0.0));
				float currentRemainingLifetime377 = ( remainingLifetime90 - _srs_deltaTime.x );
				float4 appendResult475 = (float4(simulatedParticlePos373 , 1.0));
				float4 temp_output_477_0 = mul( _depthCamMatrix, appendResult475 );
				float4 break465 = ( ( temp_output_477_0 * 0.5 ) + 0.5 );
				float2 appendResult464 = (float2(break465.x , ( 1.0 - break465.y )));
				float2 appendResult471 = (float2((temp_output_477_0).z , SAMPLE_TEXTURE2D( _SRS_depth, sampler_SRS_depth, appendResult464 ).b));
				float4 appendResult285 = (float4(simulatedParticlePos373 , 1.0));
				float4 temp_output_287_0 = mul( _snowfallDepthCamMatrix, appendResult285 );
				float4 break292 = ( ( temp_output_287_0 * 0.5 ) + 0.5 );
				float2 appendResult291 = (float2(break292.x , ( 1.0 - break292.y )));
				float2 appendResult460 = (float2((temp_output_287_0).z , SAMPLE_TEXTURE2D( _SRS_localDepth, sampler_SRS_localDepth, ( appendResult291 + _uvOffset ) ).r));
				#ifdef _SRS_LOCALDEPTH
				float2 staticSwitch455 = appendResult460;
				#else
				float2 staticSwitch455 = appendResult471;
				#endif
				float2 break462 = staticSwitch455;
				#ifdef _SRS_COLLISION
				float staticSwitch481 = ( break462.x <= break462.y ? 0.0 : currentRemainingLifetime377 );
				#else
				float staticSwitch481 = currentRemainingLifetime377;
				#endif
				float lerpResult829 = lerp( staticSwitch481 , currentRemainingLifetime377 , _isPrewarming);
				float finalRemainingLifetime375 = lerpResult829;
				float packedLifetime512 = ( ( ( remainingLifetime90 <= 0.0 ? inititalLifetime99 : previousStartLifetime515 ) / 1000.0 ) + floor( ( finalRemainingLifetime375 * 1000.0 ) ) );
				float4 appendResult94 = (float4(( ( 0.5 + ( 0.5 * temp_output_4_0_g23 ) ) + floor( ( temp_output_2_0_g23 * 1000.0 ) ) ) , packedLifetime512));
				float4 simulatedValues126 = appendResult94;
				float3 stopEmitting398 = ( remainingLifetime90 > 0.0 ? (simulatedValues126).xyz : hidePos424 );
				float3 initPosFinal683 = ( _emissionFlag == 1.0 ? visibleParticles428 : stopEmitting398 );
				float3 temp_output_2_0_g24 = initPosFinal683;
				float4 appendResult96 = (float4(( ( 0.5 + ( 0.5 * temp_output_4_0_g24 ) ) + floor( ( temp_output_2_0_g24 * 1000.0 ) ) ) , ( ( inititalLifetime99 / 1000.0 ) + floor( ( inititalLifetime99 * 1000.0 ) ) )));
				float4 initState118 = appendResult96;
				
				
				finalColor = ( remainingLifetime90 <= 0.0 ? initState118 : simulatedValues126 );
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
Node;AmplifyShaderEditor.CommentaryNode;125;-2086.075,-748.4064;Inherit;False;4446.672;1617.245;;6;779;777;693;124;781;890;Simulated;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;781;-1145.286,-39.69216;Inherit;False;2835.245;842.2164;;32;782;778;718;855;858;857;757;722;766;732;717;727;750;747;728;748;749;780;759;756;856;851;854;752;853;852;751;711;869;870;872;873;Warp Positions;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;708;-5068.111,-1002.194;Inherit;False;1323.149;761;;6;600;601;611;591;592;612;Noise;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;552;2572.463,-650.8838;Inherit;False;985.3378;541.0469;;0;Pack Init Lifetime and Simulated Lifetime to single float;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;380;-2065.052,1147.454;Inherit;False;3797.827;1362.199;;0;Collision;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;480;-2014.956,1957.781;Inherit;False;2090.9;480.4091;;10;291;292;293;300;290;301;287;299;867;868;Local Depth;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;479;-2016.727,1387.502;Inherit;False;2091.975;480.4091;;0;Global Depth;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;117;-2161.606,-2130.084;Inherit;False;3454.121;1244.036;;33;101;118;683;578;848;849;721;147;95;413;719;716;383;792;679;96;429;399;386;385;714;66;46;715;70;74;146;145;144;75;400;411;441;Initial State;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;441;-2117.065,-1147.957;Inherit;False;461.001;164.13;;3;424;451;452;NaN pos;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;411;-644.0113,-1410.667;Inherit;False;1826.266;478.5063;;15;859;669;445;406;410;414;427;428;401;862;402;861;864;865;866;Percentage of visible particles;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;400;-1643.834,-1426.868;Inherit;False;963.9756;449;;7;425;390;398;393;394;395;392;Stop emitting and reset positions to NaN;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;128;3966.95,28.6678;Inherit;False;733.1234;498.4197;;1;127;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;122;-3482.261,370.5298;Inherit;False;1281.136;589.4647;;1;677;Data Tex;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;116;-3480.304,-200.0838;Inherit;False;763.3798;434.3144;;3;604;99;115;Init Life Time;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;65;-2861.589,448.5301;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;161;-3488.031,1701.138;Inherit;False;1220.252;621.9978;;0;Horizontal Swaying;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;219;-3454.479,1941.547;Inherit;False;Property;_swayingFrequency;_swayingFrequency;6;0;Create;True;0;0;0;False;0;False;0,0,0;10,5,10;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;220;-3444.403,2106.809;Inherit;False;Property;_swayingAmplitude;_swayingAmplitude;7;0;Create;True;0;0;0;False;0;False;0,0,0;0.5,0.2,0.5;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;212;-2504.872,1934.899;Inherit;False;swayingY;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;213;-2502.914,2114.104;Inherit;False;swayingZ;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;211;-2513.79,1760.932;Inherit;False;swayingX;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;370;-2815.622,1938.62;Inherit;False;Swaying;-1;;8;059bd76c3e9e90d4e9866aa072ef1dce;0;4;7;FLOAT;0;False;11;FLOAT;0;False;13;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-1239.168,-1863.919;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;144;-1898.126,-1791.634;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;145;-2054.125,-1735.634;Inherit;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;146;-1724.126,-1743.634;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-1571.996,-1632.892;Inherit;False;Constant;_Float3;Float 3;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;392;-1359.17,-1203.369;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;395;-1295.17,-1299.369;Inherit;False;Constant;_Float4;Float 4;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;393;-1071.17,-1347.369;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;390;-1586.17,-1203.369;Inherit;False;126;simulatedValues;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;452;-1962.216,-1098.372;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;451;-2102.216,-1088.372;Inherit;False;Constant;_Float9;Float 9;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;465;-917.6667,1610.37;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;466;-1066.122,1611.147;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;467;-1249.123,1591.147;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;471;-79.54694,1596.26;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;472;-1253.389,1442.736;Inherit;False;False;False;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;473;-134.6485,1535.034;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;474;-1879.486,1730.448;Inherit;False;Constant;_Float11;Float 0;9;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;475;-1722.861,1657.66;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;476;-1418.123,1757.145;Inherit;False;Constant;_Float12;Float 1;9;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;477;-1428.727,1576.044;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;478;-1959.447,1633.41;Inherit;False;373;simulatedParticlePos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;454;-423.9471,1589.872;Inherit;True;Global;_SRS_depth;_SRS_depth;4;0;Create;True;0;0;0;False;0;False;-1;None;;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;464;-610.7812,1612.427;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;468;-773.6047,1682.169;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;460;-92.96626,2174.489;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;463;-148.0684,2113.262;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;286;-1892.903,2308.677;Inherit;False;Constant;_Float5;Float 0;9;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;285;-1736.279,2235.888;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;374;-1972.866,2211.637;Inherit;False;373;simulatedParticlePos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;289;-435.8066,2168.367;Inherit;True;Property;_SRS_localDepth;_SRS_localDepth;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;462;437.3834,1855.535;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;335;421.3834,1974.665;Inherit;False;Constant;_Float7;Float 7;12;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;371;-2800.631,2109.714;Inherit;False;Swaying;-1;;9;059bd76c3e9e90d4e9866aa072ef1dce;0;4;7;FLOAT;0;False;11;FLOAT;0;False;13;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;372;-2827.364,1778.859;Inherit;False;Swaying;-1;;10;059bd76c3e9e90d4e9866aa072ef1dce;0;4;7;FLOAT;0;False;11;FLOAT;0;False;13;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;378;301.0209,2082.486;Inherit;False;377;currentRemainingLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-3185.261,444.7085;Inherit;True;Property;_output;output;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;526;-2029.915,3664.73;Inherit;False;761.926;277.5641;;5;533;896;568;531;569;Calculate UV.x from lifetime;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;527;-2050.752,2621.343;Inherit;False;1490.446;408.2629;;8;534;894;539;901;541;554;616;617;Velocity Min over lifetime;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;528;-2047.914,3194.754;Inherit;False;1479.076;377.5332;;8;898;544;897;545;555;618;619;547;Velocity Max over lifetime;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;87;37.54694,-320.4773;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;92;-224.6831,-388.9844;Inherit;False;90;remainingLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;180;-200.3761,-576.983;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;182;-390.5034,-414.3124;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;217;-621.2668,-420.283;Inherit;False;212;swayingY;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;216;-612.167,-692.6829;Inherit;False;211;swayingX;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;218;-610.2668,-314.283;Inherit;False;213;swayingZ;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;181;-406.8083,-685.4998;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;200;-387.2713,-554.5679;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;362;-2976.146,2050.35;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;179;-618.394,-578.8929;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;60;-1812.022,-685.4061;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;605;-2029.571,-716.9012;Inherit;False;591;noiseStaticA;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;176;-3216.596,1944.839;Inherit;False;90;remainingLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;562;-1811.885,-154.3185;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;606;-2025.416,-643.5182;Inherit;False;592;noiseStaticB;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;558;-1974.984,177.8331;Inherit;False;547;velMaxOLT;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;613;-2027.076,-564.5056;Inherit;False;611;noiseStaticC;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;608;-2032.486,-99.69794;Inherit;False;592;noiseStaticB;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;609;-3204.04,1771.475;Inherit;False;591;noiseStaticA;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;615;-3203.011,1853.985;Inherit;False;592;noiseStaticB;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;610;-3186.573,2145.121;Inherit;False;611;noiseStaticC;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;515;-2598.591,816.2631;Half;False;previousStartLifetime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;90;-2598.461,653.8773;Half;False;remainingLifetime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;614;-2034.662,-23.4093;Inherit;False;611;noiseStaticC;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;425;-1315.45,-1101.059;Inherit;False;424;hidePos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;86;-3184.102,-34.84671;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;572;-4054.458,21.4651;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;599;-4533.111,-541.194;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;587;-4336.24,-864.4512;Inherit;True;Property;_staticNoise;_staticNoise;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;593;-4335.111,-662.194;Inherit;True;Property;_dynamicNoise;_dynamicNoise;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;589;-4688.784,-794.3599;Inherit;True;Property;_noiseTex;_noiseTex;12;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.FractNode;595;-4813.111,-548.194;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;598;-4939.111,-400.194;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;602;-4666.962,-550.5592;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;607;-2033.501,-174.0081;Inherit;False;591;noiseStaticA;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;559;-1972.984,82.83308;Inherit;False;534;velMinOLT;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;563;-1654.069,-155.3497;Inherit;False;5;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;3;FLOAT3;0,0,0;False;4;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;64;233.2279,-529.0474;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;8;-3432.261,445.7085;Inherit;True;Property;_SRS_particles;_SRS_particles;0;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.DynamicAppendNode;70;-1407.158,-1747.908;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleTimeNode;597;-5009.111,-548.194;Inherit;False;1;0;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;567;-1638.619,58.57329;Inherit;False;534;velMinOLT;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;377;233.8182,-326.1106;Inherit;False;currentRemainingLifetime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;569;-2007.097,3713.254;Inherit;False;90;remainingLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;516;2690.032,-427.6523;Inherit;False;Constant;_Float13;Float 13;13;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;522;2622.463,-521.6394;Inherit;False;90;remainingLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;514;2928.804,-361.4814;Inherit;False;5;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;517;2637.546,-341.8365;Inherit;False;99;inititalLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;518;2641.939,-225.837;Inherit;False;515;previousStartLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;513;2850.015,-600.8838;Inherit;False;375;finalRemainingLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;512;3330.799,-563.2;Inherit;False;packedLifetime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;82;4070.132,197.4169;Inherit;False;Constant;_zero;zero;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;4036.13,299.417;Inherit;False;118;initState;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Compare;83;4292.145,183.6842;Inherit;False;5;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;4482.584,191.7147;Float;False;True;-1;2;ASEMaterialInspector;100;5;Hidden/NOT_Lonely/Weatherade/SRS_ParticleSystem_Simulation;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;True;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;3;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;2;Include;;False;;Native;False;0;0;;Pragma;warning(disable : 4008);False;;Custom;False;0;0;;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;True;0
Node;AmplifyShaderEditor.GetLocalVarNode;91;4002.131,85.59372;Inherit;False;90;remainingLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;678;3134.804,-484.5696;Inherit;False;PackFloats;-1;;15;cc38e1ee9efb8cd41a684a3cdb95f050;0;2;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;123;-2475.706,442.5309;Inherit;False;particlesLastPos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;32.77975,-572.6797;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;821;906.9108,-220.0267;Inherit;False;Constant;_Float19;Float 19;21;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;824;-287.9984,-244.5697;Inherit;False;Global;_srs_deltaTime;_srs_deltaTime;21;0;Create;True;0;0;0;False;0;False;0,0;0.062633,15.96602;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;789;744.01,-305.8296;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;795;525.4288,-328.9282;Inherit;False;777;curParticlePos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;790;520.0101,-251.8296;Inherit;False;123;particlesLastPos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;827;-2462.433,537.827;Inherit;False;lastDir;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;519;1770.934,-449.6883;Inherit;False;512;packedLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;94;1992.945,-517.0984;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;126;2156.088,-522.245;Inherit;False;simulatedValues;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMinOpNode;820;1151.911,-298.0267;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SignOpNode;822;1144.911,-384.0267;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Compare;825;1536.193,-438.5752;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;823;1369.911,-326.0267;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;828;1333.996,-400.7778;Inherit;False;827;lastDir;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;826;1358.193,-476.5752;Inherit;False;Constant;_Float21;Float 21;21;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;819;965.9108,-300.0267;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Compare;323;594.9489,1857.426;Inherit;False;5;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;455;154.1137,1851.1;Inherit;False;Property;_SRS_LOCALDEPTH;SRS_LOCALDEPTH;13;0;Create;True;0;0;0;False;0;False;0;0;0;False;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;481;794.1261,1799.064;Inherit;False;Property;_SRS_COLLISION;SRS_COLLISION;15;0;Create;True;0;0;0;False;0;False;0;0;0;False;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;566;-1420.619,-160.4267;Inherit;False;Property;_velCurvesRand;velCurvesRand;11;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;556;-1206.381,-536.7184;Inherit;False;Property;_VEL_CURVES;VEL_CURVES;12;0;Create;True;0;0;0;False;0;False;0;0;0;False;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;375;1332.518,1805.232;Inherit;False;finalRemainingLifetime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;829;1103.756,1807.384;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;830;858.7556,2050.384;Inherit;False;Property;_isPrewarming;isPrewarming;15;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;715;-1255.677,-1645.868;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;46;-2124.613,-1869.299;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;57;-2016.204,-330.705;Inherit;False;Property;_velocityMax;_velocityMax;1;0;Create;True;0;0;0;False;0;False;0,1,0;0.5,-1,0.5;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Matrix4X4Node;453;-1960.523,1478.447;Inherit;False;Global;_depthCamMatrix;_depthCamMatrix;6;0;Create;True;0;0;0;True;0;False;1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.Matrix4X4Node;288;-1970.291,2037.129;Inherit;False;Global;_snowfallDepthCamMatrix;_snowfallDepthCamMatrix;6;0;Create;True;0;0;0;True;0;False;1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.Vector3Node;56;-2014.504,-481.3044;Inherit;False;Property;_velocityMin;_velocityMin;2;0;Create;True;0;0;0;False;0;False;0,0,0;-0.5,-0.5,-0.5;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Matrix4X4Node;66;-1802.157,-2009.532;Inherit;False;Global;_emitterMatrix;_emitterMatrix;5;0;Create;True;0;0;0;False;0;False;0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;714;-1128.677,-1712.868;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Compare;385;-272.3177,-1894.577;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;386;-472.5097,-1943.349;Inherit;False;Constant;_Float1;Float 1;9;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;429;-515.0244,-1822.401;Inherit;False;428;visibleParticles;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;96;364.6811,-1914.438;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;792;-51.44471,-1856.755;Inherit;False;Constant;_Float10;Float 10;21;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;383;-509.1054,-2028.609;Inherit;False;Property;_emissionFlag;_emissionFlag;9;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;716;-594.6772,-1544.868;Inherit;False;emitterCenter;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;719;-793.6173,-1547.734;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;413;-727.8301,-2046.874;Inherit;False;initPos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;95;-941.187,-2045.056;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;783;1041.07,-531.4504;Inherit;False;782;warpPos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;373;1490.288,-615.1406;Inherit;False;simulatedParticlePos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;776;1244.511,-615.0771;Inherit;False;Property;_WARP_POSITIONS;WARP_POSITIONS;18;0;Create;True;0;0;0;False;0;False;0;0;0;False;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;58;-1429.206,-632.4374;Inherit;False;5;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;3;FLOAT3;0,0,0;False;4;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;751;-925.7868,393.2543;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.BreakToComponentsNode;147;-1567.237,-1740.379;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RegisterLocalVarNode;721;-1907.069,-1536.036;Inherit;False;emitterSize;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;849;-2131.182,-1593.94;Inherit;False;Property;_emitterSize;_emitterSize;16;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;852;-1138.937,666.2315;Inherit;False;721;emitterSize;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;853;-964.9373,665.2315;Inherit;False;False;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;752;-420.9605,386.6328;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;854;-761.9373,542.2315;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;851;-627.9373,411.2315;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;848;-1937.182,-1647.94;Inherit;False;True;False;True;False;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;856;480.2626,150.5753;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;756;635.9763,62.1265;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;759;478.2648,-2.8862;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;780;531.1874,523.3977;Inherit;False;777;curParticlePos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;749;286.5775,347.491;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;748;130.5774,324.491;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;728;-17.88416,467.5663;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;747;471.7777,423.9909;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;750;117.2776,421.4913;Inherit;False;Constant;_Float2;Float 2;19;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;727;-144.3369,353.3658;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;717;-286.9638,269.2224;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.AbsOpNode;732;273.2593,258.0693;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;766;275.3899,-8.51103;Inherit;False;Constant;_Float18;Float 18;21;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;722;-169.0801,18.30784;Inherit;False;721;emitterSize;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;757;33.90099,20.27647;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;857;310.2474,77.52094;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;858;141.2474,163.5209;Inherit;False;Constant;_Float16;Float 16;17;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;578;-1731.82,-1866.355;Inherit;False;570;randomStartPosY;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;574;-4295.458,92.4651;Inherit;False;Constant;_randomStartPosY;_randomStartPosY;16;0;Create;True;0;0;0;False;0;False;-2,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;855;-946.9373,544.2315;Inherit;False;Constant;_Float17;Float 17;17;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;683;-103.0732,-1952.326;Inherit;False;initPosFinal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;527.791,-1917.171;Inherit;False;initState;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;127;4002.222,392.0499;Inherit;False;126;simulatedValues;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;779;1018.452,-664.4706;Inherit;False;777;curParticlePos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;693;-947.823,-535.5539;Inherit;False;finalVelocity;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;124;9.983219,-439.7026;Inherit;False;123;particlesLastPos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;777;412.7042,-536.3087;Inherit;False;curParticlePos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;603;-4293.911,-20.41521;Inherit;False;600;noiseDynamicA;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;570;-3845.081,16.26883;Inherit;False;randomStartPosY;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;604;-3411.236,-118.8692;Inherit;False;601;noiseDynamicB;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;101;-210.6407,-1674.26;Inherit;False;99;inititalLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;401;733.182,-1231.418;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;414;458.0653,-1114.196;Inherit;False;413;initPos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;410;478.106,-1190.552;Inherit;False;Constant;_Float8;Float 8;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;864;-537.9673,-1100.377;Inherit;False;Constant;_Float20;Float 20;17;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;861;-385.133,-1263.114;Inherit;False;2;2;0;FLOAT;10;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;862;-208.72,-1277.529;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;865;-208.5132,-1374.746;Inherit;False;Constant;_Float22;Float 22;17;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;859;-49.74551,-1308.077;Inherit;False;False;2;0;FLOAT;2;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;99;-2963.762,-39.69425;Inherit;False;inititalLifetime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;115;-3403.68,42.13861;Inherit;False;Property;_lifetimeMinMax;_lifetimeMinMax;3;0;Create;True;0;0;0;False;0;False;0.5,1;20,30;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;591;-3977.111,-952.194;Inherit;False;noiseStaticA;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;592;-3976.111,-868.194;Inherit;False;noiseStaticB;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;612;-3983.388,-414.3767;Inherit;False;noiseDynamicC;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;600;-3981.111,-586.194;Inherit;False;noiseDynamicA;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;601;-3977.962,-493.5592;Inherit;False;noiseDynamicB;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;611;-3975.388,-789.3767;Inherit;False;noiseStaticC;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;402;-613.1894,-1305.156;Inherit;False;Property;_particlesPercentage;_particlesPercentage;10;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;669;262.3624,-1350.23;Inherit;False;611;noiseStaticC;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;291;-764.6009,2192.555;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;292;-1061.086,2186.598;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;293;-1209.543,2187.375;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;300;-1392.541,2167.375;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OneMinusNode;290;-919.9244,2274.397;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;301;-1561.541,2333.374;Inherit;False;Constant;_Float6;Float 1;9;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;287;-1572.145,2152.272;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode;299;-1396.807,2018.965;Inherit;False;False;False;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;867;-573.0396,2195.354;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;868;-744.0396,2301.354;Inherit;False;Property;_uvOffset;_uvOffset;17;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;718;-1133.286,392.716;Inherit;False;716;emitterCenter;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;778;-525.0569,246.2133;Inherit;False;777;curParticlePos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Compare;711;803.791,280.1786;Inherit;False;2;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Compare;869;1304.879,204.1411;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;679;70.2198,-1728.936;Inherit;False;PackFloats;-1;;21;cc38e1ee9efb8cd41a684a3cdb95f050;0;2;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;677;-2841.371,688.9377;Inherit;False;UnpackFloats;-1;;22;fb7d2f1b3bb77824887f04cb05937452;0;1;2;FLOAT;0;False;2;FLOAT;0;FLOAT;1
Node;AmplifyShaderEditor.RegisterLocalVarNode;782;1477.011,207.1336;Inherit;False;warpPos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;870;936.8788,1.141113;Inherit;False;Property;_Teleporting;Teleporting;18;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;872;917.8788,67.14111;Inherit;False;Property;_AddPos;AddPos;19;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;873;1151.081,223.4006;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;887;1748.617,-570.2412;Inherit;False;Pack3dVectors;-1;;23;3d6a5c500d240fa4aa147b4a8c2bbcd0;0;2;2;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;1;FLOAT3;6
Node;AmplifyShaderEditor.FunctionNode;888;134.4948,-1949.286;Inherit;False;Pack3dVectors;-1;;24;3d6a5c500d240fa4aa147b4a8c2bbcd0;0;2;2;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;1;FLOAT3;6
Node;AmplifyShaderEditor.FunctionNode;889;-2718.291,448.0754;Inherit;False;Unpack3dVectors;-1;;25;b7fd13cda2d9947418e24c4aad49bb03;0;1;1;FLOAT3;0,0,0;False;2;FLOAT3;4;FLOAT3;3
Node;AmplifyShaderEditor.NormalizeNode;890;1706.976,-334.1374;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;394;-1377.77,-1381.969;Inherit;False;90;remainingLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;398;-884.5475,-1348.533;Inherit;False;stopEmitting;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;399;-493.3976,-1650.832;Inherit;False;398;stopEmitting;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;428;915.855,-1223.047;Inherit;False;visibleParticles;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Compare;866;122.8829,-1286.145;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;445;280.2695,-1224.863;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0.0001;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;406;491.9828,-1313.418;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;427;465.468,-1033.062;Inherit;False;424;hidePos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;424;-1854.064,-1097.366;Inherit;False;hidePos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;893;-2120.403,-1004.333;Inherit;False;Constant;_Vector0;Vector 0;20;0;Create;True;0;0;0;False;0;False;0,-9999,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;550;-2311.332,2810.333;Inherit;False;gradientTexOLT;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TFHCRemapNode;531;-1751.976,3736.294;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;4;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;568;-2016.622,3786.056;Inherit;False;515;previousStartLifetime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;551;-2550.235,2810.279;Inherit;True;Property;_gradientTexOLT;gradientTexOLT;8;0;Create;True;0;0;0;False;0;False;None;None;False;black;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.IntNode;896;-2006.459,3861.603;Inherit;False;Constant;_GradientTexSize;GradientTexSize;20;0;Create;True;0;0;0;False;0;False;255;0;False;0;1;INT;0
Node;AmplifyShaderEditor.FunctionNode;897;-1702.459,3316.603;Inherit;False;TexLoad;-1;;27;24997ab8bd7822d44bede2b16c924318;0;3;2;INT;0;False;3;INT;0;False;4;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;544;-1966.914,3276.83;Inherit;False;533;lifetimeUVx;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;898;-1976.459,3360.603;Inherit;False;Constant;_TexelIndexY;TexelIndexY;20;0;Create;True;0;0;0;False;0;False;3;0;False;0;1;INT;0
Node;AmplifyShaderEditor.GetLocalVarNode;545;-1987.535,3465.36;Inherit;False;550;gradientTexOLT;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;533;-1511.99,3736.398;Inherit;False;lifetimeUVx;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;555;-1467.659,3311.404;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;618;-1486.823,3409.424;Inherit;False;Property;_velocityMaxMultiplier;_velocityMaxMultiplier;14;0;Create;True;0;0;0;False;0;False;0,0,0;1,1,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;619;-1160.823,3321.424;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;547;-913.3785,3328.243;Inherit;False;velMaxOLT;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;894;-1709.086,2758.294;Inherit;False;TexLoad;-1;;26;24997ab8bd7822d44bede2b16c924318;0;3;2;INT;0;False;3;INT;0;False;4;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.IntNode;901;-1961.459,2813.603;Inherit;False;Constant;_TexelIndexY1;TexelIndexY;20;0;Create;True;0;0;0;False;0;False;2;0;False;0;1;INT;0
Node;AmplifyShaderEditor.GetLocalVarNode;539;-1964.752,2728.42;Inherit;False;533;lifetimeUVx;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;541;-1981.37,2903.95;Inherit;False;550;gradientTexOLT;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.ComponentMaskNode;554;-1441.179,2759.189;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;616;-1459.343,2870.209;Inherit;False;Property;_velocityMinMultiplier;_velocityMinMultiplier;13;0;Create;True;0;0;0;False;0;False;0,0,0;1,1,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;617;-1115.343,2761.209;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;534;-844.2156,2764.833;Inherit;False;velMinOLT;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
WireConnection;65;0;3;1
WireConnection;65;1;3;2
WireConnection;65;2;3;3
WireConnection;212;0;370;0
WireConnection;213;0;371;0
WireConnection;211;0;372;0
WireConnection;370;7;176;0
WireConnection;370;11;219;2
WireConnection;370;13;615;0
WireConnection;370;12;220;2
WireConnection;75;0;66;0
WireConnection;75;1;70;0
WireConnection;144;0;46;0
WireConnection;144;1;145;0
WireConnection;146;0;144;0
WireConnection;146;1;848;0
WireConnection;392;0;390;0
WireConnection;393;0;394;0
WireConnection;393;1;395;0
WireConnection;393;2;392;0
WireConnection;393;3;425;0
WireConnection;452;0;451;0
WireConnection;452;1;451;0
WireConnection;465;0;466;0
WireConnection;466;0;467;0
WireConnection;466;1;476;0
WireConnection;467;0;477;0
WireConnection;467;1;476;0
WireConnection;471;0;473;0
WireConnection;471;1;454;3
WireConnection;472;0;477;0
WireConnection;473;0;472;0
WireConnection;475;0;478;0
WireConnection;475;3;474;0
WireConnection;477;0;453;0
WireConnection;477;1;475;0
WireConnection;454;1;464;0
WireConnection;464;0;465;0
WireConnection;464;1;468;0
WireConnection;468;0;465;1
WireConnection;460;0;463;0
WireConnection;460;1;289;1
WireConnection;463;0;299;0
WireConnection;285;0;374;0
WireConnection;285;3;286;0
WireConnection;289;1;867;0
WireConnection;462;0;455;0
WireConnection;371;7;362;0
WireConnection;371;11;219;3
WireConnection;371;13;610;0
WireConnection;371;12;220;3
WireConnection;372;7;176;0
WireConnection;372;11;219;1
WireConnection;372;13;609;0
WireConnection;372;12;220;1
WireConnection;3;0;8;0
WireConnection;87;0;92;0
WireConnection;87;1;824;1
WireConnection;180;0;181;0
WireConnection;180;1;200;0
WireConnection;180;2;182;0
WireConnection;182;0;179;2
WireConnection;182;1;218;0
WireConnection;181;0;216;0
WireConnection;181;1;179;0
WireConnection;200;0;179;1
WireConnection;200;1;217;0
WireConnection;362;0;176;0
WireConnection;179;0;693;0
WireConnection;60;0;605;0
WireConnection;60;1;606;0
WireConnection;60;2;613;0
WireConnection;562;0;607;0
WireConnection;562;1;608;0
WireConnection;562;2;614;0
WireConnection;515;0;677;1
WireConnection;90;0;677;0
WireConnection;86;0;604;0
WireConnection;86;3;115;1
WireConnection;86;4;115;2
WireConnection;572;0;603;0
WireConnection;572;3;574;1
WireConnection;572;4;574;2
WireConnection;599;0;602;0
WireConnection;599;1;598;2
WireConnection;587;0;589;0
WireConnection;587;7;589;1
WireConnection;593;0;589;0
WireConnection;593;1;599;0
WireConnection;593;7;589;1
WireConnection;595;0;597;0
WireConnection;602;0;595;0
WireConnection;602;1;598;1
WireConnection;563;0;562;0
WireConnection;563;3;559;0
WireConnection;563;4;558;0
WireConnection;64;0;63;0
WireConnection;64;1;124;0
WireConnection;70;0;147;0
WireConnection;70;1;578;0
WireConnection;70;2;147;1
WireConnection;70;3;74;0
WireConnection;377;0;87;0
WireConnection;514;0;522;0
WireConnection;514;1;516;0
WireConnection;514;2;517;0
WireConnection;514;3;518;0
WireConnection;512;0;678;0
WireConnection;83;0;91;0
WireConnection;83;1;82;0
WireConnection;83;2;119;0
WireConnection;83;3;127;0
WireConnection;0;0;83;0
WireConnection;678;1;513;0
WireConnection;678;2;514;0
WireConnection;123;0;889;4
WireConnection;63;0;180;0
WireConnection;63;1;824;1
WireConnection;789;0;795;0
WireConnection;789;1;790;0
WireConnection;827;0;889;3
WireConnection;94;0;887;6
WireConnection;94;3;519;0
WireConnection;126;0;94;0
WireConnection;820;0;819;0
WireConnection;820;1;821;0
WireConnection;822;0;789;0
WireConnection;825;0;824;1
WireConnection;825;1;826;0
WireConnection;825;2;828;0
WireConnection;825;3;823;0
WireConnection;823;0;822;0
WireConnection;823;1;820;0
WireConnection;819;0;789;0
WireConnection;323;0;462;0
WireConnection;323;1;462;1
WireConnection;323;2;335;0
WireConnection;323;3;378;0
WireConnection;455;1;471;0
WireConnection;455;0;460;0
WireConnection;481;1;378;0
WireConnection;481;0;323;0
WireConnection;566;0;563;0
WireConnection;566;1;567;0
WireConnection;556;1;58;0
WireConnection;556;0;566;0
WireConnection;375;0;829;0
WireConnection;829;0;481;0
WireConnection;829;1;378;0
WireConnection;829;2;830;0
WireConnection;715;3;74;0
WireConnection;714;0;66;0
WireConnection;714;1;715;0
WireConnection;385;0;383;0
WireConnection;385;1;386;0
WireConnection;385;2;429;0
WireConnection;385;3;399;0
WireConnection;96;0;888;6
WireConnection;96;3;679;0
WireConnection;716;0;719;0
WireConnection;719;0;714;0
WireConnection;413;0;95;0
WireConnection;95;0;75;0
WireConnection;373;0;776;0
WireConnection;776;1;779;0
WireConnection;776;0;783;0
WireConnection;58;0;60;0
WireConnection;58;3;56;0
WireConnection;58;4;57;0
WireConnection;751;0;718;0
WireConnection;147;0;146;0
WireConnection;721;0;849;0
WireConnection;853;0;852;0
WireConnection;752;0;751;0
WireConnection;752;1;851;0
WireConnection;752;2;751;2
WireConnection;854;0;855;0
WireConnection;854;1;853;0
WireConnection;851;0;751;1
WireConnection;851;1;854;0
WireConnection;848;0;849;0
WireConnection;856;0;766;0
WireConnection;856;1;757;2
WireConnection;756;0;759;0
WireConnection;756;1;857;0
WireConnection;756;2;856;0
WireConnection;759;0;766;0
WireConnection;759;1;757;0
WireConnection;749;0;748;0
WireConnection;749;1;750;0
WireConnection;748;0;717;0
WireConnection;748;1;727;0
WireConnection;728;0;727;0
WireConnection;728;1;752;0
WireConnection;747;0;749;0
WireConnection;747;1;728;0
WireConnection;727;0;717;0
WireConnection;717;0;778;0
WireConnection;717;1;752;0
WireConnection;732;0;717;0
WireConnection;757;0;722;0
WireConnection;857;0;757;1
WireConnection;857;1;858;0
WireConnection;683;0;385;0
WireConnection;118;0;96;0
WireConnection;693;0;556;0
WireConnection;777;0;64;0
WireConnection;570;0;572;0
WireConnection;401;0;406;0
WireConnection;401;1;410;0
WireConnection;401;2;414;0
WireConnection;401;3;427;0
WireConnection;861;0;402;0
WireConnection;861;1;864;0
WireConnection;862;0;861;0
WireConnection;862;1;864;0
WireConnection;859;0;865;0
WireConnection;859;1;862;0
WireConnection;99;0;86;0
WireConnection;591;0;587;1
WireConnection;592;0;587;2
WireConnection;612;0;593;3
WireConnection;600;0;593;1
WireConnection;601;0;593;2
WireConnection;611;0;587;3
WireConnection;291;0;292;0
WireConnection;291;1;290;0
WireConnection;292;0;293;0
WireConnection;293;0;300;0
WireConnection;293;1;301;0
WireConnection;300;0;287;0
WireConnection;300;1;301;0
WireConnection;290;0;292;1
WireConnection;287;0;288;0
WireConnection;287;1;285;0
WireConnection;299;0;287;0
WireConnection;867;0;291;0
WireConnection;867;1;868;0
WireConnection;711;0;732;0
WireConnection;711;1;756;0
WireConnection;711;2;747;0
WireConnection;711;3;780;0
WireConnection;869;0;870;0
WireConnection;869;2;873;0
WireConnection;869;3;711;0
WireConnection;679;1;101;0
WireConnection;679;2;101;0
WireConnection;677;2;3;4
WireConnection;782;0;869;0
WireConnection;873;0;872;0
WireConnection;873;1;780;0
WireConnection;887;2;373;0
WireConnection;887;4;890;0
WireConnection;888;2;683;0
WireConnection;888;4;792;0
WireConnection;889;1;65;0
WireConnection;890;0;825;0
WireConnection;398;0;393;0
WireConnection;428;0;401;0
WireConnection;866;0;402;0
WireConnection;866;2;859;0
WireConnection;445;0;866;0
WireConnection;406;0;669;0
WireConnection;406;1;445;0
WireConnection;424;0;893;0
WireConnection;550;0;551;0
WireConnection;531;0;569;0
WireConnection;531;2;568;0
WireConnection;531;4;896;0
WireConnection;897;2;544;0
WireConnection;897;3;898;0
WireConnection;897;4;545;0
WireConnection;533;0;531;0
WireConnection;555;0;897;0
WireConnection;619;0;555;0
WireConnection;619;1;618;0
WireConnection;547;0;619;0
WireConnection;894;2;539;0
WireConnection;894;3;901;0
WireConnection;894;4;541;0
WireConnection;554;0;894;0
WireConnection;617;0;554;0
WireConnection;617;1;616;0
WireConnection;534;0;617;0
ASEEND*/
//CHKSM=CE9881E8D440843AA5B466FBE49BB64857B331EA