using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private float buildingRangeLimit = 5f;
    [SerializeField] private Building[] buildings = new Building[0];

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();
    private Color teamColor = new Color();
    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string displayName;

    //Events
    public event Action<int> ClientOnResourcesUpdated;

    public static event Action ClientOnInfoUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;



    //Getters
    public bool GetIsPartyOwner => isPartyOwner;
    public List<Unit> GetMyUnits() => myUnits;
    public List<Building> GetMyBuildings() => myBuildings;
    public int GetResources() => resources;
    public Color GetTeamColor() => teamColor;
    public Transform GetCameraTransform() => cameraTransform;
    public string GetDisplayName() => displayName;


    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 pos)
    {
        //check if we are overlapping anything in layer
        if (Physics.CheckBox(
            pos + buildingCollider.center,
            buildingCollider.size / 2,
            Quaternion.identity,
            buildingBlockLayer))
        {
            return false;
        }

        //check if we are in range of another building
        foreach (Building building in myBuildings)
        {
            if ((pos - building.transform.position).sqrMagnitude
                <= buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }
        return false;
    }



    #region Server

    [Server]
    public void SetPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandlerBuildingDespawned;
        
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandlerBuildingDespawned;
    }


    //Setters
    [Server]
    public void SetResources(int newResource) => resources = newResource;

    [Server]
    public void SetTeamColor(Color newColor) => teamColor = newColor;

    [Server]
    public void SetDisplayName(string displayName) => this.displayName = displayName;



    private void ServerHandleUnitSpawned(Unit unit)
    {
        //If the person that owns the unit is the same that owns this player
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Add(unit);
    }
    private void ServerHandleUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Remove(unit);
    }

    private void ServerHandleBuildingSpawned(Building building)
    {
        //If the person that owns the building is the same that owns this player
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Add(building);
    }
    private void ServerHandlerBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Remove(building);
    }

    [Command]
    public void CmdStartGame()
    {
        if (!isPartyOwner) { return; }
        
        ((RTSNetworkingManager)NetworkManager.singleton).StartGame();
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingID, Vector3 pos)
    {
        //find building to spawn
        Building buildingToPlace = null;
        foreach (Building building in buildings)
        {
            if (building.GetID() == buildingID)
            {
                buildingToPlace = building;
                break;
            }
        }
    
        if (!buildingToPlace) { return; }


        //check if we have enough money
        if (resources < buildingToPlace.GetPrice()) { return; }


        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();
        if (!CanPlaceBuilding(buildingCollider, pos)) { return; }


        //spawn building on spot and network server spawn it
        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, pos, buildingToPlace.transform.rotation);
        NetworkServer.Spawn(buildingInstance, connectionToClient);

        SetResources(resources - buildingToPlace.GetPrice());
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        //We dont want host. They already have server version
        if (NetworkServer.active) { return; }

        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStartClient()
    {
        if (NetworkServer.active) { return; } //stops host from adding twice

       ((RTSNetworkingManager)NetworkManager.singleton).players.Add(this);

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopClient()
    {
        ClientOnInfoUpdated?.Invoke();

        if (!isClientOnly) { return; }

        ((RTSNetworkingManager)NetworkManager.singleton).players.Remove(this); //store list of all players

        if (!hasAuthority) return; //stop from running on each client

        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if (!hasAuthority) { return; }

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }


    private void ClientHandleResourcesUpdated(int oldResource, int newResource)
    {
        ClientOnResourcesUpdated?.Invoke(newResource);
    }

    private void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    #endregion

}
