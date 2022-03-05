using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System;

[RequireComponent(typeof(UnitMovement))]
[RequireComponent(typeof(Targeter))]
[RequireComponent(typeof(Health))]
public class Unit : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private Health health = null;   

    [Header("Events")]
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;

    public UnitMovement GetUnitMovement() => unitMovement;
    public Targeter GetTargeter() => targeter;

    //These events are only being called on the server
    public static event Action<Unit> ServerOnUnitSpawned; //using a C# event
    public static event Action<Unit> ServerOnUnitDespawned;

    //Thses events are only being called on the clients
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;


    private void Awake()
    {
        if (!health)
            health = GetComponent<Health>();

        if (!unitMovement)
            unitMovement = gameObject.GetComponent<UnitMovement>();

        if (!targeter)
            targeter = gameObject.GetComponent<Targeter>();
    }

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleOnDie;

        ServerOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleOnDie;

        ServerOnUnitDespawned?.Invoke(this);
    }

    [Server]
    private void ServerHandleOnDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion


    #region Client
    [Client]
    public void Select()
    {
        if (!hasAuthority) { return; }

        onSelected?.Invoke(); //? is a saftey check if it is null
    }

    [Client]
    public void Deselect()
    {
        if (!hasAuthority) { return; }

        onDeselected?.Invoke(); 
    }

    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        //if client has authority & only a client (no host/server)
        if (!hasAuthority) { return; }

        AuthorityOnUnitDespawned?.Invoke(this);
    }

    #endregion
}
