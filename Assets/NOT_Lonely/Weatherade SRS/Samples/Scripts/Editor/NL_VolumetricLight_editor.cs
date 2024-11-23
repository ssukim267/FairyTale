using NOT_Lonely.Weatherade;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

[CustomEditor(typeof(NL_VolumetricLight))]
[CanEditMultipleObjects]
public class NL_VolumetricLight_editor : Editor
{
    SerializedProperty realtimeUpdate;
    SerializedProperty intensityMultiplier;
    SerializedProperty rangeMultiplier;
    SerializedProperty beamStartRadius;
    SerializedProperty maskHardness;
    SerializedProperty noiseIntensity;
    SerializedProperty noiseTiling;
    SerializedProperty noiseSpeed;
    SerializedProperty intersectionsDepthFade;
    SerializedProperty cameraFadeDistance;
    SerializedProperty zOffset;
    SerializedProperty noiseTexture;

    private NL_VolumetricLight nl_volumetricLight;
   
    static bool sizeProps = true;
    static bool intensityProps = true;
    static bool fadeProps = true;
    static bool noiseProps = true;

    public GUIStyle header;
    public GUIStyle lineA;
    public GUIStyle lineB;

    private void OnEnable()
    {
        realtimeUpdate = serializedObject.FindProperty("realtimeUpdate");
        intensityMultiplier = serializedObject.FindProperty("intensityMultiplier");
        rangeMultiplier = serializedObject.FindProperty("rangeMultiplier");
        beamStartRadius = serializedObject.FindProperty("beamStartRadius");
        maskHardness = serializedObject.FindProperty("maskHardness");
        noiseIntensity = serializedObject.FindProperty("noiseIntensity");
        noiseTiling = serializedObject.FindProperty("noiseTiling");
        noiseSpeed = serializedObject.FindProperty("noiseSpeed");
        intersectionsDepthFade = serializedObject.FindProperty("intersectionsDepthFade");
        cameraFadeDistance = serializedObject.FindProperty("cameraFadeDistance");
        zOffset = serializedObject.FindProperty("zOffset");
        noiseTexture = serializedObject.FindProperty("noiseTexture");

        UpdateStyles();

        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    void OnUndoRedo()
    {
        if (nl_volumetricLight == null || !Selection.Contains(nl_volumetricLight.gameObject)) return;
        nl_volumetricLight.ValidateValues();
    }

    private void UpdateStyles()
    {
        lineA = GetBackgroundStyle(new Color(0, 0, 0, 0));
        lineB = EditorGUIUtility.isProSkin ? GetBackgroundStyle(new Color(1, 1, 1, 0.05f)) : GetBackgroundStyle(new Color(1, 1, 1, 0.2f));
        header = EditorGUIUtility.isProSkin ? GetBackgroundStyle(new Color(1, 1, 1, 0.15f)) : GetBackgroundStyle(new Color(1, 1, 1, 0.5f));
    }

    private GUIStyle GetBackgroundStyle(Color color)
    {
        GUIStyle style = new GUIStyle();
        Texture2D texture = new Texture2D(1, 1);

        texture.SetPixel(0, 0, color);
        texture.Apply();

        style.normal.background = texture;
        return style;
    }

    public override void OnInspectorGUI()
    {
        nl_volumetricLight = target as NL_VolumetricLight;

        if (nl_volumetricLight == null) return;

        EditorGUI.BeginChangeCheck();
        GUILayout.Space(5);

        if (nl_volumetricLight.lightComp.type != LightType.Spot && nl_volumetricLight.lightComp.type != LightType.Point)
        {
            GUI.contentColor = Color.yellow;
            EditorGUILayout.LabelField("Only Point and Spot lights are supported.", EditorStyles.centeredGreyMiniLabel);
            GUI.contentColor = Color.white;
            nl_volumetricLight.DisableVolumetric();
            return;
        }

        float currentInspectorWidth = EditorGUIUtility.currentViewWidth - 24 - 8;
        float offset = currentInspectorWidth - (currentInspectorWidth / 1.6f);

        GUILayout.BeginHorizontal(lineA);
        EditorGUILayout.PropertyField(realtimeUpdate, new GUIContent("Update Every Frame"));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal(header);
        GUILayout.Space(currentInspectorWidth / 2 - 39);
        sizeProps = EditorGUILayout.Foldout(sizeProps, "SIZE", true);
        GUILayout.EndHorizontal();

        if (sizeProps)
        {
            GUILayout.BeginHorizontal(lineA);
            EditorGUILayout.PropertyField(rangeMultiplier, new GUIContent("Range Multiplier"));
            GUILayout.EndHorizontal();

            if(nl_volumetricLight.lightComp.type == LightType.Spot)
            {
                GUILayout.BeginHorizontal(lineB);
                EditorGUILayout.PropertyField(beamStartRadius, new GUIContent("Start Radius"));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
        }

        GUILayout.Space(1);

        GUILayout.BeginHorizontal(header);
        GUILayout.Space(currentInspectorWidth / 2 - 39);
        intensityProps = EditorGUILayout.Foldout(intensityProps, "INTENSITY", true);
        GUILayout.EndHorizontal();

        if (intensityProps)
        {
            GUILayout.BeginHorizontal(lineA);
            EditorGUILayout.PropertyField(intensityMultiplier, new GUIContent("Multiplier"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(lineB);
            EditorGUILayout.PropertyField(maskHardness, new GUIContent("Mask Hardness"));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        GUILayout.Space(1);

        GUILayout.BeginHorizontal(header);
        GUILayout.Space(currentInspectorWidth / 2 - 39);
        fadeProps = EditorGUILayout.Foldout(fadeProps, "FADE", true);
        GUILayout.EndHorizontal();

        if (fadeProps)
        {
            GUILayout.BeginVertical(lineA);    
            EditorGUILayout.PropertyField(cameraFadeDistance, new GUIContent("Distance Fade"));
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal(lineB);
            EditorGUILayout.PropertyField(intersectionsDepthFade, new GUIContent("Intersections Fade"));
            GUILayout.EndHorizontal();

            if(nl_volumetricLight.lightComp.type == LightType.Point)
            {
                GUILayout.BeginHorizontal(lineA);
                EditorGUILayout.PropertyField(zOffset, new GUIContent("Z Offset"));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
        }

        GUILayout.Space(1);

        GUILayout.BeginHorizontal(header);
        GUILayout.Space(currentInspectorWidth / 2 - 39);
        noiseProps = EditorGUILayout.Foldout(noiseProps, "NOISE", true);
        GUILayout.EndHorizontal();

        if (noiseProps)
        {
            GUILayout.BeginHorizontal(lineA);
            EditorGUILayout.PropertyField(noiseTexture, new GUIContent("Texture"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(lineB);
            EditorGUILayout.PropertyField(noiseIntensity, new GUIContent("Intensity"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(lineA);
            EditorGUILayout.PropertyField(noiseTiling, new GUIContent("Tiling"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(lineB);
            EditorGUILayout.PropertyField(noiseSpeed, new GUIContent("Speed"));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        GUILayout.Space(1);

        Undo.RecordObject(nl_volumetricLight, $"{nl_volumetricLight.name} value changed");

        nl_volumetricLight.ValidateValues();

        if (EditorGUI.EndChangeCheck())
        {
            nl_volumetricLight.UpdateValues();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
