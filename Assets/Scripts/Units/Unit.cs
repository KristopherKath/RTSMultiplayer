using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System;

[RequireComponent(typeof(UnitMovement))]
public class Unit : NetworkBehaviour
{
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;

    [SerializeField] private UnitMovement unitMovement = null;
    public UnitMovement GetUnitMovement() => unitMovement;


    //These events are only being called on the server
    public static event Action<Unit> ServerOnUnitSpawned; //using a C# event
    public static event Action<Unit> ServerOnUnitDespawned;

    //Thses events are only being called on the clients
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;


    private void Awake()
    {
        if (!unitMovement)
            unitMovement = gameObject.GetComponent<UnitMovement>();
    }

    #region Server

    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnUnitDespawned?.Invoke(this);
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

    public override void OnStartClient()
    {
        //if client has authority & only a client (no host/server)
        if (!hasAuthority || !isClientOnly) { return; }

        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!hasAuthority || !isClientOnly) { return; }

        AuthorityOnUnitDespawned?.Invoke(this);
    }

    #endregion
}
