using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float cameraZoomSpeed = .05f;
    [SerializeField]
    private float cameraPanSpeed = .25f;
    [SerializeField]
    private float normalMovementSpeed = .25f;
    [SerializeField]
    private float fastMovementSpeed = .5f;


    [SerializeField]
    private float cameraBorderWidth = 7.5f;

    //transform for getting where the camera is in 2d space
    [SerializeField]
    protected Transform cameraTransform;
    //transform for getting where the camera's parent is in 2d space
    [SerializeField]
    protected Transform parentTransform;

    //Holds the camera's rotation
    [SerializeField]
    protected Vector3 cameraRotation;
    //distance from the camera & the parent object
    [SerializeField]
    protected float cameraDistance = 10f;

    [SerializeField]
    private int cameraPosIndex = 0;
    [SerializeField]
    private Vector3[] cameraPositions;

    [SerializeField]
    private Vector2 cameraBounds;
    [SerializeField]
    private Vector2 zoomBounds;

    [SerializeField]
    private float ScrollSensitivity = 2f;
    [SerializeField]
    private float OrbitDampening = 10f;
    [SerializeField]
    private float ScrollDampening = 6f;

    private float animationTime;

    [SerializeField]
    private bool isCameraActive;

    private void Awake()
    {
        cameraTransform = this.transform;
        parentTransform = this.transform.parent;
        cameraRotation = cameraPositions[cameraPosIndex];

        Quaternion cameraQuaternion = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0);
        parentTransform.rotation = cameraQuaternion;
    }

    private void LateUpdate()
    {
        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        if(!isCameraActive)
        {
            return;
        }

        //Check for if the player presses up on  Q, E, or space & swap to a new fixed camera position
        if (Input.GetKeyUp(KeyCode.Q))
        {
            animationTime = OrbitDampening;
            cameraPosIndex--;
            if (cameraPosIndex < 0)
            {
                cameraPosIndex = cameraPositions.Length - 1;
            }

            cameraRotation = cameraPositions[cameraPosIndex];
            cameraDistance = 10f;
            UpdateCameraPosition();
            return;
        }

        //If the player releases the space-button or E key, change the camera's current position
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.E))
        {
            animationTime = OrbitDampening;
            cameraPosIndex++;
            if (cameraPosIndex >= cameraPositions.Length)
            {
                cameraPosIndex = 0;
            }

            cameraRotation = cameraPositions[cameraPosIndex];
            cameraDistance = 10f;
            UpdateCameraPosition();
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

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - cameraBorderWidth)
        {
            cameraRotation.y += cameraPanSpeed;
        }
        //Down-movement
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= cameraBorderWidth)
        {
            cameraRotation.y -= cameraPanSpeed;
        }

        cameraRotation.y = Mathf.Clamp(cameraRotation.y, 0f, 90f);

        //Left-movement
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= cameraBorderWidth)
        {
            cameraRotation.x += cameraPanSpeed;
        }
        //Right-movement
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - cameraBorderWidth)
        {
            cameraRotation.x -= cameraPanSpeed;
        }

        //Check for zoom
        //mouse wheel scrolling for zoom-in
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScroll != 0f)
        {
            mouseScroll *= ScrollSensitivity * 2;
        }

        //Check for if the player presses R or F are pressed & zoom the camera in
        if (Input.GetKey(KeyCode.R))
        {
            cameraDistance -= cameraZoomSpeed;
        }
        if (Input.GetKey(KeyCode.F))
        {
            cameraDistance += cameraZoomSpeed;
        }

        //Flip the zoom direction
        cameraDistance += mouseScroll * -1f;
        cameraDistance = Mathf.Clamp(cameraDistance, zoomBounds.x, zoomBounds.y);

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        //Get the new camera's angle
        Quaternion cameraQuaternion = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0);

        //set the parent's new rotation & animate to it using a Lerp
        parentTransform.rotation = Quaternion.Lerp(parentTransform.rotation, cameraQuaternion, animationTime * Time.deltaTime);

        //Check if we've zoomed at all
        if (cameraTransform.localPosition.z != cameraDistance * -1f)
        {
            //we've zoomed, so set the new camera's zoom & lerp to animate to it
            cameraTransform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(cameraTransform.localPosition.z, cameraDistance * -1f, Time.deltaTime * ScrollDampening));
        }
    }

    public void SetCurrentCameraIndex(int index, bool halfSpeed)
    {
        if (cameraPosIndex < 0 || cameraPosIndex >= cameraPositions.Length)
        {
            cameraPosIndex = 0;
        }
        else
        {
            cameraPosIndex = index;
        }

        cameraRotation = cameraPositions[cameraPosIndex];
        cameraDistance = 10f;
        animationTime = halfSpeed ? OrbitDampening / 2: OrbitDampening;
        UpdateCameraPosition();
    }

    public void SetCameraPositon(Vector3 newRotation)
    {
        cameraRotation = newRotation;
        cameraDistance = 10f;
        Quaternion cameraQuaternion = Quaternion.Euler(newRotation.y, newRotation.x, 0);
        parentTransform.rotation = cameraQuaternion;
    }

    public void SetCameraActiveStatus(bool activeStatus)
    {
        isCameraActive = activeStatus;
    }

    public Vector3 GetCameraRotation()
    {
        return cameraRotation;
    }
}
