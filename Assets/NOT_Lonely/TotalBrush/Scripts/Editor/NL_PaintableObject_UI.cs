#if UNITY_EDITOR
namespace NOT_Lonely.TotalBrush
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(NL_PaintableObject))]
    public class NL_PaintableObject_UI : Editor
    {
        private NL_PaintableObject paintableObject;

        public override void OnInspectorGUI()
        {
            paintableObject = target as NL_PaintableObject;

            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Open Total Brush")) NL_TotalBrush.OpenWindow();

            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(new GUIContent("Additional Vertex Streams data", "Additional Vertex Streams on the current Mesh Renderer."));

            EditorGUI.BeginDisabledGroup(paintableObject.cachedStreams == null);
            if (GUILayout.Button(new GUIContent("Restore", "Delete current painted data and restore previous vertex streams if it exists.")))
            {
                bool option = EditorUtility.DisplayDialog("Restore Previous Vertex Streams?", "You are about to DELETE CURRENT painted data and RESTORE PREVIOUS vertex streams. \nCurrent painted data will be lost. Are you sure?", "Yes, restore", "Cancel");
                switch (option)
                {
                    case true:
                        paintableObject.DeletePaintedData(true);
                        break;
                    case false:
                        break;
                }
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(new GUIContent("Clear", "Delete Vertex Streams completely including current painted data.")))
            {
                bool option = EditorUtility.DisplayDialog("Delete Vertex Streams including painted data?", "You are about to delete the Additional Vertex Streams from the Mesh Renderer including painted data. \nAre you sure?", "Yes, delete completely", "Cancel");
                switch (option)
                {
                    case true:
                        paintableObject.DeletePaintedData(false);
                        break;
                    case false:
                        break;
                }
            }

            if(GUILayout.Button("Check Streams"))
            {
                paintableObject.CheckStreams();
            }

            GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                if (paintableObject != null) serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif
