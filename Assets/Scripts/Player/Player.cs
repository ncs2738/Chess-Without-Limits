using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    public TeamColor teamColor;
    public Camera playerCamera;
    public CameraController cameraController;
    private Chessboard board;

    private void Awake()
    {
        cameraController = playerCamera.GetComponent<CameraController>();
        board = FindObjectOfType<Chessboard>();
    }

    [Client]
    public override void OnStartAuthority()
    {
        playerCamera.gameObject.SetActive(true);
        playerCamera.enabled = true;

        board.AddPlayer(this);
    }

    [Client]
    private void Update()
    {
        if(!hasAuthority)
        {
            return;
        }

        cameraController.SetCameraActiveStatus(teamColor.Equals(board.GetActivePlayer()));
    }

    [Client]
    public void InitiatePlayer(TeamColor _teamColor)
    {
        SetPlayerTeam(_teamColor);
        SetPlayerCameraIndex((int) _teamColor - 1);
    }

    private void SetPlayerTeam(TeamColor _teamColor)
    {
        teamColor = _teamColor;
    }

    public TeamColor GetPlayerTeamColor()
    {
        return teamColor;
    }

    public void SetPlayerCameraIndex(int index, bool halfSpeed = false)
    {
        cameraController.SetCurrentCameraIndex(index, halfSpeed);
    }

    public Camera GetCamera()
    {
        return playerCamera;
    }

    public Vector3 GetCameraRotation()
    {
        return cameraController.GetCameraRotation();
    }

    public void SetCameraPosition(Vector3 newPosition)
    {
        cameraController.SetCameraPositon(newPosition);
    }
}
