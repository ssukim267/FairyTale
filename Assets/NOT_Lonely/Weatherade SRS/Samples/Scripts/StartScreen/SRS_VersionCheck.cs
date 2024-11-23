#if UNITY_EDITOR
namespace NOT_Lonely.Weatherade
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEditor;
    using UnityEditor.Build;
    using System.IO;
    using System.Linq;

    //[InitializeOnLoad]
    class SRS_VersionCheck : AssetPostprocessor
    {
        public static readonly string currentVersionCheckFilePath = "Assets/NOT_Lonely/Weatherade SRS/Scripts/StartScreen/CurrentVersion.txt";
        public static readonly string targetVersionCheckFilePath = "Assets/NOT_Lonely/Weatherade SRS/Scripts/StartScreen/TargetVersion.txt";

        public static bool IsUsingURP()
        {
            return GraphicsSettings.defaultRenderPipeline != null;
        }

        static bool AddShadersAndDefine()
        {
            var platform = EditorUserBuildSettings.selectedBuildTargetGroup;
            NamedBuildTarget buildTarget = NamedBuildTarget.FromBuildTargetGroup(platform);
            string defineSymbolsString = PlayerSettings.GetScriptingDefineSymbols(buildTarget);

            if (!defineSymbolsString.Contains("WEATHERADE_INCLUDED"))
            {
                if (defineSymbolsString.Length > 0) defineSymbolsString += ";";
                defineSymbolsString += "WEATHERADE_INCLUDED";
                PlayerSettings.SetScriptingDefineSymbols(buildTarget, defineSymbolsString);
            }

            if (IsUsingURP())
            {
                if (!defineSymbolsString.Contains("USING_URP"))
                {
                    if (defineSymbolsString.Length > 0) defineSymbolsString += ";";
                    defineSymbolsString += "USING_URP";
                    PlayerSettings.SetScriptingDefineSymbols(buildTarget, defineSymbolsString);
                }
            }

            int shadersArrayLength = 4;
            Shader[] shaders = new Shader[shadersArrayLength];
            shaders[0] = Shader.Find("Hidden/NOT_Lonely/NL_GaussianBlur");
            shaders[1] = Shader.Find("Hidden/NOT_Lonely/Weatherade/NL_TexturePacking");
            shaders[2] = Shader.Find("Hidden/NOT_Lonely/Weatherade/NL_TraceMaskGen");
            shaders[3] = Shader.Find("Hidden/NOT_Lonely/Weatherade/DepthRenderer");

            var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            var serializedObject = new SerializedObject(graphicsSettingsObj);
            var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");

            for (int i = 0; i < shaders.Length; i++)
            {
                if (shaders[i] == null)
                {
                    Debug.LogError($"Shader at index {i} is missing. Fix the shaders array and recompile the script.");
                    return false;
                }

                bool hasShader = false;

                for (int j = 0; j < arrayProp.arraySize; ++j)
                {
                    if (shaders[i] == arrayProp.GetArrayElementAtIndex(j).objectReferenceValue)
                    {
                        hasShader = true;
                        Debug.Log($"Shader {shaders[i]} is already in the 'Always Included Shaders' array. Skip.");
                        break;
                    }

                }

                if (!hasShader)
                {
                    int arrayIndex = arrayProp.arraySize;
                    arrayProp.InsertArrayElementAtIndex(arrayIndex);
                    var newArrayElement = arrayProp.GetArrayElementAtIndex(arrayIndex);
                    newArrayElement.objectReferenceValue = shaders[i];

                    Debug.Log($"Shader {shaders[i]} has been added to the 'Always Included Shaders'.");
                }
            }

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("All shaders added successfully to 'Always Included Shaders'.");
            
            return true;
        }

        public static string RemoveWhiteSpaces(string source)
        {
            return string.Concat(source.Where(c => !char.IsWhiteSpace(c)));
        }

        public static string ReadFile(string filePath)
        {
            string text = string.Empty;

            if (File.Exists(filePath))
            {
                StreamReader reader = new StreamReader(filePath);
                text = RemoveWhiteSpaces(reader.ReadToEnd());
                reader.Close();
                reader.Dispose();
            }
            else
            {
                //Debug.Log(filePath + " doesn't exists");
            }

            return text;
        }

        public static string GetCurrentVersion()
        {
            return ReadFile(currentVersionCheckFilePath);
        }

        public static string GetTargetVersion()
        {
            return ReadFile(targetVersionCheckFilePath);
        }

        static void VersionCheck()
        {
            string currVer = string.Empty;
            string targVer = string.Empty;

            targVer = GetTargetVersion();
            currVer = GetCurrentVersion();

            if (!currVer.Equals(targVer))
            {
                SRS_StartScreen.OpenStartScreen();
                
                bool shadersAdded = AddShadersAndDefine();
                if(shadersAdded) UpdateVersion(currentVersionCheckFilePath, targVer);
            }
        }

        static void UpdateVersion(string curVerFilePath, string newVersion)
        {
            StreamWriter writer = new StreamWriter(curVerFilePath, false);
            writer.WriteLine(newVersion);
            writer.Close();
            writer.Dispose();
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (didDomainReload)
            {
                VersionCheck();
                //NL_Utilities.GetCurrentRP();
            }
        }
    }
}

#endif