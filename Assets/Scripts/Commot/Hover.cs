using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    
    private SpriteRenderer spriteRenderer;

    private void Start() 
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update() 
    {
        FollowMouse();
    }

    private void FollowMouse()
    {
        transform.position = InputManager.Instance.GetMouseWorldPosition();
    }

    public void Activate(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }
}
