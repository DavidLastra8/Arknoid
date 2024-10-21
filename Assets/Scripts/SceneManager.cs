using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneManager : MonoBehaviour
{
    PlayerMovement player;
    Canvas gameOver;
   public void GameOver()
    {
        if (player.playerHP == 0)
        {
           // SceneManager.Instantiate(gameOver);

        }
    }
}
