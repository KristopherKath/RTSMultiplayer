﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Targeter : NetworkBehaviour
{
    private Targetable target;

    public Targetable GetTarget() => target;

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        if (!targetGameObject.TryGetComponent<Targetable>(out Targetable target)) { return; }

        this.target = target;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }

    private void ServerHandleGameOver()
    {
        ClearTarget();
    }

    #endregion
}
