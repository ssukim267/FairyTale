#if UNITY_EDITOR
namespace NOT_Lonely.Weatherade
{
    using UnityEditor;
    using UnityEngine;

    public static class NL_Styles
    {
        public static GUIStyle header;
        public static GUIStyle header1;
        public static GUIStyle header2;
        public static GUIStyle lineA;
        public static GUIStyle lineB;
        public static GUIStyle centeredTextField;
        public static GUIStyle centeredBoldLabel;
        public static GUIStyle miniEnumBtn;
        public static GUIStyle foldoutSub;
        public static GUIStyle noPaddingButton;

        public static GUIContent renderingModeText = new GUIContent("Rendering Mode");
        public static GUIContent cullModeText = new GUIContent("Cull Mode", "Which side of the triangle should be culled by the shader.");
        public static GUIContent albedoText = new GUIContent("Albedo", "Albedo (RGB) and Opacity or Smoothness (A)");
        public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Alpha cutout threshold");
        public static GUIContent alphaToCoverageText = new GUIContent("Alpha To Mask", "aka 'Alpha to Coverage'. Use MSAA to make alpha edge softer. \nNOTE: it works only in Forward rendering. \nThis may look incorrect with the post processing AO.");
        public static GUIContent useBlueNoiseDitherText = new GUIContent("Blue Noise LOD Cross-Fade", "Use blue noise for the LOD cross-fade feature instead of standard Unity's static pattern. It's recommended to use this feature with the TAA for the best result.");
        public static GUIContent metallicMapText = new GUIContent("Metallic", "Metallic (R), Occlusion (G), Smoothness (A)");
        public static GUIContent smoothnessText = new GUIContent("Smoothness", "Smoothness value");
        public static GUIContent smoothnessScaleText = new GUIContent("Smoothness Scale", "Smoothness scale factor");
        public static GUIContent smoothnessMapChannelText = new GUIContent("Source", "Smoothness texture source");
        public static GUIContent occlusionMapChannelText = new GUIContent("Source", "Occlusion map source");
        public static GUIContent normalMapText = new GUIContent("Normal Map", "Normal Map");
        public static GUIContent occlusionText = new GUIContent("Occlusion", "Occlusion (G)");
        public static GUIContent emissionText = new GUIContent("Emission", "Emission (RGB)");
        public static GUIContent emissionUseMapText = new GUIContent("Use Emission Map", "Use RGB map for emission");
        public static GUIContent specHighlightsText = new GUIContent("Specular Highlights", "Use specular highlights.");
        public static GUIContent glossyReflectionsText = new GUIContent("Reflections", "Use glossy reflections.");
        public static GUIContent distanceFadeStartText = new GUIContent("Start Distance", "The distance at which the baked texture starts to appear.");
        public static GUIContent distanceFadeFalloffText = new GUIContent("Falloff", "The size of the fading gradient for the original/baked texture. The higher the value, the softer the blend will be.");
        public static GUIContent coverageText = new GUIContent("Enable Coverage", "If disabled, the shader will act as a Unity Standard shader.");
        public static GUIContent paintableCoverageText = new GUIContent("Paintable Coverage", "Vertex colors will be used for regular MeshRenderes, and Terrains will use an additional texture-mask that can be drawn with Total Brush.");
        public static GUIContent useAveragedNormalsText = new GUIContent("Use Average Normals", "If this option is enabled, the shader will use an additional set of normals stored in the UV4 channel. Generate these normals by clicking the button on the right, or go to the Total Brush window and click the same button there.");
        public static GUIContent coverageTex0Text = new GUIContent("Coverage Texture", "Texture which is used to build the Coverage surface. Channel mapping used: Normals (R, G), Height (B), Smoothness (A)");
        public static GUIContent stochasticText = new GUIContent("Stochastic Sampling", "Use stochastic sampling to avoid visible tiling artifacts. This option has additional performance cost.");
        public static GUIContent threeTexModeText = new GUIContent("Three Textures", "Blend up to 3 different coverage textures. Only available when 'Paintable Coverage' is enabled.");
        public static GUIContent coverageAmountText = new GUIContent("Amount", "Amount of the Coverage.");
        public static GUIContent covTriBlendContrastText = new GUIContent("Triplanar Blend Hardness", "Blend hardness between the triplanar projections.");
        public static GUIContent cov0SmoothnessText = new GUIContent("Smoothness", "Coverage smoothness range. Remaps the 'Coverage Texture' alpha channel, which is used as smoothness.");
        public static GUIContent coverageNormalScale0Text = new GUIContent("Normal Scale", "Coverage normal scale.");
        public static GUIContent heightMap0ContrastText = new GUIContent("Height Contrast", "Coverage height map contrast.");
        public static GUIContent coverageColorText = new GUIContent("Color", "Color of Coverage.");
        public static GUIContent enhanceRemap0Text = new GUIContent("Color Enhance Remap", "Remap the color enhance mask ('Coverage Texture' (A) + 'Detail Texture' (B) if used).");
        public static GUIContent snowAOIntensityText = new GUIContent("AO", "Snow ambient occlusion intensity.");
        public static GUIContent smContrastText = new GUIContent("Smoothness Contrast", "Contrast of the Coverage smoothness.");
        public static GUIContent microReliefText = new GUIContent("Micro Relief", "Micro relief of the Coverage surface which is generated from the Coverage Texture (B) channel.");
        public static GUIContent microReliefFadeText = new GUIContent("Fade Distance", "Fade distance of the micro relief. Useful to reduce 'sandy' effect on far distances.");
        public static GUIContent baseCoverageNormalsBlendText = new GUIContent("Base/Coverage Normals Blend", "Blend factor between the base normal map and the Coverage normal map, which is stored in the Coverage Texture RG channels.");
        public static GUIContent coverageNormalsOverlayText = new GUIContent("Coverage Normals Overlay", "Linear interpolation value between the base normal map and the Coverage normal map.");
        public static GUIContent coverageTilingText = new GUIContent("Tiling", "Tiling value of the Coverage Texture.");
        public static GUIContent coverageAreaMaskRangeText = new GUIContent("Mask Range", "Range of the coverage mask values. 1 = the most soft transitions.");
        public static GUIContent coverageAreaBiasText = new GUIContent("Bias", "The bias of the Coverage area.");
        public static GUIContent coverageLeakReductionText = new GUIContent("Leak Reduction", "Reducing coverage leaks at the cost of increasing mask hardness.");
        public static GUIContent precipitationDirOffsetText = new GUIContent("Direction Offset", "The offset from the original direction. Values greated than 0 will do more strong direction dependent mask.");
        public static GUIContent precipitationDirRangeText = new GUIContent("Direction Range", "How much the Coverage is dependend on the precipitation direction. The precipitation direction is given by the Snow & Rain Instance transform.");
        public static GUIContent blendByNormalsStrengthText = new GUIContent("Strength", "Amount of base normal map influence on Coverage blending.");
        public static GUIContent blendByNormalsPowerText = new GUIContent("Power", "Power of the normal map blending.");
        
        //Displacement
        public static GUIContent displacementText = new GUIContent("Displacement", "Snowdrift vertex displacement.");
        public static GUIContent coverageDisplacementText = new GUIContent("Coverage Displacement", "The amount of displacement add to the mesh vertices.");
        public static GUIContent coverageDisplacementOffsetText = new GUIContent("Offset", "Offsets the black point of the coverage mask gradients.");
        public static GUIContent heightMap0LODText = new GUIContent("Heigthmap LOD", "Set the height map MIP level. Use higher values to get rid of noisy displacement.");


        //Detail map
        public static GUIContent useCoverageDetailText = new GUIContent("Detail", "Apply detail mapping to the coverage surface.");
        public static GUIContent coverageDetailTexText = new GUIContent("Detail Texture", "Texture used for the detail mapping. Channel mapping used: Normals (R, G), Smoothness (B).");
        public static GUIContent detailTilingText = new GUIContent("Tiling", "Tiling of the 'Detail Texture'.");
        public static GUIContent detailTexRemapText = new GUIContent("Remap", "Remap the texture values, used for the smoothness/color enhance.");
        public static GUIContent detailNormalScaleText = new GUIContent("Normal Scale", "Normal scale of the details.");
        public static GUIContent detailDistanceText = new GUIContent("Distance", "The distance from the camera where the details will fade out.");

        //Sparkle
        public static GUIContent sparkleText = new GUIContent("Sparkle", "Enable Sparkle effect.");
        public static GUIContent sssText = new GUIContent("SSS", "Enable Sub-Surface Scattering effect.");
        public static GUIContent sparkleTexText = new GUIContent("Mask", "Optional texture which is used as a sparkle effect mask. If no texture is provided, the alpha channel of the main coverage texture will be used.");
        public static GUIContent sparkleTexSSText = new GUIContent("Screen-Space Mask Source", "Use the alpha channel of the main coverage texture or an additional texture as the screen-space sparkle mask.");
        public static GUIContent sparkleTexLSText = new GUIContent("Local-Space Mask Source", "The source for the sparkle mask.");
        public static GUIContent localSparkleTilingText = new GUIContent("LS Tiling", "Local-Space tiling of the sparkle mask. Available only when 'Local-Space Mask Source' set to 'Sparkle Mask'. The main texture tiling is used otherwise.");
        public static GUIContent sparklesAmountText = new GUIContent("Amount", "Amount of the snow sparkle effect.");
        public static GUIContent sparkleDistFalloffText = new GUIContent("Distance Falloff", "Max draw distance of the sparkle effect. Decrease this value to prevent artifacts on far distances.");
        public static GUIContent sparklesBrightnessText = new GUIContent("Brightness", "Brightness of the snow sparkle effect.");
        public static GUIContent screenSpaceSparklesTilingText = new GUIContent("SS Tiling", "Screen-Space tiling of the snow sparkle mask.");
        public static GUIContent sparkleHighlightMaskExpText = new GUIContent("Highlight Mask Expansion", "Expansion of the highlight mask.");
        public static GUIContent highlightBrightness0Text = new GUIContent("Highlight Brightness", "Brightness of the highlight mask.");
        public static GUIContent sparkleLightmapMaskPowText = new GUIContent("Lightmap Mask Power", "Power of the lightmap mask. Values greater than 0 prevent the effect of sparkles in dark areas.");
       
        public static GUIContent emissionMaskingText = new GUIContent("Emission Masking", "Mask the emission effect by the Coverage effect.");
        public static GUIContent maskCoverageByAlphaText = new GUIContent("Mask by Alpha", "Mask the Coverage effect by the alpha channel.");
        public static GUIContent covDefObjsLayermaskText = new GUIContent("Impact Objects", "Layer mask of objects affecting deformation. Select player and other valuable dynamic objects here.");
        public static GUIContent sss_intensityText = new GUIContent("Intensity", "Intensity of the subsurface scattering effect.");
        public static GUIContent sssMaskRemap0Text = new GUIContent("SSS Remap", "Remap the SSS mask values.");
        public static GUIContent colorEnhanceText = new GUIContent("Color Enhance", "Enhance the snow surface color. The Coverage Texture alpha is used as a source.");
        
        //Traces
        public static GUIContent tracesText = new GUIContent("Traces", "Render realtime traces from dynamic objects.");
        public static GUIContent tracesDepthText = new GUIContent("Depth");
        public static GUIContent tracesColorText = new GUIContent("Color", "Multiplier color of the trace.");
        public static GUIContent tracesBlendFactorText = new GUIContent("Blend", "Blend factor between the coverage and Color.");
        public static GUIContent tracesBaseBlend0Text = new GUIContent("Base/Snow", "Blend factor between the base surface and coverage.");
        public static GUIContent tracesColorBlendRangeText = new GUIContent("Range", "Color blend range.");
        public static GUIContent tracesNormalScaleText = new GUIContent("Normal Scale", "The normals scale of the traces.");
        public static GUIContent traceDetailText = new GUIContent("Use Detail Texture", "If enabled, the traces will use the global detail texture.");

        //Tessellation
        public static GUIContent tessellationText = new GUIContent("Tessellation", "Enable edge length tessellation.");
        public static GUIContent tessEdgeLText = new GUIContent("Edge Length", "Maximum length of the edge.");
        public static GUIContent tessFactorSnowText = new GUIContent("Snow Factor", "Multiplier for the overall snow surface tessellation."); 
        public static GUIContent tessSnowdriftRangeText = new GUIContent("Snowdrift Range", "Range of snowdrift tessellation. Use it to expand/contract tessellation areas on snowdrift slopes.");
        public static GUIContent tessMaxDispText = new GUIContent("Frustum Culling Offset", "Maximum displacement/distance from camera's clip planes on which tessellation should not be applied.");

        public static GUIContent primaryMasksText = new GUIContent("Primary Masks", "The primary masks used for the wet surface. R - puddles, G - drips, B - drips distortion noise. Note: for best results, use a 32-bit original texture.");
        public static GUIContent ripplesTexText = new GUIContent("Ripples", "Ripples texture array. RG - puddle ripples normal, B - rain spots.");
        public static GUIContent paintableWetnessText = new GUIContent("Paintable", "Paint or erase the wetness mask (R - wetness, G - puddles, B - ripples & spots, A - drips). This feature uses the vertex colors for the regular MeshRenderes, and an additional texture-mask for the Terrains. Both can be painted with Total Brush.");
        public static GUIContent wetColorText = new GUIContent("Color", "Wet surface color. The base surface color is multiplied by this color. This color is also affected by the 'Amount' value.");
        public static GUIContent wetnessAmountText = new GUIContent("Amount", "The higher the value, the wetter the surface will be.");
        public static GUIContent puddlesAmountText = new GUIContent("Amount", "The amount of puddles on the surface.");
        public static GUIContent puddlesMultText = new GUIContent("Color Multiplier", "Puddles color multiplier. Use values lower than 1 to make the puddles look more deep.");
        public static GUIContent puddlesRangeText = new GUIContent("Range", "Puddle mask range. Adjust it to change the spread and sharpness of the puddles.");
        public static GUIContent puddlesBlendStrengthText = new GUIContent("Blend", "Blend puddles with the base surface using the surface normal.");
        public static GUIContent puddlesBlendContrastText = new GUIContent("Contrast", "The contrast of the base surface.");     
        public static GUIContent puddlesTilingText = new GUIContent("Tiling", "The puddles tiling.");
        public static GUIContent puddlesSlopeText = new GUIContent("Slope", "The maximum slope of the surface where the puddles can appear. Use low values to avoid puddles on hillsides.");

        //Ripples and Spots
        public static GUIContent ripplesText = new GUIContent("Ripples and Spots", "Enable a ripple effect on puddles and a spot effect on other surfaces.");
        public static GUIContent ripplesAmountText = new GUIContent("Amount", "Amount of ripples and spots. Increasing this number hits performance.");
        public static GUIContent ripplesFramesCountText = new GUIContent("Total Frames", "The total number of frames in the riples & spots texture array. WARNING: this value must exactly match the number of texture frames in the 'Texture Array' in order to play without errors.");
        public static GUIContent ripplesIntensityText = new GUIContent("Ripples Intensity", "The intensity of the ripples on the puddles.");
        public static GUIContent ripplesTilingText = new GUIContent("Tiling", "The tiling of the ripples & spots.");
        public static GUIContent ripplesFPSText = new GUIContent("FPS", "The speed of the ripples & spots playback in frames per second.");
        public static GUIContent spotsAmountText = new GUIContent("Spots Expansion", "Intensity of spot expansion. Increase this value to get larger and denser rain spots.");
        public static GUIContent spotsIntensityText = new GUIContent("Spots Intensity", "The intensity of the rain spots.");

        public static GUIContent dripsText = new GUIContent("Drips", "Enable a flowing drips effect on vertical surfaces.");
        public static GUIContent dripsIntensityText = new GUIContent("Drips Intensity", "Intensity of the drips.");
        public static GUIContent dripsSpeedText = new GUIContent("Speed", "Speed of the drips.");
        public static GUIContent dripsTilingText = new GUIContent("Tiling", "Tiling of the drips.");
        public static GUIContent distortionAmountText = new GUIContent("Distortion", "Intensity of the drips distortion. Increase this value to get more curved drip paths.");
        public static GUIContent distortionTilingText = new GUIContent("Tiling", "Tiling of the drips distortion.");
        public static GUIContent debugOnText = new GUIContent("Enable", "Enable debug mode.");
        public static GUIContent debugOptionText = new GUIContent("Option", "Debug option.");

        public static void GetStyles()
        {
            lineA = GetBackgroundStyle(new Color(0, 0, 0, 0), "lineA");
            lineB = EditorGUIUtility.isProSkin ? GetBackgroundStyle(new Color(1, 1, 1, 0.05f), "lineB") : GetBackgroundStyle(new Color(1, 1, 1, 0.2f), "lineB");
            header = EditorGUIUtility.isProSkin ? GetBackgroundStyle(new Color(1, 1, 1, 0.25f), "header", true) : GetBackgroundStyle(new Color(1, 1, 1, 0.5f), "header", true);
            header1 = EditorGUIUtility.isProSkin ? GetBackgroundStyle(new Color(1, 1, 1, 0.2f), "header1", true) : GetBackgroundStyle(new Color(1, 1, 1, 0.5f), "header1", true);
            header2 = EditorGUIUtility.isProSkin ? GetBackgroundStyle(new Color(0.5f, 0.5f, 0.5f, 0.15f), "header2", false) : GetBackgroundStyle(new Color(0.5f, 0.5f, 0.5f, 0.5f), "header2", false);
            centeredTextField = GetCenteredFieldStyle();
            centeredBoldLabel = GetCenteredBoldLabelStyle();
            miniEnumBtn = GetMiniEnumStyle();
            foldoutSub = GetFoldoutSubStyle();
            noPaddingButton = GetNoPaddingButtonStyle();
        }

        private static GUIStyle GetBackgroundStyle(Color color, string name = "customStyle", bool bold = false)
        {
            GUIStyle style = new GUIStyle();
            Texture2D texture = new Texture2D(1, 1);

            texture.SetPixel(0, 0, color);
            texture.Apply();

            style.normal.background = texture;
            style.name = name;
            if (bold) style.fontStyle = FontStyle.Bold;
            return style;
        }

        public static GUIStyle GetFoldoutSubStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };

            return style;
        }

        public static GUIStyle GetCenteredFieldStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleCenter,
            };

            return style;
        }

        public static GUIStyle GetCenteredBoldLabelStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            return style;
        }

        public static GUIStyle GetNoPaddingButtonStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold, 
            };

            return style;
        }

        public static GUIStyle GetMiniEnumStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.textField)
            {
                fixedWidth = 12,
                fixedHeight = 15,
                overflow = new RectOffset(0, 0, -3, 0)
            };

            style.normal.textColor = new Color(0, 0, 0, 0);
            style.normal.background = (Texture2D)EditorGUIUtility.IconContent("d_icon dropdown").image;

            return style;
        }
    }
}
#endif
