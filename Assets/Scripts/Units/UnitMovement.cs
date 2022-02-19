using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : NetworkBehaviour
{
    private NavMeshAgent agent = null;
    private Camera mainCamera = null;

    private void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
    }


    //Server run procedures
    #region Server

    [Command]
    private void CmdMove(Vector3 pos)
    {
        //Validate
        if (!NavMesh.SamplePosition(pos, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        agent.SetDestination(hit.position);
    }

#endregion

//Client run procedures
#region Client

    //A start method for only objects this client has authority over
    public override void OnStartAuthority()
    {
        mainCamera = Camera.main;
    }


    [ClientCallback] //Clients only
    private void Update()
    {
        if (!hasAuthority) { return; }
        if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return; }

        CmdMove(hit.point); //Have server move gameobject
    }

    #endregion
}
