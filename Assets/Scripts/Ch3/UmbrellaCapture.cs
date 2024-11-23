using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UmbrellaCapture : MonoBehaviour
{
    public Camera arCamera; // AR ī�޶�
    public RawImage displayImage; // ĸ�ĵ� �̹����� ǥ���� UI ���

    private Texture2D capturedImage;

    public Color whiteColor = Color.white; // ��� ����

    // ĸó ��ư�� Ŭ������ �� ȣ��Ǵ� �޼���
    public void CaptureUmbrella()
    {
        // AR ī�޶� ȭ���� ĸó�ϴ� ����
        capturedImage = CaptureImageFromCamera(); // AR ī�޶󿡼� �̹����� ĸó

        // ��� �κ��� �����ϰ� ����� ó��
        Texture2D processedImage = ProcessImage(capturedImage);

        // �̹����� PlayerPrefs�� ����
        SaveImageToPlayerPrefs(processedImage);

        // ĸó�� �̹����� UI�� ����
        DisplayCapturedImage(processedImage);

        // ���ο� ��(ch3_1)���� ��ȯ
        //SceneManager.LoadScene("ch3_1");
    }

    // AR ī�޶󿡼� �̹����� ĸó�ϴ� �޼���
    private Texture2D CaptureImageFromCamera()
    {
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        arCamera.targetTexture = renderTexture;
        arCamera.Render(); // ī�޶� ȭ���� ������

        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();

        arCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        return texture;
    }

    // �̹����� PlayerPrefs�� base64�� �����ϴ� �޼���
    private void SaveImageToPlayerPrefs(Texture2D image)
    {
        string base64Image = System.Convert.ToBase64String(image.EncodeToPNG()); // �̹����� PNG�� ���ڵ�
        PlayerPrefs.SetString("UmbrellaImage", base64Image); // PlayerPrefs�� ����
        PlayerPrefs.Save(); // ����
    }

    // ĸ�ĵ� �̹����� UI�� ǥ���ϴ� �޼���
    private void DisplayCapturedImage(Texture2D image)
    {
        displayImage.texture = image;
        displayImage.gameObject.SetActive(true); // UI���� �̹����� ǥ��
    }

    // ��� �κ��� �����ϰ� ����� �޼���
    private Texture2D ProcessImage(Texture2D originalImage)
    {
        int width = originalImage.width;
        int height = originalImage.height;
        Texture2D newImage = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // �̹����� ��� �ȼ��� �˻��Ͽ� ����� �κ��� �����ϰ� ó��
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color pixelColor = originalImage.GetPixel(x, y);

                // ��� �������� üũ
                if (IsWhiteColor(pixelColor))
                {
                    newImage.SetPixel(x, y, new Color(0, 0, 0, 0)); // ����� �����ϰ� ����
                }
                else
                {
                    newImage.SetPixel(x, y, pixelColor); // ������ ������ �״�� ����
                }
            }
        }

        newImage.Apply(); // �̹��� ���� ���� ����
        return newImage;
    }

    // ���� ������� Ȯ���ϴ� �Լ�
    private bool IsWhiteColor(Color color)
    {
        float tolerance = 0.5f; // ���� ���̸� ����ϴ� ����
        return Mathf.Abs(color.r - whiteColor.r) < tolerance &&
               Mathf.Abs(color.g - whiteColor.g) < tolerance &&
               Mathf.Abs(color.b - whiteColor.b) < tolerance;
    }
}
