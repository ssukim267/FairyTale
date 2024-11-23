using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;

public class VideoDialogueManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject videoPlayerObject;
    public AudioSource audioSource;
    public Text dialogueText;
    public GameObject dialogueUI;

    public VideoClip[] videoClips;
    public AudioClip[] dialogues;

    public GameObject goodbyeUI;

    public string[] dialogueTexts = new string[]
    {
        "안녕! 봄이 오고 있나 봐.",
        "나랑 함께한 시간 정말 즐거웠어!",
        "이제는 시간이 된 것 같아. 잘 있어!",
        "테스트",
    };

    public GameObject snowmanObject; 
    public float moveDuration = 5f; 
    private Vector3 initialPosition;
    private Vector3 targetPosition;

    private void Start()
    {
        initialPosition = snowmanObject.transform.position;
        targetPosition = initialPosition + new Vector3(0, -1f, 0); 

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // 동영상 1 + 대사 1
        yield return StartCoroutine(PlayVideoWithDialogue(0));

        // 동영상 2 + 대사 2
        yield return StartCoroutine(PlayVideoWithDialogue(1));

      
        videoPlayerObject.SetActive(false);

        //대사3
        yield return StartCoroutine(PlayDialogueWithSnowmanMovement(2));

        //대사4
        yield return StartCoroutine(PlayDialogueWithSnowmanMovement(3));
    }

    //동영상 + 대사 재생
    private IEnumerator PlayVideoWithDialogue(int index)
    {
        if (index < videoClips.Length && index < dialogues.Length && index < dialogueTexts.Length)
        {

            if (index == 1)
            {
                videoPlayer.playbackSpeed = 1.7f; 
            }
            else
            {
                videoPlayer.playbackSpeed = 1.2f; 
            }

            videoPlayer.clip = videoClips[index];
            videoPlayer.SetDirectAudioMute(0, true); 
            videoPlayer.Play();

            yield return new WaitForSeconds(7f);

            dialogueText.text = dialogueTexts[index];
            dialogueUI.SetActive(true);

            audioSource.clip = dialogues[index];
            audioSource.Play();


            yield return new WaitWhile(() => audioSource.isPlaying);
            yield return new WaitWhile(() => videoPlayer.isPlaying);

            dialogueUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning($" {index} 가 없습니다.");
        }
    }

    //동영상 없이 대사 재생
    private IEnumerator PlayDialogueWithSnowmanMovement(int index)
    {
        if (index < dialogues.Length && index < dialogueTexts.Length)
        {
            dialogueText.text = dialogueTexts[index];
            dialogueUI.SetActive(true);

            audioSource.clip = dialogues[index];
            audioSource.Play();

            // 눈사람 이동
            //yield return StartCoroutine(MoveSnowman());

            yield return new WaitWhile(() => audioSource.isPlaying);

            if (index == 3)
            {
                goodbyeUI.SetActive(true);
            }

            dialogueUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"{index} 가 없습니다.");
        }
    }

    //눈사람 이동
    //private IEnumerator MoveSnowman()
    //{
    //    float elapsedTime = 0f;
    //    Vector3 startPosition = initialPosition;

    //    while (elapsedTime < moveDuration)
    //    {
    //        snowmanObject.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
    //        elapsedTime += Time.deltaTime;
    //        yield return null;
    //    }

    //    snowmanObject.transform.position = targetPosition; 
    //}
}
