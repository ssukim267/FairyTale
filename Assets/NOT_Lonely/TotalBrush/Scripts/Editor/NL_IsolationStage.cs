#if UNITY_EDITOR
namespace NOT_Lonely.TotalBrush
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.PackageManager.UI;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    public class NL_IsolationStage : PreviewSceneStage
    {
        public static NL_IsolationModeWindow ownerWindow;
        public GUIContent titleContent;

        public static Camera depthCam;
        public static List<NL_PaintableObject> paintableObjects = new List<NL_PaintableObject>();
        public static Bounds pObjectsBounds;

        public void Setup(List<NL_PaintableObject> sourcePaintableObjects)
        {
            paintableObjects = new List<NL_PaintableObject>();

            for (int i = 0; i < sourcePaintableObjects.Count; i++)
            {
                NL_PaintableObject pObj = Instantiate(sourcePaintableObjects[i]);
                StageUtility.PlaceGameObjectInCurrentStage(pObj.gameObject);

                Transform sourceParent = sourcePaintableObjects[i].transform;

                Transform lastChild = null;
                Vector3 lastChildScale = sourceParent.localScale;
                Vector3 lastChildLocalPos = sourceParent.localPosition;
                Quaternion lastChildLocalRot = sourceParent.localRotation;

                Transform parentTransform;

                while (sourceParent != null)
                {
                    parentTransform = new GameObject(sourceParent.name).transform;
                    StageUtility.PlaceGameObjectInCurrentStage(parentTransform.gameObject);

                    if (lastChild == null)//we are on the most first object in the hierarchy
                    {
                        pObj.transform.parent = parentTransform;
                        pObj.transform.localScale = Vector3.one;
                        pObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    }
                    else
                    {
                        lastChild.parent = parentTransform;
                        lastChild.localScale = lastChildScale;
                        lastChild.SetLocalPositionAndRotation(lastChildLocalPos, lastChildLocalRot);

                        lastChildScale = sourceParent.localScale;
                        lastChildLocalPos = sourceParent.localPosition;
                        lastChildLocalRot = sourceParent.localRotation;
                    }

                    lastChild = parentTransform;
                    sourceParent = sourceParent.parent;
                }

                paintableObjects.Add(pObj);
            }

            pObjectsBounds = NL_TotalBrushUtilities.CalculateBounds(paintableObjects);
            ownerWindow.Frame(pObjectsBounds, false);
            depthCam = ownerWindow.camera;
        }

        public static void FrameSelected()
        {
            ownerWindow.Frame(pObjectsBounds, false);
        }

        public static void UpdateDepthCam(in Camera sceneViewCam, in RenderTexture depthSource, out Matrix4x4 depthCamMatrix, out RenderTexture depthTexOutput)
        {
            ownerWindow.camera.targetTexture = depthSource;

            depthCamMatrix = GL.GetGPUProjectionMatrix(ownerWindow.camera.projectionMatrix, true) * ownerWindow.camera.worldToCameraMatrix;
            depthTexOutput = depthSource;
        }

        public static Vector3 GetPointWS(Vector3 mousePos, Camera cam = null)
        {
            if (cam == null) cam = ownerWindow.camera;

            return cam.ScreenToWorldPoint(new Vector3(mousePos.x / 2, cam.scaledPixelHeight - mousePos.y / 2, 10));
        }

        public static void UpdateStreams(List<NL_PaintableObject> sourcePaintableObjects)
        {
            for (int i = 0; i < paintableObjects.Count; i++)
            {
                paintableObjects[i].tempColors = sourcePaintableObjects[i].tempColors;
                paintableObjects[i].UpdateVertexStreams();
            }
        }

        protected override GUIContent CreateHeaderContent()
        {
            return titleContent;
        }
    }
}
#endif