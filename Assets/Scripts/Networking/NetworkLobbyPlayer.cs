using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkLobbyPlayer : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private Button startButton;

    [SyncVar(hook = nameof(HandleReadyStatusChange))]
    public bool IsReady = false;
    private bool isHost;

    private NetworkLobbyManager lobby;

    private NetworkLobbyManager Lobby
    {
        get
        {
            if (lobby != null)
            {
                return lobby;
            }

            return lobby = NetworkManager.singleton as NetworkLobbyManager;
        }
    }

    public void SetIsHost(bool _isHost)
    {
        isHost = _isHost;
        startButton.gameObject.SetActive(_isHost);
    }

    public bool GetIsHost()
    {
        return isHost;
    }

    public override void OnStartAuthority()
    {
        //Update the player's lobby-ui
        lobbyUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        //Add the player to the list
        Lobby.playerList.Add(this);
    }

    public override void OnStopClient()
    {
        Lobby.playerList.Remove(this);


    }

    public void HandleReadyStatusChange(bool oldValue, bool newValue) => UpdateDisplay();

    public void UpdateDisplay()
    {
        if (!hasAuthority)
        {
            foreach (var player in Lobby.playerList)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }

            return;
        }


        for(int i = 0; i < Lobby.playerList.Count; i++)
        {
            Debug.Log(Lobby.playerList[i].IsReady);
        }
    }

    public void HandleReadyStatus(bool readyStatus)
    {
        if(!isHost)
        {
            return;
        }

        startButton.interactable = readyStatus;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        Lobby.ReadyStateNotice();
    }

    [Command]
    public void CmdStartGame()
    {
        if(Lobby.playerList[0].connectionToClient != connectionToClient)
        {
            return;
        }

        //START THE GAME! :D
    }
}
