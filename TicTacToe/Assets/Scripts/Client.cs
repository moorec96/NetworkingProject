using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour {

    private string playerName;
    private string ID;

	// Use this for initialization
	void Start () {
        playerName = "";
        ID = "";
	}
	
    public void setPlayerName(string playerName)
    {
        this.playerName = playerName;
    }

    public string getPlayerName()
    {
        return playerName;
    }

    public void setID(string ID)
    {
        this.ID = ID;
    }
}
