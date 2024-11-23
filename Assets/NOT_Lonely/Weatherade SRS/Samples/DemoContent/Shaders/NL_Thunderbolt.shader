// Upgrade NOTE: upgraded instancing buffer 'NOT_LonelyNL_Thunderbolt' to new syntax.

// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NL_Thunderbolt"
{
	Properties
	{
		_Noise("Noise", 2D) = "black" {}
		[HDR]_Color("Color", Color) = (0.6036298,0.7117425,1.148698,1)
		_LightningCurvness("LightningCurvness", Float) = 2
		_LightningNoise("LightningNoise", Float) = 0.08
		_Intensity("Intensity", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Noise;
		uniform float _Intensity;

		UNITY_INSTANCING_BUFFER_START(NOT_LonelyNL_Thunderbolt)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Noise_ST)
#define _Noise_ST_arr NOT_LonelyNL_Thunderbolt
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
#define _Color_arr NOT_LonelyNL_Thunderbolt
			UNITY_DEFINE_INSTANCED_PROP(float, _LightningCurvness)
#define _LightningCurvness_arr NOT_LonelyNL_Thunderbolt
			UNITY_DEFINE_INSTANCED_PROP(float, _LightningNoise)
#define _LightningNoise_arr NOT_LonelyNL_Thunderbolt
		UNITY_INSTANCING_BUFFER_END(NOT_LonelyNL_Thunderbolt)

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 _Noise_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_Noise_ST_arr, _Noise_ST);
			float2 uv_Noise = v.texcoord * _Noise_ST_Instance.xy + _Noise_ST_Instance.zw;
			float4 temp_cast_0 = (1.0).xxxx;
			float4 break14 = ( ( tex2Dlod( _Noise, float4( uv_Noise, 0, 0.0) ) * 2.0 ) - temp_cast_0 );
			float _LightningCurvness_Instance = UNITY_ACCESS_INSTANCED_PROP(_LightningCurvness_arr, _LightningCurvness);
			float3 appendResult4 = (float3(( break14.r + ( pow( v.texcoord.xy.x , 4.0 ) * _LightningCurvness_Instance ) ) , 0.0 , ( break14.g + ( pow( v.texcoord.xy.y , 4.0 ) * _LightningCurvness_Instance ) )));
			float _LightningNoise_Instance = UNITY_ACCESS_INSTANCED_PROP(_LightningNoise_arr, _LightningNoise);
			float3 ase_vertex3Pos = v.vertex.xyz;
			v.vertex.xyz = ( ( appendResult4 * _LightningNoise_Instance ) + ase_vertex3Pos );
			v.vertex.w = 1;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 _Color_Instance = UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
			float temp_output_40_0 = ( pow( min( ( 1.0 - i.uv_texcoord.y ) , i.uv_texcoord.y ) , 1.0 ) * 2.0 );
			o.Emission = ( _Color_Instance * temp_output_40_0 ).rgb;
			o.Alpha = ( ( _Color_Instance.a * temp_output_40_0 ) * _Intensity );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd vertex:vertexDataFunc 

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
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
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
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
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
Version=19200
Node;AmplifyShaderEditor.SimpleAddOpNode;3;-100,180;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;2;-327,474;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-2335.874,-187.7052;Inherit;True;Property;_Noise;Noise;0;0;Create;True;0;0;0;False;0;False;-1;c175667f7c465994681948f27745a67f;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;17;-2033.874,41.29487;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-1866.874,-145.7052;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1825.874,18.29487;Inherit;False;Constant;_Float1;Float 1;3;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;18;-1620.874,-145.7052;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-526,44;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;14;-1291,-129;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;4;-731,-109;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-989.5342,-153.8388;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;24;-1007.534,-20.83881;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-1335.534,118.1612;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;21;-2095.573,191.7691;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;32;-857.4117,190.7331;Inherit;False;InstancedProperty;_LightningNoise;LightningNoise;3;0;Create;True;0;0;0;False;0;False;0.08;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;27;-1827.534,167.1612;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;28;-1832.534,319.1612;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-1335.534,320.1612;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-1597,228;Inherit;False;InstancedProperty;_LightningCurvness;LightningCurvness;2;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;33;-354.4226,-32.5647;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;37;-50.92395,-356.6701;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;39;115.076,-362.6701;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;38;262.076,-364.6701;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;681.076,-112.6701;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;536.076,-362.6701;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;7;-335,-245;Inherit;False;InstancedProperty;_Color;Color;1;1;[HDR];Create;True;0;0;0;False;0;False;0.6036298,0.7117425,1.148698,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1187,-112;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;NOT_Lonely/NL_Thunderbolt;False;False;False;False;True;True;True;True;True;True;True;True;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Absolute;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;683.076,-10.6701;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;920.374,67.62598;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;600.374,129.626;Inherit;False;Property;_Intensity;Intensity;4;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
WireConnection;3;0;5;0
WireConnection;3;1;2;0
WireConnection;16;0;1;0
WireConnection;16;1;17;0
WireConnection;18;0;16;0
WireConnection;18;1;19;0
WireConnection;5;0;4;0
WireConnection;5;1;32;0
WireConnection;14;0;18;0
WireConnection;4;0;23;0
WireConnection;4;2;24;0
WireConnection;23;0;14;0
WireConnection;23;1;31;0
WireConnection;24;0;14;1
WireConnection;24;1;29;0
WireConnection;31;0;27;0
WireConnection;31;1;6;0
WireConnection;27;0;21;1
WireConnection;28;0;21;2
WireConnection;29;0;28;0
WireConnection;29;1;6;0
WireConnection;37;0;33;2
WireConnection;39;0;37;0
WireConnection;39;1;33;2
WireConnection;38;0;39;0
WireConnection;34;0;7;0
WireConnection;34;1;40;0
WireConnection;40;0;38;0
WireConnection;0;2;34;0
WireConnection;0;9;41;0
WireConnection;0;11;3;0
WireConnection;35;0;7;4
WireConnection;35;1;40;0
WireConnection;41;0;35;0
WireConnection;41;1;42;0
ASEEND*/
//CHKSM=5E89684B3C2659F7CBB5464944402BB49B711FCA