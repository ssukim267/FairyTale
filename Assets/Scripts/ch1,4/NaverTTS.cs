using System;
using System.Collections;
using System.Net;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
//using OpenAI;

public class NaverTTS : MonoBehaviour
{
    private AudioSource audioSource;
    public string text;
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

    }

    void Update()
    {
        //text값이 새로 들어왔는지? 
        if (!string.IsNullOrEmpty(text) && text != previousText)
        {
            TTS();
            previousText = text;  // 이전 값 업데이트
            //Debug.Log(text);
        }

        // 오디오 재생 확인
        if (audioSource != null)
        {
            isPlaying = audioSource.isPlaying;
        }
    }

    public void TTS()
    {
        //string text = "좋은 하루 되세요."; // 음성합성할 문자값
        string url = "https://naveropenapi.apigw.ntruss.com/tts-premium/v1/tts";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Headers.Add("X-NCP-APIGW-API-KEY-ID", "ftnx77b5l3");
        request.Headers.Add("X-NCP-APIGW-API-KEY", "Zl4XoGWezU3GAWUB9D9dE8mtbjS0q00mBEkIUqto");
        request.Method = "POST";
        //Debug.Log(text);
        byte[] byteDataParams = Encoding.UTF8.GetBytes("speaker=nmeow&volume=0&speed=0&pitch=0&format=mp3&text=" + text);
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = byteDataParams.Length;
        Stream st = request.GetRequestStream();
        st.Write(byteDataParams, 0, byteDataParams.Length);
        st.Close();
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        string status = response.StatusCode.ToString();
        Console.WriteLine("status=" + status);
        //using (Stream output = File.OpenWrite("tts.mp3"))
        //using (Stream input = response.GetResponseStream())
        //{
        //    input.CopyTo(output);
        //}


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
        StartCoroutine(PlayAudio(filePath));
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
            }
        }
    }

}
