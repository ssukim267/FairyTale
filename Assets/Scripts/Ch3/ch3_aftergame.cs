using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement; // 씬 관리 기능을 위한 네임스페이스 추가

public class ch3_aftergame : MonoBehaviour
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

        yield return StartCoroutine(PlayDialogue("우와! 지우야 고마워."));
        yield return StartCoroutine(PlayDialogue("덕분에 햇빛을 막을 수 있을 거 같아."));
        yield return StartCoroutine(PlayDialogue("넌 정말 최고의 친구야!"));
        yield return StartCoroutine(PlayDialogue("점점 따뜻해지는 걸 보니까 봄이 다가오나봐"));
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
        SceneManager.LoadScene("ch4"); // 이동할 씬 이름을 적어주세요
    }
}
