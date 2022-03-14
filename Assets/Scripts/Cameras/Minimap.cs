using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.EventSystems;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform minimapRect;
    [SerializeField] private float mapScale = 20f;
    [SerializeField] private float offset = -6f;

    private Transform playerCameraTransform;

    private void Update()
    {
        if (playerCameraTransform != null) { return; }

        if (NetworkClient.connection.identity == null) { return; }

        playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        print("Clicked down");
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        print("Dragged");
        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        //find mouse position on minimap
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect, 
            mousePos,
            null,
            out Vector2 localPoint
            )) { return; }

        //finds percentage along minimap axis
        Vector2 lerp = new Vector2(
            (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width,
            (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);

        //assuming map is square, it will put camera at that position
        Vector3 newCameraPos = new Vector3(
            Mathf.Lerp(-mapScale, mapScale, lerp.x),
            playerCameraTransform.position.y,
            Mathf.Lerp(-mapScale, mapScale, lerp.y));

        playerCameraTransform.position = newCameraPos + new Vector3(0f, 0f, offset);
    }


}
