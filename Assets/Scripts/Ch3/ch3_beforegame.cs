using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement; // 씬 관리 기능을 위한 네임스페이스 추가

public class ch3_beforegame : MonoBehaviour
{
    private AudioSource audioSource;
    public GameObject targetObject; // 활성화할 오브젝트
    public GameObject gameStartButton; // 게임 시작 버튼

    void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        StartCoroutine(RunDialogueSequence());

            gameStartButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnGameStartButtonClicked);
        
    }

    IEnumerator RunDialogueSequence()
    {
        yield return new WaitForSeconds(5f);

        yield return StartCoroutine(PlayDialogue("지우야, 좋은 아침이야. 오늘따라 햇빛이 세서 내가 녹을 것 같아."));
        yield return StartCoroutine(PlayDialogue("나를 도와줄 수 있는 도구를 그려줄 수 있어? 게임 시작 버튼을 눌러줘!"));
        targetObject.SetActive(true);
    }

    IEnumerator PlayDialogue(string text)
    {
        yield return StartCoroutine(TTS(text));
    }

    IEnumerator TTS(string text)
    {
        string url = "https://naveropenapi.apigw.ntruss.com/tts-premium/v1/tts";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Headers.Add("X-NCP-APIGW-API-KEY-ID", "ftnx77b5l3");
        request.Headers.Add("X-NCP-APIGW-API-KEY", "Zl4XoGWezU3GAWUB9D9dE8mtbjS0q00mBEkIUqto");
        request.Method = "POST";

        byte[] byteDataParams = Encoding.UTF8.GetBytes($"speaker=nmeow&volume=0&speed=0&pitch=0&format=mp3&text={text}");
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = byteDataParams.Length;

        using (Stream st = request.GetRequestStream())
        {
            st.Write(byteDataParams, 0, byteDataParams.Length);
        }

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
            string filePath = Path.Combine(Application.persistentDataPath, "tts.mp3");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (Stream output = File.OpenWrite(filePath))
            using (Stream input = response.GetResponseStream())
            {
                input.CopyTo(output);
            }

            yield return StartCoroutine(PlayAudio(filePath));
        }
    }

    IEnumerator PlayAudio(string filePath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Audio error: {www.error}");
                yield break;
            }

            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = audioClip;
            audioSource.Play();
            yield return new WaitForSeconds(audioClip.length);
        }
    }

    void OnGameStartButtonClicked()
    {
        SceneManager.LoadScene("ch2"); // 이동할 씬 이름을 적어주세요
    }
}
