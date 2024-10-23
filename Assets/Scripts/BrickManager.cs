using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickManager : MonoBehaviour
{
    [SerializeField] int brickHP;
    [SerializeField] int brickScore;
    [SerializeField] GameObject powerUp;
    private int Lvl1Brick = 35;
    private int powerUpSpawner = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            brickHP--;
            if (brickHP == 0)
            {
                Lvl1Brick--;
                int randomNumber = Random.Range(0, 100);
                if (randomNumber > 0 && randomNumber < 10 && powerUpSpawner ==1)
                {
                    Instantiate(powerUp, transform.position, transform.rotation);
                    powerUpSpawner = 0;
                }
                Destroy(gameObject);
            }
            
        } 
    }
}
