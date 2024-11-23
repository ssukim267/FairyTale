#if UNITY_EDITOR
namespace NOT_Lonely.TotalBrush
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using System;
    using UnityEngine.SceneManagement;
    using UnityEngine.Rendering;

    public class NL_TotalBrush : EditorWindow
    {
        public static NL_TotalBrush splatPainterWindow;
        private SerializedObject so;

        private GameObject[] selectedObjects;

        [NonSerialized] private List<NL_PaintableObject> paintableObjects = new List<NL_PaintableObject>();
        [NonSerialized] private List<MeshRenderer> debugRenderers = new List<MeshRenderer>();

        public Terrain terrain;
        public TerrainCollider terrainCollider;
        public Material terrainMtl;

        private Color blueColor = new Color(0.137f, 0.713f, 1, 1);
        private Vector2 scrollPos;
        public Texture2D brushAlpha;
        public RenderTexture mask_prePaint;
        public RenderTexture mask_afterPaint;
        public RenderTexture normalRT;
        public Texture2D terrainMtlMaskTex;
        public Texture2D terrainMtlNormalTex;
        private string maskPostfix = "_SRS_PaintedMask";
        private string maskNormalPostfix = "_SRS_PaintedMaskNormal";

        public Material paintMtl;
        public bool isSplatInit = false;
        public int brushAlphaId;
        public GUIContent[] alphaPreviews;
        [NonSerialized] private bool inPaintMode = false;

        private Camera depthCam;
        private RenderTexture depthTex;

        public Tool lastTool;

        private bool mouseDrag;
        private bool LmbDown;
        private bool RmbDown;
        private bool mouseUp;
        private bool scrolled;
        private bool shiftHold;
        private bool ctrlHold;
        private bool altHold;
        private bool focusKeyPressed;
        private bool terrainPainting;
        private bool meshPainting;

        private RaycastHit hit;

        //styles
        public GUIStyle header;
        public GUIStyle lineA;
        public GUIStyle lineB;
        public GUIStyle centeredHeader;
        public GUIStyle centeredItalicLabel;
        public GUIStyle previewBtnStyle;
        public GUIStyle previewStyle;
        public GUIStyle sceneLabelStyle;
        public GUIStyle customCheckbox;

        public Texture2D gradientToolTex;
        public LineRenderer gradientToolLR;
        public Material gradientToolMtl;
        public Texture2D brush;

        private ComputeShader c_shader;
        public enum Mode
        {
            Mesh,
            Terrain
        }

        public enum MeshPaintTool
        {
            Brush,
            GradientFill
        }

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

        public Mode mode = Mode.Mesh;
        public MeshPaintTool paintTool = MeshPaintTool.Brush;

        //UI fields
        public bool includeChildren;
        public bool isolationMode;
        [Range(32, 4096)]public int texRes = 4096;
        public float brushSize = 1;
        [Range(0, 1)] public float brushOpacity = 1;
        public Vector2 targetValueMinMax = new Vector2(0, 1);
        [Range(0, 1)] public float fillOpacity = 0.5f;
        [Range(0, 1)] public float brushAngleRandomness = 0;
        [Range(0, 1)] public float normalThreshold = 1;
        public bool geometryCulling = false;
        [Range(0, 1)] public float cullingBias = 0.001f;
        public AnimationCurve brushHardnessFalloff = new AnimationCurve();
        public Gradient gradient;
        [Range(0, 1)] public float opacityMultiplier = 1;
        public bool clampGradientStart = false;
        public bool clampGradientEnd = false;
        public Color brushColor = Color.white;
        public Color fillColor = Color.white;
        public bool[] channelMask;
        public string[] channeMaskNames;
        public bool debugView;
        public bool debugViewShaded = true;
        public bool debugViewShowAlpha = false;
        public Selector[] paintTools;

        public Texture2D[] brushAlphas;
        public float normalThresholdInternal;
        public Material vertexDebugMtl;
        public CommandBuffer debugBuffer;
        public CommandBuffer gizmoBuffer;
        public Scene pScene;

        SerializedProperty brushSize_prop;
        SerializedProperty brushOpacity_prop;
        SerializedProperty brushAngleRandomness_prop;
        SerializedProperty brushColor_prop;
        SerializedProperty normalThreshold_prop;
        SerializedProperty opacityMultiplier_prop;
        SerializedProperty texRes_prop;
        SerializedProperty terrainMtlMaskTex_prop;
        SerializedProperty terrainMtlNormalTex_prop;

        public string toolPath;
        public GUIContent terrainBtn_label;
        public GUIContent meshBtn_label;
        public GUIContent brushToolBtn_label;
        public GUIContent gradientToolBtn_label;
        public GUIContent fillBtn_label;
        public GUIContent toggle_label;
        public GUIContent infinity_label;

        private Event e;
        private bool hasPaintableObjects;
        private bool notPaintableMtlFound = false;
        private int lastTerrainID;

        private bool soloMode;

        [MenuItem("Window/NOT_Lonely/Total Brush", false)]
        public static void OpenWindow()
        {
            splatPainterWindow = GetWindow<NL_TotalBrush>();
            splatPainterWindow.titleContent = new GUIContent("Total Brush");
            
            Vector2 minSize;
            Vector2 maxSize;

            minSize = new Vector2(400, 560);
            maxSize = new Vector2(480, 1000);

            splatPainterWindow.minSize = minSize;
            splatPainterWindow.maxSize = maxSize;

            Mode mode = Mode.Mesh;

            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Terrain>() != null)
                mode = Mode.Terrain;

            splatPainterWindow.mode = mode;
        }

        public static void OpenWindowExternal(Mode defaultMode = Mode.Mesh)
        {
            OpenWindow();
            splatPainterWindow.mode = defaultMode;
        }

        private void SetDefaultFalloffCurve()
        {
            brushHardnessFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
        }

        private void OnEnable()
        {
            ScriptableObject target = this;
            so = new SerializedObject(target);

            toolPath = NL_TotalBrushUtilities.GetToolPath(target);

            if (!Application.isPlaying)
            {
                string path = toolPath + "Shaders/Compute/" + "NL_TotalBrush_Compute.compute";
                c_shader = AssetDatabase.LoadAssetAtPath(path, typeof(ComputeShader)) as ComputeShader;

                ValidateSelection();

                InitGradientTool();
                InitBrush();
            }

            vertexDebugMtl = new Material(Shader.Find("Hidden/NOT_Lonely/NL_VertexDebug"));
            vertexDebugMtl.SetInt("_ShowAlpha", debugViewShowAlpha ? 1 : 0);
            vertexDebugMtl.SetInt("_Shaded", debugViewShaded ? 1 : 0);
            debugBuffer = new CommandBuffer();
            gizmoBuffer = new CommandBuffer();

            terrainBtn_label = new GUIContent("Terrain Extra Mask", AssetDatabase.LoadAssetAtPath($"{toolPath}UI/terrainBtn.png", typeof(Texture2D)) as Texture2D);
            meshBtn_label = new GUIContent("Mesh Vertices", AssetDatabase.LoadAssetAtPath($"{toolPath}UI/meshVertBtn.png", typeof(Texture2D)) as Texture2D);
            brushToolBtn_label = new GUIContent(AssetDatabase.LoadAssetAtPath($"{toolPath}UI/brushToolBtn.png", typeof(Texture2D)) as Texture2D);
            gradientToolBtn_label = new GUIContent(AssetDatabase.LoadAssetAtPath($"{toolPath}UI/gradientToolBtn.png", typeof(Texture2D)) as Texture2D);
            fillBtn_label = new GUIContent(AssetDatabase.LoadAssetAtPath($"{toolPath}UI/fillBtn.png", typeof(Texture2D)) as Texture2D, "Fill out the surface with selected fill color and opacity value.");
            toggle_label = new GUIContent(AssetDatabase.LoadAssetAtPath($"{toolPath}UI/toggleIcon.png", typeof(Texture2D)) as Texture2D);
            infinity_label = new GUIContent(AssetDatabase.LoadAssetAtPath($"{toolPath}UI/infinityIcon.png", typeof(Texture2D)) as Texture2D);

            paintTools = new Selector[2] { new Selector(paintTool == MeshPaintTool.Brush, brushToolBtn_label), new Selector(paintTool == MeshPaintTool.GradientFill, gradientToolBtn_label) };

            channelMask = new bool[4] { true, false, false, false };
            channeMaskNames = new string[4] { "R", "G", "B", "A" };

            UpdateStyles();

            SetDefaultFalloffCurve();

            brushAlphas = Resources.LoadAll<Texture2D>("NL_BrushAlphas");
            brushAlpha = brushAlphas[0];

            List<GUIContent> alphaPreviewsTemp = new List<GUIContent>();
            for (int i = 0; i < brushAlphas.Length; i++)
            {
                alphaPreviewsTemp.Add(new GUIContent(brushAlphas[i], brushAlphas[i].name));
            }

            alphaPreviews = new GUIContent[alphaPreviewsTemp.Count];
            alphaPreviews = alphaPreviewsTemp.ToArray();

            OnValidate();

            brushSize_prop = so.FindProperty("brushSize");
            brushOpacity_prop = so.FindProperty("brushOpacity");
            brushAngleRandomness_prop = so.FindProperty("brushAngleRandomness");
            brushColor_prop = so.FindProperty("brushColor");
            normalThreshold_prop = so.FindProperty("normalThreshold");
            opacityMultiplier_prop = so.FindProperty("opacityMultiplier");
            texRes_prop = so.FindProperty("texRes");
            terrainMtlMaskTex_prop = so.FindProperty("terrainMtlMaskTex");
            terrainMtlNormalTex_prop = so.FindProperty("terrainMtlNormalTex");

            Selection.selectionChanged += ValidateSelection;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorSceneManager.sceneClosed += OnSceneClosed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGui;
#else
        SceneView.onSceneGUIDelegate += OnSceneGui;
#endif
            lastTool = Tools.current;
        }

        private void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGui;
#else
        SceneView.onSceneGUIDelegate -= OnSceneGui;
#endif

            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Selection.selectionChanged -= ValidateSelection;

            StopPainting();

            if (gradientToolLR != null) DestroyImmediate(gradientToolLR.gameObject);
        }

        private void OnValidate()
        {
            brushSize = Mathf.Clamp(brushSize, 0, 100);
            normalThresholdInternal = (1 - normalThreshold - 0.5f) * 2;
        }

        private void ValidateTargetValueSlider()
        {
            targetValueMinMax.x = Mathf.Clamp(targetValueMinMax.x, 0, 0.999f);
            targetValueMinMax.y = Mathf.Clamp(targetValueMinMax.y, 0.001f, 1);
            if (targetValueMinMax.y <= targetValueMinMax.x) targetValueMinMax.y = targetValueMinMax.x + 0.001f;
        }

        private void PlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                StopPainting();
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                InitGradientTool();
                InitBrush();
                UpdateStyles();
                vertexDebugMtl = new Material(Shader.Find("Hidden/NOT_Lonely/NL_VertexDebug"));
                vertexDebugMtl.SetInt("_ShowAlpha", debugViewShowAlpha ? 1 : 0);
                vertexDebugMtl.SetInt("_Shaded", debugViewShaded ? 1 : 0);

                ValidateSelection();
            }
        }

        private void InitBrush()
        {
            if (brush == null) brush = NL_TotalBrushUtilities.CreateGradientTex(128, 1);
        }

        private void InitGradientTool()
        {
            if (gradient == null)
            {
                gradient = new Gradient();
                GradientColorKey[] gradientColorKeys = new GradientColorKey[2] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.black, 1) };
                GradientAlphaKey[] gradientAlphaKeys = new GradientAlphaKey[2] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) };
                gradient.SetKeys(gradientColorKeys, gradientAlphaKeys);
            }

            if (gradientToolTex == null) gradientToolTex = NL_TotalBrushUtilities.CreateGradientTex(64, 1);
            if (gradientToolLR == null) gradientToolLR = new GameObject("gradientLR").AddComponent<LineRenderer>();
            gradientToolLR.gameObject.hideFlags = HideFlags.HideAndDontSave;
            if (gradientToolMtl == null) gradientToolMtl = new Material(Shader.Find("Hidden/NOT_Lonely/NL_TotalBrushDithering"));
            gradientToolMtl.SetTexture("_Tex", gradientToolTex);
            gradientToolLR.material = gradientToolMtl;
            gradientToolLR.shadowCastingMode = ShadowCastingMode.Off;

            gradientToolLR.gameObject.SetActive(false);
        }

        private void StopPainting()
        {
            SwitchIsolationMode(false);
            inPaintMode = false;
            HandleUtility.AddDefaultControl(-1);

            Tools.current = lastTool;

            if (mode == Mode.Terrain)
            {
                //SaveTex();
                RtToTex(mask_afterPaint, terrainMtlMaskTex, maskPostfix, "_PaintedMask", TextureFormat.ARGB32);
                if(terrainMtl != null && terrainMtl.HasProperty("_PaintedMaskNormal")) RtToTex(normalRT, terrainMtlNormalTex, maskNormalPostfix, "_PaintedMaskNormal", TextureFormat.RGB24);
            }
            else
            {
                List<GameObject> selectedObjects = new List<GameObject>();
                for (int i = 0; i < paintableObjects.Count; i++)
                {
                    if (paintableObjects[i] != null) selectedObjects.Add(paintableObjects[i].gameObject);
                }
                Selection.objects = selectedObjects.ToArray();
            }

            for (int i = 0; i < paintableObjects.Count; i++)
            {
                paintableObjects[i].DisposeAll();
            }

            if (depthTex != null) depthTex.Release();
            if (depthCam != null) DestroyImmediate(depthCam.gameObject);

            if (gradientToolLR != null) gradientToolLR.gameObject.SetActive(false);
        }

        private void UpdateStyles()
        {
            lineA = NL_PainterStyles.GetBackgroundStyle(new Color(0, 0, 0, 0));
            lineB = EditorGUIUtility.isProSkin ? NL_PainterStyles.GetBackgroundStyle(new Color(1, 1, 1, 0.05f)) : NL_PainterStyles.GetBackgroundStyle(new Color(1, 1, 1, 0.2f));
            header = EditorGUIUtility.isProSkin ? NL_PainterStyles.GetBackgroundStyle(new Color(1, 1, 1, 0.15f)) : NL_PainterStyles.GetBackgroundStyle(new Color(1, 1, 1, 0.5f));
            centeredHeader = NL_PainterStyles.GetCenteredHeader();
            previewBtnStyle = NL_PainterStyles.GetPreviewBtnStyle();
            previewStyle = NL_PainterStyles.GetPreviewStyle();
            centeredItalicLabel = NL_PainterStyles.GetCenteredItalicLabel();
            sceneLabelStyle = NL_PainterStyles.GetSceneLabel(blueColor, Color.white);
            if (customCheckbox == null) customCheckbox = NL_PainterStyles.GetCustomCheckbox();

        }

        private void OnUndoRedoPerformed()
        {
            if (paintableObjects.Count == 0) StopPainting();

            for (int i = 0; i < paintableObjects.Count; i++)
            {
                if (paintableObjects[i] == null || paintableObjects[i].data == null) StopPainting();
                paintableObjects[i].UpdateVertexStreams();
            }
        }

        void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
        {
            //SaveTex();
            if (terrainMtl == null) return;

            RtToTex(mask_afterPaint, terrainMtlMaskTex, maskPostfix, "_PaintedMask", TextureFormat.ARGB32);
            if(terrainMtl.HasProperty("_PaintedMaskNormal")) RtToTex(normalRT, terrainMtlNormalTex, maskNormalPostfix, "_PaintedMaskNormal", TextureFormat.RGB24);
        }

        void OnSceneClosed(UnityEngine.SceneManagement.Scene scene)
        {
            StopPainting();
        }

        private void DrawChannelMaskArea(GUIStyle lineStyle)
        {
            float btnWidth = 42;

            if (soloMode)
            {
                channeMaskNames[0] = "Overall";
                channeMaskNames[1] = "Layer0";
                channeMaskNames[2] = "Layer1";
                channeMaskNames[3] = "Layer2";

                btnWidth = 50;
            }
            else
            {
                channeMaskNames[0] = "R";
                channeMaskNames[1] = "G";
                channeMaskNames[2] = "B";
                channeMaskNames[3] = "A";
            }

            GUILayout.Space(10);
            GUILayout.BeginHorizontal(lineStyle);
            EditorGUILayout.LabelField(new GUIContent("Channel Mask", "Channels, affected during the painting process. Use the toggle to the right to switch the Solo Mode."), GUILayout.Width(149));

            GUI.color = Color.gray;
            if (GUILayout.Button(soloMode ? toggle_label : new GUIContent(), customCheckbox, GUILayout.MaxWidth(18)))
            {
                soloMode = !soloMode;

                if (soloMode)
                {
                    for (int i = 0; i < channelMask.Length; i++)
                    {
                        channelMask[i] = false;
                    }
                    channelMask[0] = true;
                }
            }
            GUI.color = Color.white;

            tempRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && tempRect.width > 0) channelMaskLabelRect = tempRect;

            float btnsBlockSize = btnWidth * 4 + 12;

            posX = currentInspectorWidth - btnsBlockSize;
            size = btnsBlockSize;

            GUILayout.BeginArea(new Rect(posX, channelMaskLabelRect.y - 7, size, 32));
            GUILayout.BeginHorizontal();
            for (int i = 0; i < channelMask.Length; i++)
            {
                GUI.color = channelMask[i] ? Color.gray : Color.white;
                if (GUILayout.Button(channeMaskNames[i], GUILayout.MaxWidth(btnWidth), GUILayout.MaxHeight(32)))
                {
                    if (soloMode)
                    {
                        for (int j = 0; j < channelMask.Length; j++)
                        {
                            channelMask[j] = false;
                        }
                        channelMask[i] = true;
                    }
                    else
                    {
                        if (Event.current.modifiers.HasFlag(EventModifiers.Alt))
                        {
                            for (int j = 0; j < channelMask.Length; j++)
                            {
                                if (i != j) channelMask[j] = channelMask[i];
                            }
                        }
                        channelMask[i] = !channelMask[i];
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        Rect fillBtnRect = new Rect();
        Rect paintToolLabelRect = new Rect();
        Rect channelMaskLabelRect = new Rect();
        Rect startPaintBtnRect = new Rect();
        Rect tempRect;
        int scrollbarWidth;
        float currentInspectorWidth;
        float posX;
        float size;
        private void OnGUI()
        {
            float spaceWidth = 46;
            currentInspectorWidth = EditorGUIUtility.currentViewWidth - scrollbarWidth;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.BeginVertical(header);
            GUILayout.Space(5);
            GUILayout.Label("Total Brush", centeredHeader);
            GUILayout.Space(5);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            ModeSelectorArea();

            if (mode == Mode.Terrain)
            {
                GUILayout.BeginVertical(lineA);
                //GUILayout.Label("Textures");
                //EditorGUILayout.PropertyField(terrainMtlMaskTex_prop, new GUIContent("Mask Texture (RGBA)", "A texture where you want to paint the mask. If no texture provided, then it will be created automatically."));
                //EditorGUILayout.PropertyField(terrainMtlNormalTex_prop, new GUIContent("Normal", "A normal map texture where the slopes will be saved. If no texture provided, then it will be created automatically."));
                EditorGUILayout.PropertyField(texRes_prop, new GUIContent("Resolution", "Textures resolution."));
                texRes_prop.intValue = Mathf.ClosestPowerOfTwo(Mathf.Clamp(texRes_prop.intValue, 32, 4096));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(lineB);
                GUILayout.Label("Brushes");
                GUILayout.BeginHorizontal();
                GUILayout.Label(brushAlphas[brushAlphaId], previewStyle);
                brushAlphaId = GUILayout.SelectionGrid(brushAlphaId, alphaPreviews, 6, previewBtnStyle);
                brushAlpha = brushAlphas[brushAlphaId];
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.EndVertical();
            }
            else if (mode == Mode.Mesh)
            {
                GUILayout.BeginHorizontal(lineA);
                EditorGUI.BeginDisabledGroup(inPaintMode);
                EditorGUILayout.LabelField(new GUIContent("Include Children", "If enabled, then all children objects of the selection will be painted."), GUILayout.MaxWidth(149));
                GUI.color = Color.gray;
                if (GUILayout.Button(includeChildren ? toggle_label : new GUIContent(), customCheckbox, GUILayout.MaxWidth(18)))
                {
                    includeChildren = !includeChildren;
                    ValidateSelection();
                }
                GUI.color = Color.white;
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(lineB);

                //EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField(new GUIContent("Isolation (experimental)", "Opens paintable objects in an isolated environment while painting. This feature is experimental and may not work as expected."), GUILayout.MaxWidth(149));
                GUI.color = Color.gray;
                if (GUILayout.Button(isolationMode ? toggle_label : new GUIContent(), customCheckbox, GUILayout.MaxWidth(18)))
                {
                    isolationMode = !isolationMode;
                    if (inPaintMode) SwitchIsolationMode(isolationMode);
                }
                GUI.color = Color.white;
                //EditorGUI.EndDisabledGroup();

                GUILayout.EndHorizontal();

                GUILayout.BeginVertical(lineA);
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Paint Tool", "Use a regular brush or a gradient fill tool."), GUILayout.Width(149));

                tempRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && tempRect.width > 0) paintToolLabelRect = tempRect;

                posX = currentInspectorWidth - 90;
                size = 90;

                GUILayout.BeginArea(new Rect(posX, paintToolLabelRect.y - 7, size, 32));
                GUILayout.BeginHorizontal();
                for (int i = 0; i < paintTools.Length; i++)
                {
                    GUI.color = paintTools[i].isActive ? Color.gray : Color.white;
                    if (GUILayout.Button(paintTools[i].label, GUILayout.MaxWidth(42), GUILayout.MaxHeight(32)))
                    {
                        mouseUp = false;

                        for (int t = 0; t < paintTools.Length; t++)
                        {
                            paintTools[t].isActive = false;
                        }
                        paintTools[i].isActive = true;

                        paintTool = paintTools[0].isActive ? MeshPaintTool.Brush : MeshPaintTool.GradientFill;

                        if (paintTool == MeshPaintTool.Brush && inPaintMode)
                        {
                            PrepareObjectsForPaint();
                        }
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }

            if (mode == Mode.Mesh && paintTool == MeshPaintTool.Brush || mode == Mode.Terrain)
            {
                GUILayout.BeginVertical(lineB);
                EditorGUILayout.PropertyField(brushSize_prop);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(lineA);
                EditorGUILayout.PropertyField(brushOpacity_prop);
                GUILayout.EndVertical();
            }
            else if (paintTool == MeshPaintTool.GradientFill)
            {
                GUILayout.BeginVertical(lineB);
                EditorGUILayout.PropertyField(opacityMultiplier_prop);
                GUILayout.EndVertical();
            }

            GUILayout.BeginVertical(paintTool == MeshPaintTool.Brush || mode == Mode.Terrain ? lineB : lineA);
            GUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(soloMode);
            EditorGUILayout.LabelField("Target Value", GUILayout.Width(149));

            targetValueMinMax.x = EditorGUILayout.FloatField(targetValueMinMax.x, GUILayout.MaxWidth(50));

            EditorGUILayout.MinMaxSlider(ref targetValueMinMax.x, ref targetValueMinMax.y, 0, 1);

            targetValueMinMax.y = EditorGUILayout.FloatField(targetValueMinMax.y, GUILayout.MaxWidth(50));
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(paintTool == MeshPaintTool.Brush || mode == Mode.Terrain ? lineA : lineB);
            EditorGUILayout.PropertyField(normalThreshold_prop);
            GUILayout.EndVertical();

            if (mode == Mode.Mesh)
            {
                GUILayout.BeginVertical(paintTool == MeshPaintTool.Brush ? lineB : lineA);

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Geometry Culling", "If enabled, parts of the geometry that are overlapped by other geometry will not be painted. Think of it like a spray. Example: The inside of the cup will not be colored when you paint the outside."), GUILayout.MaxWidth(149));
                GUI.color = Color.gray;
                if (GUILayout.Button(geometryCulling ? toggle_label : new GUIContent(), customCheckbox, GUILayout.MaxWidth(18)))
                {
                    geometryCulling = !geometryCulling;
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                EditorGUI.BeginDisabledGroup(!geometryCulling);
                GUILayout.BeginHorizontal();
                GUILayout.Space(16);
                EditorGUILayout.LabelField(new GUIContent("Culling Bias", "How far from the surface the culling takes effect. Values more than 0 are useful to prevent from very thin partially overlayed objects block the painting. Example: some wall vertices overlayd by a plinth geometry."), GUILayout.Width(137));
                cullingBias = EditorGUILayout.Slider(cullingBias, 0, 1);
                GUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();

                GUILayout.EndVertical();

                if (paintTool == MeshPaintTool.Brush)
                {
                    GUILayout.BeginHorizontal(lineA);
                    EditorGUILayout.LabelField(new GUIContent("Hardness Falloff", "Softness of the brush starting from its center to the border."), GUILayout.Width(149));
                    brushHardnessFalloff = EditorGUILayout.CurveField(brushHardnessFalloff, blueColor, new Rect(0, 0, 1, 1));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical(lineB);
                    EditorGUILayout.PropertyField(brushColor_prop);
                    GUILayout.EndVertical();

                    if (!Application.isPlaying)
                    {
                        if (brush != null) brush = NL_TotalBrushUtilities.UpdateGradientTex(brush, brushColor, brushHardnessFalloff);
                        else InitBrush();
                    }
                }
                else if (paintTool == MeshPaintTool.GradientFill)
                {
                    GUILayout.BeginVertical(lineB);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Gradient", "The gradient color which will be applied to the mesh."), GUILayout.Width(149));

                    if (!Application.isPlaying)
                    {
                        if (gradientToolTex != null) gradientToolTex = NL_TotalBrushUtilities.UpdateGradientTex(gradientToolTex, gradient, opacityMultiplier);
                        else InitGradientTool();
                    }

                    GUI.color = Color.gray;
                    if (GUILayout.Button(!clampGradientStart ? new GUIContent(infinity_label.image, "Expand the gradient from the START point to infinity.") : new GUIContent(), customCheckbox, GUILayout.MaxWidth(18)))
                    {
                        clampGradientStart = !clampGradientStart;
                    }
                    GUI.color = Color.white;

                    gradient = EditorGUILayout.GradientField(gradient);

                    GUI.color = Color.gray;
                    if (GUILayout.Button(!clampGradientEnd ? new GUIContent(infinity_label.image, "Expand the gradient from the END point to infinity.") : new GUIContent(), customCheckbox, GUILayout.MaxWidth(18)))
                    {
                        clampGradientEnd = !clampGradientEnd;
                    }
                    GUI.color = Color.white;

                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }

                DrawChannelMaskArea(lineA);

                GUILayout.BeginVertical(lineB);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Debug View", "Show vertex colors on the paintable objects."), GUILayout.MaxWidth(149));
                GUI.color = Color.gray;
                if (GUILayout.Button(debugView ? toggle_label : new GUIContent(), customCheckbox, GUILayout.MaxWidth(18)))
                {
                    debugView = !debugView;
                    debugRenderers = GetDebugRenderers();

                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                if (debugView)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(16);
                    EditorGUILayout.LabelField(new GUIContent("Shaded", "Show a shaded mesh when in debug view."), GUILayout.MaxWidth(136));

                    GUI.color = Color.gray;
                    if (GUILayout.Button(debugViewShaded ? toggle_label : new GUIContent(), customCheckbox, GUILayout.MaxWidth(18)))
                    {
                        debugViewShaded = !debugViewShaded;
                        vertexDebugMtl.SetInt("_Shaded", debugViewShaded ? 1 : 0);
                    }
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(16);
                    EditorGUILayout.LabelField(new GUIContent("Show Alpha", "Show the alpha channel as well as RGB. The debug mesh will become semi-transparent."), GUILayout.MaxWidth(136));

                    GUI.color = Color.gray;
                    if (GUILayout.Button(debugViewShowAlpha ? toggle_label : new GUIContent(), customCheckbox, GUILayout.MaxWidth(18)))
                    {
                        debugViewShowAlpha = !debugViewShowAlpha;
                        vertexDebugMtl.SetInt("_ShowAlpha", debugViewShowAlpha ? 1 : 0);
                    }
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }

            if (mode == Mode.Terrain)
            {
                GUILayout.BeginVertical(lineB);
                EditorGUILayout.PropertyField(brushAngleRandomness_prop);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(lineA);
                EditorGUILayout.PropertyField(brushColor_prop);
                GUILayout.EndVertical();

                DrawChannelMaskArea(lineA);
            }

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(lineA);

            EditorGUI.BeginDisabledGroup(selectedObjects == null || selectedObjects.Length == 0 || Application.isPlaying || (mode == Mode.Mesh && !hasPaintableObjects) || (mode == Mode.Terrain && terrain == null));

            if (GUILayout.Button(fillBtn_label, GUILayout.Width(100), GUILayout.MaxHeight(32)))
            {
                if (mode == Mode.Terrain) FillSplat(fillOpacity);
                if (mode == Mode.Mesh)
                {
                    PrepareObjectsForPaint();

                    Undo.RegisterCompleteObjectUndo(paintableObjects.ToArray(), "NL Painter: Fill");

                    NL_FillTool.Fill(c_shader, fillColor * fillOpacity, channelMask, paintableObjects, soloMode);
                    if (isolationMode) NL_IsolationStage.UpdateStreams(paintableObjects);
                }
            }

            tempRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && tempRect.width > 0) fillBtnRect = tempRect;

            posX = fillBtnRect.x + fillBtnRect.width + spaceWidth + 6;
            size = currentInspectorWidth - posX - 3;

            GUILayout.BeginArea(new Rect(posX, fillBtnRect.y + 7, size, 24));
            GUILayout.BeginHorizontal();
            fillColor = EditorGUILayout.ColorField(fillColor, GUILayout.Width(60));
            fillOpacity = EditorGUILayout.Slider(fillOpacity, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(selectedObjects == null || selectedObjects.Length == 0 || Application.isPlaying || (mode == Mode.Mesh && !hasPaintableObjects));
            if (mode == Mode.Mesh)
            {
                if (GUILayout.Button("Average Normals")) AverageNormals();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();


            GUILayout.BeginHorizontal();
            if (mode == Mode.Terrain)
            {
                if (hasPaintableObjects && notPaintableMtlFound)
                {
                    EditorGUILayout.HelpBox("The 'Paintable' option is disabled on this terrain's material. While painting, you will not see any changes on it.", MessageType.Warning);
                }
                else
                {
                    if (terrain != null)
                        EditorGUILayout.HelpBox("Left Mouse Button to paint.\nShift + Left Mouse Button to erase.", MessageType.Info);
                    else
                        EditorGUILayout.HelpBox("Terrain not selected!\nPlease, chose one in the scene to start painting.", MessageType.Error);
                }
            }
            if (mode == Mode.Mesh)
            {
                if (hasPaintableObjects && notPaintableMtlFound)
                {
                    EditorGUILayout.HelpBox("The 'Paintable' option is disabled for some materials on selected objects. While painting, you will not see any changes on these objects.", MessageType.Warning);
                }
                else
                {
                    if (paintTool == MeshPaintTool.Brush)
                    {
                        if (hasPaintableObjects)
                            EditorGUILayout.HelpBox("Left Mouse Button to paint.\nShift + Left Mouse Button to erase.", MessageType.Info);
                        else
                            EditorGUILayout.HelpBox("No one object with a MeshRenderer selected!\nPlease, chose one in the scene to start painting.", MessageType.Error);
                    }
                    else if (paintTool == MeshPaintTool.GradientFill)
                    {
                        if (hasPaintableObjects)
                            EditorGUILayout.HelpBox("Left Mouse Button click & drag to paint with a gradient.\nShift + Left Mouse Button click & drag to erase.", MessageType.Info);
                        else
                            EditorGUILayout.HelpBox("No one object with a MeshRenderer selected!\nPlease, chose one in the scene to start painting.", MessageType.Error);
                    }
                }
            }

            GUILayout.EndHorizontal();

            PaintButton();

            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                ValidateTargetValueSlider();
                so.ApplyModifiedProperties();
                so.Update();
            }
        }

        public static void AverageNormals()
        {
            bool isOutsideCall = false;

            // if the method called from outside of the window
            if (splatPainterWindow == null)
            {
                isOutsideCall = true;

                splatPainterWindow = GetWindow<NL_TotalBrush>();
                splatPainterWindow.minSize = Vector2.zero;
                splatPainterWindow.maxSize = Vector2.zero;
                splatPainterWindow.position = new Rect();
            }

            splatPainterWindow.PrepareObjectsForPaint();

            for (int i = 0; i < splatPainterWindow.paintableObjects.Count; i++)
            {
                Vector3[] uniNormals = NL_TotalBrushUtilities.CalcAveragedNormals(splatPainterWindow.paintableObjects[i].mFilter.sharedMesh);
                splatPainterWindow.paintableObjects[i].UpdateAveragedNormals(uniNormals);

                Debug.Log($"{splatPainterWindow.paintableObjects[i].name} mesh normals have been averaged and saved into the UV4 of the vertex stream.");
            }

            if (isOutsideCall) splatPainterWindow.Close();
        }

        private void OnFocus()
        {
            if (splatPainterWindow == null) OpenWindow();
        }

        private void ModeSelectorArea()
        {
            GUILayout.BeginHorizontal();
            if (mode == Mode.Mesh)
            {
                GUI.color = Color.gray;
                if (GUILayout.Button(meshBtn_label, GUILayout.MaxHeight(32)))
                {
                    mode = Mode.Mesh;
                }
                GUI.color = Color.white;
                if (GUILayout.Button(terrainBtn_label, GUILayout.MaxHeight(32)))
                {
                    mode = Mode.Terrain;
                }
            }
            if (mode == Mode.Terrain)
            {
                GUI.color = Color.white;
                if (GUILayout.Button(meshBtn_label, GUILayout.MaxHeight(32)))
                {
                    mode = Mode.Mesh;
                }
                GUI.color = Color.gray;
                if (GUILayout.Button(terrainBtn_label, GUILayout.MaxHeight(32)))
                {
                    mode = Mode.Terrain;
                }
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
        }

        private void PaintButton()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (inPaintMode)
            {
                GUI.color = blueColor;
                if (GUILayout.Button("Stop Paint", GUILayout.MaxWidth(200), GUILayout.MaxHeight(32)))
                {
                    StopPainting();
                    ValidateSelection();
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(Application.isPlaying || mode == Mode.Terrain && terrain == null || mode == Mode.Mesh && (selectedObjects == null || selectedObjects.Length == 0 || !hasPaintableObjects));
                GUI.color = Color.white;
                if (GUILayout.Button("Start Paint", GUILayout.MaxWidth(200), GUILayout.Height(32)))
                {
                    inPaintMode = true;
                    if (mode == Mode.Mesh)
                    {
                        PrepareObjectsForPaint();

                        if (isolationMode) SwitchIsolationMode(true);
                        else InitDepth();
                    }
                    if (mode == Mode.Terrain)
                    {
                        InitTextures();
                    }

                    mouseDrag = false;
                    LmbDown = false;

                    lastTool = Tools.current;
                    Tools.current = Tool.None;
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                }


                tempRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && tempRect.width > 0) startPaintBtnRect = tempRect;

                if (splatPainterWindow != null)
                {
                    if (startPaintBtnRect.max.y + 10 > splatPainterWindow.position.height) scrollbarWidth = 14;
                    else scrollbarWidth = 0;
                }

                EditorGUI.EndDisabledGroup();
            }

            GUI.color = Color.white;
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        private void SwitchIsolationMode(bool state)
        {
            if (state) InitDepth();
            NL_IsolationModeWindow.SwitchIsolationMode(state, paintableObjects);
        }

        private void InitDepth()
        {
            if (depthCam != null) DestroyImmediate(depthCam.gameObject);
            if (depthTex != null) DestroyImmediate(depthTex);

            depthTex = new RenderTexture(SceneView.lastActiveSceneView.camera.pixelWidth / 2, SceneView.lastActiveSceneView.camera.pixelHeight / 2, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            depthCam = new GameObject("NL_Painter_DepthCam").AddComponent<Camera>();
            depthCam.enabled = false;
            depthCam.gameObject.hideFlags = HideFlags.HideAndDontSave;
            depthCam.targetTexture = depthTex;
        }

        private void PrepareObjectsForPaint()
        {
            //clean up paintable objects colliders
            if (selectedObjects != null && selectedObjects.Length > 0)
            {
                paintableObjects = GetPaintableObjects();
            }

            for (int i = 0; i < paintableObjects.Count; i++)
            {
                paintableObjects[i].InitVertexStream();
                paintableObjects[i].InitVertColorBuffers(c_shader);
                paintableObjects[i].ConvertLocalToWorld(c_shader);
                paintableObjects[i].data.normals = GetNormalsWS(paintableObjects[i]);
            }
        }

        private List<NL_PaintableObject> GetPaintableObjects()
        {
            List<NL_PaintableObject> pObjects = new List<NL_PaintableObject>();
            hasPaintableObjects = false;

            for (int i = 0; i < paintableObjects.Count; i++)
            {
                paintableObjects[i].DisposeAll();
            }

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                if (selectedObjects[i] == null || !selectedObjects[i].gameObject.scene.IsValid()) continue;

                selectedObjects[i].TryGetComponent(out NL_PaintableObject pObj);

                if (pObj == null)
                {
                    if (selectedObjects[i].TryGetComponent(out MeshFilter tempMeshFilter) && selectedObjects[i].TryGetComponent(out MeshRenderer tempMeshRenderer) && tempMeshFilter.sharedMesh != null)
                    {
                        pObj = (NL_PaintableObject)Undo.AddComponent(selectedObjects[i], typeof(NL_PaintableObject));
                    }
                }

                if (pObj != null)
                {
                    pObj.InitPaintableObject();
                    pObjects.Add(pObj);
                    hasPaintableObjects = true;
                }
            }

            return pObjects;
        }

        private Vector3[] GetNormalsWS(NL_PaintableObject pObject)
        {
            Vector3[] normals = pObject.mFilter.sharedMesh.normals;

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = pObject.mFilter.transform.TransformDirection(normals[i]);
            }
            return normals;
        }


        private void ValidateSelection()
        {
            hasPaintableObjects = false;
            notPaintableMtlFound = false;

            selectedObjects = Selection.gameObjects;

            if (selectedObjects == null || selectedObjects.Length == 0) return;

            if (includeChildren)
            {
                List<GameObject> wholeSelection = new List<GameObject>();

                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    Transform[] children = selectedObjects[i].GetComponentsInChildren<Transform>(false);

                    if (children != null && children.Length > 0)
                    {
                        for (int j = 0; j < children.Length; j++)
                        {
                            wholeSelection.Add(children[j].gameObject);
                        }
                    }
                }

                if (wholeSelection.Count > 0)
                {
                    selectedObjects = wholeSelection.ToArray();
                }
            }

            if (debugView) debugRenderers = GetDebugRenderers();

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                if (selectedObjects[i].TryGetComponent(out MeshFilter tempMeshFilter) && selectedObjects[i].TryGetComponent(out MeshRenderer tempMeshRenderer) && selectedObjects[i].gameObject.scene.IsValid() && mode == Mode.Mesh)
                {
                    hasPaintableObjects = true;
                    for (int m = 0; m < tempMeshRenderer.sharedMaterials.Length; m++)
                    {
                        Material mtl = tempMeshRenderer.sharedMaterials[m];
                        if (mtl.HasFloat("_PaintableCoverage") && mtl.GetFloat("_PaintableCoverage") == 0)
                        {
                            notPaintableMtlFound = true;
                            break;
                        }
                    }
                }

                if(selectedObjects[i].TryGetComponent(out Terrain tempTerrain) && selectedObjects[i].gameObject.scene.IsValid() && mode == Mode.Terrain)
                {
                    hasPaintableObjects = true;
                    if(tempTerrain.GetInstanceID() != lastTerrainID)
                    {
                        terrainMtlMaskTex = null;
                        terrainMtlNormalTex = null;
                    }

                    lastTerrainID = tempTerrain.GetInstanceID();
                    Material mtl = tempTerrain.materialTemplate;
                    if (mtl.HasFloat("_PaintableCoverage") && mtl.GetFloat("_PaintableCoverage") == 0)
                    {
                        notPaintableMtlFound = true;
                    }
                }
            }
            Repaint();
        }

        private List<MeshRenderer> GetDebugRenderers()
        {
            List<MeshRenderer> dRends = new List<MeshRenderer>();
            for (int i = 0; i < selectedObjects.Length; i++)
            {

                if (selectedObjects[i].TryGetComponent(out MeshRenderer rend) && selectedObjects[i].TryGetComponent(out MeshFilter f) && f.sharedMesh != null)
                {
                    dRends.Add(rend);
                }
            }

            return dRends;
        }

        private Vector3 mousePosA = Vector2.zero;
        private Vector3 mousePosB = Vector2.zero;
        private Vector3 pointA_WS;
        private Vector3 pointB_WS;
        private Matrix4x4 depthCamMatrix;
        private SceneView sceneView;
        private void OnSceneGui(SceneView currentSceneView)
        {
            sceneView = currentSceneView;

            if (mode == Mode.Terrain) TerrainModePainting();
            else MeshModePainting();

            if (!inPaintMode) return;

            HandleControls();

            if (e != null && e.type == EventType.Repaint)
            {
                if (isolationMode)
                    NL_IsolationStage.UpdateDepthCam(currentSceneView.camera, depthTex, out depthCamMatrix, out depthTex);
                else
                    depthCamMatrix = GetDepthCamMatrix(currentSceneView.camera);

                if (paintTool == MeshPaintTool.GradientFill && !altHold && gradientToolLR != null)
                {
                    if (LmbDown)
                    {
                        if (isolationMode) pointA_WS = NL_IsolationStage.GetPointWS(mousePosA, sceneView.camera);
                        else pointA_WS = currentSceneView.camera.ScreenToWorldPoint(new Vector3(mousePosA.x, currentSceneView.camera.pixelHeight - mousePosA.y, 10));
                    }

                    if (mouseUp && !RmbDown)
                    {
                        Vector2 camPixelSize;
                        Matrix4x4 viewProjectionMatrix;
                        if (isolationMode)
                        {
                            camPixelSize = new Vector2(NL_IsolationStage.ownerWindow.camera.pixelWidth * 2, NL_IsolationStage.ownerWindow.camera.pixelHeight * 2);
                            viewProjectionMatrix = depthCamMatrix;

                            for (int i = 0; i < paintableObjects.Count; i++)
                            {
                                paintableObjects[i].ConvertLocalToScreen(c_shader, viewProjectionMatrix, camPixelSize);
                            }
                        }
                        else
                        {
                            camPixelSize = new Vector2(currentSceneView.camera.pixelWidth, currentSceneView.camera.pixelHeight);
                            viewProjectionMatrix = GL.GetGPUProjectionMatrix(currentSceneView.camera.projectionMatrix, false) * currentSceneView.camera.worldToCameraMatrix;

                            for (int i = 0; i < paintableObjects.Count; i++)
                            {
                                paintableObjects[i].ConvertLocalToScreen(c_shader, viewProjectionMatrix, camPixelSize);
                            }
                        }

                        mouseUp = false;

                        Undo.RegisterCompleteObjectUndo(paintableObjects.ToArray(), "NL Painter Gradient Fill");
                        NL_GradientFillTool.ApplyGradient(c_shader, mousePosA, mousePosB, clampGradientStart, clampGradientEnd, gradientToolTex, targetValueMinMax, shiftHold ? -opacityMultiplier : opacityMultiplier, depthTex, depthCamMatrix, geometryCulling ? cullingBias : -100, normalThresholdInternal, channelMask, paintableObjects);
                        if (isolationMode) NL_IsolationStage.UpdateStreams(paintableObjects);
                    }

                    if (mouseDrag)
                    {

                        if (isolationMode) pointB_WS = NL_IsolationStage.GetPointWS(mousePosB);
                        else pointB_WS = currentSceneView.camera.ScreenToWorldPoint(new Vector3(mousePosB.x, currentSceneView.camera.pixelHeight - mousePosB.y, 10));

                        DrawGradientGizmo(pointA_WS, pointB_WS, 5, blueColor, currentSceneView.camera);
                    }

                    if (e.mousePosition.x < 0 || e.mousePosition.x > currentSceneView.camera.pixelWidth || e.mousePosition.y < 0 || e.mousePosition.y > currentSceneView.camera.pixelHeight)
                    {
                        mouseDrag = false;
                    }
                }
                else
                {
                    mouseUp = false;
                }

                Vector3 mousePosWS;

                if (isolationMode) mousePosWS = NL_IsolationStage.GetPointWS(e.mousePosition, sceneView.camera);
                else mousePosWS = currentSceneView.camera.ScreenToWorldPoint(new Vector3(e.mousePosition.x, currentSceneView.camera.pixelHeight - e.mousePosition.y, 10));

                Vector3 sceneLabelPos = mousePosWS - currentSceneView.camera.transform.up * 0.31f;

                if (paintTool == MeshPaintTool.Brush)
                {
                    DrawBrushGizmo(validHit.point, validHit.normal, currentSceneView.camera, sceneLabelPos);
                }
                else if (paintTool == MeshPaintTool.GradientFill)
                {
                    DrawPropertiesInScene(mousePosWS - currentSceneView.camera.transform.up * 0.31f);

                    if (isolationMode) NL_TotalBrushUtilities.DrawGradientPoint(mousePosWS, blueColor, new Color(0, 0, 0, 0.5f), NL_IsolationStage.ownerWindow.camera);
                    else DrawGradientPoint(mousePosWS, blueColor, new Color(0, 0, 0, 0.5f), currentSceneView.camera);
                }

                if (meshPainting && LmbDown)
                {
                    Undo.RegisterCompleteObjectUndo(paintableObjects.ToArray(), "NL Painter Changes");
                }

                if ((LmbDown || mouseDrag) && !altHold && !ctrlHold)
                {
                    //Disable erase functionallity on GBA channels in Splat Mode 
                    float op = 1;
                    if (soloMode && !channelMask[0]) op = brushOpacity;
                    else
                    {
                        op = shiftHold ? -brushOpacity : brushOpacity;
                    }

                    if (terrainPainting) PaintSplat(soloMode ? new Vector2(0, 1) : targetValueMinMax, GetUvSpacePos(), op, brushSize, brushAngleRandomness, brushAlpha);
                    if (meshPainting)
                    {
                        NL_BrushTool.PaintVerices(c_shader, soloMode ? new Vector2(0, 1) : targetValueMinMax, op, validHit.point, brushSize, brush, depthTex, depthCamMatrix, geometryCulling ? cullingBias : -100, normalThresholdInternal, channelMask, paintableObjects, soloMode);
                        if (isolationMode) NL_IsolationStage.UpdateStreams(paintableObjects);
                    }
                }

                if (focusKeyPressed && isolationMode) NL_IsolationStage.FrameSelected();

                shiftHold = false;
                ctrlHold = false;
                altHold = false;
                focusKeyPressed = false;
                terrainPainting = false;
                meshPainting = false;
                validHit.point = Vector3.positiveInfinity;
            }

            //Update view only when one of valid events detected to prevent any calculations in idle state
            if (e != null && (e.type == EventType.MouseDown || e.type == EventType.MouseMove || e.type == EventType.MouseDrag))
            {
                currentSceneView.Repaint();
            }
        }

        private Matrix4x4 GetDepthCamMatrix(Camera sceneViewCam)
        {
            if (depthCam == null) return Matrix4x4.zero;

            depthCam.Render();
            depthCam.transform.SetPositionAndRotation(sceneViewCam.transform.position, sceneViewCam.transform.rotation);
            depthCam.orthographic = sceneViewCam.orthographic;
            depthCam.fieldOfView = sceneViewCam.fieldOfView;
            depthCam.orthographicSize = sceneViewCam.orthographicSize;

            return GL.GetGPUProjectionMatrix(depthCam.projectionMatrix, true) * depthCam.worldToCameraMatrix;
        }

        private void DrawGradientPoint(Vector3 point, Color color, Color outlineColor, Camera cam)
        {
            float size = cam.orthographic ? cam.orthographicSize / 50 : cam.fieldOfView / 500;
            Handles.color = outlineColor;
            Handles.DrawSolidDisc(point, -cam.transform.forward, size);
            Handles.color = color;
            Handles.DrawSolidDisc(point, -cam.transform.forward, size * 0.7f);
        }

        private void DrawPropertiesInScene(Vector3 sceneLabelPos, Vector3 mousePos = new Vector3(), Vector3 normal = new Vector3(), Transform sceneViewTransform = null, Color outlineColor = new Color(), Color color = new Color(), Color intensityColor = new Color(), Color targetOpacityMinColor = new Color(), Color targetOpacityMaxColor = new Color(), float thickness = 0)
        {
            if (shiftHold && !LmbDown && !mouseDrag && scrolled)
            {
                if (sceneViewTransform != null)
                {
                    DrawSceneLabel(sceneLabelPos, $"Size = {brushSize.ToString("0.0")}");

                    Vector3 camToBrushDir = mousePos - sceneViewTransform.position;
                    Vector3 camToBrushRight = Vector3.Cross(normal, camToBrushDir).normalized;
                    Vector3 rightArrowDir = ((mousePos + Vector3.ProjectOnPlane(camToBrushRight, normal)) - mousePos) * (brushSize / 2);
                    Vector3 leftArrowDir = ((mousePos + Vector3.ProjectOnPlane(-camToBrushRight, normal)) - mousePos) * (brushSize / 2);
                    DrawArrowGizmo(mousePos, rightArrowDir, color, 5, outlineColor);
                    DrawArrowGizmo(mousePos, leftArrowDir, color, 5, outlineColor);
                }
            }
            else if ((e.modifiers.HasFlag(EventModifiers.Control) || e.modifiers.HasFlag(EventModifiers.Command)) && e.modifiers.HasFlag(EventModifiers.Alt) && !LmbDown && !mouseDrag && scrolled)
            {
                if (sceneViewTransform != null)
                {
                    Handles.color = intensityColor;
                    Handles.DrawSolidDisc(mousePos, normal, brushSize / 2);
                    DrawSceneLabel(sceneLabelPos, $"Opacity = {brushOpacity.ToString("0.000")}");
                }
                else
                {
                    DrawSceneLabel(sceneLabelPos, $"Opacity Multiplier = {opacityMultiplier.ToString("0.000")}");
                }

            }
            else if (altHold && !LmbDown && !mouseDrag && scrolled)
            {
                if (sceneViewTransform != null)
                {
                    Handles.color = targetOpacityMaxColor;
                    Handles.DrawWireDisc(mousePos, normal, brushSize / 2, thickness);
                }
                DrawSceneLabel(sceneLabelPos, $"Max Opacity = {targetValueMinMax.y.ToString("0.000")}");
            }
            else if (ctrlHold && !LmbDown && !mouseDrag && scrolled)
            {
                if (sceneViewTransform != null)
                {
                    Handles.color = targetOpacityMinColor;
                    Handles.DrawWireDisc(mousePos, normal, brushSize / 2, thickness);
                }
                DrawSceneLabel(sceneLabelPos, $"Min Opacity = {targetValueMinMax.x.ToString("0.000")}");
            }
            else
            {
                scrolled = false;
            }
        }

        private void DrawGradientGizmo(Vector3 pointA, Vector3 pointB, float segmentSize, Color color, Camera cam)
        {
            float width = cam.orthographic ? cam.orthographicSize : cam.fieldOfView / 10;
            Handles.color = Color.white;
            gradientToolLR.SetPosition(0, pointA);
            gradientToolLR.SetPosition(1, pointB);
            gradientToolLR.widthMultiplier = width;

            gizmoBuffer.Clear();
            gizmoBuffer.DrawRenderer(gradientToolLR, gradientToolMtl);
            Graphics.ExecuteCommandBuffer(gizmoBuffer);

            Vector3 dir = Vector3.Cross((pointA - pointB).normalized, cam.transform.forward) * width * 10;

            if (clampGradientStart)
            {
                Handles.color = new Color(0, 0, 0, 0.2f);
                Handles.DrawLine(pointA - dir, pointA + dir, 3);
                Handles.color = color;
                Handles.DrawDottedLine(pointA - dir, pointA + dir, segmentSize);
            }
            if (clampGradientEnd)
            {
                Handles.color = new Color(0, 0, 0, 0.2f);
                Handles.DrawLine(pointB - dir, pointB + dir, 3);
                Handles.color = color;
                Handles.DrawDottedLine(pointB - dir, pointB + dir, segmentSize);
            }

            Handles.color = color;
            Handles.DrawDottedLine(pointA, pointB, segmentSize);

            DrawGradientPoint(pointA, color, new Color(0, 0, 0, 0.5f), cam);
        }

        private void HandleControls()
        {
            if (e == null) return;

            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    if (paintTool == MeshPaintTool.GradientFill)
                    {
                        mousePosA = e.mousePosition;
                        mousePosA.z = 0;
                    }

                    LmbDown = true;
                    RmbDown = false;
                    mouseUp = false;
                    mouseDrag = false;
                }
                else if (e.button == 1)
                {
                    RmbDown = true;
                    LmbDown = false;
                    mouseUp = false;
                    mouseDrag = false;
                }
            }
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                if (paintTool == MeshPaintTool.GradientFill)
                {

                    mousePosB = e.mousePosition;
                    mousePosB.z = 0;
                }

                mouseUp = true;
                mouseDrag = false;
            }
            if (e.type == EventType.MouseDrag && e.button == 0 && !RmbDown)
            {
                if (paintTool == MeshPaintTool.GradientFill) mousePosB = e.mousePosition;
                mouseDrag = true;
            }

            if (e.type == EventType.MouseUp && e.button == 0) LmbDown = false;
            if (e.modifiers == EventModifiers.Shift) shiftHold = true;
            if (e.modifiers == EventModifiers.Alt) altHold = true;
            if (e.modifiers == EventModifiers.Control || e.modifiers == EventModifiers.Command) ctrlHold = true;
            if (e.keyCode == KeyCode.F) focusKeyPressed = true;
        }

        private void TerrainModePainting()
        {
            if (selectedObjects.Length != 1)
            {
                terrain = null;
                return;
            }

            if (selectedObjects[0] != null)
            {
                terrain = selectedObjects[0].GetComponent<Terrain>();

                if (terrain != null)
                {
                    terrainCollider = terrain.GetComponent<TerrainCollider>();
                }
            }
            else
            {
                terrain = null;
            }

            if (selectedObjects[0] == null || terrain == null) return;

            e = Event.current;

            if (!inPaintMode) return;

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (terrainCollider.Raycast(ray, out hit, 1000))
            {
                validHit = hit;
                ChangeSettingsWithHotkeys();
                HandleControls();
                terrainPainting = true;
            }
            else
            {
                validHit.point = Vector3.positiveInfinity;
            }
        }

        RaycastHit validHit = new RaycastHit();
        private void MeshModePainting()
        {
            if (debugView)
            {
                for (int i = 0; i < debugRenderers.Count; i++)
                {
                    ShowVertexColors(debugRenderers[i]);
                }
            }

            if (paintableObjects.Count == 0) return;

            e = Event.current;

            if (!inPaintMode) return;
            lastTool = Tools.current;
            Tools.current = Tool.None;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (paintTool == MeshPaintTool.Brush)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                bool hasObj = false;
                List<RaycastHit> allHits = new List<RaycastHit>();
                float minDist = Mathf.Infinity;

                for (int i = 0; i < paintableObjects.Count; i++)
                {
                    if (paintableObjects[i].mCollider == null)
                    {
                        StopPainting();
                        return;
                    }

                    if (paintableObjects[i].mCollider.Raycast(ray, out hit, 1000))
                    {
                        allHits.Add(hit);

                        for (int h = 0; h < allHits.Count; h++)
                        {
                            float dist = Vector3.Distance(allHits[h].point, SceneView.currentDrawingSceneView.camera.transform.position);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                hasObj = true;
                                validHit = allHits[h];
                            }
                        }
                    }
                }

                if (hasObj)
                {
                    ChangeSettingsWithHotkeys();
                    HandleControls();
                    meshPainting = true;
                }
                else
                {
                    validHit.point = Vector3.positiveInfinity;
                }
            }
            else
            {
                ChangeSettingsWithHotkeys();
            }
        }

        private void ShowVertexColors(MeshRenderer debugRend)
        {
            if (debugRend == null) return;

            debugBuffer.Clear();
            for (int i = 0; i < debugRend.sharedMaterials.Length; i++)
            {
                debugBuffer.DrawRenderer(debugRend, vertexDebugMtl, i);
            }

            Graphics.ExecuteCommandBuffer(debugBuffer);
        }

        private float GetScrollValue(Vector2 scrollInput)
        {
            if (Mathf.Abs(scrollInput.x) > Mathf.Abs(scrollInput.y))
                return scrollInput.x;
            else 
                return scrollInput.y;
        }

        private void ChangeSettingsWithHotkeys()
        {
            if (e.type == EventType.ScrollWheel)
            {
                if (e.modifiers.HasFlag(EventModifiers.Control) && e.modifiers.HasFlag(EventModifiers.Alt))
                {
                    e.Use();
                    ChangeBrushOpacity(Mathf.Sign(e.delta.y));
                }
                else if (e.modifiers == EventModifiers.Shift)
                {
                    if (paintTool != MeshPaintTool.GradientFill)
                    {
                        ChangeBrushSize(Mathf.Sign(GetScrollValue(e.delta))); //Unity 2022+ fix: GetScrollValue returns current active scroll axis
                        e.Use();
                    }
                }
                else if (e.modifiers == EventModifiers.Alt && !(e.modifiers == EventModifiers.Control || e.modifiers == EventModifiers.Command))
                {
                    e.Use();
                    ChangeTargetValueMax(Mathf.Sign(e.delta.y));
                }
                else if (e.modifiers != EventModifiers.Alt && (e.modifiers == EventModifiers.Control || e.modifiers == EventModifiers.Command))
                {
                    e.Use();
                    ChangeTargetValueMin(Mathf.Sign(e.delta.y));
                }

                ValidateTargetValueSlider();
                so.ApplyModifiedProperties();
                so.Update();
                Repaint();

                scrolled = true;
            }
        }

        private void DrawBrushGizmo(Vector3 pos, Vector3 normal, Camera cam, Vector3 sceneLabelPos)
        {
            if (hit.point == Vector3.positiveInfinity || paintTool == MeshPaintTool.GradientFill) return;

            Transform sceneViewTransform = cam.transform;

            float distanceMultiplier = Vector3.Distance(cam.transform.position, pos);
            Vector3 camDir = (pos - cam.transform.position).normalized;

            Color intensityColor = blueColor;
            Color targetOpacityMaxColor = blueColor;
            Color targetOpacityMinColor = blueColor;
            Color color = Vector3.Dot(normal, -camDir) < normalThresholdInternal ? Color.red : Color.white;
            Color outlineColor = new Color(0, 0, 0, 0.5f);
            color.a = 0.5f;

            intensityColor.a = Mathf.Lerp(0, 0.7f, brushOpacity);
            targetOpacityMaxColor.a = targetValueMinMax.y;
            targetOpacityMinColor.a = targetValueMinMax.x;

            Handles.color = new Color(0, 0, 0, 0.5f);
            Handles.DrawWireDisc(pos, normal, brushSize / 2, 5);
            Handles.color = color;
            Handles.DrawLine(pos, pos + normal * distanceMultiplier * 0.1f);
            Handles.DrawWireDisc(pos, normal, brushSize / 2);


            float thickness = (50f / distanceMultiplier) * brushSize;

            DrawPropertiesInScene(sceneLabelPos, pos, normal, sceneViewTransform, outlineColor, color, intensityColor, targetOpacityMinColor, targetOpacityMaxColor, thickness);
        }

        private void DrawSceneLabel(Vector3 pos, string text)
        {
            Handles.color = blueColor;
            Handles.Label(pos, text, sceneLabelStyle);
            Handles.color = Color.white;
        }

        private void DrawArrowGizmo(Vector3 pos, Vector3 direction, Color color, float outlineThickness = 0, Color outlineColor = new Color(), float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
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

        private void ChangeBrushSize(float wheelSign)
        {
            brushSize = Mathf.Clamp(brushSize - wheelSign * 0.2f, 0, 100);
        }

        private void ChangeBrushOpacity(float wheelSign)
        {
            if (paintTool == MeshPaintTool.Brush)
                brushOpacity = Mathf.Clamp(brushOpacity - wheelSign * 0.025f, 0.005f, 1);
            else
                opacityMultiplier = Mathf.Clamp(opacityMultiplier - wheelSign * 0.025f, 0.005f, 1);
        }

        private void ChangeTargetValueMin(float wheelSign)
        {
            targetValueMinMax.x = Mathf.Clamp(targetValueMinMax.x - wheelSign * 0.025f, 0.005f, 1);
        }

        private void ChangeTargetValueMax(float wheelSign)
        {
            targetValueMinMax.y = Mathf.Clamp(targetValueMinMax.y - wheelSign * 0.025f, 0.005f, 1);
        }

        private Vector2 GetUvSpacePos()
        {
            Vector3 terrainSize = terrain.terrainData.size;
            Vector3 relativeMousePos = validHit.point - terrain.transform.position;

            Vector2 uvSpacePos = new Vector2(relativeMousePos.x / terrainSize.x, relativeMousePos.z / terrainSize.z);

            return uvSpacePos;
        }

        private string GetTexPath(Material mtl, string texPostfix)
        {
            string splatMapPath = AssetDatabase.GetAssetPath(mtl);
            splatMapPath = splatMapPath.Remove(splatMapPath.Length - 4) + texPostfix + ".png";

            return splatMapPath;
        }

        private void RtToTex(RenderTexture rt, Texture2D tex, string texPostfix, string shaderPropName, TextureFormat format)
        {
            if (rt == null) return;

            string splatMapPath = GetTexPath(terrainMtl, texPostfix);
            string absoluteSplatMapPath = Application.dataPath.Remove(Application.dataPath.Length - 7) + "/" + splatMapPath;

            tex = new Texture2D(rt.width, rt.height, format, false);

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(absoluteSplatMapPath, bytes);

            tex = (Texture2D)AssetDatabase.LoadAssetAtPath(splatMapPath, typeof(Texture2D));

            AssetDatabase.Refresh();
            terrainMtl.SetTexture(shaderPropName, tex);
            SetTextureImportSettings(splatMapPath);
        }

        private void SetTextureImportSettings(string texPath)
        {
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texPath);
            TextureImporterSettings settings = new TextureImporterSettings();
            TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
            importer.ReadTextureSettings(settings);
            settings.sRGBTexture = false;
            platformSettings.format = TextureImporterFormat.Automatic;
            platformSettings.textureCompression = TextureImporterCompression.CompressedHQ;
            //platformSettings.format = TextureImporterFormat.R8;
            platformSettings.maxTextureSize = texRes;
            importer.SetTextureSettings(settings);
            importer.SetPlatformTextureSettings(platformSettings);
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        private void PaintSplat(Vector2 targetValueMinMax, Vector2 brushPosition, float brushOpacity, float brushSize, float brushAngleRandom, Texture2D brushTex)
        {
            paintMtl.SetFloat("_IsFilling", 0);
            paintMtl.SetVector("_TargetValMinMax", targetValueMinMax);

            paintMtl.SetVector("_BrushPos", brushPosition);
            paintMtl.SetTexture("_BrushAlpha", brushTex);

            Vector4 intensity = Vector4.zero;

            intensity = GetIntensity(channelMask, brushColor, brushOpacity, false, soloMode);

            paintMtl.SetVector("_BrushOpacity", intensity);
            paintMtl.SetFloat("_BrushAngle", UnityEngine.Random.Range(0, Mathf.Lerp(0, 360, brushAngleRandom) * (Mathf.PI / 180)));

            paintMtl.SetVector("_BrushSize", new Vector2(brushSize / terrain.terrainData.size.x, brushSize / terrain.terrainData.size.z));
            paintMtl.SetTexture("_SplatTex", mask_prePaint);

            float terFactor = 1 / ((terrain.terrainData.size.x + terrain.terrainData.size.z) / 2);
            float nSpread = terFactor * 0.25f;

            paintMtl.SetFloat("_NormalSpread", nSpread);
            paintMtl.SetFloat("_NormalScale", terFactor * 2);

            Graphics.Blit(mask_prePaint, mask_afterPaint, paintMtl, 0);
            Graphics.Blit(mask_afterPaint, mask_prePaint);

            Graphics.Blit(mask_afterPaint, normalRT, paintMtl, 1);

            RenderTexture.active = null;

            terrainMtl.SetTexture("_PaintedMask", mask_afterPaint);
            if(terrainMtl.HasProperty("_PaintedMaskNormal")) terrainMtl.SetTexture("_PaintedMaskNormal", normalRT);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private void FillSplat(float fillingValue)
        {
            InitTextures(false);

            paintMtl.SetFloat("_IsFilling", 1);

            paintMtl.SetVector("_FillingValue", GetIntensity(channelMask, fillColor, fillingValue, true, soloMode));
            paintMtl.SetTexture("_SplatTex", mask_prePaint);

            Graphics.Blit(mask_prePaint, mask_afterPaint, paintMtl, 0);
            Graphics.Blit(mask_afterPaint, mask_prePaint);
            Graphics.Blit(mask_afterPaint, normalRT, paintMtl, 1);

            RenderTexture.active = null;

            terrainMtl.SetTexture("_PaintedMask", mask_afterPaint);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            if (!inPaintMode)
            {
                RtToTex(mask_afterPaint, terrainMtlMaskTex, maskPostfix, "_PaintedMask", TextureFormat.ARGB32);
                if(terrainMtl.HasProperty("_PaintedMaskNormal")) RtToTex(normalRT, terrainMtlNormalTex, maskNormalPostfix, "_PaintedMaskNormal", TextureFormat.RGB24);
            }
        }

        private Vector4 GetIntensity(bool[] cMask, Vector4 color, float opacity, bool isFilling = false, bool soloMode = false)
        {
            Vector4 intensity = isFilling ? Vector4.one * -1 : Vector4.zero;

            color *= opacity;

            if (cMask[0]) intensity.x = color.x; // R channel used for the overall snow mask, so paint into it as usual

            if (soloMode)
            {
                if (!cMask[0])
                {
                    //In splat mode we must paint only into one channel while erase other 3 channels
                    intensity.y = cMask[1] ? color.y : -color.y;
                    intensity.z = cMask[2] ? color.z : -color.z;
                    intensity.w = cMask[3] ? color.w : -color.w;
                }
            }
            else
            {
                if (cMask[1]) intensity.y = color.y;
                if (cMask[2]) intensity.z = color.z;
                if (cMask[3]) intensity.w = color.w;
            }

            return intensity;
        }

        private void InitTextures(bool fill = true)
        {
            paintMtl = new Material(Shader.Find("Hidden/NOT_Lonely/TotalBrush/MaskPainting"));

            terrainMtl = terrain.materialTemplate;

            if (terrainMtlMaskTex == null) terrainMtlMaskTex = terrainMtl.GetTexture("_PaintedMask") as Texture2D;
            else terrainMtl.SetTexture("_PaintedMask", terrainMtlMaskTex);

            if (terrainMtl.HasTexture("_PaintedMaskNormal"))
            {
                if (terrainMtlNormalTex == null) terrainMtlNormalTex = terrainMtl.GetTexture("_PaintedMaskNormal") as Texture2D;
                else terrainMtl.SetTexture("_PaintedMaskNormal", terrainMtlNormalTex);
            }

            if (mask_prePaint != null) mask_prePaint.Release();
            if (mask_afterPaint != null) mask_afterPaint.Release();
            if(normalRT != null) normalRT.Release();

            int res = 4096;

            mask_afterPaint = new RenderTexture(res, res, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            mask_prePaint = new RenderTexture(res, res, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            normalRT = new RenderTexture(res, res, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

            if (terrainMtlMaskTex != null)
            {
                Graphics.Blit(terrainMtlMaskTex, mask_prePaint);
                Graphics.Blit(terrainMtlMaskTex, mask_afterPaint);
                if(terrainMtl.HasProperty("_PaintedMaskNormal")) Graphics.Blit(terrainMtlNormalTex, normalRT);
                //Debug.Log("get tex from mtl");
            }
            else
            {
                if (fill)
                {
                    FillSplat(0.5001f);
                    RtToTex(mask_afterPaint, terrainMtlMaskTex, maskPostfix, "_PaintedMask", TextureFormat.ARGB32);
                    if (terrainMtl.HasTexture("_PaintedMaskNormal")) RtToTex(normalRT, terrainMtlNormalTex, maskNormalPostfix, "_PaintedMaskNormal", TextureFormat.RGB24);
                }
                Debug.Log("New tex created");
            }

            terrainMtl.SetTexture("_PaintedMask", mask_prePaint);
            if (terrainMtl.HasTexture("_PaintedMaskNormal")) terrainMtl.SetTexture("_PaintedMaskNormal", normalRT);

            isSplatInit = true;

            RenderTexture.active = null;
        }
    }
}
#endif
