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
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();
    private Color teamColor = new Color();

    //Events
    public event Action<int> ClientOnResourcesUpdated;


    //Getters
    public List<Unit> GetMyUnits() => myUnits;
    public List<Building> GetMyBuildings() => myBuildings;
    public int GetResources() => resources;
    public Color GetTeamColor() => teamColor;
    public Transform GetCameraTransform() => cameraTransform;


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

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandlerBuildingDespawned;
    
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

    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority) { return; }

        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
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

    #endregion

}
