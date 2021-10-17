using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class MainMenu : NetworkManager
{
    //Reads in the menu scene
    [Scene] [SerializeField] private string menuScene = string.Empty;


   // [Header("Room")]
   // [Scene] [SerializeField] private string menuScene = string.Empty;

    // Update is called once per frame
    void Update()
    {
        
    }
}
