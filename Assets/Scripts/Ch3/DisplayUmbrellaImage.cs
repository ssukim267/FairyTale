using UnityEngine;
using UnityEngine.UI;

public class DisplayUmbrellaImage: MonoBehaviour
{
    public RawImage displayImage; // 캡쳐된 이미지를 표시할 UI 요소

    void Start()
    {
        // PlayerPrefs에서 이미지 불러오기
        string base64Image = PlayerPrefs.GetString("UmbrellaImage", null);

        if (!string.IsNullOrEmpty(base64Image))
        {
            byte[] imageBytes = System.Convert.FromBase64String(base64Image);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes); // base64에서 이미지로 변환
            displayImage.texture = texture; // RawImage에 텍스처 할당
            displayImage.gameObject.SetActive(true); // 이미지 표시
        }
    }
}
