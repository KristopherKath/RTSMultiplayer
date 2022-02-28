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

    
    [ServerCallback] //Only server can call this method, and it wont log warnings to the console
    private void Update()
    {
        if (!agent.hasPath) { return; } //stops bug where tanks dont move sometimes

        if (agent.remainingDistance > agent.stoppingDistance) { return; }

        agent.ResetPath(); //This will stop tanks from colliding with eachother to reach a location
    }

    [Command]
    public void CmdMove(Vector3 pos)
    {
        //Validate
        if (!NavMesh.SamplePosition(pos, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        agent.SetDestination(hit.position);
    }

    #endregion
}
