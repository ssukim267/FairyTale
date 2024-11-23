using UnityEngine;
using UnityEngine.SceneManagement;

public class ch3toch3_1 : MonoBehaviour
{
    // OnClick 이벤트에 연결할 함수
    public void ChangeToScene()
    {
        SceneManager.LoadScene("ch3_1"); // 씬 이름을 문자열로 전달
    }
}
