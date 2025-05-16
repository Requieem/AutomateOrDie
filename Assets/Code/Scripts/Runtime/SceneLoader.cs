using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Scripts.Runtime
{
    public class SceneLoader : MonoBehaviour
    {
        [Header("Scene Loader")]
        [Tooltip("Name of the scene to load. Make sure it's added to the Build Settings.")]
        [SerializeField] private string m_sceneToLoad;

        public void LoadSceneDelayed()
        {
            Invoke(nameof(LoadScene), 1f);
        }

        /// <summary>
        /// Loads the assigned scene.
        /// </summary>
        public void LoadScene()
        {
            if (string.IsNullOrEmpty(m_sceneToLoad))
            {
                Debug.LogWarning("SceneLoader: No scene name assigned.");
                return;
            }

            if (Application.CanStreamedLevelBeLoaded(m_sceneToLoad))
            {
                SceneManager.LoadScene(m_sceneToLoad);
            }
            else
            {
                Debug.LogError($"SceneLoader: Scene '{m_sceneToLoad}' cannot be loaded. Check if it's in the Build Settings.");
            }
        }
    }
}