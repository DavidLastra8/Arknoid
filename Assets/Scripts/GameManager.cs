using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    PlayerMovement player;
    public bool isPausing = false;
    [SerializeField] private UIManager uiManager;
    public static GameManager instance;

    private void Start()
    {
        instance = this;
    }
    public void GameOver()
    {
        
        SceneManager.LoadScene("GameOverScene");
        
    }
    public void LoadLvl2()
    {
        SceneManager.LoadScene("Lvl2");
    }
    public void LoadLvl1() 
    {
        SceneManager.LoadScene("Lvl1");
    }

    public void LoadTransitionLvl1()
    {
        SceneManager.LoadScene("TransitionLvl1");
    }

    public void LoadTransitionLvl2()
    {
        SceneManager.LoadScene("TransitionLvl2");
    }

    public void LoadBackToMenu()
    {
        SceneManager.LoadScene("InitialScreen");
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("HighScore", uiManager.highScore);
        PlayerPrefs.SetInt("CurrentScore", uiManager.currentScore);
        PlayerPrefs.SetInt("CurrentLives", uiManager.currentLives);
        PlayerPrefs.SetString("CurrentScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        uiManager.highScore = PlayerPrefs.GetInt("HighScore");
        uiManager.currentScore = PlayerPrefs.GetInt("CurrentScore");
        uiManager.currentLives = PlayerPrefs.GetInt("CurrentLives");
        SceneManager.LoadScene(PlayerPrefs.GetString("CurrentScene"));
    }

    public void PauseMenuActivation()
    {
        uiManager.PauseMenu();
    }

    public void PauseMenuDeActivation()
    {
        uiManager.HidePauseMenu();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            isPausing = !isPausing;
        }

        if (isPausing == true) 
        {
            PauseMenuActivation();
            Time.timeScale = 0;
        }
        else
        {
            PauseMenuDeActivation();
            Time.timeScale = 1;
        }
    }
}
