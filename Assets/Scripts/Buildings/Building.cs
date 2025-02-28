﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Building : NetworkBehaviour
{
    [SerializeField] private GameObject buildingPreview;
    [SerializeField] private Sprite icon;
    [SerializeField] private int id = -1;
    [SerializeField] private int price = 100;

    //Events
    public static event Action<Building> ServerOnBuildingSpawned;
    public static event Action<Building> ServerOnBuildingDespawned;
    public static event Action<Building> AuthorityOnBuildingSpawned;
    public static event Action<Building> AuthorityOnBuildingDespawned;

    //Getters
    public Sprite GetIcon() => icon;
    public int GetID() => id;
    public int GetPrice() => price;
    public GameObject GetBuildingPreview() => buildingPreview;

    #region Server

    public override void OnStartServer()
    {
        ServerOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBuildingDespawned?.Invoke(this);
    }

    #endregion


    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        //if client has authority & only a client (no host/server)
        if (!hasAuthority) { return; }

        AuthorityOnBuildingDespawned?.Invoke(this);
    }

    #endregion
}
