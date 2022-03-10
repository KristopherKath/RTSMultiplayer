using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : NetworkBehaviour
{
    [Tooltip("The targeter script to handle targeting calls")]
    [SerializeField] private Targeter targeter = null;
    [Tooltip("The nav mesh agent")]
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private float chaseRange = 10f;

    private void Awake()
    {
        if (!agent)
            agent = gameObject.GetComponent<NavMeshAgent>();
        if (!targeter)
            targeter = gameObject.GetComponent<Targeter>();
    }


    //Server run procedures
    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }


    [ServerCallback] //Only server can call this method, and it wont log warnings to the console
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        //Handle targeting
        if (target != null) 
        { 
            //if out of chase range then chase
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            //stop chasing
            else if (agent.hasPath)
            {
                agent.ResetPath();
            }
            return; 
        }

        //movement logic
        if (!agent.hasPath) { return; } //stops bug where tanks dont move sometimes
        if (agent.remainingDistance > agent.stoppingDistance) { return; }
        agent.ResetPath(); //This will stop tanks from colliding with eachother to reach a location
    }

    [Command]
    public void CmdMove(Vector3 pos)
    {
        if (targeter)
            targeter.ClearTarget();

        //Validate position
        if (!NavMesh.SamplePosition(pos, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        agent.SetDestination(hit.position);
    }

    #endregion
}
