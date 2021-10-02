using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public TeamColor teamColor;
    public Camera playerCamera;
    public CameraController cameraController;
    public Chessboard board;

    private void Awake()
    {
        cameraController = playerCamera.GetComponent<CameraController>();
    }

    private void Update()
    {
        cameraController.SetCameraActiveStatus(teamColor.Equals(board.GetActivePlayer()));
    }

    public void InitiatePlayer(TeamColor _teamColor, Chessboard _board)
    {
        SetPlayerTeam(_teamColor);
        SetPlayerCameraIndex((int) _teamColor - 1);
        board = _board;
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
