// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NL_Skybox"
{
	Properties
	{
		[HDR]_Color("Sky Color", Color) = (0.2971698,0.6498447,1,1)
		[HDR]_GroundColor("Ground Color", Color) = (0.0471698,0.03082621,0.02781239,1)
		[Header(Fog)][Space]_FogHeight("Fog Height", Range( 0 , 1)) = 1
		_FogSmoothness("Fog Smoothness", Range( 0.01 , 1)) = 1
		_FogFill("Fog Fill", Range( 0 , 1)) = 0
		_GroundFog("Ground Fog", Range( 0 , 1)) = 1
		[Header(Sun)][Space]_SunDiskSize("Sun Disk Size", Range( 0 , 1)) = 0.038
		_SunDiskSharpness("Sun Disk Sharpness", Range( 0 , 1)) = 0.999
		_SunDiskIntensity("Sun Disk Intensity", Float) = 4
		[Header(Light Scattering)][Space]_ScatteringSize("Scattering Size", Range( 0 , 2)) = 1.2
		_ScatteringBrightness("Scattering Brightness", Float) = 5
		[Header(Clouds)][NoScaleOffset][SingleLineTexture][Space]_CloudsTexture("Clouds Texture", 2D) = "black" {}
		_CloudsIntensity("Clouds Intensity", Float) = 0.8
		_CloudsHardness("Clouds Hardness", Float) = 2
		_CloudsTilingandSpeed("Clouds Tiling and Speed", Vector) = (0.4,0.2,0,0.006)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Background"  "Queue" = "Background+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  "PreviewType"="Skybox" }
		Cull Off
		ZWrite Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 2.0
		#pragma surface surf Unlit keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		struct Input
		{
			float3 worldPos;
			float3 viewDir;
			float2 uv_texcoord;
		};

		uniform float4 _Color;
		uniform float _SunDiskSize;
		uniform float _SunDiskSharpness;
		uniform float _SunDiskIntensity;
		uniform float _ScatteringSize;
		uniform float _ScatteringBrightness;
		uniform float LightningRadius;
		uniform float3 LightningPosition;
		uniform float4 LightningColor;
		uniform float LightningIntensity;
		uniform sampler2D _CloudsTexture;
		uniform float4 _CloudsTilingandSpeed;
		uniform float _CloudsHardness;
		uniform float _CloudsIntensity;
		uniform half _FogHeight;
		uniform half _FogSmoothness;
		uniform half _FogFill;
		uniform float4 _GroundColor;
		uniform float _GroundFog;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult251 = dot( ase_worldlightDir , float3(0,1,0) );
			float temp_output_45_0_g61 = ( 1.0 - (0.0 + (_SunDiskSize - 0.0) * (0.05 - 0.0) / (1.0 - 0.0)) );
			float dotResult4_g61 = dot( -ase_worldlightDir , i.viewDir );
			float temp_output_6_0_g61 = saturate( dotResult4_g61 );
			float dotResult7_g61 = dot( temp_output_6_0_g61 , temp_output_6_0_g61 );
			float smoothstepResult9_g61 = smoothstep( temp_output_45_0_g61 , ( temp_output_45_0_g61 + ( 1.0 - (-0.999 + (_SunDiskSharpness - 0.0) * (1.0 - -0.999) / (1.0 - 0.0)) ) ) , dotResult7_g61);
			float dotResult20_g61 = dot( ase_worldPos.y , 1.0 );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float4 SunDisk59 = ( saturate( ( smoothstepResult9_g61 * saturate( dotResult20_g61 ) ) ) * ase_lightColor * ( _SunDiskIntensity * 50.0 ) );
			float temp_output_3_0_g62 = ( 1.0 - _ScatteringSize );
			float dotResult9_g62 = dot( -ase_worldlightDir , i.viewDir );
			float temp_output_10_0_g62 = saturate( dotResult9_g62 );
			float dotResult14_g62 = dot( temp_output_10_0_g62 , temp_output_10_0_g62 );
			float smoothstepResult1_g62 = smoothstep( temp_output_3_0_g62 , ( temp_output_3_0_g62 + 4.0 ) , dotResult14_g62);
			float dotResult16_g62 = dot( ase_worldPos.y , 1.0 );
			float4 Scattering148 = ( saturate( ( smoothstepResult1_g62 * saturate( dotResult16_g62 ) ) ) * ase_lightColor * _ScatteringBrightness );
			float temp_output_3_0_g63 = ( 1.0 - LightningRadius );
			float3 normalizeResult279 = normalize( ( LightningPosition - _WorldSpaceCameraPos ) );
			float dotResult9_g63 = dot( -normalizeResult279 , i.viewDir );
			float temp_output_10_0_g63 = saturate( dotResult9_g63 );
			float dotResult14_g63 = dot( temp_output_10_0_g63 , temp_output_10_0_g63 );
			float smoothstepResult1_g63 = smoothstep( temp_output_3_0_g63 , ( temp_output_3_0_g63 + 4.0 ) , dotResult14_g63);
			float dotResult16_g63 = dot( ase_worldPos.y , 1.0 );
			float4 Lightning283 = ( saturate( ( smoothstepResult1_g63 * saturate( dotResult16_g63 ) ) ) * LightningColor * LightningIntensity );
			float2 appendResult190 = (float2(_CloudsTilingandSpeed.z , _CloudsTilingandSpeed.w));
			float2 temp_output_187_0 = ( appendResult190 * _Time.y );
			float2 appendResult189 = (float2(_CloudsTilingandSpeed.x , _CloudsTilingandSpeed.y));
			float3 normalizeResult163 = normalize( ase_worldPos );
			float3 break164 = normalizeResult163;
			float2 appendResult166 = (float2(break164.x , break164.z));
			float2 temp_output_186_0 = ( appendResult189 * appendResult166 );
			float temp_output_173_0 = pow( break164.y , ( 1.0 - 0.6 ) );
			float3 temp_output_203_0 = ( ase_lightColor.rgb * ( pow( max( tex2D( _CloudsTexture, ( temp_output_187_0 + ( temp_output_186_0 / temp_output_173_0 ) ) ).r , tex2D( _CloudsTexture, ( ( temp_output_187_0 * 0.2 ) + ( ( temp_output_186_0 * 0.7 ) / temp_output_173_0 ) ) ).r ) , _CloudsHardness ) * _CloudsIntensity ) );
			float temp_output_231_0 = ( 1.0 - 0.2 );
			float dotResult226 = dot( -ase_worldlightDir , i.viewDir );
			float temp_output_227_0 = saturate( dotResult226 );
			float dotResult228 = dot( temp_output_227_0 , temp_output_227_0 );
			float smoothstepResult229 = smoothstep( temp_output_231_0 , ( temp_output_231_0 + 1.0 ) , dotResult228);
			float dotResult234 = dot( ase_worldPos.y , 1.0 );
			float CloudsLightScattering240 = saturate( ( smoothstepResult229 * saturate( dotResult234 ) ) );
			float SunIntensity248 = _SunDiskIntensity;
			float3 temp_output_207_0 = ( temp_output_203_0 + ( temp_output_203_0 * ( CloudsLightScattering240 * ( SunIntensity248 * 10.0 ) ) ) );
			float grayscale243 = Luminance(saturate( SunDisk59 ).rgb);
			float3 Clouds159 = ( temp_output_207_0 + ( temp_output_207_0 * grayscale243 ) );
			float grayscale218 = Luminance(Clouds159);
			float4 lerpResult214 = lerp( ( ( saturate( dotResult251 ) * _Color ) + SunDisk59 + Scattering148 + Lightning283 ) , float4( Clouds159 , 0.0 ) , saturate( grayscale218 ));
			float smoothstepResult259 = smoothstep( 0.0 , _FogHeight , abs( i.uv_texcoord.y ));
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float groundMask260 = step( ase_vertex3Pos.y , 0.0 );
			float lerpResult21 = lerp( saturate( ( pow( smoothstepResult259 , ( 1.0 - _FogSmoothness ) ) * ( 1.0 - groundMask260 ) ) ) , 0.0 , _FogFill);
			half FogMask22 = lerpResult21;
			float4 lerpResult24 = lerp( unity_FogColor , lerpResult214 , FogMask22);
			float4 lerpResult272 = lerp( lerpResult24 , _GroundColor , ( groundMask260 * ( 1.0 - _GroundFog ) ));
			o.Emission = lerpResult272.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.CommentaryNode;157;-2687.963,-2863.571;Inherit;False;3696.464;1382.841;;60;242;243;247;250;249;206;222;240;238;225;228;227;226;224;239;231;232;237;236;230;229;200;202;180;178;233;234;235;188;172;181;196;246;245;159;207;213;203;205;201;199;198;194;195;191;193;192;186;167;190;189;184;187;166;173;164;179;158;163;162;Clouds;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;143;-2685.315,-1348.117;Inherit;False;1106;502;;6;148;147;146;145;150;154;Scattering;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;30;-2670.333,-738.7761;Inherit;False;1083.564;598.4313;;7;91;56;31;59;38;36;248;Sun Disk;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;5;-2875.423,1266.971;Inherit;False;2363;484;;17;17;21;22;69;19;18;16;14;9;12;10;29;73;75;76;259;260;Fog;0.5235849,0.7689387,1,1;0;0
Node;AmplifyShaderEditor.AbsOpNode;12;-2417.001,1336.451;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;14;-2146.916,1639.742;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;16;-1954.916,1319.742;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;18;-1078.642,1312;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;21;-917.1924,1311.783;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;31;-2587.864,-687.7458;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LightColorNode;56;-2529.281,-520.1909;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;59;-1831.293,-539.78;Float;False;SunDisk;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightColorNode;146;-2571.021,-1154.02;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;61;259.2397,-14.31915;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;214;476.9827,18.11542;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;160;-54.19965,265.7645;Inherit;False;159;Clouds;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;215;343.9827,196.1154;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;145;-2641.021,-1299.02;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TFHCGrayscale;218;164.0356,260.2703;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;248;-2284.376,-220.3663;Inherit;False;SunIntensity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;91;-2568.366,-218.6589;Inherit;False;Property;_SunDiskIntensity;Sun Disk Intensity;9;0;Create;True;0;0;0;False;0;False;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-2643.807,-301.6425;Float;False;Property;_SunDiskSharpness;Sun Disk Sharpness;8;0;Create;True;0;0;0;False;0;False;0.999;0.73;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;162;-2653.492,-1960.331;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;163;-2416.268,-1962.331;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;158;-1047.815,-2019.524;Inherit;True;Property;_CloudsTex_01;Clouds Tex_01;11;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;179;-1050.492,-1794.164;Inherit;True;Property;_CloudsTex_02;Clouds Tex_02;11;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;164;-2256.492,-1964.331;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.PowerNode;173;-2028.644,-1877.998;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;166;-2098.602,-2011.236;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;187;-1648.548,-2100.909;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;184;-1916.711,-2155.653;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;189;-2099.336,-2118.291;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;190;-1878.335,-2257.291;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;167;-1766.877,-1989.176;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;-1934.551,-2062.909;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;192;-1333.334,-1736.291;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;193;-1501.334,-1694.292;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;-1478.334,-1824.291;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;198;-674.0283,-1880.36;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;199;-449.0275,-1891.36;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;201;-276.0275,-1887.36;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;205;-291.5807,-2015.693;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;203;-127.098,-1910.08;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;213;95.45172,-1832.427;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;207;253.3273,-1905.702;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;159;792.7277,-1906.532;Inherit;False;Clouds;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;245;624.5851,-1897.625;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;246;440.5851,-1825.625;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;181;-2267.495,-1814.163;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;172;-2595.644,-1804.997;Inherit;False;Constant;_HorizonCurvature;Horizon Curvature;10;0;Create;True;0;0;0;False;0;False;0.6;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;188;-2434.336,-2340.291;Inherit;False;Property;_CloudsTilingandSpeed;Clouds Tiling and Speed;15;0;Create;True;0;0;0;False;0;False;0.4,0.2,0,0.006;0.4,0.3,0.5,0.2;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;235;-1689.24,-2470.731;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;234;-1835.289,-2436.169;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;233;-2037.844,-2475.203;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;180;-1434.147,-2002.736;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SmoothstepOpNode;229;-1507.91,-2670.412;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;230;-1667.909,-2574.412;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;236;-1299.91,-2654.412;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;237;-1139.91,-2654.412;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;232;-2002.908,-2556.412;Inherit;False;Constant;_CloudsLightScatteringSharpness;Clouds Light Scattering Sharpness;16;0;Create;True;0;0;0;False;0;False;1;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;231;-1866.836,-2633.757;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;239;-2099.816,-2638.097;Inherit;False;Constant;_CloudsLightScattering;Clouds Light Scattering;17;0;Create;True;0;0;0;False;0;False;0.2;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;224;-2251.724,-2802.953;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;226;-2091.724,-2738.954;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;227;-1899.724,-2754.954;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;228;-1675.725,-2738.954;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;225;-2276.724,-2702.954;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;240;-978.6327,-2661.208;Inherit;False;CloudsLightScattering;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;222;-106.3366,-1698.631;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;206;-372.793,-1701.476;Inherit;False;240;CloudsLightScattering;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;249;-540.6107,-1619.699;Inherit;False;248;SunIntensity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;250;-275.9172,-1616.67;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;247;78.58523,-1589.629;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCGrayscale;243;214.5852,-1593.629;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;242;-104.4147,-1592.629;Inherit;False;59;SunDisk;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;196;-1671.188,-1876.75;Inherit;False;Constant;_Float2;Float 2;14;0;Create;True;0;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;194;-1648.189,-1790.752;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;195;-1824.189,-1694.751;Inherit;False;Constant;_Float1;Float 1;14;0;Create;True;0;0;0;False;0;False;0.7;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;200;-713.0284,-1741.36;Inherit;False;Property;_CloudsHardness;Clouds Hardness;14;0;Create;True;0;0;0;False;0;False;2;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-472.0275,-1781.361;Inherit;False;Property;_CloudsIntensity;Clouds Intensity;13;0;Create;True;0;0;0;False;0;False;0.8;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;178;-1372.492,-2219.163;Inherit;True;Property;_CloudsTexture;Clouds Texture;12;3;[Header];[NoScaleOffset];[SingleLineTexture];Create;True;1;Clouds;0;0;False;1;Space;False;None;None;False;black;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;150;-2634.71,-930.216;Inherit;False;Property;_ScatteringBrightness;Scattering Brightness;11;0;Create;True;0;0;0;False;0;False;5;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-2645.702,-381.5362;Float;False;Property;_SunDiskSize;Sun Disk Size;7;1;[Header];Create;True;1;Sun;0;0;False;1;Space;False;0.038;0.01;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;253;151.1134,-228.9604;Inherit;False;2;2;0;FLOAT;1;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;257;-2283.241,-537.459;Inherit;False;SunDiskMask;-1;;61;ec657a9d7170539428dc06107d524cca;0;5;29;FLOAT3;0.1,0,0;False;30;COLOR;1,1,1,1;False;27;FLOAT;0.1;False;28;FLOAT;0;False;55;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;238;-2493.367,-2804.518;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-2689.681,1315.194;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;259;-2237.397,1357.38;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-2620.423,1495.971;Half;False;Property;_FogHeight;Fog Height;2;1;[Header];Create;True;1;Fog;0;0;False;1;Space;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-2515.423,1641.972;Half;False;Property;_FogSmoothness;Fog Smoothness;3;0;Create;True;0;0;0;False;0;False;1;0.49;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;147;-2658.021,-1026.02;Inherit;False;Property;_ScatteringSize;Scattering Size;10;1;[Header];Create;True;1;Light Scattering;0;0;False;1;Space;False;1.2;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.FogAndAmbientColorsNode;25;638.8307,-108.4983;Inherit;False;unity_FogColor;0;1;COLOR;0
Node;AmplifyShaderEditor.Vector3Node;252;-385.8866,-682.9604;Inherit;False;Constant;_Vector0;Vector 0;15;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;251;-204.3469,-741.9876;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;254;-56.86456,-589.2256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;258;-460.5237,-875.585;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;23;-549.7762,-475.4183;Inherit;False;Property;_Color;Sky Color;0;1;[HDR];Create;False;0;0;0;False;0;False;0.2971698,0.6498447,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;75;-1951.058,1503.514;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;69;-2207.983,1476.802;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;17;-1191.191,1604.784;Half;False;Property;_FogFill;Fog Fill;4;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1117.191,1508.784;Half;False;Constant;_Float03;Float 03;55;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-728.5403,1310.242;Half;True;FogMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;76;-1462.802,1396.556;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-1297.189,1294.914;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;24;1079.751,0.2319107;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;90;1714.361,-36.53809;Float;False;True;-1;0;;0;0;Unlit;NOT_Lonely/NL_Skybox;False;False;False;False;True;True;True;True;True;True;True;True;False;False;True;True;False;False;False;False;False;Off;2;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0;True;False;0;True;Background;;Background;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;False;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;5;-1;-1;-1;1;PreviewType=Skybox;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.LerpOp;272;1350.695,25.12078;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;693.0024,108.3727;Inherit;True;22;FogMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;263;1021.365,152.8554;Inherit;False;Property;_GroundColor;Ground Color;1;1;[HDR];Create;True;0;0;0;False;0;False;0.0471698,0.03082621,0.02781239,1;0.04705882,0.03137255,0.02745098,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;273;1174.672,343.147;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;262;709.8221,345.1829;Inherit;True;260;groundMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;274;984.9146,444.0681;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;260;-1718.01,1444.134;Inherit;True;groundMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;79;646.713,570.4578;Inherit;False;Property;_GroundFog;Ground Fog;6;0;Create;True;0;0;0;False;0;False;1;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;276;-2532.607,155.9954;Inherit;False;Global;LightningPosition;LightningPosition;16;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;277;-2549.607,404.9954;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;278;-2245.607,277.9954;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;279;-2036.607,278.9954;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;280;-2093.701,368.1671;Inherit;False;Global;LightningColor;LightningColor;16;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;281;-1826.701,430.1671;Inherit;False;Global;LightningRadius;LightningRadius;16;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;282;-1823.701,515.1671;Inherit;False;Global;LightningIntensity;LightningIntensity;16;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;275;-1552.92,297.7424;Inherit;False;SkyLightScattering;-1;;63;c9766a5bf2251134fa76ef6d0b56f3ed;0;4;12;FLOAT3;0.1,0,0;False;21;COLOR;1,1,1,1;False;8;FLOAT;0.1;False;23;FLOAT;0.1;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;154;-2313.021,-1129.02;Inherit;False;SkyLightScattering;-1;;62;c9766a5bf2251134fa76ef6d0b56f3ed;0;4;12;FLOAT3;0.1,0,0;False;21;COLOR;1,1,1,1;False;8;FLOAT;0.1;False;23;FLOAT;0.1;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;148;-1804.021,-1124.02;Inherit;False;Scattering;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;283;-1248.653,295.6377;Inherit;False;Lightning;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;-22.5461,-52.5492;Inherit;False;59;SunDisk;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;151;-36.3174,54.0701;Inherit;False;148;Scattering;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;284;-44.05359,149.682;Inherit;False;283;Lightning;1;0;OBJECT;;False;1;COLOR;0
WireConnection;12;0;29;2
WireConnection;14;0;10;0
WireConnection;16;0;259;0
WireConnection;16;1;14;0
WireConnection;18;0;73;0
WireConnection;21;0;18;0
WireConnection;21;1;19;0
WireConnection;21;2;17;0
WireConnection;59;0;257;0
WireConnection;61;0;253;0
WireConnection;61;1;65;0
WireConnection;61;2;151;0
WireConnection;61;3;284;0
WireConnection;214;0;61;0
WireConnection;214;1;160;0
WireConnection;214;2;215;0
WireConnection;215;0;218;0
WireConnection;218;0;160;0
WireConnection;248;0;91;0
WireConnection;163;0;162;0
WireConnection;158;0;178;0
WireConnection;158;1;180;0
WireConnection;158;7;178;1
WireConnection;179;0;178;0
WireConnection;179;1;192;0
WireConnection;179;7;178;1
WireConnection;164;0;163;0
WireConnection;173;0;164;1
WireConnection;173;1;181;0
WireConnection;166;0;164;0
WireConnection;166;1;164;2
WireConnection;187;0;190;0
WireConnection;187;1;184;0
WireConnection;189;0;188;1
WireConnection;189;1;188;2
WireConnection;190;0;188;3
WireConnection;190;1;188;4
WireConnection;167;0;186;0
WireConnection;167;1;173;0
WireConnection;186;0;189;0
WireConnection;186;1;166;0
WireConnection;192;0;191;0
WireConnection;192;1;193;0
WireConnection;193;0;194;0
WireConnection;193;1;173;0
WireConnection;191;0;187;0
WireConnection;191;1;196;0
WireConnection;198;0;158;1
WireConnection;198;1;179;1
WireConnection;199;0;198;0
WireConnection;199;1;200;0
WireConnection;201;0;199;0
WireConnection;201;1;202;0
WireConnection;203;0;205;1
WireConnection;203;1;201;0
WireConnection;213;0;203;0
WireConnection;213;1;222;0
WireConnection;207;0;203;0
WireConnection;207;1;213;0
WireConnection;159;0;245;0
WireConnection;245;0;207;0
WireConnection;245;1;246;0
WireConnection;246;0;207;0
WireConnection;246;1;243;0
WireConnection;181;0;172;0
WireConnection;235;0;234;0
WireConnection;234;0;233;2
WireConnection;180;0;187;0
WireConnection;180;1;167;0
WireConnection;229;0;228;0
WireConnection;229;1;231;0
WireConnection;229;2;230;0
WireConnection;230;0;231;0
WireConnection;230;1;232;0
WireConnection;236;0;229;0
WireConnection;236;1;235;0
WireConnection;237;0;236;0
WireConnection;231;0;239;0
WireConnection;224;0;238;0
WireConnection;226;0;224;0
WireConnection;226;1;225;0
WireConnection;227;0;226;0
WireConnection;228;0;227;0
WireConnection;228;1;227;0
WireConnection;240;0;237;0
WireConnection;222;0;206;0
WireConnection;222;1;250;0
WireConnection;250;0;249;0
WireConnection;247;0;242;0
WireConnection;243;0;247;0
WireConnection;194;0;186;0
WireConnection;194;1;195;0
WireConnection;253;0;254;0
WireConnection;253;1;23;0
WireConnection;257;29;31;0
WireConnection;257;30;56;0
WireConnection;257;27;36;0
WireConnection;257;28;38;0
WireConnection;257;55;91;0
WireConnection;259;0;12;0
WireConnection;259;2;9;0
WireConnection;251;0;258;0
WireConnection;251;1;252;0
WireConnection;254;0;251;0
WireConnection;75;0;69;2
WireConnection;22;0;21;0
WireConnection;76;0;260;0
WireConnection;73;0;16;0
WireConnection;73;1;76;0
WireConnection;24;0;25;0
WireConnection;24;1;214;0
WireConnection;24;2;64;0
WireConnection;90;2;272;0
WireConnection;272;0;24;0
WireConnection;272;1;263;0
WireConnection;272;2;273;0
WireConnection;273;0;262;0
WireConnection;273;1;274;0
WireConnection;274;0;79;0
WireConnection;260;0;75;0
WireConnection;278;0;276;0
WireConnection;278;1;277;0
WireConnection;279;0;278;0
WireConnection;275;12;279;0
WireConnection;275;21;280;0
WireConnection;275;8;281;0
WireConnection;275;23;282;0
WireConnection;154;12;145;0
WireConnection;154;21;146;0
WireConnection;154;8;147;0
WireConnection;154;23;150;0
WireConnection;148;0;154;0
WireConnection;283;0;275;0
ASEEND*/
//CHKSM=5C0766BC7BA92D9851FB41A32D9906AE713A8A8D