// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NL_PointLightHalo"
{
	Properties
	{
		_color("_color", Color) = (1,1,1,1)
		_intensity("_intensity", Float) = 1
		_intersectionsDepthFade("_intersectionsDepthFade", Float) = 3
		_cameraFadeDistance("_cameraFadeDistance", Float) = 20
		_noiseSpeed("_noiseSpeed", Vector) = (0.01,0.01,0,0)
		_noise("noise", 2D) = "white" {}
		_haloSize("_haloSize", Float) = 1
		_noiseTiling("_noiseTiling", Float) = 1
		_maskHardness("_maskHardness", Range( 0 , 1)) = 0.01
		_zOffset("_zOffset", Float) = 0.2
		_noiseIntensity("_noiseIntensity", Range( 0 , 1)) = 0.5
		_randomValue("_randomValue", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+1" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap vertex:vertexDataFunc 
		struct Input
		{
			float4 screenPos;
			float eyeDepth;
			float2 uv_texcoord;
		};

		uniform float _zOffset;
		uniform float3 _viewCamRight;
		uniform float _haloSize;
		uniform float3 _viewCamUp;
		uniform float4 _color;
		uniform float _intensity;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _intersectionsDepthFade;
		uniform float _cameraFadeDistance;
		uniform sampler2D _noise;
		uniform float _noiseTiling;
		uniform float2 _noiseSpeed;
		uniform float _randomValue;
		uniform float _noiseIntensity;
		uniform float _maskHardness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float zOffset468 = _zOffset;
			float3 objToWorld435 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float3 pivotWS463 = objToWorld435;
			float3 normalizeResult445 = normalize( ( _WorldSpaceCameraPos - pivotWS463 ) );
			float2 temp_cast_0 = (1.0).xx;
			float temp_output_457_0 = distance( pivotWS463 , _WorldSpaceCameraPos );
			float haloSize480 = ( _haloSize * ( ( temp_output_457_0 - zOffset468 ) / temp_output_457_0 ) );
			float2 break426 = ( ( ( v.texcoord.xy * 2.0 ) - temp_cast_0 ) * haloSize480 );
			float3 worldToObj434 = mul( unity_WorldToObject, float4( ( ( zOffset468 * normalizeResult445 ) + ( pivotWS463 + ( _viewCamRight * break426.x ) + ( break426.y * _viewCamUp ) ) ), 1 ) ).xyz;
			float3 billboardSpherical413 = worldToObj434;
			v.vertex.xyz = billboardSpherical413;
			v.vertex.w = 1;
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth186 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth186 = saturate( abs( ( screenDepth186 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _intersectionsDepthFade ) ) );
			float temp_output_497_0 = abs( _cameraFadeDistance );
			float cameraDepthFade494 = (( i.eyeDepth -_ProjectionParams.y - 0.0 ) / temp_output_497_0);
			float cameraDepthFade493 = (( i.eyeDepth -_ProjectionParams.y - 0.0 ) / temp_output_497_0);
			float3 objToWorld435 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float3 pivotWS463 = objToWorld435;
			float temp_output_457_0 = distance( pivotWS463 , _WorldSpaceCameraPos );
			float zOffset468 = _zOffset;
			float haloSize480 = ( _haloSize * ( ( temp_output_457_0 - zOffset468 ) / temp_output_457_0 ) );
			float temp_output_356_0 = ( _noiseTiling * haloSize480 * 0.06 );
			float2 temp_cast_0 = (0.5).xx;
			float2 temp_output_479_0 = ( i.uv_texcoord - temp_cast_0 );
			float2 temp_output_337_0 = ( _noiseSpeed * _Time.y );
			float2 temp_cast_1 = (0.5).xx;
			float lerpResult489 = lerp( 1.0 , ( tex2D( _noise, ( ( temp_output_356_0 * temp_output_479_0 ) + frac( temp_output_337_0 ) + _randomValue ) ).r * tex2D( _noise, ( ( temp_output_479_0 * ( temp_output_356_0 * 0.5 ) ) + frac( ( temp_output_337_0 * -0.3 ) ) + _randomValue ) ).r ) , _noiseIntensity);
			float2 temp_cast_2 = (0.5).xx;
			float2 temp_output_382_0 = ( ( i.uv_texcoord - temp_cast_2 ) * 2.0 );
			float2 temp_cast_3 = (0.5).xx;
			float dotResult374 = dot( temp_output_382_0 , temp_output_382_0 );
			float opacity248 = saturate( ( _intensity * 0.5 * distanceDepth186 * saturate( ( _cameraFadeDistance >= 0.0 ? cameraDepthFade494 : ( 1.0 - cameraDepthFade493 ) ) ) * lerpResult489 * saturate( ( 1.0 - pow( dotResult374 , _maskHardness ) ) ) ) );
			float3 linearToGamma181 = LinearToGammaSpace( ( _color * max( opacity248 , 1.0 ) * _intensity ).rgb );
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
Node;AmplifyShaderEditor.CommentaryNode;251;-157.3897,-243.6471;Inherit;False;1558.649;482.154;;7;31;252;181;245;243;179;32;Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;247;-2852.133,-1544.89;Inherit;False;2656.864;2095.161;;52;248;215;187;183;186;182;34;256;322;321;347;334;346;349;338;339;340;337;333;336;348;330;331;356;357;343;355;377;378;374;381;372;382;383;384;386;387;477;479;482;485;489;490;491;492;493;494;495;496;497;498;500;Opacity;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;283.6025,7.816541;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;179;92.91473,19.71973;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;243;-85.85477,87.71444;Inherit;False;Constant;_Float2;Float 2;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;245;-107.3897,6.197984;Inherit;False;248;opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;256;-1845.824,-230.7147;Inherit;True;Property;_noise_a;noise_a;15;0;Create;True;0;0;0;False;0;False;-1;None;28715250e68caeb4d9f1b861866f0f99;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;322;-1848.991,-8.701195;Inherit;True;Property;_noise_b;noise_b;15;0;Create;True;0;0;0;False;0;False;-1;None;28715250e68caeb4d9f1b861866f0f99;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;321;-2119.681,-114.6032;Inherit;True;Property;_noise;noise;5;0;Create;True;0;0;0;False;0;False;None;28715250e68caeb4d9f1b861866f0f99;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;347;-2031.587,-251.1983;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FractNode;334;-2188.662,-197.5183;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;349;-2003.46,81.98766;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;343;-1534.798,-97.10442;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;182;-867.9831,-841.1964;Inherit;True;6;6;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;382;-2278.84,-1212.07;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;372;-2464.383,-1261.781;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;378;-2678.413,-1192.417;Inherit;False;Constant;_Float6;Float 6;16;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;377;-2731.719,-1341.441;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;383;-2465.84,-1122.07;Inherit;False;Constant;_Float7;Float 7;16;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;374;-2026.384,-1236.781;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;386;-1798.84,-1237.07;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;381;-1634.738,-1238.377;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;384;-1457.84,-1190.07;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2801.202,425.9969;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;NOT_Lonely/NL_PointLightHalo;False;False;False;False;True;True;True;True;True;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;False;1;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;False;2;5;False;;10;False;;0;5;False;;1;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Absolute;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.GetLocalVarNode;427;2567.149,722.099;Inherit;False;413;billboardSpherical;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;249;2607.571,613.4659;Inherit;False;248;opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;254;2597.153,476.1633;Inherit;False;252;color;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;356;-2601.995,-117.6489;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;407;-1894.503,1296.45;Inherit;False;Global;_viewCamUp;_viewCamUp;11;0;Create;True;0;0;0;False;0;False;0,0,0;0.1274884,0.9771377,-0.1701431;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.BreakToComponentsNode;426;-1850.946,1155.224;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;417;-2405.468,1283.272;Inherit;False;Constant;_Float9;Float 5;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;418;-2356.98,1152.534;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;419;-2198.979,1158.534;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;420;-2039.994,1157.122;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;415;-2590.974,1077.636;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;412;-1418.994,1124.137;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;411;-1652.993,1227.529;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;410;-1653.878,1092.074;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;414;-1905.642,988.6226;Inherit;False;Global;_viewCamRight;_viewCamRight;11;0;Create;True;0;0;0;False;0;False;0,0,0;0.8092907,-0.003308911,0.5873991;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;443;-1669.43,743.6419;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;444;-1375.049,940.1437;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;445;-1220.585,943.9057;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;446;-1057.736,918.3368;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;447;-832.5427,1096.735;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformPositionNode;434;-680.0296,1091.202;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;413;-454.2015,1090.951;Inherit;False;billboardSpherical;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;469;-1232.067,862.2567;Inherit;False;468;zOffset;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;416;-2548.803,1216.611;Inherit;False;Constant;_Float8;Float 4;5;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;435;-2166.572,828.9458;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;468;-1942.664,729.289;Inherit;False;zOffset;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;463;-1932.124,829.8717;Inherit;False;pivotWS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;464;-1638.685,947.7107;Inherit;False;463;pivotWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;461;-2678.685,1608.952;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;457;-2893.774,1682.015;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;460;-2514.07,1662.917;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;470;-2895.165,1580.442;Inherit;False;468;zOffset;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;458;-3168.308,1732.401;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;465;-3114.629,1619.462;Inherit;False;463;pivotWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;481;-2365.35,1415.398;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;480;-2227.254,1410.03;Inherit;False;haloSize;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;424;-2579.797,1372.698;Inherit;False;Property;_haloSize;_haloSize;6;0;Create;True;0;0;0;False;0;False;1;20.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;482;-2815.322,-64.33813;Inherit;False;480;haloSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;330;-2596.163,167.2276;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;331;-2802.259,167.8275;Inherit;False;Constant;_Float3;Float 3;18;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;339;-2430.76,276.6114;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;340;-2620.658,409.1112;Inherit;False;Constant;_Float4;Float 4;18;0;Create;True;0;0;0;False;0;False;-0.3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;337;-2602.66,275.8442;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;333;-2817.656,380.8441;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;338;-2166.263,221.0115;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;348;-2173.259,81.78754;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;346;-2197.359,-306.125;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;479;-2353.336,-35.60974;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;485;-2523.411,38.58746;Inherit;False;Constant;_Float1;Float 1;15;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;477;-2641.381,-269.3699;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;357;-2817.994,31.3512;Inherit;False;Constant;_Float5;Float 5;15;0;Create;True;0;0;0;False;0;False;0.06;0.03;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-1063.313,-959.6521;Inherit;False;Property;_intensity;_intensity;1;0;Create;True;0;0;0;False;0;False;1;16.07;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;425;-2626.096,1459.08;Inherit;False;Property;_haloSizeMultiplier;_haloSizeMultiplier;7;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;183;-1058.984,-849.1966;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;492;-604.3644,-836.4093;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;248;-441.715,-838.5024;Inherit;False;opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;489;-1194.961,-284.0774;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;490;-1359.961,-288.0774;Inherit;False;Constant;_Float11;Float 11;13;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;491;-1505.436,-213.1045;Inherit;False;Property;_noiseIntensity;_noiseIntensity;11;0;Create;True;0;0;0;False;0;False;0.5;0.24;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;336;-2807.656,242.8444;Inherit;False;Property;_noiseSpeed;_noiseSpeed;4;0;Create;True;0;0;0;False;0;False;0.01,0.01;0.09,-0.01;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;355;-2814.712,-148.1873;Inherit;False;Property;_noiseTiling;_noiseTiling;8;0;Create;True;0;0;0;False;0;False;1;1.03;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;394;-2134.076,728.238;Inherit;False;Property;_zOffset;_zOffset;10;0;Create;True;0;0;0;False;0;False;0.2;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;387;-2127.84,-983.0701;Inherit;False;Property;_maskHardness;_maskHardness;9;0;Create;True;0;0;0;False;0;False;0.01;0.01;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CameraDepthFade;493;-1767.342,-407.6748;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CameraDepthFade;494;-1768.144,-528.5702;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;496;-1510.482,-409.5548;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;497;-1902.143,-454.5699;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;498;-1168.218,-599.733;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;215;-2142.013,-610.6079;Inherit;False;Property;_cameraFadeDistance;_cameraFadeDistance;3;0;Create;True;0;0;0;False;0;False;20;10.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;186;-1177.869,-751.0172;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;187;-1461.007,-730.5921;Inherit;False;Property;_intersectionsDepthFade;_intersectionsDepthFade;2;0;Create;True;0;0;0;False;0;False;3;1.96;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;500;-2454.835,-367.0875;Inherit;False;Property;_randomValue;_randomValue;12;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;495;-1337.283,-615.7871;Inherit;False;3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;31;28.17856,-193.647;Inherit;False;Property;_color;_color;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,0.945098,0.7882353,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LinearToGammaNode;181;732.2031,1.662629;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;252;966.5715,-2.234245;Inherit;False;color;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
WireConnection;32;0;31;0
WireConnection;32;1;179;0
WireConnection;32;2;34;0
WireConnection;179;0;245;0
WireConnection;179;1;243;0
WireConnection;256;0;321;0
WireConnection;256;1;347;0
WireConnection;256;7;321;1
WireConnection;322;0;321;0
WireConnection;322;1;349;0
WireConnection;322;7;321;1
WireConnection;347;0;346;0
WireConnection;347;1;334;0
WireConnection;347;2;500;0
WireConnection;334;0;337;0
WireConnection;349;0;348;0
WireConnection;349;1;338;0
WireConnection;349;2;500;0
WireConnection;343;0;256;1
WireConnection;343;1;322;1
WireConnection;182;0;34;0
WireConnection;182;1;183;0
WireConnection;182;2;186;0
WireConnection;182;3;498;0
WireConnection;182;4;489;0
WireConnection;182;5;384;0
WireConnection;382;0;372;0
WireConnection;382;1;383;0
WireConnection;372;0;377;0
WireConnection;372;1;378;0
WireConnection;374;0;382;0
WireConnection;374;1;382;0
WireConnection;386;0;374;0
WireConnection;386;1;387;0
WireConnection;381;0;386;0
WireConnection;384;0;381;0
WireConnection;0;2;254;0
WireConnection;0;9;249;0
WireConnection;0;11;427;0
WireConnection;356;0;355;0
WireConnection;356;1;482;0
WireConnection;356;2;357;0
WireConnection;426;0;420;0
WireConnection;418;0;415;0
WireConnection;418;1;416;0
WireConnection;419;0;418;0
WireConnection;419;1;417;0
WireConnection;420;0;419;0
WireConnection;420;1;480;0
WireConnection;412;0;464;0
WireConnection;412;1;410;0
WireConnection;412;2;411;0
WireConnection;411;0;426;1
WireConnection;411;1;407;0
WireConnection;410;0;414;0
WireConnection;410;1;426;0
WireConnection;444;0;443;0
WireConnection;444;1;464;0
WireConnection;445;0;444;0
WireConnection;446;0;469;0
WireConnection;446;1;445;0
WireConnection;447;0;446;0
WireConnection;447;1;412;0
WireConnection;434;0;447;0
WireConnection;413;0;434;0
WireConnection;468;0;394;0
WireConnection;463;0;435;0
WireConnection;461;0;457;0
WireConnection;461;1;470;0
WireConnection;457;0;465;0
WireConnection;457;1;458;0
WireConnection;460;0;461;0
WireConnection;460;1;457;0
WireConnection;481;0;424;0
WireConnection;481;1;460;0
WireConnection;480;0;481;0
WireConnection;330;0;356;0
WireConnection;330;1;331;0
WireConnection;339;0;337;0
WireConnection;339;1;340;0
WireConnection;337;0;336;0
WireConnection;337;1;333;0
WireConnection;338;0;339;0
WireConnection;348;0;479;0
WireConnection;348;1;330;0
WireConnection;346;0;356;0
WireConnection;346;1;479;0
WireConnection;479;0;477;0
WireConnection;479;1;485;0
WireConnection;492;0;182;0
WireConnection;248;0;492;0
WireConnection;489;0;490;0
WireConnection;489;1;343;0
WireConnection;489;2;491;0
WireConnection;493;0;497;0
WireConnection;494;0;497;0
WireConnection;496;0;493;0
WireConnection;497;0;215;0
WireConnection;498;0;495;0
WireConnection;186;0;187;0
WireConnection;495;0;215;0
WireConnection;495;2;494;0
WireConnection;495;3;496;0
WireConnection;181;0;32;0
WireConnection;252;0;181;0
ASEEND*/
//CHKSM=7DAB23579D53B1465BE76A3A83F582B2EBE52B0F