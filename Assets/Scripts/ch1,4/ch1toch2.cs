using UnityEngine;
using UnityEngine.SceneManagement;

public class ch1toch2 : MonoBehaviour
{
    public string nextSceneName = "ch2_beforegame"; // 이동할 씬 이름

    // 버튼 클릭 시 호출되는 메서드
    public void OnButtonClick()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
