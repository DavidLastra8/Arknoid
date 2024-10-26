using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickManager : MonoBehaviour
{
    [SerializeField] int brickHP;
    [SerializeField] int brickScore;
    public PlayerMovement playerMovement;
    [SerializeField] GameObject powerUp;
    public static int Lvl1Brick = 30;
    public static int Lvl2Brick = 30;
    private int GameScore = 0;
    private static int powerUpCount = 0; 
    private const int maxPowerUps = 2;
    private UIManager uiManager;
    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>(); 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            SoundManager.instance.PlaySound(SoundManager.instance.crashSound);
            brickHP -=playerMovement.playerDamage;
            GameScore += brickScore;

            if (uiManager != null)
            {
                uiManager.UpdateScore(GameScore); 
            }

            if (brickHP <= 0)
            {
                Lvl1Brick--;
                int randomNumber = Random.Range(0, 100);
                if (randomNumber > 0 && randomNumber < 25 && powerUpCount < maxPowerUps)
                {
                    Instantiate(powerUp, transform.position, transform.rotation);
                    powerUpCount++;
                }
                Destroy(gameObject);
            }
            
        }

        if (collision.gameObject.CompareTag("Ball2"))
        {
            SoundManager.instance.PlaySound(SoundManager.instance.crashSound);
            brickHP -= playerMovement.playerDamage;
            GameScore += brickScore;

            if (uiManager != null)
            {
                uiManager.UpdateScore(GameScore);
            }

            if (brickHP <= 0)
            {
                Lvl2Brick--;
                int randomNumber = Random.Range(0, 100);
                if (randomNumber > 0 && randomNumber < 25 && powerUpCount < maxPowerUps)
                {
                    Instantiate(powerUp, transform.position, transform.rotation);
                    powerUpCount++;
                }
                Destroy(gameObject);
            }

        }
    }
    private void Update()
    {
        if (Lvl1Brick == 0)
        {
            GameManager.instance.LoadTransitionLvl2();
            GameManager.instance.SaveData();
        }

        if (Lvl2Brick == 0)
        {
            GameManager.instance.LoadLvl1();
            GameManager.instance.SaveData();
        }
    }

    
}
