using NOT_Lonely.TotalBrush;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), id_ovrl, "")]
public class NL_TotalBrushOverlay : ToolbarOverlay
{
    NL_TotalBrushOverlay() : base(NL_TotalBrushOverlayBtn.id)
    {

    }

    private const string id_ovrl = "TotalBrush-overlay";
    public override VisualElement CreatePanelContent()
    {
        VisualElement root = new VisualElement();
        root.style.width = new StyleLength(new Length(32, LengthUnit.Pixel));
        root.style.height = new StyleLength(new Length(20, LengthUnit.Pixel));

        Button openTB = new Button(() => OpenTotalBrush());
        openTB.style.flexGrow = 1;
        openTB.style.marginBottom = 0;
        openTB.style.marginTop = 0;
        openTB.style.marginLeft = 0;
        openTB.style.marginRight = 0;
        openTB.text = "TB";

        root.Add(openTB);

        return root;
    }

    public static void OpenTotalBrush()
    {
        NL_TotalBrush.Mode mode = NL_TotalBrush.Mode.Mesh;

        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Terrain>() != null)
            mode = NL_TotalBrush.Mode.Terrain;

        NL_TotalBrush.OpenWindowExternal(mode);
    }
}
