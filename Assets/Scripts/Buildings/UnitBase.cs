using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

[RequireComponent(typeof(Health))]
class UnitBase : NetworkBehaviour
{
    [SerializeField] private Health health;

    public static event Action<int> ServerOnPlayerDie;

    public static event Action<UnitBase> ServerOnBaseSpawned;
    public static event Action<UnitBase> ServerOnBaseDespawned;

    private void Awake()
    {
        if (!health)
            health = GetComponent<Health>();
    }


    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleOnDie;

        ServerOnBaseSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleOnDie;

        ServerOnBaseDespawned?.Invoke(this);
    }

    [Server]
    private void ServerHandleOnDie()
    {
        ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);

        NetworkServer.Destroy(gameObject);
    }

    #endregion


    #region Client 


    #endregion


}
