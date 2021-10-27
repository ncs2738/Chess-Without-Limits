using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private NetworkLobbyManager networkManager = null;

    [Header("Lobby Menu UI")]
    [SerializeField] private GameObject mainMenu = null;
    [SerializeField] private TMP_InputField ipAddressInputField = null;
    [SerializeField] private Button joinButton = null;

    //Add & remove the events to the lobby-manager when the lobby-menu is enabled & disabled
    private void OnEnable()
    {
        NetworkLobbyManager.OnClientConnected += HandleClientConnected;
        NetworkLobbyManager.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        NetworkLobbyManager.OnClientConnected -= HandleClientConnected;
        NetworkLobbyManager.OnClientDisconnected -= HandleClientDisconnected;
    }

    //When the user presses the join button, grab & set the ip address & start the client.
    public void JoinLobby()
    {
        string ipAddress = ipAddressInputField.text;

        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();

        //Disable the join button so players can't spam it.
        joinButton.interactable = false;
    }

    //If we successfully connect to the server, disable the lobby, but re-enable the join button.
    private void HandleClientConnected()
    {
        //Re-enable the join button for future use.
        joinButton.interactable = true;

        //Disable the lobbies.
        gameObject.SetActive(false);
        mainMenu.SetActive(false);
    }

    //If we failed to connect, re-enable the join-button.
    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}
