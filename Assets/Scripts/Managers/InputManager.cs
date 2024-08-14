using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{

    [SerializeField] private LayerMask mousePlaneLayerMask;

    public Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.nearClipPlane;  // 카메라의 near clip plane으로 z값 설정
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        return mouseWorldPosition;
    }

    public bool GetPosition(out Vector3 position)
    {
        Vector3 clickPosition = GetMouseWorldPosition();
        RaycastHit2D raycastHit = Physics2D.Raycast(clickPosition, Vector3.zero, 0f, mousePlaneLayerMask);
        if (raycastHit.collider != null)
        {
            position = raycastHit.point;
            return true;
        }

        position = Vector3.zero;
        return false;
    }
}
