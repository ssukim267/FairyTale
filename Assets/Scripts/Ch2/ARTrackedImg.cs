using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SceneManagement; // �� ������ ���� ���ӽ����̽�

public class ARTrackedImg : MonoBehaviour
{
    // AR �̹��� ������ �����ϴ� ARTrackedImageManager
    public ARTrackedImageManager trackedImageManager;

    // �̹��� �̸��� Ű��, �ش� �̹����� ������ �� ǥ���� 3D ������Ʈ�� ������ ������ ��ųʸ�
    private Dictionary<string, GameObject> _prefabDic = new Dictionary<string, GameObject>();

    // ������Ʈ Ȯ�� �� ��� ���� ����
    private float previousTouchDistance = 0; // ���� �� �հ��� �� �Ÿ�
    private Vector3 initialScale; // ������Ʈ �ʱ� ũ��
    public GameObject ObjectPool; // ������Ʈ�� ��� �ִ� �θ� ������Ʈ

    // ������Ʈ ȸ�� ���� ����
    private float previousTouchPositionX = 0f; // ���� ��ġ ��ġ
    private float initialRotationY = 0f; // ������Ʈ ȸ�� �ʱⰪ

    // ������Ʈ�� �̹��� �� ������ ���� ������Ʈ ����� Ȱ��
    public GameObject[] _objectList;

    // puzzleSolved�� public static ������ ����
    public static bool puzzlesolved = false;

    // nextchapter ��ư�� ����
    public GameObject nextchapter;

    void Awake()
    {
        // 3D ������Ʈ�� �̸��� �Բ� ��ųʸ��� �߰�
        foreach (GameObject obj in _objectList)
        {
            string objectName = obj.name;  // ������Ʈ �̸��� ������
            _prefabDic.Add(objectName, obj);  // �̸��� Ű�� �Ͽ� ��ųʸ��� ����
        }

        // nextchapter ��ư ��Ȱ��ȭ ���·� �ʱ�ȭ
        nextchapter.SetActive(false);
    }

    private void OnEnable()
    {
        // �̹��� ������ ���۵ǰų� ������Ʈ �� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯 ���
        trackedImageManager.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
        // ��Ȱ��ȭ �� �̺�Ʈ �ڵ鷯 ����
        trackedImageManager.trackedImagesChanged -= ImageChanged;
    }

    void Update()
    {
        // �� �հ��� ��ġ �� ũ�� ����
        if (Input.touchCount == 2)
        {
            ChangeScale();
        }
        // �� �հ��� ��ġ �� ȸ��
        else if (Input.touchCount == 1)
        {
            OnTouchRotate();
        }
    }

    // ũ�� ���� ���
    void ChangeScale()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        // ��ġ ���� �� �Ÿ��� ������Ʈ �ʱ� ũ�� ����
        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            previousTouchDistance = Vector2.Distance(touch1.position, touch2.position);
            initialScale = ObjectPool.transform.localScale;
        }
        // ��ġ �̵� �� ũ�� ����
        else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float currentTouchDistance = Vector2.Distance(touch1.position, touch2.position);
            if (Mathf.Approximately(previousTouchDistance, 0)) return;

            // ������ �̿��� ũ�� ����
            float scaleFactor = currentTouchDistance / previousTouchDistance;
            ObjectPool.transform.localScale = initialScale * scaleFactor; // ������Ʈ ũ�� ����
        }
    }

    // ȸ�� ���
    void OnTouchRotate()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // ��ġ ���� �� �ʱ� ȸ���� ����
            if (touch.phase == TouchPhase.Began)
            {
                previousTouchPositionX = touch.position.x;
                initialRotationY = ObjectPool.transform.eulerAngles.y; // ȸ�� �ʱⰪ
            }

            // ��ġ �̵� �� ȸ�� ����
            else if (touch.phase == TouchPhase.Moved)
            {
                // �հ��� �̵��� ���
                float deltaPositionX = touch.position.x - previousTouchPositionX;

                // �̵� �Ÿ� ��ʷ� ȸ��
                float rotationFactor = deltaPositionX * 0.2f; // 0.2�� ȸ�� �ӵ�

                // ȸ���� ������Ʈ
                float newRotationY = initialRotationY + rotationFactor;
                ObjectPool.transform.rotation = Quaternion.Euler(0, newRotationY, 0); // Y�� ���� ȸ��
            }
        }
    }

    // �̹��� ���� ���°� ����Ǿ��� �� ȣ��
    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // ���� ������ �̹��� ó��
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateImage(trackedImage); // �ش� �̹����� �´� ������Ʈ Ȱ��ȭ

            // �̹��� ������ ���۵Ǿ����� puzzlesolved�� true�� ����
            puzzlesolved = true;

            // ��ư Ȱ��ȭ
            nextchapter.SetActive(true);
        }

        // ���� ���°� ����� �̹��� ó��
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage); // ������Ʈ ��ġ �� ȸ�� ������Ʈ
        }

        // ������ ������ �̹��� ó��
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            DisableImage(trackedImage); // �ش� ������Ʈ ��Ȱ��ȭ
        }
    }

    // ������ �̹����� �´� ������Ʈ ��ġ �� ȸ�� ������Ʈ
    private void UpdateImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;  // ������ �̹��� �̸�
        if (_prefabDic.ContainsKey(imageName))
        {
            GameObject obj = _prefabDic[imageName];  // �ش� �̸��� ������Ʈ
            obj.transform.position = trackedImage.transform.position; // �̹��� ��ġ�� ������Ʈ ��ġ ���߱�
            obj.SetActive(true); // ������Ʈ Ȱ��ȭ
        }
    }

    // ������ ������ �̹����� ���� ������Ʈ ��Ȱ��ȭ
    private void DisableImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;  // ������ �̹��� �̸�
        if (_prefabDic.ContainsKey(imageName))
        {
            GameObject obj = _prefabDic[imageName];  // �ش� �̸��� ������Ʈ
            obj.SetActive(false); // ������Ʈ ��Ȱ��ȭ
        }
    }

    public void OnNextChapterButtonClicked()
    {
        // ch3 ������ �̵�
        SceneManager.LoadScene("ch3_beforegame");
    }
}
