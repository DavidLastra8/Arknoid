using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.down;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            
        }

        if (collision.gameObject.CompareTag("DeathCollider"))
        {
            Destroy(gameObject);
        }
    }
}
