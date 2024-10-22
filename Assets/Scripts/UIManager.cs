using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager: MonoBehaviour 
{
    [SerializeField] GameObject pauseMenu;
    public void PauseMenu()
    {
        pauseMenu.SetActive(true);
    }

    
}
