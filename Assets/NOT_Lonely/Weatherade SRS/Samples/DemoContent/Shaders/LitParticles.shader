// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NOT_Lonely/NL_LitSmoke"
{
	Properties
	{
		_NormalScale("NormalScale", Float) = 0.45
		_NoiseTiling("NoiseTiling", Float) = 1
		_NoiseSpeed("Noise Speed", Float) = -0.5
		_LightHardness("Light Hardness", Range( 0 , 1)) = 1
		_LightOffset("Light Offset", Range( 0 , 1)) = 0.3
		_MainTex("Noise", 2D) = "white" {}
		_Translucensy("Translucensy", Range( 0 , 1)) = 0.1
		_BlendWithColor("BlendWithColor", Range( 0 , 1)) = 0
		_Color("Color", Color) = (0,0,0,0)

	}
	
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
	LOD 100

		Cull Back
		CGINCLUDE
		#pragma target 3.0 
		ENDCG
		
		
		Pass
		{
			
			Name "ForwardBase"
			Tags { "LightMode"="ForwardBase" }

			CGINCLUDE
			#pragma target 3.0
			ENDCG
			Blend SrcAlpha OneMinusSrcAlpha
			AlphaToMask Off
			Cull Back
			ColorMask RGBA
			ZWrite Off
			ZTest LEqual
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#ifndef UNITY_PASS_FORWARDBASE
			#define UNITY_PASS_FORWARDBASE
			#endif
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#define ASE_SHADOWS 1
			#define ASE_NEEDS_FRAG_COLOR

			//This is a late directive
			
			uniform float _Translucensy;
			uniform float4 _Color;
			uniform float _BlendWithColor;
			uniform float _NoiseSpeed;
			uniform float _NoiseTiling;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float _NormalScale;
			uniform float _LightHardness;
			uniform float _LightOffset;
			UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
			uniform float4 _CameraDepthTexture_TexelSize;
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
			


			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_color : COLOR;
				UNITY_SHADOW_COORDS(1)
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
			};
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float3 ase_worldPos = mul(unity_ObjectToWorld, float4( (v.vertex).xyz, 1 )).xyz;
				o.ase_texcoord2.xyz = ase_worldPos;
				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord4 = screenPos;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord3.xyz = v.ase_texcoord.xyz;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;
				o.ase_texcoord3.w = 0;
				
				v.vertex.xyz +=  float3(0,0,0) ;
				o.pos = UnityObjectToClipPos(v.vertex);
				
				return o;
			}
			
			float4 frag (v2f i ) : SV_Target
			{
				float3 outColor;
				float outAlpha;

				#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
				float4 ase_lightColor = 0;
				#else //aselc
				float4 ase_lightColor = _LightColor0;
				#endif //aselc
				float4 lerpResult91 = lerp( unity_AmbientSky , ase_lightColor , _Translucensy);
				float4 lerpResult93 = lerp( lerpResult91 , _Color , _BlendWithColor);
				float3 ase_worldPos = i.ase_texcoord2.xyz;
				UNITY_LIGHT_ATTENUATION(ase_atten, i, ase_worldPos)
				float3 appendResult35 = (float3(( ase_lightColor * ase_atten ).rgb));
				float mulTime46 = _Time.y * _NoiseSpeed;
				float2 texCoord2 = i.ase_texcoord3.xyz.xy * float2( 2,2 ) + float2( -1,-1 );
				float simplePerlin2D17 = snoise( ( mulTime46 + texCoord2 + i.ase_texcoord3.xyz.z )*( _NoiseTiling * 0.5 ) );
				simplePerlin2D17 = simplePerlin2D17*0.5 + 0.5;
				float2 uv_MainTex = i.ase_texcoord3.xyz.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode71 = tex2D( _MainTex, ( ( mulTime46 * i.ase_texcoord3.xyz.z * 0.25 ) + i.ase_texcoord3.xyz.z + uv_MainTex ) );
				float temp_output_85_0 = (tex2DNode71.r*0.5 + 0.5);
				float simplePerlin2D19 = snoise( ( ( mulTime46 * 0.8 ) + texCoord2 + 2.5 + i.ase_texcoord3.xyz.z )*( _NoiseTiling * 0.7 ) );
				simplePerlin2D19 = simplePerlin2D19*0.5 + 0.5;
				float simplePerlin2D20 = snoise( ( ( mulTime46 * 0.5 ) + texCoord2 + 8.3 + i.ase_texcoord3.xyz.z )*( _NoiseTiling * 0.9 ) );
				simplePerlin2D20 = simplePerlin2D20*0.5 + 0.5;
				float3 appendResult22 = (float3(( simplePerlin2D17 * temp_output_85_0 ) , ( simplePerlin2D19 * temp_output_85_0 ) , ( simplePerlin2D20 * temp_output_85_0 )));
				float dotResult4 = dot( texCoord2 , texCoord2 );
				float temp_output_7_0 = saturate( ( 1.0 - dotResult4 ) );
				float3 appendResult3 = (float3(texCoord2 , sqrt( temp_output_7_0 )));
				float3 worldSpaceLightDir = UnityWorldSpaceLightDir(ase_worldPos);
				float3 worldToViewDir13 = mul( UNITY_MATRIX_V, float4( worldSpaceLightDir, 0 ) ).xyz;
				float dotResult11 = dot( ( ( appendResult22 * _NormalScale ) + appendResult3 ) , worldToViewDir13 );
				float temp_output_15_0 = saturate( (dotResult11*_LightHardness + _LightOffset) );
				float4 lerpResult34 = lerp( lerpResult93 , float4( appendResult35 , 0.0 ) , temp_output_15_0);
				float smoothstepResult31 = smoothstep( 0.0 , 1.0 , temp_output_7_0);
				float noise88 = min( simplePerlin2D20 , tex2DNode71.r );
				float4 screenPos = i.ase_texcoord4;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth36 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
				float distanceDepth36 = saturate( abs( ( screenDepth36 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 1.0 ) ) );
				float alpha26 = ( smoothstepResult31 * noise88 * i.ase_color.a * distanceDepth36 );
				#ifdef UNITY_PASS_FORWARDADD
				float4 staticSwitch50 = float4( ( appendResult35 * temp_output_15_0 * alpha26 ) , 0.0 );
				#else
				float4 staticSwitch50 = ( i.ase_color * lerpResult34 );
				#endif
				
				
				outColor = staticSwitch50.rgb;
				outAlpha = alpha26;
				clip(outAlpha);
				return float4(outColor,outAlpha);
			}
			ENDCG
		}
		
		
		Pass
		{
			Name "ForwardAdd"
			Tags { "LightMode"="ForwardAdd" }
			ZWrite Off
			Blend One One
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdadd_fullshadows
			#ifndef UNITY_PASS_FORWARDADD
			#define UNITY_PASS_FORWARDADD
			#endif
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#define ASE_SHADOWS 1
			#define ASE_NEEDS_FRAG_COLOR

			//This is a late directive
			
			uniform float _Translucensy;
			uniform float4 _Color;
			uniform float _BlendWithColor;
			uniform float _NoiseSpeed;
			uniform float _NoiseTiling;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float _NormalScale;
			uniform float _LightHardness;
			uniform float _LightOffset;
			UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
			uniform float4 _CameraDepthTexture_TexelSize;
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
			


			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_color : COLOR;
				UNITY_SHADOW_COORDS(1)
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
			};
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float3 ase_worldPos = mul(unity_ObjectToWorld, float4( (v.vertex).xyz, 1 )).xyz;
				o.ase_texcoord2.xyz = ase_worldPos;
				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord4 = screenPos;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord3.xyz = v.ase_texcoord.xyz;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;
				o.ase_texcoord3.w = 0;
				
				v.vertex.xyz +=  float3(0,0,0) ;
				o.pos = UnityObjectToClipPos(v.vertex);
				
				return o;
			}
			
			float4 frag (v2f i ) : SV_Target
			{
				float3 outColor;
				float outAlpha;

				#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
				float4 ase_lightColor = 0;
				#else //aselc
				float4 ase_lightColor = _LightColor0;
				#endif //aselc
				float4 lerpResult91 = lerp( unity_AmbientSky , ase_lightColor , _Translucensy);
				float4 lerpResult93 = lerp( lerpResult91 , _Color , _BlendWithColor);
				float3 ase_worldPos = i.ase_texcoord2.xyz;
				UNITY_LIGHT_ATTENUATION(ase_atten, i, ase_worldPos)
				float3 appendResult35 = (float3(( ase_lightColor * ase_atten ).rgb));
				float mulTime46 = _Time.y * _NoiseSpeed;
				float2 texCoord2 = i.ase_texcoord3.xyz.xy * float2( 2,2 ) + float2( -1,-1 );
				float simplePerlin2D17 = snoise( ( mulTime46 + texCoord2 + i.ase_texcoord3.xyz.z )*( _NoiseTiling * 0.5 ) );
				simplePerlin2D17 = simplePerlin2D17*0.5 + 0.5;
				float2 uv_MainTex = i.ase_texcoord3.xyz.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode71 = tex2D( _MainTex, ( ( mulTime46 * i.ase_texcoord3.xyz.z * 0.25 ) + i.ase_texcoord3.xyz.z + uv_MainTex ) );
				float temp_output_85_0 = (tex2DNode71.r*0.5 + 0.5);
				float simplePerlin2D19 = snoise( ( ( mulTime46 * 0.8 ) + texCoord2 + 2.5 + i.ase_texcoord3.xyz.z )*( _NoiseTiling * 0.7 ) );
				simplePerlin2D19 = simplePerlin2D19*0.5 + 0.5;
				float simplePerlin2D20 = snoise( ( ( mulTime46 * 0.5 ) + texCoord2 + 8.3 + i.ase_texcoord3.xyz.z )*( _NoiseTiling * 0.9 ) );
				simplePerlin2D20 = simplePerlin2D20*0.5 + 0.5;
				float3 appendResult22 = (float3(( simplePerlin2D17 * temp_output_85_0 ) , ( simplePerlin2D19 * temp_output_85_0 ) , ( simplePerlin2D20 * temp_output_85_0 )));
				float dotResult4 = dot( texCoord2 , texCoord2 );
				float temp_output_7_0 = saturate( ( 1.0 - dotResult4 ) );
				float3 appendResult3 = (float3(texCoord2 , sqrt( temp_output_7_0 )));
				float3 worldSpaceLightDir = UnityWorldSpaceLightDir(ase_worldPos);
				float3 worldToViewDir13 = mul( UNITY_MATRIX_V, float4( worldSpaceLightDir, 0 ) ).xyz;
				float dotResult11 = dot( ( ( appendResult22 * _NormalScale ) + appendResult3 ) , worldToViewDir13 );
				float temp_output_15_0 = saturate( (dotResult11*_LightHardness + _LightOffset) );
				float4 lerpResult34 = lerp( lerpResult93 , float4( appendResult35 , 0.0 ) , temp_output_15_0);
				float smoothstepResult31 = smoothstep( 0.0 , 1.0 , temp_output_7_0);
				float noise88 = min( simplePerlin2D20 , tex2DNode71.r );
				float4 screenPos = i.ase_texcoord4;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth36 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
				float distanceDepth36 = saturate( abs( ( screenDepth36 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 1.0 ) ) );
				float alpha26 = ( smoothstepResult31 * noise88 * i.ase_color.a * distanceDepth36 );
				#ifdef UNITY_PASS_FORWARDADD
				float4 staticSwitch50 = float4( ( appendResult35 * temp_output_15_0 * alpha26 ) , 0.0 );
				#else
				float4 staticSwitch50 = ( i.ase_color * lerpResult34 );
				#endif
				
				
				outColor = staticSwitch50.rgb;
				outAlpha = alpha26;
				clip(outAlpha);
				return float4(outColor,outAlpha);
			}
			ENDCG
		}

	
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.DynamicAppendNode;3;-975.7901,0.1932755;Inherit;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SqrtOpNode;9;-1134.79,85.19328;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;25;-558.5671,-2.199219;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;32;-1073.149,650.1049;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DepthFade;36;-1095.666,859.0327;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;17;-1561.88,-1053.097;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;19;-1554.88,-806.0971;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;0.7;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;20;-1557.88,-571.0972;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;0.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;31;-1062.149,412.1049;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-819.1494,482.1049;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-1812.847,-1026.353;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;56;-1808.847,-1227.353;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;18;-1789.88,-817.0971;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT2;2.5,2.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-1796.847,-686.3533;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.7;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;21;-1786.88,-560.0972;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT2;3.2,3.2;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-2171.858,-705.7051;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;0;False;0;False;2.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-2172.592,-507.1661;Inherit;False;Constant;_Float1;Float 0;3;0;Create;True;0;0;0;False;0;False;8.3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-2387.561,-637.1035;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-2388.561,-776.1035;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;46;-2749.092,-745.8623;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;4;-1985.791,112.1933;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;5;-1733.791,119.1933;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;7;-1436.79,128.1933;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;10;-844.6864,226.3361;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SaturateNode;15;225.7591,52.64368;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;11;-328.6826,44.51706;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-1799.529,-391.7603;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-214.5959,623.725;Inherit;False;alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-1974.376,-309.9627;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-2253.353,-206.4627;Inherit;False;Constant;_Float2;Float 2;7;0;Create;True;0;0;0;False;0;False;0.25;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;22;-682.5837,-860.8157;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;82;-928.1265,-963.3663;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-393.2769,-844.1472;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-700.5875,-680.5072;Inherit;False;Property;_NormalScale;NormalScale;0;0;Create;True;0;0;0;False;0;False;0.45;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;-938.1265,-809.3663;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;75;-1729.652,-195.9421;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;71;-1590.739,-225.5625;Inherit;True;Property;_MainTex;Noise;5;0;Create;False;0;0;0;False;0;False;-1;28715250e68caeb4d9f1b861866f0f99;28715250e68caeb4d9f1b861866f0f99;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;-950.1106,-553.2426;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;30;-1072.149,574.1049;Inherit;False;88;noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;85;-1219.752,-297.3804;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;89;-868.3306,-309.6499;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;88;-678.0239,-310.1739;Inherit;False;noise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;91;330.5682,-320.9066;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;65;-33.85352,51.54559;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;77;-2049.369,-134.0706;Inherit;False;0;71;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;35;279.0793,-123.6154;Inherit;False;FLOAT3;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LightAttenuation;38;-339.6664,-151.9673;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;37;-273.6664,-299.9673;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-66.66638,-135.9673;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FogAndAmbientColorsNode;90;-241.4318,-420.9066;Inherit;False;unity_AmbientSky;0;1;COLOR;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;64;-2667.884,-438.8245;Inherit;False;0;3;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;13;-568.4202,188.6527;Inherit;False;World;View;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-2636.857,146.3614;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;2,2;False;1;FLOAT2;-1,-1;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;53;-2248.847,-952.3533;Inherit;False;Property;_NoiseTiling;NoiseTiling;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-3002.605,-740.8074;Inherit;False;Property;_NoiseSpeed;Noise Speed;2;0;Create;True;0;0;0;False;0;False;-0.5;-0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;92;49.56824,-239.9066;Inherit;False;Property;_Translucensy;Translucensy;6;0;Create;True;0;0;0;False;0;False;0.1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-374.1563,431.1609;Inherit;False;Property;_LightOffset;Light Offset;4;0;Create;True;0;0;0;False;0;False;0.3;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;67;-377.1563,348.1609;Inherit;False;Property;_LightHardness;Light Hardness;3;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;27;903.6127,571.6461;Inherit;False;26;alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;1350.966,105.6307;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;44;1547.89,61.96418;Float;False;False;-1;2;ASEMaterialInspector;100;1;New Amplify Shader;e1de45c0d41f68c41b2cc20c8b9c05ef;True;ShadowCaster;0;3;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;1204.054,350.9725;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;43;2116.523,345.5624;Float;False;False;-1;2;ASEMaterialInspector;100;7;New Amplify Shader;e1de45c0d41f68c41b2cc20c8b9c05ef;True;Deferred;0;2;Deferred;4;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Deferred;True;2;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.LerpOp;34;1116.519,36.64531;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;50;1651.229,153.5313;Inherit;False;Property;_Keyword0;Keyword 0;1;0;Create;True;0;0;0;False;0;False;0;0;0;False;UNITY_PASS_FORWARDADD;Toggle;2;Key0;Key1;Fetch;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;41;2238.16,171.8807;Float;False;True;-1;2;ASEMaterialInspector;100;7;NOT_Lonely/NL_LitSmoke;e1de45c0d41f68c41b2cc20c8b9c05ef;True;ForwardBase;0;0;ForwardBase;3;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;False;0;True;True;2;5;False;;10;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;2;False;;True;3;False;;True;False;0;False;;0;False;;True;1;LightMode=ForwardBase;True;2;False;0;;0;0;Standard;0;0;4;True;True;False;False;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;42;2238.16,283.8807;Float;False;False;-1;2;ASEMaterialInspector;100;7;New Amplify Shader;e1de45c0d41f68c41b2cc20c8b9c05ef;True;ForwardAdd;0;1;ForwardAdd;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;RenderType=Opaque=RenderType;True;2;False;0;True;True;4;1;False;;1;False;;0;5;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;True;1;LightMode=ForwardAdd;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.VertexColorNode;33;1083.143,-426.3043;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;94;520.8934,-131.8588;Inherit;False;Property;_BlendWithColor;BlendWithColor;7;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;93;829.8934,-262.8588;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;95;515.8934,-322.8588;Inherit;False;Property;_Color;Color;8;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;3;0;2;0
WireConnection;3;2;9;0
WireConnection;9;0;7;0
WireConnection;25;0;23;0
WireConnection;25;1;3;0
WireConnection;17;0;56;0
WireConnection;17;1;52;0
WireConnection;19;0;18;0
WireConnection;19;1;54;0
WireConnection;20;0;21;0
WireConnection;20;1;55;0
WireConnection;31;0;7;0
WireConnection;28;0;31;0
WireConnection;28;1;30;0
WireConnection;28;2;32;4
WireConnection;28;3;36;0
WireConnection;52;0;53;0
WireConnection;56;0;46;0
WireConnection;56;1;2;0
WireConnection;56;2;64;3
WireConnection;18;0;62;0
WireConnection;18;1;2;0
WireConnection;18;2;57;0
WireConnection;18;3;64;3
WireConnection;54;0;53;0
WireConnection;21;0;60;0
WireConnection;21;1;2;0
WireConnection;21;2;59;0
WireConnection;21;3;64;3
WireConnection;60;0;46;0
WireConnection;62;0;46;0
WireConnection;46;0;63;0
WireConnection;4;0;2;0
WireConnection;4;1;2;0
WireConnection;5;0;4;0
WireConnection;7;0;5;0
WireConnection;15;0;65;0
WireConnection;11;0;25;0
WireConnection;11;1;13;0
WireConnection;55;0;53;0
WireConnection;26;0;28;0
WireConnection;76;0;46;0
WireConnection;76;1;64;3
WireConnection;76;2;79;0
WireConnection;22;0;82;0
WireConnection;22;1;81;0
WireConnection;22;2;72;0
WireConnection;82;0;17;0
WireConnection;82;1;85;0
WireConnection;23;0;22;0
WireConnection;23;1;24;0
WireConnection;81;0;19;0
WireConnection;81;1;85;0
WireConnection;75;0;76;0
WireConnection;75;1;64;3
WireConnection;75;2;77;0
WireConnection;71;1;75;0
WireConnection;72;0;20;0
WireConnection;72;1;85;0
WireConnection;85;0;71;1
WireConnection;89;0;20;0
WireConnection;89;1;71;1
WireConnection;88;0;89;0
WireConnection;91;0;90;0
WireConnection;91;1;37;0
WireConnection;91;2;92;0
WireConnection;65;0;11;0
WireConnection;65;1;67;0
WireConnection;65;2;66;0
WireConnection;35;0;39;0
WireConnection;39;0;37;0
WireConnection;39;1;38;0
WireConnection;13;0;10;0
WireConnection;40;0;33;0
WireConnection;40;1;34;0
WireConnection;51;0;35;0
WireConnection;51;1;15;0
WireConnection;51;2;27;0
WireConnection;34;0;93;0
WireConnection;34;1;35;0
WireConnection;34;2;15;0
WireConnection;50;1;40;0
WireConnection;50;0;51;0
WireConnection;41;0;50;0
WireConnection;41;1;27;0
WireConnection;93;0;91;0
WireConnection;93;1;95;0
WireConnection;93;2;94;0
ASEEND*/
//CHKSM=BB6AF8AC8FC54758E5D0E550AEB45857EC43CA6D