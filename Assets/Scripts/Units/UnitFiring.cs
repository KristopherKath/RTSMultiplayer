using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Targeter))]
public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 15f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 15f;

    private float lastFireTime;
    private Targetable target;

    private void Awake()
    {
        if (targeter == null)
            targeter = gameObject.GetComponent<Targeter>();
    }

    [ServerCallback]
    private void Update()
    {
        target = targeter.GetTarget();

        if (target == null) { return; }

        //Check if we are in range
        if (!CanFireAtTarget()) { return; }

        //get how much to rotate
        Quaternion targetRotation = Quaternion.LookRotation(
            target.transform.position - transform.position);

        //rotate
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime); 

        //fire
        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            //Get the rotation for the projectile to the target point
            Quaternion projectileRotation = Quaternion.LookRotation(
                target.GetAimAtPoint().position - projectileSpawnPoint.position);

            GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation); //spawns on the server
            NetworkServer.Spawn(projectileInstance, connectionToClient); //spawn on the network. Client that owns this script owns this projectile

            lastFireTime = Time.time;
        }
    }

    [Server] //if a client calls this then we will get a warning. This means we did something wrong
    private bool CanFireAtTarget()
    {
        return (target.transform.position - transform.position).sqrMagnitude < fireRange * fireRange;
    }
}
