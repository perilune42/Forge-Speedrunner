using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static bool GenerateNewMap;

    private void Awake()
    {
        GenerateNewMap = false;
    }

    public void StartGame()
    {
        GenerateNewMap = true;
        SceneManager.LoadScene("World");
    }
}