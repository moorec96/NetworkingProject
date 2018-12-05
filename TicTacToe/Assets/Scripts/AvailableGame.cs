/**
 * Class: AvailableGame
 * Purpose: This class contains one available game for the player to join.
 * Author: Caleb Moore
 * Date: 12/4/2018
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class AvailableGame : MonoBehaviour{
    private string hostName;    //Name of game creator
    private string gameID;      //game identification number
    public Text gameIDText;     //String to be displayed on screen
    public Text hostNameText;   //String of host's name
    public Button joinBtn;      //Reference to join button
    public Client client;       //Client that can join the game

    public void Start()
    {
        client = client = GameObject.Find("ClientObj").GetComponent<Client>();  //Find client object that passes from scene to scene
        joinBtn.onClick.AddListener(joinGame);                                  //Add listener to listen for join button click
    }

    /**
     * Method: JoinGame
     * Purpose: Have client send join game request to server, and then load the game scene
     * Parameters: N/A
     * Return Val: N/A
     * */
    public void joinGame()
    {
        client.joinGame(gameID);
        SceneManager.LoadScene("Game");
    }

    /**
     * Method: getGameID
     * Purpose: return game ID
     * Parameters: N/A
     * Return Val: gameID
     * */
    public string getGameID()
    {
        return gameID;
    }

    /**
     * Method: setGameIDText
     * Purpose: Take in a string and set the gameID text to it 
     * Parameters: text -> gameID string
     * Return Val: N/A
     * */
    public void setGameIDText(string text)
    {
        gameID = text;
        gameIDText.text = "GameID: " + text;
    }

    /**
     * Method: getHostName
     * Purpose: return host name
     * Parameters: N/A
     * Return Val: hostName
     * */
    public string getHostName()
    {
        return hostName;
    }

    /**
     * Method: setHostNameText
     * Purpose: Take in a string and set the hostName text to it
     * Parameters: text -> hostName string
     * Return Val: N/A
     * */
    public void setHostNameText(string text)
    {
        hostName = hostName;
        hostNameText.text = "HostName: " + text;
    }

}
