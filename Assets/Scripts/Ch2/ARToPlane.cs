/*
using UnityEngine;
using UnityEngine.XR.ARFoundation;


public class ARToPlane : MonoBehaviour
{
    public ARCameraBackground arCameraBackground; // AR 카메라 배경
    public RenderTexture renderTexture;          // Render Texture
    public Material planeMaterial;               // Plane에 적용할 Material

    void Update()
    {
        if (arCameraBackground.material != null)
        {
            // AR 카메라 화면을 Render Texture로 복사
            Graphics.Blit(null, renderTexture, arCameraBackground.material);

            // Plane의 Material에 Render Texture를 연결
            if (planeMaterial != null)
            {
                planeMaterial.mainTexture = renderTexture;
            }
        }
    }
}

*/



using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARToPlane : MonoBehaviour
{
    public ARCameraManager arCameraManager; // ARCameraManager를 연결
    public RenderTexture renderTexture;    // Render Texture를 연결
    public Material planeMaterial;         // Plane에 사용할 Material

    private void OnEnable()
    {
        // frameReceived 이벤트에 구독
        arCameraManager.frameReceived += OnCameraFrameReceived;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        arCameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (args.textures.Count > 0)
        {
            // 첫 번째 텍스처(Android 기준으로 단일 텍스처 사용)
            var cameraTexture = args.textures[0];
            Graphics.Blit(cameraTexture, renderTexture, planeMaterial);

            // Plane의 Material에 Render Texture 적용
            planeMaterial.mainTexture = renderTexture;
        }
    }
}


/*

using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARToPlane : MonoBehaviour
{
    public ARCameraManager arCameraManager; // ARCameraManager를 연결
    private Texture2D cameraTexture;

    void Start()
    {
        // AR 카메라의 텍스처를 가져와서 material에 적용
        if (arCameraManager != null)
        {
            arCameraManager.frameReceived += OnCameraFrameReceived;
        }
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        // AR 카메라의 텍스처를 가져오기
        if (args.textures.Count > 0)
        {
            var cameraFrameTexture = args.textures[0];

            // 텍스처를 렌더러의 mainTexture로 설정
            GetComponent<Renderer>().material.mainTexture = cameraFrameTexture;
        }
    }

    void OnDestroy()
    {
        // AR 카메라에서 받은 프레임 이벤트 구독 해제
        if (arCameraManager != null)
        {
            arCameraManager.frameReceived -= OnCameraFrameReceived;
        }
    }
}
*/