using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

[RequireComponent(typeof(UnitMovement))]
public class Unit : NetworkBehaviour
{
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;

    private UnitMovement unitMovement = null;
    
    public UnitMovement GetUnitMovement() => unitMovement;

    private void Awake()
    {
        unitMovement = gameObject.GetComponent<UnitMovement>();
    }


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
    #endregion
}
