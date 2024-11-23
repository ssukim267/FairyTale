namespace NOT_Lonely
{
    using NOT_Lonely.Weatherade;
    using System;
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
#if UNITY_POST_PROCESSING_STACK_V2
    using UnityEngine.Rendering.PostProcessing;
#endif
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using static NOT_Lonely.GameManager;

    public class GameManager : MonoBehaviour
    {
        public Image fade;
        public TextMeshProUGUI startTitle;
        public float fadeSpeed = 1;
        public AnimationCurve screenFadeCurve;
        public AnimationCurve startTitleFadeCurve;
        public AnimationCurve audioFadeCurve;

        public GameObject exitMenu;

        [Serializable]
        public class FirsPersonCharacter
        {
            public SimpleFPController controller;
            public SimpleGun gun;
        }


        [Serializable]
        public class Car
        {
#if BCG_RCC
            public RCC_CarControllerV3 rccController;
#endif
            public NL_SimpleCarController simpleController;
            public GameObject cameraObj;
            public Transform characterControllerExitPoint;
        }

        public Car car;
        private bool inCar = false;


        public FirsPersonCharacter firsPersonCharacter;

        public SRS_ParticleSystem srsParticleSystem;
        public CoverageBase coverageInstance;

        public Rigidbody[] carCargo;
        public GameObject trees;

        [Serializable]
        public class Hints
        {
            public GameObject hints;
            public GameObject showHints;
        }

        public Hints hints;

        public NL_TimeOfDay timeOfDayController;

#if UNITY_POST_PROCESSING_STACK_V2
        private PostProcessLayer[] ppLayers;
#endif

        public UnityEvent OnEnterCar;
        public UnityEvent OnExitCar;

        public UnityEvent OnPostProcessDisabled;
        public UnityEvent OnPostPorcessEnabled;

        public UnityEvent OnTreesDisabled;
        public UnityEvent OnTreesEnabled;

        private void Awake()
        {
            fade.color = new Color(0, 0, 0, 1);
            AudioListener.volume = 0;

            for (int i = 0; i < carCargo.Length; i++)
            {
                if (carCargo[i] != null) carCargo[i].isKinematic = true;
            }
#if UNITY_POST_PROCESSING_STACK_V2
            ppLayers = NL_Utilities.FindObjectsOfType<PostProcessLayer>(true);
#endif

            exitMenu.SetActive(false);
        }

        IEnumerator Start()
        {
#if BCG_RCC
            if (car.rccController != null)
            {
                car.rccController.SetEngine(false);
                car.rccController.SetCanControl(false);
            }
#endif
            if (car.simpleController != null) car.simpleController.SwitchEngine(false);

            yield return new WaitForFixedUpdate();

            for (int i = 0; i < carCargo.Length; i++)
            {
                if (carCargo[i] == null) continue;

                carCargo[i].isKinematic = false;
                carCargo[i].velocity = Vector3.zero;
                carCargo[i].transform.parent = null;
            }

            yield return new WaitForSeconds(1f);

            StartCoroutine(FadeOut());
        }

        IEnumerator FadeOut()
        {
            float v = 0;
            AudioListener.volume = 0;
            float screenFadeVal = 1;
            float startTitleFadeVal = 1;
            float audioFadeVal = 0;

            while (true)
            {
                if (v < 1)
                {
                    v += Time.deltaTime * fadeSpeed;
                    screenFadeVal = screenFadeCurve.Evaluate(v);
                    startTitleFadeVal = startTitleFadeCurve.Evaluate(v);
                    audioFadeVal = audioFadeCurve.Evaluate(v);

                    AudioListener.volume = audioFadeVal;
                    fade.color = new Color(0, 0, 0, screenFadeVal);
                    if (startTitle != null) startTitle.color = new Color(1, 1, 1, startTitleFadeVal);
                }
                else
                {
                    v = 1;
                    AudioListener.volume = 1;
                    fade.color = new Color(0, 0, 0, 0);
                    if (startTitle != null) startTitle.color = new Color(0, 0, 0, 0);
                    yield break;
                }
                yield return null;
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!exitMenu.activeSelf)
                {
                    exitMenu.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    firsPersonCharacter.controller.enabled = false;
#if BCG_RCC
                    if (car.rccController != null) car.rccController.enabled = false;
#endif
                    if (car.simpleController != null) car.simpleController.SwitchEngine(false);

                    if (firsPersonCharacter.gun != null) firsPersonCharacter.gun.enabled = false;
                }
                else
                {
                    Resume();
                }
            }

            if (exitMenu.activeSelf) return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
#if BCG_RCC
                if (car.rccController != null)
                {
                    if (!inCar)
                    {
                        if (Vector3.Distance(car.rccController.transform.position, firsPersonCharacter.controller.transform.position) < 3.5f)
                        {
                            car.rccController.SetEngine(true);
                            car.rccController.SetCanControl(true);
                            car.cameraObj.SetActive(true);
                            firsPersonCharacter.controller.gameObject.SetActive(false);
                            srsParticleSystem.SetFollowTarget(car.rccController.transform);
                            coverageInstance.followTarget = car.rccController.transform;
                            inCar = true;
                            OnEnterCar.Invoke();
                        }
                    }
                    else
                    {
                        car.rccController.SetEngine(false);
                        car.rccController.SetCanControl(false);
                        car.cameraObj.SetActive(false);
                        firsPersonCharacter.controller.transform.SetPositionAndRotation(car.characterControllerExitPoint.position, car.characterControllerExitPoint.rotation);
                        firsPersonCharacter.controller.transform.eulerAngles = new Vector3(0, firsPersonCharacter.controller.transform.eulerAngles.y, 0);
                        firsPersonCharacter.controller.gameObject.SetActive(true);

                        srsParticleSystem.SetFollowTarget(firsPersonCharacter.controller.transform);
                        coverageInstance.followTarget = firsPersonCharacter.controller.transform;

                        inCar = false;
                        OnExitCar.Invoke();
                    }

                }else if(car.simpleController != null)
                {
                    SimpleCarControllerFallback(); // No RCC car controller. Fallback to simple car controller
                }
#else
                // No RCC car controller. Fallback to simple car controller
                SimpleCarControllerFallback();
#endif
            }


            if (Input.GetKeyDown(KeyCode.Alpha1))
                timeOfDayController.ChangeStateGradually(0);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                timeOfDayController.ChangeStateGradually(1);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                timeOfDayController.ChangeStateGradually(2);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                timeOfDayController.ChangeStateGradually(3);

#if UNITY_POST_PROCESSING_STACK_V2
            if (Input.GetKeyDown(KeyCode.P))
            {
                for (int i = 0; i < ppLayers.Length; i++)
                {
                    ppLayers[i].enabled = !ppLayers[i].enabled;
                }

                if (ppLayers[0].enabled) OnPostPorcessEnabled.Invoke();
                else OnPostProcessDisabled.Invoke();
            }
#endif
            if (Input.GetKeyDown(KeyCode.T))
            {
                trees.SetActive(!trees.activeSelf);

                if (trees.activeSelf) OnTreesEnabled.Invoke();
                else OnTreesDisabled.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                if (SceneManager.GetActiveScene().buildIndex == 0) SceneManager.LoadScene(1);
                else SceneManager.LoadScene(0);
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                hints.hints.SetActive(!hints.hints.activeSelf);
                hints.showHints.SetActive(!hints.hints.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                StartCoroutine(TakeScreenshot());
            }

            /*
            if (Input.GetKeyDown(KeyCode.I)) Time.timeScale = Time.timeScale == 1 ? 0f : 1;
            if (Input.GetKeyDown(KeyCode.O)) Time.timeScale = Time.timeScale == 1 ? 0.3f : 1;
            */
        }

        private void SimpleCarControllerFallback()
        {
            if (!inCar)
            {
                if (Vector3.Distance(car.simpleController.transform.position, firsPersonCharacter.controller.transform.position) < 3.5f)
                {
                    car.simpleController.SwitchEngine(true);
                    if(car.cameraObj != null) car.cameraObj.SetActive(true);
                    firsPersonCharacter.controller.gameObject.SetActive(false);
                    srsParticleSystem.SetFollowTarget(car.simpleController.transform);
                    coverageInstance.followTarget = car.simpleController.transform;
                    inCar = true;
                    OnEnterCar.Invoke();
                }
            }
            else
            {
                car.simpleController.SwitchEngine(false);
                if (car.cameraObj != null) car.cameraObj.SetActive(false);
                firsPersonCharacter.controller.transform.SetPositionAndRotation(car.characterControllerExitPoint.position, car.characterControllerExitPoint.rotation);
                firsPersonCharacter.controller.transform.eulerAngles = new Vector3(0, firsPersonCharacter.controller.transform.eulerAngles.y, 0);
                firsPersonCharacter.controller.gameObject.SetActive(true);

                srsParticleSystem.SetFollowTarget(firsPersonCharacter.controller.transform);
                coverageInstance.followTarget = firsPersonCharacter.controller.transform;

                inCar = false;
                OnExitCar.Invoke();
            }
        }

        IEnumerator TakeScreenshot()
        {
            bool hintsTextState = hints.hints.activeSelf;
            bool showHintsState = hints.showHints.activeSelf;
            hints.hints.SetActive(false);
            hints.showHints.SetActive(false);

            string dateTime = DateTime.Now.ToString().Replace('.', '-').Replace(':', '-');
            ScreenCapture.CaptureScreenshot($"Screenshot_frame_{Time.renderedFrameCount}_{dateTime}.png");

            yield return new WaitForEndOfFrame();

            if (hintsTextState == true) hints.hints.SetActive(true);
            if (showHintsState == true) hints.showHints.SetActive(true);
        }

        public void Resume()
        {
            exitMenu.SetActive(false);
            firsPersonCharacter.controller.enabled = true;
            if (firsPersonCharacter.gun != null) firsPersonCharacter.gun.enabled = true;
#if BCG_RCC
            if (car.rccController != null) car.rccController.enabled = true;
#endif
            if (car.simpleController != null && inCar) car.simpleController.SwitchEngine(true);
        }

        public void ExitDemo()
        {
            Application.Quit();
        }

        public void OpenAssetStore(string url)
        {
            Application.OpenURL(url);
        }
    }
}
