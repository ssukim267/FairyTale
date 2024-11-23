// Upgrade NOTE: upgraded instancing buffer 'NOT_LonelyNL_SpotLightBeam' to new syntax.

// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NL_SpotLightBeam"
{
	Properties
	{
		[HideInInspector]_beamDir("_beamDir", Vector) = (0,0,0,0)
		[HideInInspector]_startRadius("_startRadius", Float) = 0.2
		[HideInInspector]_endRadius("_endRadius", Float) = 1
		[HideInInspector]_length("_length", Float) = 1
		[HideInInspector]_rangeMultiplier("_rangeMultiplier", Float) = 1
		[HideInInspector]_color("_color", Color) = (1,1,1,1)
		[HideInInspector]_intensity("_intensity", Float) = 1
		[HideInInspector]_intersectionsDepthFade("_intersectionsDepthFade", Float) = 3
		[HideInInspector]_cameraFadeDistance("_cameraFadeDistance", Float) = 20
		[HideInInspector]_noiseIntensity("_noiseIntensity", Range( 0 , 1)) = 0.7558382
		[HideInInspector]_spotAngle("_spotAngle", Float) = 0
		[HideInInspector]_noiseSpeed("_noiseSpeed", Vector) = (1,1,0,0)
		[HideInInspector]_noise("noise", 2D) = "white" {}
		[HideInInspector]_noiseTiling("_noiseTiling", Float) = 1
		[HideInInspector]_maskHardness("_maskHardness", Float) = 0.01
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.5
		#pragma multi_compile_instancing
		#pragma surface surf Unlit alpha:fade keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float eyeDepth;
			float3 worldPos;
			float3 worldNormal;
			half ASEIsFrontFacing : VFACE;
			INTERNAL_DATA
		};

		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform sampler2D _noise;

		UNITY_INSTANCING_BUFFER_START(NOT_LonelyNL_SpotLightBeam)
			UNITY_DEFINE_INSTANCED_PROP(float4, _color)
#define _color_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float3, _beamDir)
#define _beamDir_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float2, _noiseSpeed)
#define _noiseSpeed_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float, _startRadius)
#define _startRadius_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float, _endRadius)
#define _endRadius_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float, _rangeMultiplier)
#define _rangeMultiplier_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float, _length)
#define _length_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float, _intensity)
#define _intensity_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float, _maskHardness)
#define _maskHardness_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float, _intersectionsDepthFade)
#define _intersectionsDepthFade_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float, _cameraFadeDistance)
#define _cameraFadeDistance_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float, _noiseTiling)
#define _noiseTiling_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float, _noiseIntensity)
#define _noiseIntensity_arr NOT_LonelyNL_SpotLightBeam
			UNITY_DEFINE_INSTANCED_PROP(float, _spotAngle)
#define _spotAngle_arr NOT_LonelyNL_SpotLightBeam
		UNITY_INSTANCING_BUFFER_END(NOT_LonelyNL_SpotLightBeam)

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float _startRadius_Instance = UNITY_ACCESS_INSTANCED_PROP(_startRadius_arr, _startRadius);
			float _endRadius_Instance = UNITY_ACCESS_INSTANCED_PROP(_endRadius_arr, _endRadius);
			float _rangeMultiplier_Instance = UNITY_ACCESS_INSTANCED_PROP(_rangeMultiplier_arr, _rangeMultiplier);
			float3 ase_vertexNormal = v.normal.xyz;
			float3 _beamDir_Instance = UNITY_ACCESS_INSTANCED_PROP(_beamDir_arr, _beamDir);
			float3 worldToObjDir18 = mul( unity_WorldToObject, float4( _beamDir_Instance, 0 ) ).xyz;
			float3 beamDirLocal241 = worldToObjDir18;
			float _length_Instance = UNITY_ACCESS_INSTANCED_PROP(_length_arr, _length);
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 vPos202 = ( ( ( (_startRadius_Instance + (v.texcoord.xy.y - 0.0) * (( _endRadius_Instance * _rangeMultiplier_Instance ) - _startRadius_Instance) / (1.0 - 0.0)) * ase_vertexNormal ) + ( v.texcoord.xy.y * beamDirLocal241 * ( _rangeMultiplier_Instance * _length_Instance ) ) ) + ase_vertex3Pos );
			v.vertex.xyz += vPos202;
			v.vertex.w = 1;
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float _intensity_Instance = UNITY_ACCESS_INSTANCED_PROP(_intensity_arr, _intensity);
			float4 _color_Instance = UNITY_ACCESS_INSTANCED_PROP(_color_arr, _color);
			float _maskHardness_Instance = UNITY_ACCESS_INSTANCED_PROP(_maskHardness_arr, _maskHardness);
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float _intersectionsDepthFade_Instance = UNITY_ACCESS_INSTANCED_PROP(_intersectionsDepthFade_arr, _intersectionsDepthFade);
			float screenDepth186 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth186 = saturate( abs( ( screenDepth186 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _intersectionsDepthFade_Instance ) ) );
			float _cameraFadeDistance_Instance = UNITY_ACCESS_INSTANCED_PROP(_cameraFadeDistance_arr, _cameraFadeDistance);
			float temp_output_373_0 = abs( _cameraFadeDistance_Instance );
			float cameraDepthFade372 = (( i.eyeDepth -_ProjectionParams.y - 0.0 ) / temp_output_373_0);
			float cameraDepthFade214 = (( i.eyeDepth -_ProjectionParams.y - 0.0 ) / temp_output_373_0);
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float2 appendResult345 = (float2(ase_vertex3Pos.x , ase_vertex3Pos.y));
			float _noiseTiling_Instance = UNITY_ACCESS_INSTANCED_PROP(_noiseTiling_arr, _noiseTiling);
			float temp_output_356_0 = ( 0.01 * _noiseTiling_Instance );
			float2 _noiseSpeed_Instance = UNITY_ACCESS_INSTANCED_PROP(_noiseSpeed_arr, _noiseSpeed);
			float2 temp_output_337_0 = ( _noiseSpeed_Instance * _Time.y );
			float _noiseIntensity_Instance = UNITY_ACCESS_INSTANCED_PROP(_noiseIntensity_arr, _noiseIntensity);
			float lerpResult365 = lerp( 1.0 , ( tex2D( _noise, ( ( appendResult345 * temp_output_356_0 ) + frac( temp_output_337_0 ) ) ).r * tex2D( _noise, ( ( appendResult345 * ( temp_output_356_0 * 0.5 ) ) + frac( ( temp_output_337_0 * -0.3 ) ) ) ).r ) , _noiseIntensity_Instance);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			ase_vertexNormal = normalize( ase_vertexNormal );
			float3 _beamDir_Instance = UNITY_ACCESS_INSTANCED_PROP(_beamDir_arr, _beamDir);
			float3 worldToObjDir18 = mul( unity_WorldToObject, float4( _beamDir_Instance, 0 ) ).xyz;
			float3 beamDirLocal241 = worldToObjDir18;
			float _spotAngle_Instance = UNITY_ACCESS_INSTANCED_PROP(_spotAngle_arr, _spotAngle);
			float3 lerpResult220 = lerp( ase_vertexNormal , -beamDirLocal241 , (0.0 + (_spotAngle_Instance - 0.0) * (1.0 - 0.0) / (179.0 - 0.0)));
			float3 objToWorldDir233 = mul( unity_ObjectToWorld, float4( ( lerpResult220 * (i.ASEIsFrontFacing > 0 ? +1 : -1 ) ), 0 ) ).xyz;
			float3 n160 = objToWorldDir233;
			float fresnelNdotV47 = dot( n160, ase_worldViewDir );
			float fresnelNode47 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV47, 1.0 ) );
			float opacity248 = saturate( ( ( _intensity_Instance * 0.5 * ( 1.0 - pow( i.uv_texcoord.y , _maskHardness_Instance ) ) * distanceDepth186 * saturate( ( _cameraFadeDistance_Instance >= 0.0 ? cameraDepthFade372 : ( 1.0 - cameraDepthFade214 ) ) ) * lerpResult365 ) * ( 1.0 - fresnelNode47 ) ) );
			float3 linearToGamma181 = LinearToGammaSpace( ( _intensity_Instance * ( _color_Instance * max( opacity248 , 1.0 ) ) ).rgb );
			float3 color252 = linearToGamma181;
			o.Emission = color252;
			o.Alpha = opacity248;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.CommentaryNode;255;-2513.198,128.9834;Inherit;False;1316.357;533.9451;;10;221;220;231;235;233;160;226;242;227;230;Normals;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;251;-2526.43,-416.8221;Inherit;False;1485.467;504.059;;8;364;31;252;181;243;179;32;377;Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;250;-2552.417,-2065.626;Inherit;False;2912.591;1544.475;;49;187;369;215;368;184;185;218;214;213;186;34;183;182;367;336;355;321;366;365;343;281;345;357;356;331;330;348;333;337;340;339;338;349;346;334;347;322;256;161;50;48;51;176;46;47;248;371;372;373;Fade Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;206;-2500.135,810.6855;Inherit;False;1582.545;896.6393;;18;16;26;17;10;9;370;1;11;241;18;4;202;2;3;27;15;23;6;Vertex Pos;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-1651.145,964.285;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-1456.135,978.584;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-1717.134,1292.585;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-2117.134,1536.584;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;3;-1284.945,982.485;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;2;-1491.3,1130.375;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;202;-1126.507,981.3419;Inherit;False;vPos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalVertexDataNode;4;-1849.144,1046.286;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;18;-2211.334,1303.785;Inherit;False;World;Object;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;241;-1976.041,1309.282;Inherit;False;beamDirLocal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-2085.436,-165.3586;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;179;-2276.125,-153.4554;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;243;-2454.895,-85.46077;Inherit;False;Constant;_Float2;Float 2;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;221;-2306.359,178.9834;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;220;-2007.357,253.9834;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;235;-1820.578,248.039;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformDirectionNode;233;-1643.466,247.4544;Inherit;False;Object;World;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NegateNode;226;-2209.78,356.9987;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;242;-2423.239,352.8729;Inherit;False;241;beamDirLocal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;230;-2262.198,455.928;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;179;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;11;-1864.044,876.2855;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-2449.547,914.886;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;370;-2085.533,1117.725;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;256;-1539.078,-1291.697;Inherit;True;Property;_noise_a;noise_a;15;0;Create;True;0;0;0;False;0;False;-1;None;28715250e68caeb4d9f1b861866f0f99;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;322;-1542.245,-1069.684;Inherit;True;Property;_noise_b;noise_b;15;0;Create;True;0;0;0;False;0;False;-1;None;28715250e68caeb4d9f1b861866f0f99;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;347;-1724.842,-1312.181;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FractNode;334;-1881.917,-1258.501;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;346;-1891.615,-1363.108;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;349;-1696.714,-978.9965;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FractNode;338;-1823.519,-903.9725;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;339;-1990.118,-867.5725;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;340;-2180.019,-735.0724;Inherit;False;Constant;_Float4;Float 4;18;0;Create;True;0;0;0;False;0;False;-0.3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;337;-2162.02,-868.3397;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;333;-2377.02,-763.3397;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;348;-1948.615,-1007.396;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;330;-2155.524,-976.9565;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;331;-2361.623,-976.3566;Inherit;False;Constant;_Float3;Float 3;18;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;356;-2340.26,-1152.631;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;345;-2148.27,-1286.425;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PosVertexDataNode;281;-2352.323,-1311.628;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;343;-1228.052,-1158.087;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;365;-919.6873,-1329.353;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;366;-1084.687,-1333.353;Inherit;False;Constant;_Float11;Float 11;13;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;182;-655.9155,-1571.746;Inherit;False;6;6;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;161;-1019.403,-1139.05;Inherit;False;160;n;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-1009.717,-1053.927;Inherit;False;Property;_fresnelBias;_fresnelBias;7;0;Create;True;0;0;0;False;0;False;0.35;0.24;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;48;-580.0771,-1116.028;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-1017.717,-950.928;Inherit;False;Property;_fresnelPower;_fresnelPower;8;0;Create;True;0;0;0;False;0;False;2;0.43;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;176;-358.6064,-1168.447;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;46;-204.0865,-1167.848;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;47;-811.7183,-1102.927;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;248;-35.57867,-1170.712;Inherit;False;opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;321;-1812.937,-1175.586;Inherit;True;Property;_noise;noise;14;1;[HideInInspector];Create;True;0;0;0;False;0;False;None;28715250e68caeb4d9f1b861866f0f99;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.OneMinusNode;185;-1164.127,-1969.848;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;184;-1648.135,-2024.767;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;368;-1354.825,-1978.995;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;183;-969.9379,-1947.081;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CameraDepthFade;214;-2109.007,-1539.975;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CameraDepthFade;372;-2109.808,-1660.87;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;218;-1852.144,-1541.855;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;373;-2243.808,-1586.87;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;213;-1509.88,-1732.033;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;371;-1678.945,-1748.087;Inherit;False;3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;249;1029.77,-297.2679;Inherit;False;248;opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;203;1144.013,-117.8274;Inherit;False;202;vPos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TwoSidedSign;231;-2000.931,381.9774;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;160;-1420.839,251.326;Inherit;False;n;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DepthFade;186;-1122.422,-1853.703;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;215;-2493.007,-1747.975;Inherit;False;InstancedProperty;_cameraFadeDistance;_cameraFadeDistance;10;1;[HideInInspector];Create;True;0;0;0;False;0;False;20;10.09;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;369;-1580.823,-1901.995;Inherit;False;InstancedProperty;_maskHardness;_maskHardness;16;1;[HideInInspector];Create;True;0;0;0;False;0;False;0.01;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;187;-1417.417,-1811.703;Inherit;False;InstancedProperty;_intersectionsDepthFade;_intersectionsDepthFade;9;1;[HideInInspector];Create;True;0;0;0;False;0;False;3;2.73;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;357;-2512.26,-1188.631;Inherit;False;Constant;_Float5;Float 5;18;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;355;-2507.978,-1108.17;Inherit;False;InstancedProperty;_noiseTiling;_noiseTiling;15;1;[HideInInspector];Create;True;0;0;0;False;0;False;1;1.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;336;-2367.02,-901.3397;Inherit;False;InstancedProperty;_noiseSpeed;_noiseSpeed;13;1;[HideInInspector];Create;True;0;0;0;False;0;False;1,1;0.1,-0.004;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;367;-1230.161,-1258.38;Inherit;False;InstancedProperty;_noiseIntensity;_noiseIntensity;11;1;[HideInInspector];Create;True;0;0;0;False;0;False;0.7558382;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;31;-2340.862,-366.8221;Inherit;False;InstancedProperty;_color;_color;5;1;[HideInInspector];Create;True;0;0;0;False;0;False;1,1,1,1;1,0.9469339,0.7877358,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;227;-2463.198,451.9279;Inherit;False;InstancedProperty;_spotAngle;_spotAngle;12;1;[HideInInspector];Create;True;0;0;0;False;0;False;0;77.85;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-2296.847,1113.886;Inherit;False;InstancedProperty;_endRadius;_endRadius;2;1;[HideInInspector];Create;True;0;0;0;False;0;False;1;16.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;17;-2390.335,1304.785;Inherit;False;InstancedProperty;_beamDir;_beamDir;0;1;[HideInInspector];Create;True;0;0;0;False;0;False;0,0,0;0,-1,-1.192093E-07;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;26;-2442.639,1509.883;Inherit;False;InstancedProperty;_rangeMultiplier;_rangeMultiplier;4;1;[HideInInspector];Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-2291.858,1610.884;Inherit;False;InstancedProperty;_length;_length;3;1;[HideInInspector];Create;True;0;0;0;False;0;False;1;20.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-2091.942,901.8851;Inherit;False;InstancedProperty;_startRadius;_startRadius;1;1;[HideInInspector];Create;True;0;0;0;False;0;False;0.2;0.23;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;376;1426.172,-361.2155;Float;False;True;-1;3;ASEMaterialInspector;0;0;Unlit;NOT_Lonely/NL_SpotLightBeam;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;False;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.GetLocalVarNode;254;1026.332,-414.5219;Inherit;False;252;color;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;364;-2482.142,-197.1841;Inherit;False;248;opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;252;-1469.301,-172.8122;Inherit;False;color;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-2106.267,-615.1611;Inherit;False;InstancedProperty;_intensity;_intensity;6;1;[HideInInspector];Create;True;0;0;0;False;0;False;1;1.04;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LinearToGammaNode;181;-1811.843,-177.4637;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;377;-1975.679,-370.2365;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
WireConnection;6;0;11;0
WireConnection;6;1;4;0
WireConnection;23;0;6;0
WireConnection;23;1;15;0
WireConnection;15;0;1;2
WireConnection;15;1;241;0
WireConnection;15;2;27;0
WireConnection;27;0;26;0
WireConnection;27;1;16;0
WireConnection;3;0;23;0
WireConnection;3;1;2;0
WireConnection;202;0;3;0
WireConnection;18;0;17;0
WireConnection;241;0;18;0
WireConnection;32;0;31;0
WireConnection;32;1;179;0
WireConnection;179;0;364;0
WireConnection;179;1;243;0
WireConnection;220;0;221;0
WireConnection;220;1;226;0
WireConnection;220;2;230;0
WireConnection;235;0;220;0
WireConnection;235;1;231;0
WireConnection;233;0;235;0
WireConnection;226;0;242;0
WireConnection;230;0;227;0
WireConnection;11;0;1;2
WireConnection;11;3;9;0
WireConnection;11;4;370;0
WireConnection;370;0;10;0
WireConnection;370;1;26;0
WireConnection;256;0;321;0
WireConnection;256;1;347;0
WireConnection;256;7;321;1
WireConnection;322;0;321;0
WireConnection;322;1;349;0
WireConnection;322;7;321;1
WireConnection;347;0;346;0
WireConnection;347;1;334;0
WireConnection;334;0;337;0
WireConnection;346;0;345;0
WireConnection;346;1;356;0
WireConnection;349;0;348;0
WireConnection;349;1;338;0
WireConnection;338;0;339;0
WireConnection;339;0;337;0
WireConnection;339;1;340;0
WireConnection;337;0;336;0
WireConnection;337;1;333;0
WireConnection;348;0;345;0
WireConnection;348;1;330;0
WireConnection;330;0;356;0
WireConnection;330;1;331;0
WireConnection;356;0;357;0
WireConnection;356;1;355;0
WireConnection;345;0;281;1
WireConnection;345;1;281;2
WireConnection;343;0;256;1
WireConnection;343;1;322;1
WireConnection;365;0;366;0
WireConnection;365;1;343;0
WireConnection;365;2;367;0
WireConnection;182;0;34;0
WireConnection;182;1;183;0
WireConnection;182;2;185;0
WireConnection;182;3;186;0
WireConnection;182;4;213;0
WireConnection;182;5;365;0
WireConnection;48;0;47;0
WireConnection;176;0;182;0
WireConnection;176;1;48;0
WireConnection;46;0;176;0
WireConnection;47;0;161;0
WireConnection;248;0;46;0
WireConnection;185;0;368;0
WireConnection;368;0;184;2
WireConnection;368;1;369;0
WireConnection;214;0;373;0
WireConnection;372;0;373;0
WireConnection;218;0;214;0
WireConnection;373;0;215;0
WireConnection;213;0;371;0
WireConnection;371;0;215;0
WireConnection;371;2;372;0
WireConnection;371;3;218;0
WireConnection;160;0;233;0
WireConnection;186;0;187;0
WireConnection;376;2;254;0
WireConnection;376;9;249;0
WireConnection;376;11;203;0
WireConnection;252;0;181;0
WireConnection;181;0;377;0
WireConnection;377;0;34;0
WireConnection;377;1;32;0
ASEEND*/
//CHKSM=87C192B6C79A3A956D046DE7E13F30EB733F29AE