using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TMP_Text remainingUnitsText;
    [SerializeField] private Image unitProgressImage;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7f;
    [SerializeField] private float unitSpawnDuration = 5f;


    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;
    private float progressImageVelocity;


    private void Awake()
    {
        if (!health)
            health = GetComponent<Health>();
    }



    private void Update()
    {
        if (isServer)
        {
            ProduceUnits();
        }

        if (isClient)
        {
            UpdateTimerDisplay();
        }
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
        if (queuedUnits == maxUnitQueue) { return; }

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
    
        if (player.GetResources() < unitPrefab.GetResourceCost()) { return; }

        queuedUnits++;

        player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());
    }

    private void ProduceUnits()
    {
        if (queuedUnits == 0) { return; }

        unitTimer += Time.deltaTime;

        if (unitTimer < unitSpawnDuration) { return; }

        //create instance and spawn
        GameObject unitInstance = Instantiate(unitPrefab.gameObject, spawnPoint.position, spawnPoint.rotation); //Server will run this, but not clients

        //The given gameobject will be spawned on each each client. Must be a registered prefab on NetworkManager. 
        //connectionToClient is the client that called this. So that client will be the owner.
        NetworkServer.Spawn(unitInstance, connectionToClient);

        //get random position to move to after spawning
        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = spawnPoint.position.y;

        //move unit
        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(spawnPoint.position + spawnOffset);

        queuedUnits--;
        unitTimer = 0f;
    }

    #endregion

    #region Client

    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;

        if (newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(
                unitProgressImage.fillAmount, 
                newProgress, 
                ref progressImageVelocity, 
                0.1f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!hasAuthority) return;

        CmdSpawnUnit();
    }

    private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
    {
        remainingUnitsText.text = newUnits.ToString();
    }    

    #endregion
}
