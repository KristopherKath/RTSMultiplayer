using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

//this is all client side
public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private GameObject gameOverDisplayParent;
    [SerializeField] private TMP_Text winnerNameText = null;


    private void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    public void LeaveGame()
    {
        //if host
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            //stop hosting
            NetworkManager.singleton.StopHost();
        }
        //if client
        else
        {
            //stop client
            NetworkManager.singleton.StopClient();
        }
    }

    private void ClientHandleGameOver(string winner)
    {
        winnerNameText.text = $"{winner} Has Won!";
        gameOverDisplayParent.SetActive(true);
    }
}

