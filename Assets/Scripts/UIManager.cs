using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager: MonoBehaviour 
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject canvas;
    [SerializeField] private GameManager gameManager;
    [SerializeField] TMP_Text scoreText; 
    [SerializeField] TMP_Text livesText; 
    [SerializeField] TMP_Text highScoreText;
    [SerializeField ]private PlayerMovement playerMovement;


    public int currentScore;
    public int currentLives;
    public int highScore;

    private void Start()
    {
        
        LoadData();
        ResetUI();
    }

    private void Update()
    {
        UpdateUI();
    }
    public void PauseMenu()
    {
        pauseMenu.SetActive(true);
    }

    public void HidePauseMenu()
    {
        pauseMenu.SetActive(false);
    }

    public void Continue()
    {
        gameManager.isPausing = false;
    }

    public void ResetGame()
    {
        SceneManager.LoadScene("InitialScreen");
    }

    public void LoadLvl1()
    {
        SceneManager.LoadScene("Lvl1");
    }

    public void LoadLvl2()
    {
        SceneManager.LoadScene("Lvl2");
    }

    public void LoadData()
    {
        currentScore = PlayerPrefs.GetInt("Score", 0);
        currentLives = PlayerPrefs.GetInt("Lives", 3);
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        scoreText.text = "Score: " + currentScore;
        livesText.text = "Lives: " + currentLives;
        highScoreText.text = "Best: " + highScore;
    }

    private void UpdateUI()
    {

        scoreText.text = "Score: " + currentScore;
        livesText.text = "Lives: " + playerMovement.playerHP;
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (currentScore > highScore)
        {
            highScoreText.text = "Best: " + currentScore;
            PlayerPrefs.SetInt("HighScore", currentScore); 
            PlayerPrefs.Save();
        }
    }

    public void UpdateScore(int newScore)
    {
        currentScore += newScore;
        PlayerPrefs.SetInt("Score", currentScore);
        PlayerPrefs.Save();
    }

    public void ResetUI()
    {
        currentScore = 0;
       
    }
}
