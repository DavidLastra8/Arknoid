using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickManager : MonoBehaviour
{
    [SerializeField] int brickHP;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            brickHP--;
            if (brickHP == 0)
            {
                Destroy(gameObject);
            }
            
        } 
    }
}
