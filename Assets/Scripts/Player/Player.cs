using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //public GameObject CameraPrefab;
    public TeamColor teamColor;
    public Camera playerCamera;
    public CameraController cameraController;

    private void Start()
    {
        // GameObject agga = CameraPrefab.gameObject.transform.GetChild(0).gameObject;
        //playerCamera = agga.GetComponent<Camera>();
        cameraController = playerCamera.GetComponent<CameraController>();
    }

    public void SetPlayerTeam(TeamColor _teamColor)
    {
        teamColor = _teamColor;
    }

    public void SetPlayerCameraPosition(int index)
    {
        cameraController.SetCurrentCameraPosition(index);
    }

    public Camera GetCamera()
    {
        return playerCamera;
    }

    /*
    public Player(TeamColor _teamColor)
    {
        teamColor = _teamColor;
    }
    */
}
