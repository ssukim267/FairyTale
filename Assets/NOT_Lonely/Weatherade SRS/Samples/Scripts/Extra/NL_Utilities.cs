namespace NOT_Lonely.Weatherade
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    public static class NL_Utilities
    {
        public enum RP
        {
            BiRP,
            URP,
            HDRP
        }
        public static RP currentRP = RP.BiRP;

#if UNITY_EDITOR
        public static string GetToolPath(ScriptableObject so)
        {
            var script = MonoScript.FromScriptableObject(so);
            string toolRootPath = AssetDatabase.GetAssetPath(script);
            toolRootPath = toolRootPath.Replace('\\', '/');
            toolRootPath = toolRootPath.Replace("Scripts/Editor/" + script.name + ".cs", "");

            return toolRootPath;
        }

        public static void DrawCenteredBoldHeader(string text)
        {
            Vector2 textSize = NL_Styles.centeredBoldLabel.CalcSize(new GUIContent(text));
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(text, NL_Styles.centeredBoldLabel, GUILayout.MinWidth(textSize.x));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        public static void CalcFoldoutSpace(string text)
        {
            float currentInspectorWidth = EditorGUIUtility.currentViewWidth - 24 - 8;
            float textWidth = EditorStyles.foldout.CalcSize(new GUIContent(text)).x;
            float foldoutWidth = currentInspectorWidth / 2 - (textWidth / 2);
            GUILayout.Space(foldoutWidth);
        }

        public static bool DrawFoldout(bool foldoutVal, string foldoutText)
        {
            GUILayout.BeginHorizontal(NL_Styles.header);

            float inspectorWidth = EditorGUIUtility.currentViewWidth - 24;

            float textWidth = EditorStyles.foldout.CalcSize(new GUIContent(foldoutText)).x;
            float foldoutWidth = inspectorWidth / 2 - (textWidth / 2);

            GUILayout.Space(foldoutWidth);
            foldoutVal = EditorGUILayout.Foldout(foldoutVal, foldoutText, true);

            GUILayout.EndHorizontal();

            return foldoutVal;
        }

        public static bool DrawFoldout(SerializedProperty foldoutProp, string foldoutText)
        {
            GUILayout.BeginHorizontal(NL_Styles.header);

            float inspectorWidth = EditorGUIUtility.currentViewWidth - 24;

            float textWidth = EditorStyles.foldout.CalcSize(new GUIContent(foldoutText)).x;
            float foldoutWidth = inspectorWidth / 2 - (textWidth / 2);

            GUILayout.Space(foldoutWidth);
            foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, foldoutText, true);

            GUILayout.EndHorizontal();

            return foldoutProp.boolValue;
        }

#if USING_URP
        public static void BeginUICategory(string headerText, GUIStyle backgroundStyle, GUIStyle foldoutStyle = null, SerializedProperty foldoutVal = null)
#else
        public static void BeginUICategory(string headerText, GUIStyle backgroundStyle, SerializedProperty foldoutVal = null)
#endif
        {
            if (backgroundStyle != null) GUILayout.BeginVertical(backgroundStyle);
            else GUILayout.BeginVertical();
#if USING_URP
            if(foldoutStyle == null) foldoutStyle = EditorStyles.foldout;
#endif
            if (foldoutVal != null)
            {
                GUILayout.BeginHorizontal(NL_Styles.lineB);
                EditorGUI.indentLevel++;
#if USING_URP
                foldoutVal.boolValue = EditorGUILayout.Foldout(foldoutVal.boolValue, headerText, true, foldoutStyle);
#else
                foldoutVal.boolValue = EditorGUILayout.Foldout(foldoutVal.boolValue, headerText, true, NL_Styles.foldoutSub);
#endif
                EditorGUI.indentLevel--;
                GUILayout.EndHorizontal();

                if(foldoutVal.boolValue) GUILayout.Space(5);
            }
            else
            {
                GUILayout.Label(headerText, EditorStyles.boldLabel);
                GUILayout.Space(5);
            }
        }

        public static void EndUICategory(float space = 5)
        {
            GUILayout.EndVertical();
            GUILayout.Space(space);
        }
#endif

        public static T FindObjectOfType<T>(bool includeInactive) where T : UnityEngine.Object
        {
#if UNITY_2022_2_OR_NEWER
            return (T)UnityEngine.Object.FindAnyObjectByType(typeof(T), includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#else
            return UnityEngine.Object.FindObjectOfType<T>(includeInactive);
#endif

        }

        public static T[] FindObjectsOfType<T>(bool includeInactive) where T : UnityEngine.Object
        {
#if UNITY_2022_2_OR_NEWER
            return UnityEngine.Object.FindObjectsByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
            return UnityEngine.Object.FindObjectsOfType<T>(includeInactive);
#endif
        }

        public static RenderTexture UpdateOrCreateRT(RenderTexture rt, int res, RenderTextureFormat rtFormat, string rtName, int explicitDepthPrecission = -1)
        {
            int depthPrecission;

            if (explicitDepthPrecission == -1)
                depthPrecission = rtFormat == RenderTextureFormat.Depth ? 16 : 0;
            else
                depthPrecission = explicitDepthPrecission;

            if (rt == null)
            {
                rt = new RenderTexture(res, res, depthPrecission, rtFormat, RenderTextureReadWrite.Linear);
                rt.name = rtName;
            }
            else
            {
                rt.Release();
                rt.height = res;
                rt.width = res;
                rt.depth = depthPrecission;
                rt.Create();
            }

            return rt;
        }

        public static float Remap(this float num, float lowVal, float highVal, float min, float max)
        {
            return min + (num - lowVal) * (max - min) / (highVal - lowVal);
        }

        public static Texture2D UpdateGradientTex(Gradient colorGradientA, Gradient colorGradientB, Texture2D gradientTex)
        {
            for (int x = 0; x < gradientTex.width; x++)
            {
                float t = (float)x / gradientTex.width;

                Color gColorA = colorGradientA.Evaluate(t);
                Color gColorB = colorGradientB.Evaluate(t);

                gColorA = gColorA.linear;
                gColorB = gColorB.linear;
                gradientTex.SetPixel(x, 0, gColorA);
                gradientTex.SetPixel(x, 1, gColorB);
            }

            gradientTex.Apply();

            return gradientTex;
        }

        /*
        public static Texture2D UpdateGradientTex(AnimationCurve sizeMin, AnimationCurve sizeMax, AnimationCurve rotationMin, AnimationCurve rotationMax, Texture2D gradientTex)
        {
            for (int x = 0; x < gradientTex.width; x++)
            {
                float t = (float)x / gradientTex.width;

                float sizeMinValue = sizeMin.Evaluate(t);
                float sizeMaxValue = sizeMax.Evaluate(t);
                float rotationMinValue = rotationMin.Evaluate(t);
                float rotationMaxValue = rotationMax.Evaluate(t);

                Color gColor = new Color(sizeMinValue, sizeMaxValue, rotationMinValue, rotationMaxValue);

                gColor = gColor.linear;
                gradientTex.SetPixel(x, 1, gColor);
            }

            gradientTex.Apply();

            return gradientTex;
        }
        */

        public static Vector3 TransformWorldToLocalDir(Transform targetTransform, float x, float y, float z)
        {
            Vector3 transformedVector = targetTransform.InverseTransformDirection(x, -y, z);

            transformedVector.x = -transformedVector.x;
            transformedVector.z = -transformedVector.z;

            transformedVector = transformedVector * -1;

            return transformedVector;
        }

        public static Texture2D UpdateGradientTexWithVelocity(AnimationCurve xVelocityMin, AnimationCurve yVelocityMin, AnimationCurve zVelocityMin, AnimationCurve xVelocityMax, AnimationCurve yVelocityMax, AnimationCurve zVelocityMax, Texture2D gradientTex, Transform emitterTransform = null)
        {
            for (int x = 0; x < gradientTex.width; x++)
            {
                float t = (float)x / gradientTex.width;

                float xVelocityMinVal = xVelocityMin.Evaluate(t);
                float yVelocityMinVal = yVelocityMin.Evaluate(t);
                float zVelocityMinVal = zVelocityMin.Evaluate(t);
                float xVelocityMaxVal = xVelocityMax.Evaluate(t);
                float yVelocityMaxVal = yVelocityMax.Evaluate(t);
                float zVelocityMaxVal = zVelocityMax.Evaluate(t);

                Color gColorRow1 = gradientTex.GetPixel(x, 2);
                Color gColorRow2 = gradientTex.GetPixel(x, 3);

                Vector3 min;
                Vector3 max;

                if (emitterTransform != null)
                {
                    min = TransformWorldToLocalDir(emitterTransform, xVelocityMinVal, yVelocityMinVal, zVelocityMinVal);
                    max = TransformWorldToLocalDir(emitterTransform, xVelocityMaxVal, yVelocityMaxVal, zVelocityMaxVal);
                }
                else
                {
                    min.x = xVelocityMin.Evaluate(t);
                    min.y = yVelocityMin.Evaluate(t);
                    min.z = zVelocityMin.Evaluate(t);
                    max.x = xVelocityMax.Evaluate(t);
                    max.y = yVelocityMax.Evaluate(t);
                    max.z = zVelocityMax.Evaluate(t);
                }

                gColorRow1 = new Color(min.x, min.y, min.z, gColorRow1.a);
                gColorRow2 = new Color(max.x, max.y, max.z, gColorRow2.a);

                /*
                gColorRow1 = new Color(xVelocityMinVal, yVelocityMinVal, zVelocityMinVal, gColorRow1.a);
                gColorRow2 = new Color(xVelocityMaxVal, yVelocityMaxVal, zVelocityMaxVal, gColorRow2.a);
                */

                gradientTex.SetPixel(x, 2, gColorRow1);
                gradientTex.SetPixel(x, 3, gColorRow2);
            }

            gradientTex.Apply();

            return gradientTex;
        }

        public static Texture2D UpdateGradientTexWithSparklesAmount(Texture2D gradientTex, int frequency)
        {
            if (frequency <= 0) return gradientTex;

            for (int x = 0; x < gradientTex.width; x++)
            {
                Color gColor = gradientTex.GetPixel(x, 2);
                gColor.a = 0;
                gradientTex.SetPixel(x, 2, gColor);
            }
            gradientTex.Apply();

            int step = Mathf.Clamp(Random.Range(1, gradientTex.width / frequency), 0, gradientTex.width / 2);

            for (int x = 0; x < gradientTex.width; x+=step)
            {
                Color gColor = gradientTex.GetPixel(x, 2);
                gColor.a = 1;
                gradientTex.SetPixel(x, 2, gColor);

                step = Mathf.Clamp(Random.Range(1, gradientTex.width / frequency), 0, gradientTex.width - x);
            }

            gradientTex.Apply();

            return gradientTex;
        }

        public static Texture2D Generate2dNoise(int width, int height)
        {
            Texture2D noiseTex = new Texture2D(width, height, TextureFormat.RGB24, false, true);
            noiseTex.filterMode = FilterMode.Point;
            noiseTex.anisoLevel = 0;

            Color pixelColor;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    pixelColor = new Color(Random.Range(0.0001f, 1.0f), Random.Range(0.0001f, 1.0f), Random.Range(0.0001f, 1.0f), float.NaN);
                    noiseTex.SetPixel(x, y, pixelColor);
                }
            }

            noiseTex.Apply();

            return noiseTex;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Manual keyword set.
        /// </summary>
        /// <param name="m">Material, where the keyword will be set.</param>
        /// <param name="keyword">Keyword name.</param>
        /// <param name="state">Keyword state to be set.</param>
        public static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }

        public static void SetKeyword(Material mtl, bool value, string propName, string keywordName)
        {
            string ovrdName = propName + "Override";

            if (mtl.HasProperty(ovrdName) && mtl.GetFloat(ovrdName) == 0)
            {
                mtl.SetFloat(propName, value ? 1 : 0);
                SetKeyword(mtl, keywordName, value);
            }     
        }

        public static GUIStyle GetBackgroundStyle(Color color, string name = "customStyle")
        {
            GUIStyle style = new GUIStyle();
            Texture2D texture = new Texture2D(1, 1);

            texture.SetPixel(0, 0, color);
            texture.Apply();

            style.normal.background = texture;
            style.name = name;
            return style;
        }

        public static void DrawArrowGizmo(Vector3 pos, Vector3 direction, Color color, float outlineThickness = 0, Color outlineColor = new Color(), float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            if (float.IsNaN(direction.x) || float.IsNaN(direction.y) || float.IsNaN(direction.z) || direction == Vector3.zero) return;

            Color initColor = Handles.color;

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back;
            Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
            Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;

            Vector3 arrowSourcePos = pos + direction;

            if (outlineThickness > 0)
            {
                Handles.color = outlineColor;
                Handles.DrawLine(pos, arrowSourcePos, outlineThickness);
                Handles.DrawLine(arrowSourcePos, arrowSourcePos + right * arrowHeadLength, outlineThickness);
                Handles.DrawLine(arrowSourcePos, arrowSourcePos + left * arrowHeadLength, outlineThickness);
                Handles.DrawLine(arrowSourcePos, arrowSourcePos + up * arrowHeadLength, outlineThickness);
                Handles.DrawLine(arrowSourcePos, arrowSourcePos + down * arrowHeadLength, outlineThickness);
            }

            Handles.color = color;
            Handles.DrawLine(pos, arrowSourcePos);
            Handles.DrawLine(arrowSourcePos, arrowSourcePos + right * arrowHeadLength);
            Handles.DrawLine(arrowSourcePos, arrowSourcePos + left * arrowHeadLength);
            Handles.DrawLine(arrowSourcePos, arrowSourcePos + up * arrowHeadLength);
            Handles.DrawLine(arrowSourcePos, arrowSourcePos + down * arrowHeadLength);

            Handles.color = initColor;
        }

        public static RP GetCurrentRP()
        {
            currentRP = RP.BiRP;

            if (GraphicsSettings.currentRenderPipeline != null)
            {
                if (GraphicsSettings.currentRenderPipeline.GetType().Name == "UniversalRenderPipelineAsset")
                    currentRP = RP.URP;
                else if (GraphicsSettings.currentRenderPipeline.GetType().Name == "HDRenderPipelineAsset")
                    currentRP = RP.HDRP;
            }

            return currentRP;
        }
#endif
    }
}
