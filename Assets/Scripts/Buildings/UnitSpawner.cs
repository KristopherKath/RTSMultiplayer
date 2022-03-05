using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Health))]
public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform spawnPoint;

    private void Awake()
    {
        if (!health)
            health = GetComponent<Health>();
    }


    #region Server


    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleOnDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleOnDie;
    }

    [Server]
    private void ServerHandleOnDie()
    {
        NetworkServer.Destroy(gameObject);
    }


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
