using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickManager : MonoBehaviour
{
    [SerializeField] int brickHP;
    [SerializeField] int brickScore;
    private int Lvl1Brick = 35;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            brickHP--;
            if (brickHP == 0)
            {
                Lvl1Brick--;
                Destroy(gameObject);
            }
            
        } 
    }
}
