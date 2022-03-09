using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private LayerMask floorMask = new LayerMask(); //for checking if we place on floor

    private Camera mainCamera;
    private RTSPlayer player;
    private GameObject buildingPreviewInsance;
    private Renderer buildingRendererInstance;


    private void Start()
    {
        mainCamera = Camera.main;

        iconImage.sprite = building.GetIcon();
        priceText.text = building.GetPrice().ToString();
    }

    private void Update()
    {
        if (!player)
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if (!buildingPreviewInsance) { return; }

        UpdateBuildingPreview();
    }

    //create preview instance
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        buildingPreviewInsance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = buildingPreviewInsance.GetComponentInChildren<Renderer>();

        buildingPreviewInsance.SetActive(false);
    }

    //place building if in valid spot
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!buildingPreviewInsance) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            //place building
        }

        Destroy(buildingPreviewInsance);
    }

    private void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }
        buildingPreviewInsance.transform.position = hit.point;

        if (!buildingPreviewInsance.activeSelf)
        {
            buildingPreviewInsance.SetActive(true);
        }
    }

}
