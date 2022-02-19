using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RTSNetworkingManager : NetworkManager
{
    [SerializeField] GameObject unitSpawnerPrefab;

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
}
