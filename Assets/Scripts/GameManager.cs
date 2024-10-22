using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    PlayerMovement player;
    public bool isPausing = false;
    [SerializeField] private UIManager uiManager;
    public void GameOver()
    {
        
            SceneManager.LoadScene("GameOverScene");
        
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
