using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandlerSystem : MonoBehaviour
{
    // public static InputHandlerSystem Instance { get; private set; }

    // private InputActionAsset inputActions;
    // private InputAction mouseUpAction;
    // private Coroutine mouseHoldCoroutine;
    
    // [SerializeField] private TowerPlacement towerPlacement;

    // private void Awake()
    // {
    //     if (Instance != null)
    //     {
    //         Debug.LogError("There's more than one InputHandler! " + transform + " - " + Instance);
    //         Destroy(gameObject);
    //         return;
    //     }
    //     Instance = this;

    //     inputActions = Resources.Load<InputActionAsset>("InputSystem/PlayerInputActions");

    //     var playerActionMap = inputActions.FindActionMap("MouseClick");
    //     mouseUpAction = playerActionMap.FindAction("MouseUp");
    // }

    // private void OnEnable()
    // {
    //     mouseUpAction.performed += HandleMouseUp;
    //     mouseUpAction.Enable();
    // }

    // private void OnDisable()
    // {
    //     mouseUpAction.performed -= HandleMouseUp;
    //     mouseUpAction.Disable();
    // }

    // public void HandleMouseDown()
    // {
    //     if (mouseHoldCoroutine == null)
    //     {
    //         towerPlacement.RefreshVisual();
    //         mouseHoldCoroutine = StartCoroutine(MouseHoldRoutine());
    //     }
    // }

    // private void HandleMouseUp(InputAction.CallbackContext context)
    // {
    //     towerPlacement.OnMouseUp();
    //     if (mouseHoldCoroutine != null)
    //     {
    //         StopCoroutine(mouseHoldCoroutine);
    //         mouseHoldCoroutine = null;
    //     }
    // }

    // private IEnumerator MouseHoldRoutine()
    // {
    //     while (true)
    //     {
    //         towerPlacement.UpdateMousePosition();
    //         towerPlacement.HandleMouseHoldAction();
    //         yield return new WaitForSeconds(0.01f);
    //     }
    // }
}
