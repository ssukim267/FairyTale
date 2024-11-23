using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AudioSource audioSource; 
    public AudioClip[] dialogues; 
    public GameObject sttButton; 
    public AudioCheckManager silenceChecker; 

    //눈덩이 게임
    public GameObject snowballGameUI; 
    public GameObject snowballObject; 
    public Camera mainCamera; 
    private bool isSnowballGrowing = false; 
    public float snowballRollingDuration = 10f; 

    //눈사람 선택
    public GameObject[] snowmanOptions;
    public bool isSelectionActive = false; 
    public bool SnowActive = false; 
    private bool isSnowActive = false;

    //대사
    public GameObject next;
    public Text dialogueText; 
    public GameObject dialogueUI; 
    private string[] dialogue = new string[]
    {
        "안녕 지우야! 내 목소리 잘들리니? ",
        "잘 들린다니, 다행이다! 나는 눈 속에 갇힌 눈사람이야. 눈으로 나를 만들어 줘!",
        "와 다됐다! 그런데 나 너무 추워 지우야..\n 내가 춥지 않도록 목도리와 모자를 골라줄래?",
        "안녕 지우야, 나는 너를 부른 눈이라고 해. \n나를 만들어줘서 고마워~",
    };


    private void Start()
    {
        StartCoroutine(StartDialogueAfterDelay(5f));
    }

    private void Update()
    {
        //눈덩이 게임
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObject = hit.collider.gameObject;

                
                if (clickedObject == snowballObject && !isSnowballGrowing)
                {
                    StartCoroutine(GrowSnowball()); 
                    Debug.Log("눈덩이 클릭됨!");
                }
            }
        }

        // 눈사람 선택
        if (isSelectionActive && Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject selectedSnowman = hit.collider.gameObject;

                foreach (GameObject snowman in snowmanOptions)
                {
                    if (selectedSnowman == snowman)
                    {
                        SelectSnowman(selectedSnowman); 
                        break;
                    }
                }
            }
        }
    }


    //첫대사 시작 + stt 버튼
    private IEnumerator StartDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("대사 1 시작");
        PlayDialogue(0, () =>
        {
            sttButton.SetActive(true); 
            Debug.Log("STT 버튼 활성화됨");
        });
    }

    //대사 관리
    private void PlayDialogue(int index, System.Action onComplete = null)
    {
        if (index >= 0 && index < dialogues.Length)
        {
            audioSource.clip = dialogues[index];
            audioSource.Play();
            dialogueText.text = dialogue[index]; 
            dialogueUI.SetActive(true);

            StartCoroutine(WaitForDialogueToFinish(onComplete));
        }
        else
        {
            Debug.LogWarning($"{index} 가 없습니다.");
        }
    }

    //대기
    private IEnumerator WaitForDialogueToFinish(System.Action onComplete)
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        onComplete?.Invoke();
        dialogueUI.SetActive(false);
    }

    //버튼 관리
    public void OnSTTButtonClicked()
    {
        silenceChecker.StartChecking(OnSilenceDetected);
    }

    
    private void OnSilenceDetected()
    {
        silenceChecker.StopChecking();
        sttButton.SetActive(false);

        PlayDialogue(1, () =>
        {
            StartCoroutine(HandleSnowballGame());
        });
    }


    //눈덩이 게임 관리
    private IEnumerator HandleSnowballGame()
    {

        snowballGameUI.SetActive(true);
        yield return new WaitForSeconds(3f);

        snowballGameUI.SetActive(false); 
        snowballObject.SetActive(true);

        yield return new WaitForSeconds(snowballRollingDuration);
        snowballObject.SetActive(false);

        ActivateSnowmanSelection();

        yield return new WaitForSeconds(1f); 

    }

    //눈덩이 클릭 시
    private IEnumerator GrowSnowball()
    {
        isSnowballGrowing = true; 

        float growthAmount = 0.1f; 
        Vector3 targetScale = snowballObject.transform.localScale + new Vector3(growthAmount, growthAmount, growthAmount); 
        float duration = 0.5f; 
        float elapsedTime = 0f;

  
        Vector3 initialScale = snowballObject.transform.localScale;
        Vector3 initialPosition = snowballObject.transform.position;
        Quaternion initialRotation = snowballObject.transform.rotation;

        Vector3 targetPosition = initialPosition + new Vector3(0.5f, 0f, 0f); 
        Quaternion targetRotation = initialRotation * Quaternion.Euler(50, 0, 0);

        AudioSource audioSource = snowballObject.GetComponent<AudioSource>();
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play(); 
        }

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            snowballObject.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            snowballObject.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            snowballObject.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최종 업데이트
        snowballObject.transform.localScale = targetScale;
        snowballObject.transform.position = targetPosition;
        snowballObject.transform.rotation = targetRotation;

        isSnowballGrowing = false; 
    }

    //눈사람 선택
    private void ActivateSnowmanSelection()
    {
        foreach (GameObject snowman in snowmanOptions)
        {
            snowman.SetActive(true); 
        }

        isSelectionActive = true; 
        isSnowActive = true;

        PlayDialogue(2, null); 
    }

    private void HandleSnowmanSelection(GameObject clickedObject)
    {
        foreach (GameObject snowman in snowmanOptions)
        {
            if (clickedObject == snowman)
            {
                SelectSnowman(snowman);
                break;
            }
        }
    }

    private void SelectSnowman(GameObject selectedSnowman)
    {
        foreach (GameObject snowman in snowmanOptions)
        {
            snowman.SetActive(false); 
        }

        selectedSnowman.SetActive(true);
        isSelectionActive = false;

        StartCoroutine(MoveSnowmanToCamera(selectedSnowman));
    }

    //눈사람 선택 효과
    private IEnumerator MoveSnowmanToCamera(GameObject selectedSnowman)
    {
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward * 2f;
        float moveSpeed = 3f;

        while (Vector3.Distance(selectedSnowman.transform.position, targetPosition) > 3f)
        {
            selectedSnowman.transform.position = Vector3.MoveTowards(selectedSnowman.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        PlayDialogue(3, null); 
        next.SetActive(true);

    }
}
