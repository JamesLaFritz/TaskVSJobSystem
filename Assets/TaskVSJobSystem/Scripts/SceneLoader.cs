// SceneLoader.cs
// 06-09-2022
// James LaFritz

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private Coroutine m_loadSceneCoroutine;

    [SerializeField] private int indexToLoad = 0;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && m_loadSceneCoroutine == null)
        {
            m_loadSceneCoroutine = StartCoroutine(LoadSceneAsync());
        }

        if (Input.GetKeyDown(KeyCode.Escape) && m_loadSceneCoroutine != null)
        {
            StopCoroutine(m_loadSceneCoroutine);
            m_loadSceneCoroutine = null;
        }
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(indexToLoad);
        yield return new WaitWhile(() => !asyncLoad.isDone);
        // SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        // SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(indexToLoad));
        // yield return new WaitWhile(() => !asyncLoad.isDone);
    }
}
