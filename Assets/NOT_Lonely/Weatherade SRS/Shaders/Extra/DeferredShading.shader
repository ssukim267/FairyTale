Shader "Hidden/NOT_Lonely/Weatherade/DeferredShading" {
	
	Properties {}

	SubShader {
		Pass {
			ZWrite Off
			Blend [_SrcBlend] [_DstBlend]

			CGPROGRAM

			sampler2D _LightBuffer;

			#pragma target 3.0
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram
			
			#pragma exclude_renderers nomrt
			#pragma multi_compile_lightpass
			#pragma multi_compile _ UNITY_HDR_ON
			#pragma multi_compile _ SRS_SNOW_ON

			#include "UnityPBSLighting.cginc"

			struct VertexData {
			float4 vertex : POSITION;
    		float3 normal : NORMAL;
			};
			struct Interpolators {
				float4 pos : SV_POSITION;
			    float4 uv : TEXCOORD0;
			    float3 ray : TEXCOORD1;
			};

			//UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			
			sampler2D _CameraGBufferTexture0;
			
			sampler2D _CameraGBufferTexture1;
			sampler2D _CameraGBufferTexture2;
			
			#if !defined (SHADOWS_SHADOWMASK)
				sampler2D _CameraGBufferTexture4;
			#endif
			
			#include "UnityCG.cginc"
			#include "UnityDeferredLibrary.cginc"
			#include "UnityStandardUtils.cginc"
			#include "UnityGBuffer.cginc"
			#include "UnityStandardBRDF.cginc"

			Interpolators VertexProgram (VertexData v) {
				Interpolators i;
				i.pos = UnityObjectToClipPos(v.vertex);
				i.uv = ComputeScreenPos(i.pos);
    			i.ray = lerp(
					UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1),
					v.normal,
					_LightAsQuad
				);
				return i;
			}

			#if defined(SRS_SNOW_ON)
				#include_with_pragmas "../CGIncludes/SRS_DeferredShading.cginc"
			#else

			half4 CalculateLight (unity_v2f_deferred i)
			{
			    float3 wpos;
			    float2 uv;
			    float atten, fadeDist;
			    UnityLight light;
			    UNITY_INITIALIZE_OUTPUT(UnityLight, light);
			    UnityDeferredCalculateLightParams (i, wpos, uv, light.dir, atten, fadeDist);
			
			    light.color = _LightColor.rgb * atten;
			
			    // unpack Gbuffer
			    half4 gbuffer0 = tex2D (_CameraGBufferTexture0, uv);
			    half4 gbuffer1 = tex2D (_CameraGBufferTexture1, uv);
			    half4 gbuffer2 = tex2D (_CameraGBufferTexture2, uv);
			    UnityStandardData data = UnityStandardDataFromGbuffer(gbuffer0, gbuffer1, gbuffer2);
			
			    float3 eyeVec = normalize(wpos-_WorldSpaceCameraPos);
			    half oneMinusReflectivity = 1 - SpecularStrength(data.specularColor.rgb);
			
			    UnityIndirect ind;
			    UNITY_INITIALIZE_OUTPUT(UnityIndirect, ind);
			    ind.diffuse = 0;
			    ind.specular = 0;
			
			    half4 res = UNITY_BRDF_PBS (data.diffuseColor, data.specularColor, oneMinusReflectivity, data.smoothness, data.normalWorld, -eyeVec, light, ind);
			
			    return res;
			}
			
			#ifdef UNITY_HDR_ON
			half4
			#else
			fixed4
			#endif
			FragmentProgram (Interpolators i) : SV_Target
			{
			    half4 c = CalculateLight(i);
			    #ifdef UNITY_HDR_ON
			    return c;
			    #else
			    return exp2(-c);
			    #endif
			}

			#endif

			ENDCG
		}

		// Pass 2: Final decode pass.
		// Used only with HDR off, to decode the logarithmic buffer into the main RT
		Pass 
			{
			    ZTest Always Cull Off ZWrite Off
			    Stencil {
			        ref [_StencilNonBackground]
			        readmask [_StencilNonBackground]
			        // Normally just comp would be sufficient, but there's a bug and only front face stencil state is set (case 583207)
			        compback equal
			        compfront equal
			    }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers nomrt

			#include "UnityCG.cginc"

			sampler2D _LightBuffer;
			struct v2f {
			    float4 vertex : SV_POSITION;
			    float2 texcoord : TEXCOORD0;
			};

			v2f vert (float4 vertex : POSITION, float2 texcoord : TEXCOORD0)
			{
			    v2f o;
			    o.vertex = UnityObjectToClipPos(vertex);
			    o.texcoord = texcoord.xy;
			#ifdef UNITY_SINGLE_PASS_STEREO
			    o.texcoord = TransformStereoScreenSpaceTex(o.texcoord, 1.0f);
			#endif
			    return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
			    return -log2(tex2D(_LightBuffer, i.texcoord));
			}
			ENDCG
		}
	}
}