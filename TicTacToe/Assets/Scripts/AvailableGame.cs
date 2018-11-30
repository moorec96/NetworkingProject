using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AvailableGame : MonoBehaviour{
    private string hostName;
    private string gameID;
    public Text gameIDText;
    public Text hostNameText;

    public string getGameID()
    {
        return gameID;
    }
    public void setGameIDText(string text)
    {
        gameID = text;
        gameIDText.text = text;
    }

    public string getHostName()
    {
        return hostName;
    }
    public void setHostNameText(string text)
    {
        hostName = hostName;
        hostNameText.text = text;
    }

}
