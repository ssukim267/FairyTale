using UnityEngine;
using UnityEngine.SceneManagement;

public class ch1toch2 : MonoBehaviour
{
    public string nextSceneName = "ch2_beforegame"; // �̵��� �� �̸�

    // ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    public void OnButtonClick()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
