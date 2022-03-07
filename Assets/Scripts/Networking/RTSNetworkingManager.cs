using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class RTSNetworkingManager : NetworkManager
{
    [Header("RTS Variables")]
    [SerializeField] private GameObject unitSpawnerPrefab;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab;

    //Occurs after the player is created
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        //Spawn the unit spawner prefab
        GameObject unitSpawnerInstance = Instantiate(
            unitSpawnerPrefab, 
            conn.identity.transform.position, 
            conn.identity.transform.rotation);

        //Spawn gameobject for each client and give owner to prefab
        NetworkServer.Spawn(unitSpawnerInstance, conn);
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
