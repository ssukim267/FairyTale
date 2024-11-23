#if UNITY_EDITOR
namespace NOT_Lonely
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Weatherade;

    public class BatchShaderSwapper : EditorWindow
    {
        private GUIStyle header;
        private GUIStyle lineA;
        private GUIStyle lineB;

        private Shader[] shaders;
        private string[] shaderNames;

        private Vector2 scrollPosition;

        public class Selector
        {
            public bool isActive;
            public GUIContent label;

            public Selector(bool isActiveState, GUIContent btnLabel)
            {
                isActive = isActiveState;
                label = btnLabel;
            }
        }

        public Selector[] mode;

        private class ShaderPair
        {
            public Shader originalShader;
            public Shader replacementShader;
            public int replacementID;
            public int originalID;
        }

        private class FilteredMaterial
        {
            public Material mtl;
            public Shader replacementShader;
        }

        private List<ShaderPair> shaderPairs = new List<ShaderPair>();
        private List<Material> mtls = new List<Material>();
        private string[] shaderExceptionNames = new string[3] { "Hidden/InternalErrorShader", "Hidden/NOT_Lonely/Weatherade/Snow Coverage (Tessellation)", "Hidden/NOT_Lonely/Weatherade/Snow Coverage (Terrain-Tessellation)" };

        public enum ReplacementMode
        {
            Selected,
            Scene,
            Project
        }

        private ReplacementMode replacementMode = ReplacementMode.Selected;

        [MenuItem("Window/NOT_Lonely/Batch Shader Swapper")]
        public static void ShowWindow()
        {
            BatchShaderSwapper window = GetWindow<BatchShaderSwapper>("Batch Shader Swapper");
            window.minSize = new Vector2(400, 200);
            window.Show();
        }

        private void GetStyles()
        {
            lineA = GetBackgroundStyle(new Color(0, 0, 0, 0), "lineA");
            lineB = EditorGUIUtility.isProSkin ? GetBackgroundStyle(new Color(1, 1, 1, 0.05f), "lineB") : GetBackgroundStyle(new Color(1, 1, 1, 0.2f), "lineB");
            header = EditorGUIUtility.isProSkin ? GetBackgroundStyle(new Color(0, 0, 0, 0.3f), "header", true) : GetBackgroundStyle(new Color(0, 0, 0, 0.5f), "header", true);
        }

        private GUIStyle GetBackgroundStyle(Color color, string name = "customStyle", bool bold = false)
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

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            //Get all shaders
            shaders = Resources.FindObjectsOfTypeAll<Shader>();

            //Create available shaders list
            List<string> shaderNameList = new List<string>();
            for (int i = 0; i < shaders.Length; i++)
            {
                if (!ValidateShader(shaders[i])) continue;
                shaderNameList.Add(shaders[i].name);
            }
            shaderNames = shaderNameList.ToArray();

            mode = new Selector[3] {
            new Selector(replacementMode == ReplacementMode.Selected, new GUIContent("Selected", "Change shaders only for SELECTED materials.")),
            new Selector(replacementMode == ReplacementMode.Scene, new GUIContent("Scene", "Change shaders only for SCENE materials.")),
            new Selector(replacementMode == ReplacementMode.Project, new GUIContent("Project", "Change shaders for the ALL PROJECT materials."))
        };

            if (replacementMode == ReplacementMode.Selected) UpdateShaderList();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            if (replacementMode == ReplacementMode.Selected) UpdateShaderList();
        }

        private void ModeSelectorArea()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            for (int i = 0; i < mode.Length; i++)
            {
                GUI.color = mode[i].isActive ? Color.gray : Color.white;
                if (GUILayout.Button(mode[i].label, GUILayout.MaxWidth(80), GUILayout.MaxHeight(32)))
                {
                    for (int t = 0; t < mode.Length; t++)
                    {
                        mode[t].isActive = false;
                    }
                    mode[i].isActive = true;
                    replacementMode = (ReplacementMode)i;
                    if (replacementMode == ReplacementMode.Selected) UpdateShaderList();
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        private void OnGUI()
        {
            if (header == null) GetStyles();
            ModeSelectorArea();

            if (replacementMode != ReplacementMode.Selected)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Find Shaders", "Find all used shaders. In 'Scene' mode it will find all shaders used in the scene. In 'Project' mode it will find all shaders in the project."), GUILayout.Width(100), GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                {
                    UpdateShaderList();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal(header);
            GUILayout.Label(new GUIContent("Source Shaders", "Source shaders that will be swapped to 'Target Shaders'"), GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal(header);
            GUILayout.Label(new GUIContent("Target Shaders", "Target shaders that will be used to swap 'Source Shaders' to."), GUILayout.MinWidth(130), GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            if (replacementMode == ReplacementMode.Selected && mtls.Count == 0)
                EditorGUILayout.HelpBox("Select at least one material in the Project window.", MessageType.Warning);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.BeginVertical(header);
            //Show shaders list
            for (int i = 0; i < shaderPairs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(i % 2 == 0 ? lineB : lineA);
                // Select original shader
                shaderPairs[i].originalID = EditorGUILayout.Popup(shaderPairs[i].originalID, shaderNames);
                shaderPairs[i].originalShader = Shader.Find(shaderNames[shaderPairs[i].originalID]);
                GUILayout.Label(">", GUILayout.MaxWidth(10));

                // Select replacement shader
                shaderPairs[i].replacementID = EditorGUILayout.Popup(shaderPairs[i].replacementID, shaderNames);
                shaderPairs[i].replacementShader = Shader.Find(shaderNames[shaderPairs[i].replacementID]);

                if (GUILayout.Button(new GUIContent("-", "Delete this element from the list"), GUILayout.MaxWidth(21)))
                {
                    shaderPairs.Remove(shaderPairs[i]);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (replacementMode != ReplacementMode.Selected)
            {
                GUILayout.BeginHorizontal(lineB);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+", GUILayout.MaxWidth(21))) shaderPairs.Add(new ShaderPair());
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Replace Shaders", GUILayout.Width(120), GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                ReplaceShaders();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void UpdateShaderList()
        {
            GetMaterials();
            GetShadersFromMaterials();
        }

        private void GetMaterials()
        {
            mtls = new List<Material>();
            UnityEngine.Object[] selectedMaterials = new UnityEngine.Object[0];
            switch (replacementMode)
            {
                case ReplacementMode.Selected:
                    selectedMaterials = Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets);
                    for (int i = 0; i < selectedMaterials.Length; i++)
                    {
                        mtls.Add(selectedMaterials[i] as Material);
                    }
                    break;
                case ReplacementMode.Project:
                    mtls = Resources.FindObjectsOfTypeAll<Material>().ToList();
                    break;
                case ReplacementMode.Scene:
                    mtls = FindMaterialsInScene();
                    break;
            }


        }

        private List<Material> FindMaterialsInScene()
        {
            GameObject[] objects = NL_Utilities.FindObjectsOfType<GameObject>(true);

            List<Material> materials = new List<Material>();

            for (int i = 0; i < objects.Length; i++)
            {
                GameObject obj = objects[i];

                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

                for (int j = 0; j < renderers.Length; j++)
                {
                    Renderer renderer = renderers[j];

                    for (int k = 0; k < renderer.sharedMaterials.Length; k++)
                    {
                        Material material = renderer.sharedMaterials[k];
                        if (!materials.Contains(material) && material != null) materials.Add(material);
                    }
                }
            }

            return materials;
        }

        public bool ValidateShader(Shader selectedShader)
        {
            bool matched = false;

            if (selectedShader.name.Contains("Hidden"))
            {
                if (shaderExceptionNames.Contains(selectedShader.name))
                    matched = true;
            }
            else matched = true;

            return matched;
        }

        private void GetShadersFromMaterials()
        {
            List<Shader> selectedShaders = new List<Shader>();
            shaderPairs = new List<ShaderPair>();

            for (int i = 0; i < mtls.Count; i++)
            {
                if (!selectedShaders.Contains(mtls[i].shader)) selectedShaders.Add(mtls[i].shader);
            }

            for (int i = 0; i < selectedShaders.Count; i++)
            {
                if (!ValidateShader(selectedShaders[i])) continue;

                ShaderPair pair = new ShaderPair();
                pair.originalShader = selectedShaders[i];
                pair.replacementShader = Shader.Find("NOT_Lonely/Weatherade/Snow Coverage");
                pair.originalID = Array.IndexOf(shaderNames, selectedShaders[i].name);
                pair.replacementID = Array.IndexOf(shaderNames, "NOT_Lonely/Weatherade/Snow Coverage");
                shaderPairs.Add(pair);
            }

            Repaint();
        }

        private List<FilteredMaterial> GetFilteredMaterials(List<Material> rawMtls)
        {
            List<FilteredMaterial> filteredMtls = new List<FilteredMaterial>();
            for (int m = 0; m < rawMtls.Count; m++)
            {
                for (int i = 0; i < shaderPairs.Count; i++)
                {
                    if (rawMtls[m].shader == shaderPairs[i].originalShader)
                    {
                        FilteredMaterial fMaterial = new FilteredMaterial();
                        fMaterial.mtl = rawMtls[m];
                        fMaterial.replacementShader = shaderPairs[i].replacementShader;

                        filteredMtls.Add(fMaterial);

                        break;
                    }
                }
            }
            return filteredMtls;
        }

        private void ReplaceShaders()
        {
            List<FilteredMaterial> filteredMtls = GetFilteredMaterials(mtls);

            bool confirmed = EditorUtility.DisplayDialog("Swap Shaders", $"\nYou are about to replace {shaderPairs.Count} shaders in {filteredMtls.Count} materials. \n\n Do you want to continue?", "Yes", "No");

            if (confirmed)
            {
                ReplaceSelected(filteredMtls);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                UpdateShaderList();
            }
        }

        private void ReplaceSelected(List<FilteredMaterial> filteredMtls)
        {
            for (int i = 0; i < filteredMtls.Count; i++)
            {
                filteredMtls[i].mtl.shader = filteredMtls[i].replacementShader;
            }
        }
    }
}
#endif
