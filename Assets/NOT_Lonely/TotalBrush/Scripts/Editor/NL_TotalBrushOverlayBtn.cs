using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using NOT_Lonely.TotalBrush;

[EditorToolbarElement(id, typeof(SceneView))]
public class NL_TotalBrushOverlayBtn : EditorToolbarButton
{
    public const string id = "OpenTotalBrush-btn";

    NL_TotalBrushOverlayBtn()
    {
        //text = "Open Total Brush";
        tooltip = "Open the Total Brush window";
        icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/NOT_Lonely/TotalBrush/UI/ToolbarIcon_openTB.png");
        clicked += OnOpenTotalBrush;
    }

    void OnOpenTotalBrush()
    {
        NL_TotalBrushOverlay.OpenTotalBrush();
    }
}
