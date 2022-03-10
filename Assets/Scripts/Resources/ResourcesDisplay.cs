﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText;

    private RTSPlayer player;

    private void Update()
    {
        if (!player)
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if (player)
        {
            ClientHandleResourcesUpdated(player.GetResources());
            player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
        }
    }

    private void OnDestroy()
    {
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    private void ClientHandleResourcesUpdated(int resource)
    {
        resourcesText.text = $"Resources: {resource}";
    }
}