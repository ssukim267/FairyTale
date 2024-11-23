#if UNITY_EDITOR
namespace NOT_Lonely.Weatherade
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    using UnityEditor;

    [CustomEditor(typeof(SRS_TraceMaskGenerator))]
    public class SRS_TraceMaskGenerator_editor : Editor
    {
        private SRS_TraceMaskGenerator srs_traceMaskGenerator;

        private SerializedProperty allowEditInPlaymode;
        private SerializedProperty maskRes;
        private SerializedProperty blurKernelSize;
        private SerializedProperty normalSpread;
        private SerializedProperty depthBias;
        private SerializedProperty sourceMaskBrightness;
        private SerializedProperty borderIntensity;
        private SerializedProperty indent;
        private SerializedProperty decaySpeed;
        private SerializedProperty noiseIntensity;
        private SerializedProperty noiseTiling;
        private SerializedProperty areaFalloffHardness;
        private SerializedProperty traceSurfLayermask;
        private SerializedProperty traceObjsLayermask;
        private SerializedProperty maskMtl;
        private SerializedProperty normalsMtl;

        private void OnEnable()
        {
            allowEditInPlaymode = serializedObject.FindProperty("allowEditInPlaymode");
            maskRes = serializedObject.FindProperty("maskRes");
            blurKernelSize = serializedObject.FindProperty("blurKernelSize");
            normalSpread = serializedObject.FindProperty("normalSpread");
            depthBias = serializedObject.FindProperty("depthBias");
            sourceMaskBrightness = serializedObject.FindProperty("sourceMaskBrightness");
            borderIntensity = serializedObject.FindProperty("borderIntensity");
            indent = serializedObject.FindProperty("indent");
            decaySpeed = serializedObject.FindProperty("decaySpeed");
            noiseIntensity = serializedObject.FindProperty("noiseIntensity");
            noiseTiling = serializedObject.FindProperty("noiseTiling");
            areaFalloffHardness = serializedObject.FindProperty("areaFalloffHardness");
            traceSurfLayermask = serializedObject.FindProperty("traceSurfLayermask");
            traceObjsLayermask = serializedObject.FindProperty("traceObjsLayermask");

            maskMtl = serializedObject.FindProperty("traceMaskCalcMtl");
            normalsMtl = serializedObject.FindProperty("traceNormalCalcMtl");
        }

        public override void OnInspectorGUI()
        {
            srs_traceMaskGenerator = target as SRS_TraceMaskGenerator;

            if (NL_Styles.lineB == null || NL_Styles.lineB.normal.background == null) NL_Styles.GetStyles();

            NL_Utilities.DrawCenteredBoldHeader("WEATHERADE TRACE MASK GENERATOR");

            EditorGUI.BeginChangeCheck();

            if (!srs_traceMaskGenerator.enabled) EditorGUILayout.HelpBox("Weatherade Trace Renderer does not work when its component is disabled.", MessageType.Warning);
            
            else if (Application.isPlaying)
            {
                if(allowEditInPlaymode.boolValue) EditorGUILayout.HelpBox("Some properties are not editable in the Play Mode.", MessageType.Warning);
                else EditorGUILayout.HelpBox("'Allow Edit in Play Mode' is disabled. Enable it to edit properties in Play Mode.", MessageType.Warning);
            }

            EditorGUI.BeginDisabledGroup(!srs_traceMaskGenerator.enabled);
            GUILayout.BeginVertical(NL_Styles.lineB);
            EditorGUILayout.PropertyField(allowEditInPlaymode, new GUIContent("Allow Edit in Playmode", "If enabled you can edit the settings in playmode through the inspector UI. Useful for fine tuning. Note: this does not affects on in-build playing."));
            GUILayout.EndVertical();

            EditorGUI.BeginDisabledGroup(Application.isPlaying && !allowEditInPlaymode.boolValue);
            
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            GUILayout.BeginVertical(NL_Styles.lineA);
            EditorGUILayout.PropertyField(maskRes, new GUIContent("Resolution", "The resolution of the generated traces mask texture."));
            GUILayout.EndVertical();

            EditorGUI.EndDisabledGroup();

            GUILayout.BeginVertical(NL_Styles.lineB);
            EditorGUILayout.PropertyField(sourceMaskBrightness, new GUIContent("Mask Intensity", "Source mask intensity multiplier."));
            GUILayout.EndVertical();

            GUILayout.BeginVertical(NL_Styles.lineA);
            EditorGUILayout.PropertyField(blurKernelSize, new GUIContent("Blur Kernel Size", "Amount of blur applied to the initial trace mask."));
            GUILayout.EndVertical();

            GUILayout.BeginVertical(NL_Styles.lineB);
            EditorGUILayout.PropertyField(normalSpread, new GUIContent("Normal Spread", "Amount of sample offset when generating the trace normal map."));
            GUILayout.EndVertical();

            GUILayout.BeginVertical(NL_Styles.lineA);
            EditorGUILayout.PropertyField(depthBias, new GUIContent("Intersection Bias", "The Intersection offset between Trace Objects and Surfaces."));
            GUILayout.EndVertical();

            GUILayout.BeginVertical(NL_Styles.lineB);
            EditorGUILayout.PropertyField(indent, new GUIContent("Depth", "Depth of the trace. If set to 1, the snow will be pushed down to the surface of the ground."));
            GUILayout.EndVertical();
           
            GUILayout.BeginVertical(NL_Styles.lineA);
            EditorGUILayout.PropertyField(borderIntensity, new GUIContent("Border Height", "Height of the outline border around the trace."));
            GUILayout.EndVertical();
 

            GUILayout.BeginVertical(NL_Styles.lineB);
            EditorGUILayout.PropertyField(noiseIntensity, new GUIContent("Noise", "Noise intensity on the trace."));
            GUILayout.EndVertical();

            if (noiseIntensity.floatValue > 0)
            {
                GUILayout.BeginHorizontal(NL_Styles.lineB);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(noiseTiling, new GUIContent("Tiling", "Noise tiling."));
                EditorGUI.indentLevel--;
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginVertical(NL_Styles.lineA);
            EditorGUILayout.PropertyField(areaFalloffHardness, new GUIContent("Area Falloff Hardness", "A soft mask that prevents the simple disappearance of traces at the border of the coverage volume."));
            GUILayout.EndVertical();

            GUILayout.BeginVertical(NL_Styles.lineB);
            EditorGUILayout.PropertyField(decaySpeed, new GUIContent("Decay Speed", "How fast the traces will dissapear. Useful for creating effects such as snowstorms."));
            GUILayout.EndVertical();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            GUILayout.BeginVertical(NL_Styles.lineA);
            EditorGUILayout.PropertyField(traceSurfLayermask, new GUIContent("Surfaces", "The layer mask of the surfaces to be included in the process of generating the trace mask. It is recommended that only ground surfaces be specified."));
            GUILayout.EndVertical();

            GUILayout.BeginVertical(NL_Styles.lineA);
            EditorGUILayout.PropertyField(traceObjsLayermask, new GUIContent("Tracer Objects", "The layer mask of objects, that will generate traces. It is recommended that only characters or other valuable dynamic objects be specified."));
            GUILayout.EndVertical();

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                srs_traceMaskGenerator.ValidateValues();
            }
        }
    }
}
#endif
