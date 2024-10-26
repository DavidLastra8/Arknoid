using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public Slider movementSlider; // Asigna el slider desde el Inspector
    private Vector3 offset;
    private bool IsAutomatic = false;
    public int playerDamage = 1;
    private Camera mainCamera;
    [SerializeField] private GameObject ball;
    public int playerHP = 3;
    private Vector2 startPosition;

    private void Start()
    {
        mainCamera = Camera.main;
        startPosition = transform.position;

        movementSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        float newX = Mathf.Lerp(-5f, 5f, value); 
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    void Update()
    {
        if (IsAutomatic)
        {
            transform.position = new Vector3(ball.transform.position.x, transform.position.y, transform.position.z);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            IsAutomatic = !IsAutomatic;
        }
    }

    public void ResetPlayerPosition()
    {
        transform.position = startPosition;
        movementSlider.value = 0.5f; 
    }
}