using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] string mainMenu;
    public GameObject PausePanel;
    // Update is called once per frame
    void Update()
    {
        
    }
    public void Pause()
    {
        PausePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void Continue()
    {
        Time.timeScale = 1;
        PausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting");
    }
    public void BackToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(mainMenu);
    }
}
