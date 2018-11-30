﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;


public class Client : MonoBehaviour {
    private string playerName;
    private string ID;
    private Socket sock;
    private static string server = "178.128.66.50";
    private int playerNum;
    private bool isTurn; 

    private string joinLobbyHeader = "PUT /api/lobby/join HTTP/1.1\r\nHost: " + server + "\r\nConnection: keep-alive\r\nContent-Length: ";
    private string getGamesListHeader = "GET /api/game/list HTTP/1.1\r\nHost: " + server + "\r\nConnection: keep-alive";
	private void Awake()
	{
        DontDestroyOnLoad(this);
	}

	// Use this for initialization
	void Start () {
        playerName = "";
        ID = "";
        sock = null;
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

    public string getID(){
        return ID;
    }

    public int getPlayerNum()
    {
        return playerNum;
    }

    public void connectSocket(int input){
        Socket sock = SocketFactory.createSocket(server, 80);

        string sockRequest = "";
        byte[] dataToSend = Encoding.ASCII.GetBytes(sockRequest);
        byte[] dataReceived = new byte[1024];


        if(input == 0){         //Use socket to connect to lobby
            string reqBody = "----------\r\nContent-Disposition: form-data; name=\"user_id\"\r\n\r\n" + ID + 
                            "\r\n---------";
            sockRequest = joinLobbyHeader + reqBody.Length + "\r\n\r\n" + reqBody;
        }
        else if(input == 1){    //Use socket to connect to game 
            
        }


        //print(sockRequest);
        sock.Send(dataToSend);

        /*
        int bytes = 0;
        string resp = "Data from server " + server + ":\r\n";

        do
        {
            bytes = sock.Receive(dataReceived);
            resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);
        }
        while (sock.Available > 0);
        //sock.Close();
        */
    }

    public void sendMove(string move){
        byte[] jsonFile = Encoding.ASCII.GetBytes(move);
        sock.Send(jsonFile);
    }

    public string receiveMove(){
        string resp = "";


        return resp;
    }
}
