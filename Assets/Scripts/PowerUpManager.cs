using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    private float speed = 0.1f;
    private PlayerMovement playerMovement;

    private void Start()
    {
        playerMovement = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector2(transform.position.x,transform.position.y-speed);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SoundManager.instance.PlaySound(SoundManager.instance.powerUpSound);
            if (playerMovement != null)
            {
                playerMovement.playerHP += 1;
            }
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("DeathCollider"))
        {
            Destroy(gameObject);
        }
    }
}
