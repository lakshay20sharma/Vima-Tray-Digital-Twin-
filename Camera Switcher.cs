using UnityEngine;
using UnityEngine.InputSystem; // new input system

public class CameraSwitcher : MonoBehaviour
{
    public Camera[] cameras;
    private int currentCameraIndex = 0;

    void Start()
    {
        // Enable only the first camera
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == currentCameraIndex);
        }
    }

    void Update()
    {
        // Press C to switch to next camera
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            cameras[currentCameraIndex].gameObject.SetActive(false);

            currentCameraIndex++;
            if (currentCameraIndex >= cameras.Length)
                currentCameraIndex = 0;

            cameras[currentCameraIndex].gameObject.SetActive(true);
        }
    }
}
