using UnityEngine;
using UnityEngine.InputSystem;   // <-- Required

public class ToggleGraphCanvas : MonoBehaviour
{
    public GameObject graphCanvasObject;

    void Update()
    {
        // New Input System check
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            if (graphCanvasObject != null)
                graphCanvasObject.SetActive(!graphCanvasObject.activeSelf);
        }
    }
}
