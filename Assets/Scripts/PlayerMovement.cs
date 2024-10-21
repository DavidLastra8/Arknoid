using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 offset;
    private bool IsDragging = false;
    private bool IsAutomatic = false;
    private Camera mainCamera;
    [SerializeField] private GameObject ball;
    public int playerHP = 3;
    private Vector2 startPosition;
    private void Start()
    {
        mainCamera = Camera.main;
        startPosition = transform.position;
        
        
    }
    void OnMouseDown()
    {
        IsDragging = true;
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseUp() 
    { 
        IsDragging = false; 
    }



    // Update is called once per frame
    void Update()
    {
        if (IsAutomatic == false)
        {
            if (IsDragging)
            {
                Vector3 newPosition = GetMouseWorldPos() + offset;
                transform.position = new Vector3(newPosition.x, transform.position.y, transform.position.z);
            }
        }
        else
        {
            transform.position = new Vector3(ball.transform.position.x, transform.position.y, transform.position.z);
        }
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            IsAutomatic = !IsAutomatic;
        }

    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = mainCamera.nearClipPlane;
        return mainCamera.ScreenToWorldPoint(pos);
    }

    public void ResetPlayerPosition()
    {
        transform.position = startPosition;

    }

}
