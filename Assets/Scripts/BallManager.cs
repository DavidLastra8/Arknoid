using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] int initialSpeed = 300;
    [SerializeField] int speed;
    [SerializeField] private PlayerMovement player;
    private Vector2 velocity;
    private Vector2 startPosition;
    [SerializeField] private GameManager gameManager;
    

    // Start is called before the first frame update
    void Start()
    {
       
        startPosition = transform.position;
        Invoke("ResetBall",2f);
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("DeathCollider"))
        {
            SoundManager.instance.PlaySound(SoundManager.instance.deathSound);
            player.playerHP--;

            if (player.playerHP <= 0)
            {
                gameManager.GameOver();
                StopBall();
                
            }
            else
            {
                speed = 0;
                ResetPosition();
            }

        }
        if (collision.gameObject.CompareTag("Player"))
        {
            SoundManager.instance.PlaySound(SoundManager.instance.crashSound);
        }
        if (rigidbody.velocity.magnitude <= 13)
        {
            rigidbody.velocity = rigidbody.velocity * 1.08f;
        }

    }
   
    public void ResetPosition()
    {
        FindObjectOfType<PlayerMovement>().ResetPlayerPosition();
        BallResetPosition();
    }

    private void ResetBall()
    {
        velocity.x = Random.Range(-1f, 1f);
        velocity.y = 1f;
        speed = initialSpeed;
        rigidbody.AddForce(velocity * speed);
    }
   
    private void BallResetPosition()
    {
        rigidbody.velocity = Vector2.zero;
        transform.position = startPosition;

        Invoke("ResetBall", 2f);
    }
    
    private void StopBall()
    {
        rigidbody.velocity = Vector2.zero;
        transform.position = startPosition;
    }

}
