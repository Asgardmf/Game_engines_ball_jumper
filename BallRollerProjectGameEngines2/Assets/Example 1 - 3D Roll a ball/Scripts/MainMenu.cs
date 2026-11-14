using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] AudioClip menuMusic;
    public string firstLevel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.PlayMusic(menuMusic); 
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void StartGame()
    {
        SceneManager.LoadScene(firstLevel);
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting");
    }
}
