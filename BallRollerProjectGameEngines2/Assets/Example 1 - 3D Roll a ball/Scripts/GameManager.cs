using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VectorGraphics;

// Central point for level flow. Manages Player Restart
// and has methods that are called by the Finish Level zone.
public class GameManager : MonoBehaviour
{
    [SerializeField] Transform startPoint;
    [SerializeField] PlayerController player;
    [SerializeField] float levelCompleteDelay = 1.5f;
    [SerializeField] GameObject resultsPanel;

    // the textfields are not continuable for more Levels. when more levels are added, change structure to loop through the Build settings scenes for displaying the highscores and current runtimes
    [SerializeField] TMP_Text level1TimeHighScoreText;
    [SerializeField] TMP_Text level2TimeHighScoreText;
    [SerializeField] TMP_Text level1TimeThisTryText;
    [SerializeField] TMP_Text level2TimeThisTryText;
    [SerializeField] string firstLevel;
    public float levelStartTime;

    private void Start()
    {
        Time.timeScale = 1f;
        player.OnRespawn(startPoint.position);
        levelStartTime = Time.time;
        if (SceneManager.GetActiveScene().name == firstLevel)
        {
            ClearRunTimes();
        }
    }

    // Called by Player when it touches a KillZone.
    public void OnPlayerEnteredKillzone()
    {
        player.OnRespawn(startPoint.position);
    }

    // Called by FinishZone when the player reaches the end of the level.
    public void OnPlayerReachedFinish()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        float elapsed = Time.time - levelStartTime;

        PlayerPrefs.SetFloat($"RunTime_{sceneName}", elapsed);

        string key = $"BestTime_{sceneName}";
        if (!PlayerPrefs.HasKey(key) || elapsed < PlayerPrefs.GetFloat(key))
        {
            PlayerPrefs.SetFloat(key, elapsed);
        }
        PlayerPrefs.Save();
        // Invoke calls a method by name after a delay - a simpler alternative
        // to a coroutine for a single delayed call.
        Invoke(nameof(LoadNextLevel), levelCompleteDelay);
    }

    void LoadNextLevel()
    {
        // Scenes only have a valid build index if they're added to
        // File > Build Profiles > Scene List, otherwise this index code ownt work.
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            showResults();
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
    }
    void showResults()
    {
        Time.timeScale = 1f;

        level1TimeHighScoreText.text = "Level 1 High Score: " + FormatBestTime("DoodleJumpScene1");
        level2TimeHighScoreText.text = "Level 2 High Score: " + FormatBestTime("Scene 2");
        level1TimeThisTryText.text = "Level 1 Current Run: " + FormatCurrentTime("DoodleJumpScene1");
        level2TimeThisTryText.text = "Level 2 Current Run: " + FormatCurrentTime("Scene 2");

        resultsPanel.SetActive(true);
    }

    string FormatBestTime(string sceneName)
    {
        string key = $"BestTime_{sceneName}";

        if (!PlayerPrefs.HasKey(key))
        {
            return "--:--.--";
        }

        float time = PlayerPrefs.GetFloat(key);
        int minutes = Mathf.FloorToInt(time / 60f);
        float seconds = time % 60f;

        return $"{minutes:00}:{seconds:00.00}";
    }
    string FormatCurrentTime(string sceneName)
    {
        string key = $"RunTime_{sceneName}";

        if (!PlayerPrefs.HasKey(key))
        {
            return "--:--.--";
        }

        float time = PlayerPrefs.GetFloat(key);
        int minutes = Mathf.FloorToInt(time / 60f);
        float seconds = time % 60f;

        return $"{minutes:00}:{seconds:00.00}";
    }
    void ClearRunTimes()
    {
        for(int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            PlayerPrefs.DeleteKey($"RunTime_{sceneName}");
        }

        PlayerPrefs.Save();
    }
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

