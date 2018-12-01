using System.Collections;
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
    private string gameID;

    private bool hasCreatedGame;

    private string joinLobbyHeader = "PUT /api/lobby/join HTTP/1.1\r\nHost: " + server + "\r\nConnection: keep-alive\r\nContent-Length: ";
    private string joinGameHeader = "POST /api/game/join HTTP/1.1\r\nHost: " + server + "\r\nConnection: keep-alive\r\nContent-Length: ";


	private void Awake()
	{
        DontDestroyOnLoad(this);
	}

	// Use this for initialization
	void Start () {
        playerName = "";
        ID = "";
        sock = null;
        hasCreatedGame = false;
        gameID = "";
        sock = SocketFactory.createSocket(server, 80);
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

    public void setPlayerNum(int num)
    {
        playerNum = num;
    }

    public int getPlayerNum()
    {
        return playerNum;
    }

    public void setHasCreatedGame(bool val)
    {
        this.hasCreatedGame = val;
    }

    public bool getHasCreatedGame()
    {
        return hasCreatedGame;
    }

    public void setGameID(string gameID)
    {
        this.gameID = gameID;
    }

    public string getGameID()
    {
        return gameID;
    }

    public Socket getSock()
    {
        return sock;
    }

    public void connectSocket(){

        string sockRequest = "";
        
        byte[] dataReceived = new byte[1024];
        string reqBody = "----------\r\nContent-Disposition: form-data; name=\"user_id\"\r\n\r\n" + ID + 
                            "\r\n---------";
        sockRequest = joinLobbyHeader + reqBody.Length + "\r\n\r\n" + reqBody;
        byte[] dataToSend = Encoding.ASCII.GetBytes(sockRequest);
        sock.Send(dataToSend);

        string resp = "";
        int bytes = 0;
        do
        {
            bytes = sock.Receive(dataReceived);
            resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);
        }
        while (sock.Available > 0);

        print("Lobby Connection: " + resp);

    }

    public void joinGame(string gameID)
    {
        string body = "----------\r\nContent-Disposition: form-data; name=\"user_id\"\r\n\r\n" + ID +
                            "\r\n---------" + "----------\r\nContent-Disposition: form-data; name=\"game_id\"\r\n\r\n" + gameID +
                            "\r\n---------";
        string sockRequest = joinGameHeader + body.Length + "\r\n\r\n" + body;
        byte[] dataToSend = Encoding.ASCII.GetBytes(sockRequest);
        sock.Send(dataToSend);
    }

    public void sendMove(string move){
        byte[] jsonFile = Encoding.ASCII.GetBytes(move);
        sock.Send(jsonFile);
        isTurn = false;
    }

    public string receiveMove(){
        string resp = "";
        int bytes = 0;
        byte[] dataReceived = new byte[1024];
        do
        {
            bytes = sock.Receive(dataReceived);
            resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);
        }
        while (sock.Available > 0);

        return resp;
    }
}
