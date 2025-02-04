using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils
{
    public class LevelsMusic : MonoBehaviour
    {
        public static LevelsMusic instance;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
                SceneManager.sceneLoaded += instance.OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.buildIndex is 5 or 0)
            {
                instance.GetComponent<AudioSource>().Stop();
            }
            else if(!instance.GetComponent<AudioSource>().isPlaying)
            {
                instance.GetComponent<AudioSource>().Play();
            }
        }
    }
}
