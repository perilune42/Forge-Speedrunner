using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static bool GenerateNewMap;

    [Header("Editor Refs")]
    [SerializeField] private GameObject mainMenuLayoutGroup;
    [SerializeField] private GameObject startGameLayoutGroup;

    private void Awake()
    {
        GenerateNewMap = false;
    }

    public void StartGame()
    {
        mainMenuLayoutGroup.SetActive(false);
        startGameLayoutGroup.SetActive(true);
    }

    public void StartGameDifficulty(int difficulty)
    {
        // Set initial timer / scaling stuff

        GenerateNewMap = true;
        StartCoroutine(LoadSceneAsync());
    }

    public void BackFromStartGame()
    {
        mainMenuLayoutGroup.SetActive(true);
        startGameLayoutGroup.SetActive(false);
    }

    public void CreditsButton()
    {
        // Credits
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator LoadSceneAsync()
    {
        // Start fade

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("World");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
