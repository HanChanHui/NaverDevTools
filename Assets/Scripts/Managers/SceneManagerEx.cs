using System.Collections;
using Consts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HornSpirit {
    public class SceneManagerEx : MonoBehaviour {
        //public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }

        private static SceneManagerEx _instance;

        public static SceneManagerEx Instance {
            get {
                if (_instance == null) {
                    var obj = FindObjectOfType<SceneManagerEx>();
                    if (obj != null) {
                        _instance = obj;
                    } else {
                        _instance = Create();
                    }
                }
                return _instance;
            }
        }

        private static SceneManagerEx Create() {
            return Instantiate(Resources.Load<SceneManagerEx>("UI/Loading/UI_Loading"));
        }

        private void Awake() {
            if (Instance != this) {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        public CanvasGroup canvasGroup;
        private string _loadSceneName;

        public void LoadScene(SceneType type) {
            //Create();
            gameObject.SetActive(true);
            SceneManager.sceneLoaded += OnSceneLoaded;
            _loadSceneName = GetSceneName(type);
            StartCoroutine(LoadSceneProcess());

            //CurrentScene.Clear();
        }

        IEnumerator LoadSceneProcess() {
            yield return StartCoroutine(Fade(true));


            AsyncOperation op = SceneManager.LoadSceneAsync(_loadSceneName);
            op.allowSceneActivation = false;

            while (!op.isDone) {
                yield return null;
                if (op.progress > 0.9f) {

                } else {
                    //yield return new WaitForSeconds(0.1f);
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }

        IEnumerator Fade(bool isFadeIn) {
            if (!isFadeIn) {
                yield return new WaitForSeconds(0.2f);
            }

            float timer = 0f;
            while (timer <= 1f) {
                yield return null;
                timer += Time.unscaledDeltaTime * 2f;
                canvasGroup.alpha = isFadeIn ? Mathf.Lerp(0f, 1f, timer) : Mathf.Lerp(1f, 0f, timer);
            }

            if (!isFadeIn) {
                gameObject.SetActive(false);
            }
        }

        public void OnSceneLoaded(Scene arg0, LoadSceneMode arg1) {
            if (arg0.name == _loadSceneName) {
                StartCoroutine(Fade(false));
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        string GetSceneName(SceneType type) {
            string sceneName = System.Enum.GetName(typeof(SceneType), type);
            return sceneName;
        }

        public void Clear() {
            //CurrentScene.Clear();
        }
    }
}