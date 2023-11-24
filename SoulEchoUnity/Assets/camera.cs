using UnityEngine;

public class SmoothCameraMovement : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 2.0f;

    private float mouseX = 0.0f;
    private float mouseY = 0.0f;

    void Update()
    {
        // Get input for mouse movement
        mouseX += Input.GetAxis("Mouse X") * rotationSpeed;
        mouseY -= Input.GetAxis("Mouse Y") * rotationSpeed;

        // Limit vertical rotation to 90 degrees up and down
        mouseY = Mathf.Clamp(mouseY, -90.0f, 90.0f);

        // Set the new rotation angles
        transform.rotation = Quaternion.Euler(mouseY, mouseX, 0.0f);
    }
}
