using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SceneManagement; // 씬 관리를 위한 네임스페이스

public class ARTrackedImg : MonoBehaviour
{
    // AR 이미지 추적을 관리하는 ARTrackedImageManager
    public ARTrackedImageManager trackedImageManager;

    // 이미지 이름을 키로, 해당 이미지를 추적할 때 표시할 3D 오브젝트를 값으로 가지는 딕셔너리
    private Dictionary<string, GameObject> _prefabDic = new Dictionary<string, GameObject>();

    // 오브젝트 확대 및 축소 관련 변수
    private float previousTouchDistance = 0; // 이전 두 손가락 간 거리
    private Vector3 initialScale; // 오브젝트 초기 크기
    public GameObject ObjectPool; // 오브젝트를 담고 있는 부모 오브젝트

    // 오브젝트 회전 관련 변수
    private float previousTouchPositionX = 0f; // 이전 터치 위치
    private float initialRotationY = 0f; // 오브젝트 회전 초기값

    // 오브젝트와 이미지 간 연결을 위해 오브젝트 목록을 활용
    public GameObject[] _objectList;

    // puzzleSolved를 public static 변수로 선언
    public static bool puzzlesolved = false;

    // nextchapter 버튼을 연결
    public GameObject nextchapter;

    void Awake()
    {
        // 3D 오브젝트를 이름과 함께 딕셔너리에 추가
        foreach (GameObject obj in _objectList)
        {
            string objectName = obj.name;  // 오브젝트 이름을 가져옴
            _prefabDic.Add(objectName, obj);  // 이름을 키로 하여 딕셔너리에 저장
        }

        // nextchapter 버튼 비활성화 상태로 초기화
        nextchapter.SetActive(false);
    }

    private void OnEnable()
    {
        // 이미지 추적이 시작되거나 업데이트 될 때 호출되는 이벤트 핸들러 등록
        trackedImageManager.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
        // 비활성화 시 이벤트 핸들러 해제
        trackedImageManager.trackedImagesChanged -= ImageChanged;
    }

    void Update()
    {
        // 두 손가락 터치 시 크기 조절
        if (Input.touchCount == 2)
        {
            ChangeScale();
        }
        // 한 손가락 터치 시 회전
        else if (Input.touchCount == 1)
        {
            OnTouchRotate();
        }
    }

    // 크기 조절 기능
    void ChangeScale()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        // 터치 시작 시 거리와 오브젝트 초기 크기 저장
        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            previousTouchDistance = Vector2.Distance(touch1.position, touch2.position);
            initialScale = ObjectPool.transform.localScale;
        }
        // 터치 이동 시 크기 조절
        else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float currentTouchDistance = Vector2.Distance(touch1.position, touch2.position);
            if (Mathf.Approximately(previousTouchDistance, 0)) return;

            // 비율을 이용해 크기 조정
            float scaleFactor = currentTouchDistance / previousTouchDistance;
            ObjectPool.transform.localScale = initialScale * scaleFactor; // 오브젝트 크기 변경
        }
    }

    // 회전 기능
    void OnTouchRotate()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // 터치 시작 시 초기 회전값 저장
            if (touch.phase == TouchPhase.Began)
            {
                previousTouchPositionX = touch.position.x;
                initialRotationY = ObjectPool.transform.eulerAngles.y; // 회전 초기값
            }

            // 터치 이동 시 회전 적용
            else if (touch.phase == TouchPhase.Moved)
            {
                // 손가락 이동량 계산
                float deltaPositionX = touch.position.x - previousTouchPositionX;

                // 이동 거리 비례로 회전
                float rotationFactor = deltaPositionX * 0.2f; // 0.2는 회전 속도

                // 회전값 업데이트
                float newRotationY = initialRotationY + rotationFactor;
                ObjectPool.transform.rotation = Quaternion.Euler(0, newRotationY, 0); // Y축 기준 회전
            }
        }
    }

    // 이미지 추적 상태가 변경되었을 때 호출
    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // 새로 추적된 이미지 처리
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateImage(trackedImage); // 해당 이미지에 맞는 오브젝트 활성화

            // 이미지 추적이 시작되었으면 puzzlesolved를 true로 설정
            puzzlesolved = true;

            // 버튼 활성화
            nextchapter.SetActive(true);
        }

        // 추적 상태가 변경된 이미지 처리
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage); // 오브젝트 위치 및 회전 업데이트
        }

        // 추적이 중지된 이미지 처리
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            DisableImage(trackedImage); // 해당 오브젝트 비활성화
        }
    }

    // 추적된 이미지에 맞는 오브젝트 위치 및 회전 업데이트
    private void UpdateImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;  // 추적된 이미지 이름
        if (_prefabDic.ContainsKey(imageName))
        {
            GameObject obj = _prefabDic[imageName];  // 해당 이름의 오브젝트
            obj.transform.position = trackedImage.transform.position; // 이미지 위치에 오브젝트 위치 맞추기
            obj.SetActive(true); // 오브젝트 활성화
        }
    }

    // 추적이 중지된 이미지에 대한 오브젝트 비활성화
    private void DisableImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;  // 추적된 이미지 이름
        if (_prefabDic.ContainsKey(imageName))
        {
            GameObject obj = _prefabDic[imageName];  // 해당 이름의 오브젝트
            obj.SetActive(false); // 오브젝트 비활성화
        }
    }

    public void OnNextChapterButtonClicked()
    {
        // ch3 씬으로 이동
        SceneManager.LoadScene("ch3_beforegame");
    }
}
