using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;

public class RTSNetworkingManager : NetworkManager
{
    [Header("RTS Variables")]
    [SerializeField] private GameObject unitSpawnerPrefab;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab;


    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;


    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect();

        ClientOnConnected?.Invoke();
    }
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect();

        ClientOnDisconnected?.Invoke();
    }

    //Occurs after the player is created
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        player.SetTeamColor(new Color(
            UnityEngine.Random.Range(0f, 1f), 
            UnityEngine.Random.Range(0f, 1f), 
            UnityEngine.Random.Range(0f, 1f)));
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        //if the scene is a map (not menu)
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            //create the game over handler
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

            //spawne it for each client
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
        }
    }
}
