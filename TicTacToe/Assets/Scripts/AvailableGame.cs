using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class AvailableGame : MonoBehaviour{
    private string hostName;
    private string gameID;
    public Text gameIDText;
    public Text hostNameText;
    public Button joinBtn;
    public Client client;

    public void Start()
    {
        client = client = GameObject.Find("ClientObj").GetComponent<Client>();
        joinBtn.onClick.AddListener(joinGame);
    }

    public void joinGame()
    {
        client.joinGame(gameID);
        SceneManager.LoadScene("Game");
    }

    public string getGameID()
    {
        return gameID;
    }
    public void setGameIDText(string text)
    {
        gameID = text;
        gameIDText.text = "GameID: " + text;
    }

    public string getHostName()
    {
        return hostName;
    }
    public void setHostNameText(string text)
    {
        hostName = hostName;
        hostNameText.text = "HostName: " + text;
    }

}
