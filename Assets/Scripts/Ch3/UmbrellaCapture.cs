using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UmbrellaCapture : MonoBehaviour
{
    public Camera arCamera; // AR 카메라
    public RawImage displayImage; // 캡쳐된 이미지를 표시할 UI 요소

    private Texture2D capturedImage;

    public Color whiteColor = Color.white; // 흰색 색상

    // 캡처 버튼을 클릭했을 때 호출되는 메서드
    public void CaptureUmbrella()
    {
        // AR 카메라 화면을 캡처하는 과정
        capturedImage = CaptureImageFromCamera(); // AR 카메라에서 이미지를 캡처

        // 흰색 부분을 투명하게 만드는 처리
        Texture2D processedImage = ProcessImage(capturedImage);

        // 이미지를 PlayerPrefs에 저장
        SaveImageToPlayerPrefs(processedImage);

        // 캡처한 이미지를 UI에 띄우기
        DisplayCapturedImage(processedImage);

        // 새로운 씬(ch3_1)으로 전환
        //SceneManager.LoadScene("ch3_1");
    }

    // AR 카메라에서 이미지를 캡처하는 메서드
    private Texture2D CaptureImageFromCamera()
    {
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        arCamera.targetTexture = renderTexture;
        arCamera.Render(); // 카메라 화면을 렌더링

        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();

        arCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        return texture;
    }

    // 이미지를 PlayerPrefs에 base64로 저장하는 메서드
    private void SaveImageToPlayerPrefs(Texture2D image)
    {
        string base64Image = System.Convert.ToBase64String(image.EncodeToPNG()); // 이미지를 PNG로 인코딩
        PlayerPrefs.SetString("UmbrellaImage", base64Image); // PlayerPrefs에 저장
        PlayerPrefs.Save(); // 저장
    }

    // 캡쳐된 이미지를 UI에 표시하는 메서드
    private void DisplayCapturedImage(Texture2D image)
    {
        displayImage.texture = image;
        displayImage.gameObject.SetActive(true); // UI에서 이미지를 표시
    }

    // 흰색 부분을 투명하게 만드는 메서드
    private Texture2D ProcessImage(Texture2D originalImage)
    {
        int width = originalImage.width;
        int height = originalImage.height;
        Texture2D newImage = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // 이미지의 모든 픽셀을 검사하여 흰색인 부분을 투명하게 처리
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color pixelColor = originalImage.GetPixel(x, y);

                // 흰색 색상인지 체크
                if (IsWhiteColor(pixelColor))
                {
                    newImage.SetPixel(x, y, new Color(0, 0, 0, 0)); // 흰색을 투명하게 설정
                }
                else
                {
                    newImage.SetPixel(x, y, pixelColor); // 나머지 색상은 그대로 유지
                }
            }
        }

        newImage.Apply(); // 이미지 변경 사항 적용
        return newImage;
    }

    // 색이 흰색인지 확인하는 함수
    private bool IsWhiteColor(Color color)
    {
        float tolerance = 0.5f; // 색상 차이를 허용하는 범위
        return Mathf.Abs(color.r - whiteColor.r) < tolerance &&
               Mathf.Abs(color.g - whiteColor.g) < tolerance &&
               Mathf.Abs(color.b - whiteColor.b) < tolerance;
    }
}
