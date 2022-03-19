using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];


    private void Start()
    {
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        RTSNetworkingManager.ClientOnConnected += HandleClientConnected;
    }

    private void OnDestroy()
    {
        RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        RTSNetworkingManager.ClientOnConnected -= HandleClientConnected;
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        startGameButton.gameObject.SetActive(state);
    }

    public void StartGame()
    { 
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame(); 
    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    private void ClientHandleInfoUpdated()
    {
        List<RTSPlayer> players = ((RTSNetworkingManager)NetworkManager.singleton).players;

        for (int i = 0; i < players.Count; i++)
        {
            playerNameTexts[i].text = players[i].GetDisplayName();
        }

        for (int i = players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting for Player...";
        }

        startGameButton.interactable = players.Count >= 2;
    }

    public void LeaveLobby()
    {
        //is a host
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(0);
        }
    }
}
