using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager: MonoBehaviour 
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject canvas;
    [SerializeField] private GameManager gameManager;
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
        SceneManager.LoadScene("Lvl1");
    }
    
}
