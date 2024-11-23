/*
using UnityEngine;
using UnityEngine.XR.ARFoundation;


public class ARToPlane : MonoBehaviour
{
    public ARCameraBackground arCameraBackground; // AR ī�޶� ���
    public RenderTexture renderTexture;          // Render Texture
    public Material planeMaterial;               // Plane�� ������ Material

    void Update()
    {
        if (arCameraBackground.material != null)
        {
            // AR ī�޶� ȭ���� Render Texture�� ����
            Graphics.Blit(null, renderTexture, arCameraBackground.material);

            // Plane�� Material�� Render Texture�� ����
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
    public ARCameraManager arCameraManager; // ARCameraManager�� ����
    public RenderTexture renderTexture;    // Render Texture�� ����
    public Material planeMaterial;         // Plane�� ����� Material

    private void OnEnable()
    {
        // frameReceived �̺�Ʈ�� ����
        arCameraManager.frameReceived += OnCameraFrameReceived;
    }

    private void OnDisable()
    {
        // �̺�Ʈ ���� ����
        arCameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (args.textures.Count > 0)
        {
            // ù ��° �ؽ�ó(Android �������� ���� �ؽ�ó ���)
            var cameraTexture = args.textures[0];
            Graphics.Blit(cameraTexture, renderTexture, planeMaterial);

            // Plane�� Material�� Render Texture ����
            planeMaterial.mainTexture = renderTexture;
        }
    }
}


/*

using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARToPlane : MonoBehaviour
{
    public ARCameraManager arCameraManager; // ARCameraManager�� ����
    private Texture2D cameraTexture;

    void Start()
    {
        // AR ī�޶��� �ؽ�ó�� �����ͼ� material�� ����
        if (arCameraManager != null)
        {
            arCameraManager.frameReceived += OnCameraFrameReceived;
        }
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        // AR ī�޶��� �ؽ�ó�� ��������
        if (args.textures.Count > 0)
        {
            var cameraFrameTexture = args.textures[0];

            // �ؽ�ó�� �������� mainTexture�� ����
            GetComponent<Renderer>().material.mainTexture = cameraFrameTexture;
        }
    }

    void OnDestroy()
    {
        // AR ī�޶󿡼� ���� ������ �̺�Ʈ ���� ����
        if (arCameraManager != null)
        {
            arCameraManager.frameReceived -= OnCameraFrameReceived;
        }
    }
}
*/