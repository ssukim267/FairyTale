//NaverTTS1

using System;
using System.Collections;
using System.Net;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class NaverTTS1 : MonoBehaviour
{
    private AudioSource audioSource;


    // 각 대사 그룹을 배열로 정의
    private string[] preGameDialogue = new string[]
    {
        "짠! 앞에 있는 새와 똑같이 만들면 돼.",
        "다 만들었으면 나에게 완성된 걸 보여줘!",
    };

    private string[] inGameDialogue = new string[]
    {
        "잘 하고 있어, 지우! 조금만 더!"
    };

    private string[] postGameDialogue = new string[]
    {
        "우와! 새가 나타났어. 정말 잘 했어, 지우!",
        "화면에 손가락을 대서 새를 돌려보거나 크기를 키워봐!"
    };

    private string previousText = null;
    public bool isPlaying = false;



    void Start()
    {
        // AudioSource를 컴포넌트에 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 게임 시작 후 5초 뒤에 대사 시작
        StartCoroutine(StartDialogueWithDelay());
    }

    void Update()
    {

        // puzzleSolved가 true일 때, inGameDialogue 실행
        if (ARTrackedImg.puzzlesolved)
        {
            StartCoroutine(PlayPostGameDialogue());
            ARTrackedImg.puzzlesolved = false; // 대사 실행 후 puzzleSolved를 다시 false로 설정
        }

        // 오디오 재생 확인
        if (audioSource != null)
        {
            isPlaying = audioSource.isPlaying;
        }
    }

    IEnumerator StartDialogueWithDelay()
    {
        // 5초 대기 후 preGameDialogue 실행
        yield return new WaitForSeconds(3f);

        foreach (string text in preGameDialogue)
        {
            yield return StartCoroutine(PlayText(text)); // 게임 전 대사
        }

        // 30초 대기 후 inGameDialogue 실행
        yield return new WaitForSeconds(30f);

        foreach (string text in inGameDialogue)
        {
            yield return StartCoroutine(PlayText(text)); // 게임 중 대사
        }
    }

    // puzzleSolved가 true일 때, 게임 중 대사 실행
    IEnumerator PlayPostGameDialogue()
    {
        previousText = null;
        foreach (string text in postGameDialogue)
        {
            yield return StartCoroutine(PlayText(text)); // 게임 중 대사
        }
        ARTrackedImg.puzzlesolved = false;
    }


    // 텍스트 음성 합성 및 재생
    IEnumerator PlayText(string text)
    {
        // 텍스트 값이 새로 들어왔는지 확인
        if (!string.IsNullOrEmpty(text) && text != previousText)
        {
            // 음성 합성 호출
            yield return StartCoroutine(TTS(text));
            previousText = text;  // 이전 값 업데이트
        }
    }

    // TTS 요청
    IEnumerator TTS(string text)
    {
        string url = "https://naveropenapi.apigw.ntruss.com/tts-premium/v1/tts";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Headers.Add("X-NCP-APIGW-API-KEY-ID", "ftnx77b5l3");
        request.Headers.Add("X-NCP-APIGW-API-KEY", "Zl4XoGWezU3GAWUB9D9dE8mtbjS0q00mBEkIUqto");
        request.Method = "POST";

        byte[] byteDataParams = Encoding.UTF8.GetBytes("speaker=nmeow&volume=0&speed=0&pitch=0&format=mp3&text=" + text);
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = byteDataParams.Length;
        Stream st = request.GetRequestStream();
        st.Write(byteDataParams, 0, byteDataParams.Length);
        st.Close();

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        string status = response.StatusCode.ToString();
        Debug.Log("status=" + status);

        // 파일 경로 설정
        string filePath = Path.Combine(Application.persistentDataPath, "tts.mp3");

        // 이전 파일 삭제
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Previous audio file deleted.");
        }

        // 파일 저장
        using (Stream output = File.OpenWrite(filePath))
        using (Stream input = response.GetResponseStream())
        {
            input.CopyTo(output);
        }

        Debug.Log(filePath);

        // 오디오 재생
        yield return StartCoroutine(PlayAudio(filePath));
    }

    // 오디오 파일을 재생
    IEnumerator PlayAudio(string filePath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = audioClip;
                audioSource.Play();
                Debug.Log("Audio playing...");
                // 음성이 끝날 때까지 대기
                yield return new WaitForSeconds(audioClip.length);
            }
        }
    }
}
