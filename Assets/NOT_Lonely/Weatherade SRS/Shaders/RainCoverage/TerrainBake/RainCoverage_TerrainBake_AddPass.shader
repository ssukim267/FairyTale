// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Hidden/NOT_Lonely/Weatherade/RainTerrainBakeAddPass" {
    Properties{
        [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
        _MapID("MapID", float) = 0 //needed to set a particular map when baking the distant map
    }
    SubShader
    {
		Tags
		{
			"TerrainCompatible" = "True"
			"Queue" = "Geometry-99"
    	    "RenderType" = "Opaque"
		}
		LOD 100
        Pass
        {
            Tags { "LightMode"="ForwardBase" }

			Blend One One

            CGPROGRAM
			#define SRS_TERRAIN
			#define FORWARD_BASE_PASS
			#define TERRAIN_SPLAT_ADDPASS
            
			#pragma shader_feature_local _COVERAGE_ON
			#pragma shader_feature_local_fragment _PAINTABLE_COVERAGE_ON
			#pragma shader_feature_local_fragment _DRIPS_ON
			#pragma shader_feature_local_fragment _STOCHASTIC_ON
			#pragma shader_feature_local _RIPPLES_ON

			#pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag
			
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap   
			
			#include_with_pragmas "../../CGIncludes/SRS_TerrainBake.cginc"
			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"

            ENDCG
        }
    }

    Fallback "Hidden/TerrainEngine/Splatmap/Diffuse-AddPass"
}
