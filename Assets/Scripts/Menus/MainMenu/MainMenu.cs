using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private NetworkLobbyManager networkManager = null;

    [Header("Main Menu UI")]
    [SerializeField] private GameObject mainMenu = null;

    public void HostLobby()
    {
        networkManager.StartHost();

        mainMenu.SetActive(false);
    }

    /* TODO: add for local player
    public void PlayLocalGame()
    {

    }
    */

    public void QuitGame()
    {
        Application.Quit();
    }
}
