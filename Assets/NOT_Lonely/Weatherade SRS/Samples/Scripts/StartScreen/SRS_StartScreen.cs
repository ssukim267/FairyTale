#if UNITY_EDITOR
namespace NOT_Lonely.Weatherade
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.PackageManager.Requests;
    using UnityEditor.PackageManager;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEditor.Build;
    using System;

    public class SRS_StartScreen : EditorWindow
    {
        private string curVersion;
        public static SRS_StartScreen startScreen;

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

        public Selector[] menu;

        private Texture2D cover;
        private bool isPostProcessingPackageExists;
        private bool isLinearSpace;
        private Vector2 scrollPos;
        static AddRequest Request;
        static bool downloading;
        private GUIContent[] assetPreviews;
        private int selectedAssetID;

        public class AssetButton
        {
            public GUIContent guiContent;
            public string storeLink;
        }

        private List<AssetButton> assetButtons;

        public enum MenuButton
        {
            Welcome,
            URP,
            DemoContent,
            Manual,
            Forum,
            DirectSupport,
            OtherAssets
        }

        private MenuButton menuButton = MenuButton.Welcome;

        [MenuItem("Window/NOT_Lonely/Weatherade: Snow and Rain System/Start Screen")]
        public static void OpenStartScreen()
        {
            startScreen = GetWindow(typeof(SRS_StartScreen), true) as SRS_StartScreen;
            startScreen.titleContent = new GUIContent("Weatherade: Snow and Rain System");
            startScreen.minSize = new Vector2(614, 614);
            startScreen.maxSize = new Vector2(614, 614);
        }

        private void OnEnable()
        {
            curVersion = SRS_VersionCheck.GetCurrentVersion();

            menu = new Selector[7] {
            new Selector(menuButton == MenuButton.Welcome, new GUIContent("Welcome")),
            new Selector(menuButton == MenuButton.OtherAssets, new GUIContent("URP")),
            new Selector(menuButton == MenuButton.DemoContent, new GUIContent("Demo Content")),
            new Selector(menuButton == MenuButton.Manual, new GUIContent("Manual")),
            new Selector(menuButton == MenuButton.Forum, new GUIContent("Forum")),
            new Selector(menuButton == MenuButton.DirectSupport, new GUIContent("Direct Support")),
            new Selector(menuButton == MenuButton.OtherAssets, new GUIContent("Other Assets"))
            };

            cover = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/NOT_Lonely/Weatherade SRS/UI/SnowCover.png", typeof(Texture2D));

            CheckPostProcessingPackage();
            isLinearSpace = PlayerSettings.colorSpace == ColorSpace.Linear;

            GetAssetPreviews();
        }



        private void GetAssetPreviews()
        {
            string[] paths = Directory.GetFiles("Assets/NOT_Lonely/Weatherade SRS/UI/OtherAssetsIcons", "*.png");
            assetButtons = new List<AssetButton>();

            foreach (string filePath in paths)
            {
                Texture2D texture = new Texture2D(2, 2);
                byte[] textureData = File.ReadAllBytes(filePath);
                texture.LoadImage(textureData);

                AssetButton btn = new AssetButton();
                string name = Path.GetFileName(filePath.Replace(".png", ""));
                btn.guiContent = new GUIContent(texture, name);
                btn.storeLink = SRS_VersionCheck.ReadFile($"Assets/NOT_Lonely/Weatherade SRS/UI/OtherAssetsIcons/{name}.txt");
                //Debug.Log(btn.storeLink);
                assetButtons.Add(btn);
            }
        }

        private void OnDisable()
        {
            if (downloading)
            {
                CancelDownload();
                Debug.LogWarning("The download process has been canceled due to Weatherade Start Screen is closed.");
            }
        }

        private void CheckPostProcessingPackage()
        {
            var platform = EditorUserBuildSettings.selectedBuildTargetGroup;
            NamedBuildTarget buildTarget = NamedBuildTarget.FromBuildTargetGroup(platform);
            string defineSymbolsString = PlayerSettings.GetScriptingDefineSymbols(buildTarget);

            isPostProcessingPackageExists = defineSymbolsString.Contains("UNITY_POST_PROCESSING_STACK_V2");
        }

        private void DrawCenteredHeader(string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(text, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnGUI()
        {
            if (NL_Styles.noPaddingButton == null) NL_Styles.GetStyles();

            GUILayout.BeginVertical();

            float inspectorWidth = EditorGUIUtility.currentViewWidth;
            float imageWidth = inspectorWidth;
            float imageHeight = imageWidth * cover.height / cover.width;
            Rect rect = GUILayoutUtility.GetRect(imageWidth, imageHeight);
            GUI.DrawTexture(rect, cover, ScaleMode.ScaleToFit);

            GUILayout.BeginHorizontal();
            MenuButtonsArea();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

            if (menuButton == MenuButton.Welcome)
            {
                DrawCenteredHeader("Welcome!");

                GUILayout.Label("Thank you for choosing and purchasing Weatherade: Snow and Rain System. Please, read the manual and watch the Get Started video before you begin using the system.", EditorStyles.wordWrappedLabel);
                EditorGUILayout.HelpBox("To get demo scenes with all the content presented in the playable demo, go to the Demo Content tab.", MessageType.Info);
                EditorGUILayout.HelpBox("If your project uses URP, go to the URP tab to upgrade.", MessageType.Warning);
            }

            if(menuButton == MenuButton.URP)
            {
                if (SRS_VersionCheck.IsUsingURP())
                {
                    GUILayout.Label("Your project uses Universal RP. \nClick the 'Import URP' button bellow to install Weatherade URP support package or update it if it's already installed.", EditorStyles.wordWrappedLabel);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Import URP", GUILayout.MaxWidth(100)))
                    {
                        AssetDatabase.ImportPackage("Assets/NOT_Lonely/Weatherade SRS/URP Support/WeatheradeSRS_URP.unitypackage", false);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label("Your project does not use Universal RP.", EditorStyles.wordWrappedLabel);
                }
            }

            if(menuButton == MenuButton.DemoContent)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                DrawCenteredHeader("Project Check");
#if !USING_URP
                if (!isPostProcessingPackageExists)
                {
                    EditorGUILayout.HelpBox("The Unity Post Processing package is not installed. Install it so that the sample scenes look the same as in the playable demo.", MessageType.Warning);
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (Request != null && Request.Status == StatusCode.InProgress)
                    {
                        GUILayout.Label("Installing...");
                    }
                    else
                    {
                        if (GUILayout.Button("Install Post Processing Stack package", GUILayout.Width(240), GUILayout.Height(32)))
                            InstallPackage("com.unity.postprocessing");
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Post Processing Package:");
                    GUI.color = Color.green;
                    GUILayout.Label("Installed");
                    GUI.color = Color.white;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(5);
#endif

                if (!isLinearSpace)
                {
                    EditorGUILayout.HelpBox("Project's Color Space is set to Gamma. Demo scenes has been created with Linear color space in mind.", MessageType.Warning);
                    if (GUILayout.Button("Set Color Space to Linear", GUILayout.Width(240), GUILayout.Height(32)))
                    {
                        bool option = EditorUtility.DisplayDialog("Change color space to Linear?", "You are about to change the project color space. " +
                            "\n\n This will force all textures to be re-imported, which can take a significant amount of time.\n\n Do you want to continue?", "Yes", "No");

                        switch (option)
                        {
                            case true:
                                PlayerSettings.colorSpace = ColorSpace.Linear;
                                break;

                            case false:
                                break;
                        }
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Project Color Space:");
                    GUI.color = Color.green;
                    GUILayout.Label("Linear (that's fine)");
                    GUI.color = Color.white;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();

                GUILayout.Space(10);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                DrawCenteredHeader("Basic Sample Scenes");

                GUILayout.Label("If you want to quickly test the Weatherade SRS, you can start from exploring very basic sample scenes placed " +
                    "at the Assets/NOT_Lonely/Weatherade SRS/Samples/ folder. \nOr just press one of the buttons below to open the scene:", EditorStyles.wordWrappedLabel);
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Snow", GUILayout.Width(196), GUILayout.Height(32)))
                    EditorSceneManager.OpenScene("Assets/NOT_Lonely/Weatherade SRS/Samples/BasicSetupSnow.unity");
                if (GUILayout.Button("Rain", GUILayout.Width(196), GUILayout.Height(32)))
                    EditorSceneManager.OpenScene("Assets/NOT_Lonely/Weatherade SRS/Samples/BasicSetupRain.unity");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.Space(10);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                DrawCenteredHeader("Extra Demo Content");
                GUILayout.Label("Extra demo content is a great starting point to explore the Weatherade's capabilities. " +
                    "To reduce package size, it is packed separately from the main Weatherade package and can be downloaded by clicking the button below.", EditorStyles.wordWrappedLabel);

                EditorGUILayout.HelpBox("\n1. Click the 'Download Demo Content' button below. This will download the package into your project root folder. " +
                    "\n\n2. Click 'Import' in the popup window and wait while Unity imports the demo content." +
                    "\n\n3. Sample scenes will be available in the Assets/NOT_Lonely/Samples folder along with all demo content." +
                    "\n\n Or use manual download and import if you have problems with the first method. \n", MessageType.Info);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (uwr != null && uwr.downloadProgress < 1)
                {
                    GUI.color = new Color(0.192f, 0.998f, 1.4f, 1);
                    GUILayout.Label($"Downloading {(uwr.downloadProgress * 100).ToString("0.0")}% - don't close this window!");
                    GUI.color = Color.white;
                    if (GUILayout.Button("Cancel", GUILayout.Width(60), GUILayout.Height(32))) CancelDownload();
                }
                else
                {
                    if (GUILayout.Button("Download Demo Content (~730mb)", GUILayout.Width(240), GUILayout.Height(32)))
                        DownloadFile("https://not-lonely.com/sources/weatherade/WeatheradeDemoContent.unitypackage", "WeatheradeDemoContent.unitypackage");
                    if (GUILayout.Button("Manual Download", GUILayout.Width(240), GUILayout.Height(32)))
                        Application.OpenURL("https://drive.google.com/file/d/1zr7IYPy57_Q3zhpFHfgj559M8tbp2NNZ/view");
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            if(menuButton == MenuButton.Manual)
            {
                EditorGUILayout.HelpBox("Quick start (snow example): \n\n1. Add a Snow Coverage Instance to your scene from the GameObject > NOT_Lonely > Weatherade menu." +
                    "\n\n2. If you already have any models in your scene, you can quickly convert their materials to use the Weatherade shaders by going to the Window > NOT_Lonely > Batch Shader Swapper tool. " +
                    "You can choose a conversion mode there, for example, 'Scene', press the 'Find Shaders' button, set the 'Target Shaders' to Snow Coverage and press the 'Replace Shaders' button." +
                    "\n\n3. A basic snow should appear on the surfaces. Now you can tweak it globally from the Snow Coverage Instance you created at the 1 step, or override any property on a per-material basis.\n", MessageType.Info);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Get Started Video", GUILayout.Width(196), GUILayout.Height(32)))
                    Application.OpenURL("https://youtu.be/vEo_aOvDnHg?si=LUXlJsauCjdcGG8Y");
                
                if (GUILayout.Button("Detailed Manual (online)", GUILayout.Width(196), GUILayout.Height(32)))
                    Application.OpenURL("https://wiki.not-lonely.com/doku.php/weatherade_manual:introduction");

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            if(menuButton == MenuButton.Forum)
            {
                GUILayout.Label("If you have a problem with the Weatherade system, or you want to suggest a feature, or report a bug, please write a post on the forum:", EditorStyles.wordWrappedLabel);
                if (GUILayout.Button("Go to the forum"))
                {
                    Application.OpenURL("https://forum.unity.com/threads/released-weatherade-snow-and-rain-system.1543433/");
                }
            }

            if(menuButton == MenuButton.DirectSupport)
            {
                GUILayout.Label("If you have a problem with the Weatherade system, or you want to suggest a feature, or report a bug, please write to the support email:", EditorStyles.wordWrappedLabel);
                if(GUILayout.Button("Copy Support Email Adress To Clipboard"))
                {
                    GUIUtility.systemCopyBuffer = "support@not-lonely.com";
                }
            }

            if(menuButton == MenuButton.OtherAssets)
            {
                int buttonsPerRow = 2;
                int buttonCount = assetButtons.Count;

                int rowCount = Mathf.CeilToInt((float)buttonCount / buttonsPerRow);

                GUILayout.BeginVertical();
                for (int i = 0; i < rowCount; i++)
                {
                    GUILayout.BeginHorizontal();
                    for (int j = 0; j < buttonsPerRow; j++)
                    {
                        int index = i * buttonsPerRow + j;
                        if (index < buttonCount)
                        {
                            //GUI.contentColor = Color.white;

                            if (GUILayout.Button(assetButtons[index].guiContent, NL_Styles.noPaddingButton, GUILayout.MaxWidth(294), GUILayout.MaxHeight(196)))
                            {
                                Application.OpenURL(assetButtons[index].storeLink);
                            }
                            /*
                            if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                            {
                                GUI.contentColor = Color.green;
                                Repaint();
                            }
                            else
                            {
                                GUI.contentColor = Color.white;
                            }
                            */
                        }
                        else
                        {
                            if (GUILayout.Button("Open Asset Store", NL_Styles.noPaddingButton, GUILayout.MaxWidth(294), GUILayout.MaxHeight(196)))
                            {
                                Application.OpenURL("https://assetstore.unity.com/publishers/5889");
                            }
                        }

                       
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            curVersion = SRS_VersionCheck.GetCurrentVersion();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Installed version: {curVersion}");

            GUILayout.EndVertical();
        }

        private void MenuButtonsArea()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            for (int i = 0; i < menu.Length; i++)
            {
                GUI.color = menu[i].isActive ? Color.gray : Color.white;
                if (GUILayout.Button(menu[i].label, GUILayout.MaxWidth(119), GUILayout.MaxHeight(32)))
                {
                    for (int t = 0; t < menu.Length; t++)
                    {
                        menu[t].isActive = false;
                    }
                    menu[i].isActive = true;
                    menuButton = (MenuButton)i;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        private void InstallPackage(string packageName)
        {
            Request = Client.Add(packageName);
            EditorApplication.update += Progress;
        }

        private void Progress()
        {
            if (Request.IsCompleted)
            {
                if (Request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + Request.Result.packageId);
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log(Request.Error.message);

                EditorApplication.update -= Progress;
            }
        }

        UnityWebRequest uwr;
        string demoPackagePath;
        private void DownloadFile(string link, string fileName)
        {
            EditorApplication.update += DownloadProgress;
            downloading = true;

            uwr = new UnityWebRequest(link, UnityWebRequest.kHttpVerbGET);
            string path = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), fileName);
            demoPackagePath = path;
            uwr.downloadHandler = new DownloadHandlerFile(path);
            uwr.SendWebRequest();
        }

        private void DownloadProgress()
        {
            Repaint();

            if (uwr.downloadProgress >= 1) 
            {
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(uwr.error);
                }
                else
                {
                    Debug.Log($"Successfully downloaded and saved to: {demoPackagePath}");
                    AssetDatabase.ImportPackage(demoPackagePath, true);
                }

                EditorApplication.update -= DownloadProgress;
                downloading = false;
                uwr.downloadHandler.Dispose();
                uwr.Dispose();
                uwr = null;
            }
        }

        private void CancelDownload()
        {
            if (uwr == null) return;

            EditorApplication.update -= DownloadProgress;
            downloading = false;
            uwr.downloadHandler.Dispose();
            uwr.Dispose();
            uwr = null;
        }
    }
}
#endif