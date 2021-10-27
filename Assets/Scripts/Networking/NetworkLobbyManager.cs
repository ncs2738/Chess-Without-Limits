using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using System.Linq;

public class NetworkLobbyManager : NetworkManager
{
    [SerializeField] int playerCount = 2;
    [Scene] [SerializeField] private string menuScene = string.Empty;

    [Header("Player prefab")]
    [SerializeField] private NetworkLobbyPlayer lobbyPlayerPrefab;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    //List of all the players in the room - lets us display their data
    public List<NetworkLobbyPlayer> playerList { get; } = new List<NetworkLobbyPlayer>();

    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
    }

    public override void OnStartClient()
    {
        GameObject[] spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (GameObject prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        //TODO: rewrite this to allow players to join as observers when in-game;
        //NOTE: WILL HAVE TO CHECK IF ALL PLAYER SLOTS ARE FILLED FIRST + INCREASE MAX PLAYER COUNT
        if (SceneManager.GetActiveScene().path != menuScene)
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //Check if we're still in the menu scene...
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            //Check if this is the host-player
            bool isHost = playerList.Count == 0;

            //We are!
            //Instantiate a new lobby-player
            NetworkLobbyPlayer lobbyPlayer = Instantiate(lobbyPlayerPrefab);

            //Set the player-host status
            lobbyPlayer.IsHost = isHost;

            //And add the new player's connection
            NetworkServer.AddPlayerForConnection(conn, lobbyPlayer.gameObject);
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if(conn.identity != null)
        {
            NetworkLobbyPlayer player = conn.identity.GetComponent<NetworkLobbyPlayer>();

            playerList.Remove(player);

            ReadyStateNotice();
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        playerList.Clear();
    }

    public void ReadyStateNotice()
    {
        foreach(NetworkLobbyPlayer player in playerList)
        {
            player.HandleReadyStatus(IsPlayerReady());
        }
    }

    private bool IsPlayerReady()
    {
        if(numPlayers != playerCount)
        {
            return false;
        }

        foreach (NetworkLobbyPlayer player in playerList)
        {
            if(!player.isReady)
            {
                return false;
            }
        }

        return true;
    }
}
