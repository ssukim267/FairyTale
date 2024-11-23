#if UNITY_EDITOR
namespace NOT_Lonely.TotalBrush
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    [EditorWindowTitle(title = "Isolation Window", useTypeNameAsIconName = false)]
    public class NL_IsolationModeWindow : SceneView
    {
        public static NL_IsolationStage stage;
        public static NL_IsolationModeWindow window;

        public static void SwitchIsolationMode(bool state, List<NL_PaintableObject> pObjects = null)
        {
            if (state)
            {
                window = GetWindow<NL_IsolationModeWindow>(typeof(SceneView));

                window.sceneLighting = false;
                window.drawGizmos = false;
                window.SetupWindow(pObjects);
                window.Repaint();
            }
            else
            {
                StageUtility.GoToMainStage();
                if (stage != null) DestroyImmediate(stage);
                if (window != null) window.Close();
            }
        }

        private void SetupWindow(List<NL_PaintableObject> pObjects = null)
        {
            titleContent = new GUIContent("Isolation Mode");

            stage = ScriptableObject.CreateInstance<NL_IsolationStage>();
            NL_IsolationStage.ownerWindow = this;
            stage.titleContent = titleContent;

            StageUtility.GoToStage(stage, true);

            stage.Setup(pObjects);
        }
    }
}
#endif
