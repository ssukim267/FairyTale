using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class Introend : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Video Player ������Ʈ
    public string nextSceneName = "ch1"; // �̵��� �� �̸�

    void Start()
    {
        // VideoPlayer�� loopPointReached �̺�Ʈ�� �޼��� ���
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // ������ ������ ������ ������ �̵�
        SceneManager.LoadScene(nextSceneName);
    }
}
