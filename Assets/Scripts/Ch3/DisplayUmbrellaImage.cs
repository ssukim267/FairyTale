using UnityEngine;
using UnityEngine.UI;

public class DisplayUmbrellaImage: MonoBehaviour
{
    public RawImage displayImage; // ĸ�ĵ� �̹����� ǥ���� UI ���

    void Start()
    {
        // PlayerPrefs���� �̹��� �ҷ�����
        string base64Image = PlayerPrefs.GetString("UmbrellaImage", null);

        if (!string.IsNullOrEmpty(base64Image))
        {
            byte[] imageBytes = System.Convert.FromBase64String(base64Image);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes); // base64���� �̹����� ��ȯ
            displayImage.texture = texture; // RawImage�� �ؽ�ó �Ҵ�
            displayImage.gameObject.SetActive(true); // �̹��� ǥ��
        }
    }
}
