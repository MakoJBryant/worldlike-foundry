using UnityEngine;
using UnityEngine.InputSystem;

public class PlanetClickSelect : MonoBehaviour
{
    public Camera cam;
    public OrbitCameraController cameraController;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                cameraController.SetTarget(hit.transform);
            }
        }
    }
}