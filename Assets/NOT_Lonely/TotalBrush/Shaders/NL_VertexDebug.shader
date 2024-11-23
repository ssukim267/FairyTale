// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/NOT_Lonely/NL_VertexDebug"
{
	Properties
	{
		_ShowAlpha("_ShowAlpha", Int) = 1
		_Shaded("_Shaded", Int) = 1

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		Offset -1 , 0.01
		
		
		
		Pass
		{
			Name "Unlit"

			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_WORLD_POSITION


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float3 ase_normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform int _Shaded;
			uniform int _ShowAlpha;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord1.xyz = ase_worldNormal;
				
				o.ase_color = v.color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
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
				float3 appendResult10 = (float3(i.ase_color.r , i.ase_color.g , i.ase_color.b));
				float grayscale27 = Luminance(appendResult10);
				float lerpResult26 = lerp( 0.2 , 1.0 , grayscale27);
				float3 temp_cast_1 = (lerpResult26).xxx;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(WorldPosition);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldNormal = i.ase_texcoord1.xyz;
				float fresnelNdotV6 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode6 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV6, 1.0 ) );
				float3 lerpResult12 = lerp( temp_cast_1 , appendResult10 , saturate( ( 1.0 - fresnelNode6 ) ));
				float4 appendResult11 = (float4(( (float)_Shaded == 0.0 ? appendResult10 : lerpResult12 ) , ( (float)_ShowAlpha == 0.0 ? 1.0 : i.ase_color.a )));
				
				
				finalColor = appendResult11;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.IntNode;23;48.20117,-256.607;Inherit;False;Property;_Shaded;_Shaded;1;0;Create;True;0;0;0;False;0;False;1;1;False;0;1;INT;0
Node;AmplifyShaderEditor.DynamicAppendNode;11;807.829,385.0487;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;3;1008.272,383.1273;Float;False;True;-1;2;ASEMaterialInspector;100;5;Hidden/NOT_Lonely/NL_VertexDebug;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;2;False;;True;3;False;;True;True;-1;False;;0.01;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.Compare;20;594.033,506.1437;Inherit;False;0;4;0;INT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;420.2361,550.5845;Inherit;False;Constant;_Float2;Float 2;1;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;19;398.6642,477.2409;Inherit;False;Property;_ShowAlpha;_ShowAlpha;0;0;Create;True;0;0;0;False;0;False;1;1;False;0;1;INT;0
Node;AmplifyShaderEditor.Compare;22;386.2012,-84.60699;Inherit;False;0;4;0;INT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;12;111.1406,123.3553;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;9;-484.5,432;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;8;-315.5,433;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;6;-807.5,438;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;1;-803,90;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;10;-574.1572,110.0285;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCGrayscale;27;-375.7266,221.4734;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;26;-169.7266,177.4734;Inherit;False;3;0;FLOAT;0.2;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
WireConnection;11;0;22;0
WireConnection;11;3;20;0
WireConnection;3;0;11;0
WireConnection;20;0;19;0
WireConnection;20;2;21;0
WireConnection;20;3;1;4
WireConnection;22;0;23;0
WireConnection;22;2;10;0
WireConnection;22;3;12;0
WireConnection;12;0;26;0
WireConnection;12;1;10;0
WireConnection;12;2;8;0
WireConnection;9;0;6;0
WireConnection;8;0;9;0
WireConnection;10;0;1;1
WireConnection;10;1;1;2
WireConnection;10;2;1;3
WireConnection;27;0;10;0
WireConnection;26;2;27;0
ASEEND*/
//CHKSM=79089D6E952BE4EC677896B0183739F65AF5EB97