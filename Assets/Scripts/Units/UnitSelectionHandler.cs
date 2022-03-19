using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class UnitSelectionHandler : MonoBehaviour
{
    public List<Unit> SelectedUnits { get; } = new List<Unit>();


    [SerializeField] private RectTransform unitSelectionArea; 
    [SerializeField] private LayerMask layerMask = new LayerMask();


    private Vector2 startPos; //the start pos where mouse first clicks
    private RTSPlayer player;
    private Camera mainCamera;


    private void Start()
    {
        mainCamera = Camera.main;

        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void Update()
    {
        HandleSelection();
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void HandleSelection()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            FinishSelectionArea();
        }
    }

    private void StartSelectionArea()
    {
        //Shift key will append to current list
        if (!Keyboard.current.leftShiftKey.isPressed)
            DeselectUnits();

        unitSelectionArea.gameObject.SetActive(true);

        startPos = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    private void DeselectUnits()
    {
        foreach (Unit selectedUnit in SelectedUnits)
        {
            selectedUnit.Deselect();
        }
        SelectedUnits.Clear();
    }

    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPos.x;
        float areaHeight = mousePosition.y - startPos.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight)); 
        unitSelectionArea.anchoredPosition = startPos + new Vector2(areaWidth / 2, areaHeight / 2); //anchor should be in the middle of the rectangle
    }

    // Check for units in the selection area and raise selected event
    private void FinishSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        //Raycast for single units
        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; } //Check if we hit something

            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) { return; } //check if it is a unit

            if (!unit.hasAuthority) { return; } //check if unit is ours

            SelectedUnits.Add(unit);

            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select(); //Raise select event
            }

            return;
        }

        //Raycast for multi-select
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach(Unit unit in player.GetMyUnits())
        {
            if (SelectedUnits.Contains(unit)) { continue; } //Dont add units already in list

            Vector3 unitScreenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
            
            if (unitScreenPosition.x > min.x && unitScreenPosition.x < max.x &&
                unitScreenPosition.y > min.y && unitScreenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
}
