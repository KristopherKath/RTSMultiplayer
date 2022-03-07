using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UnitSelectionHandler))]
public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    private Camera mainCamera;

    private void Awake()
    {
        if (!unitSelectionHandler)
            unitSelectionHandler = gameObject.GetComponent<UnitSelectionHandler>();

        mainCamera = Camera.main;

        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {
        TryMovementCommand();
    }



    private void TryMovementCommand()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            //If targetable object is ours then move to it
            if (target.hasAuthority)
            {
                TryMove(hit.point);
                return;
            }

            //Attack target
            TryTarget(target);
            return;
        }

        TryMove(hit.point);
    }

    private void TryMove(Vector3 point)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement().CmdMove(point);
        }
    }

    private void TryTarget(Targetable target)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter().CmdSetTarget(target.gameObject);
        }
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
}
