using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    [SyncVar(hook = nameof(HandleHealthUpdated))] //when this value gets updated call the hooked method
    private int currentHealth;

    //Events
    public event Action ServerOnDie;
    public event Action<int, int> ClientOnHealthUpdated; 

    #region Server 

    public override void OnStartServer()
    {
        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;

        currentHealth = maxHealth;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if (currentHealth == 0) { return; }

        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if (currentHealth != 0) { return; }

        ServerOnDie?.Invoke(); //raise death event
    }

    [Server]
    public void ServerHandlePlayerDie(int playerID)
    {
        if ( connectionToClient.connectionId != playerID) { return; }

        DealDamage(currentHealth);
    }

    #endregion


    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth); //raise event for health display
    }

    #endregion

}
