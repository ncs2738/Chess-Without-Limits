using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldCameraController : MonoBehaviour
{
    [SerializeField]
    private float cameraPanSpeed = 7.5f;
    [SerializeField]
    private float normalMovementSpeed = 7.5f;
    [SerializeField]
    private float fastMovementSpeed = 15f;

    [SerializeField]
    private float rotationSpeed = 5f;
    [SerializeField]
    private float scrollSpeed = 2f;

    [SerializeField]
    private float cameraBorderWidth = 7.5f;

    [SerializeField]
    private Vector3 newPosition;
    [SerializeField]
    private Quaternion newRotation;

    [SerializeField]
    private int cameraPosIndex = 0;
    [SerializeField]
    private CameraTransforms[] cameraPositions;

    [SerializeField]
    private Vector2 cameraBounds;
    [SerializeField]
    private Vector2 zoomBounds;

    private void Start()
    {
        newPosition = transform.position; // cameraPositions[cameraPosIndex].GetPosition();
        //transform.eulerAngles = cameraPositions[cameraPosIndex].GetRotation();
        //transform.position = newPosition;
        newRotation = transform.rotation;
    }

    private void Update()
    {
        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        //If the player releases the space-button, change the camera's current position
        if (Input.GetKeyUp(KeyCode.Space))
        {
            cameraPosIndex++;
            if (cameraPosIndex >= cameraPositions.Length)
            {
                cameraPosIndex = 0;
            }

            newPosition = cameraPositions[cameraPosIndex].GetPosition();
            //transform.eulerAngles = cameraPositions[cameraPosIndex].GetRotation();
            transform.position = newPosition;
            return;
        }

        //Check for shift-key input to speed up our camera movement speed
        if (Input.GetKey(KeyCode.LeftShift))
        {
            cameraPanSpeed = fastMovementSpeed;
        }
        else
        {
            cameraPanSpeed = normalMovementSpeed;
        }

        //Up-movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - cameraBorderWidth)
        {
            newPosition.z += (cameraPanSpeed * Time.deltaTime);
        }
        //Down-movement
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= cameraBorderWidth)
        {
            newPosition.z -= (cameraPanSpeed * Time.deltaTime);
        }
        //Right-movement
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - cameraBorderWidth)
        {
            newPosition.x += (cameraPanSpeed * Time.deltaTime);
        }
        //Left-movement
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= cameraBorderWidth)
        {
            newPosition.x -= (cameraPanSpeed * Time.deltaTime);
        }

        //Check for if the player presses Q or E to rotate the camera accordingly
        if (Input.GetKey(KeyCode.Q))
        {
            newRotation *= Quaternion.Euler(Vector3.up * rotationSpeed);
        }
        if (Input.GetKey(KeyCode.E))
        {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationSpeed);
        }

        //Check for zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        newPosition.y -= scroll * scrollSpeed * 150f * Time.deltaTime;

        //Check for if the player presses R or F are pressed & zoom the camera in
        if (Input.GetKey(KeyCode.R))
        {
            // newPosition.y -= scrollSpeed * 2 * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.F))
        {
            // newPosition.y -= scrollSpeed * -2 * Time.deltaTime;
        }

        //Clamp the camera's bounds
        newPosition.x = Mathf.Clamp(newPosition.x, -cameraBounds.x, cameraBounds.x);
        newPosition.z = Mathf.Clamp(newPosition.z, -cameraBounds.y, cameraBounds.y);
        newPosition.y = Mathf.Clamp(newPosition.y, zoomBounds.x, zoomBounds.y);

        //Set the new transforms
        transform.position = newPosition;
        transform.rotation = newRotation;
    }
}