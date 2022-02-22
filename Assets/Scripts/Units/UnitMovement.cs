using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : NetworkBehaviour
{
    private NavMeshAgent agent = null;

    private void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
    }


    //Server run procedures
    #region Server

    [Command]
    public void CmdMove(Vector3 pos)
    {
        //Validate
        if (!NavMesh.SamplePosition(pos, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        agent.SetDestination(hit.position);
    }

    #endregion
}
