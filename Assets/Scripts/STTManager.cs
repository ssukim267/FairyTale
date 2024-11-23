using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using OpenAI;

public class STTManager : MonoBehaviour
{
    private string apiKey = "AIzaSyDBgjyaZMEztMaU6qedbWJRaYNLMougoK0";  
    private string apiUrl = "https://speech.googleapis.com/v1/speech:recognize?key=";
    private AudioSource audioSource;
    //public Button startRecordingButton;
    //public Button stopRecordingButton;

    public ChatGPT chatGPT; 

    private AudioClip recordedClip;
    public bool isRecording = false;

    private float silenceDuration = 0.0f;
    private float maxSilenceTime = 2.0f;     // 공백 기준 5초

    public GameObject letter;

    //public InputField resultInputField;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        //startRecordingButton.onClick.AddListener(StartRecording);

        //stopRecordingButton.onClick.AddListener(StopRecording);

    }

    // 녹음 시작
    public void StartRecording()
    {
        if (!isRecording)
        {
            silenceDuration = 0;
            isRecording = true;
            recordedClip = Microphone.Start(null, true, 59, 44100);
            StartCoroutine(CheckSilence());
    }
}

    private IEnumerator CheckSilence()
    {
        float[] samples = new float[300];
        float silenceDuration = 0;

        while (isRecording)
        {
            // 오디오 소스
            int micPosition = Microphone.GetPosition(null) - (samples.Length + 1);
            if (micPosition < 0)
            {
                yield return null;  // 오디오가 없는 경우
                continue;
            }

            recordedClip.GetData(samples, micPosition);

            bool hasSound = false;
            foreach (float sample in samples)
            {
                // 음량 체크
                if (Mathf.Abs(sample) > 0.1f)
                {
                    hasSound = true;
                    break;
                }
            }

            // 공백일때 시간 증가
            if (!hasSound)
            {
                silenceDuration += Time.deltaTime;

                //시간이 5초 이상인 경우 녹음 중지
                if (silenceDuration >= maxSilenceTime)
                {
                    StopRecording();
                    yield break;
                }
            }
            else
            {
                silenceDuration = 0; // 음량이 인식되는 경우 공백 변수 리셋
            }

            yield return null;  
        }
    }


    // 녹음 종료
    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            Microphone.End(null);

            // 오디오 데이터를 WAV로 변환
            byte[] audioData = WavUtility.FromAudioClip(recordedClip);
            StartCoroutine(SendSTTRequest(audioData));
        }
    }

    // API 요청
    IEnumerator SendSTTRequest(byte[] audioData)
    {

        string jsonRequest = "{\"config\":{\"encoding\":\"LINEAR16\",\"sampleRateHertz\":44100,\"languageCode\":\"ko-KR\"},\"audio\":{\"content\":\"" + System.Convert.ToBase64String(audioData) + "\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);

        using (UnityWebRequest www = new UnityWebRequest(apiUrl + apiKey, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            // 응답 대기
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
  
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);

                // transcript 추출
                var response = JsonUtility.FromJson<STTResponse>(jsonResponse);
                if (response.results.Length > 0)
                {
                    string transcript = response.results[0].alternatives[0].transcript;
                    Debug.Log("Transcript: " + transcript);

                    letter.SetActive(true);

                    // 텍스트 바로 전달
                    chatGPT.content = transcript;

                    // ChatGPT로 텍스트 전송
                    chatGPT.SendReply();
                  

                }
                else
                {
     
                    Debug.Log("질문을 듣지 못 했습니다.");
                }
            }
        }
    }

    // STT API 응답 클래스 정의
    [System.Serializable]
    public class STTResponse
    {
        public Result[] results;

        [System.Serializable]
        public class Result
        {
            public Alternative[] alternatives;

            [System.Serializable]
            public class Alternative
            {
                public string transcript;
            }
        }
    }
}