using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform spawnPoint;






    #region Server

    [Command]
    private void CmdSpawnUnit()
    {
        GameObject unitInstance = Instantiate(unitPrefab, spawnPoint.position, spawnPoint.rotation); //Server will run this, but not clients

        //The given gameobject will be spawned on each each client. Must be a registered prefab on NetworkManager. 
            //connectionToClient is the client that called this. So that client will be the owner.
        NetworkServer.Spawn(unitInstance, connectionToClient); 
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!hasAuthority) return;

        CmdSpawnUnit();
    }

    #endregion
}
