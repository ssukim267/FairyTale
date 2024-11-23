using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class Introend : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Video Player 컴포넌트
    public string nextSceneName = "ch1"; // 이동할 씬 이름

    void Start()
    {
        // VideoPlayer의 loopPointReached 이벤트에 메서드 등록
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // 비디오가 끝나면 지정된 씬으로 이동
        SceneManager.LoadScene(nextSceneName);
    }
}
