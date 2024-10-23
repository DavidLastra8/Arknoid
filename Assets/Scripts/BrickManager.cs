using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickManager : MonoBehaviour
{
    [SerializeField] int brickHP;
    [SerializeField] int brickScore;
    [SerializeField] private PlayerMovement playerMovement = new PlayerMovement();
    [SerializeField] GameObject powerUp;
    private int Lvl1Brick = 35;
    private static int powerUpCount = 0; 
    private const int maxPowerUps = 2; 


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            brickHP-=playerMovement.playerDamage;
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
    }
}
