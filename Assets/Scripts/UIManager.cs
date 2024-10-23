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

    }
    
}
