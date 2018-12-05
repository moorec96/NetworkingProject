/**
 * Class: Client
 * Purpose: Contains client data such as ID, socket, etc. 
 * Author: Caleb Moore
 * Date: 12/04/2018
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

public class Client : MonoBehaviour {
    private string playerName;
    private string ID;
    private Socket sock;
    private static string server = "localhost:9999";
    private int playerNum;
    private bool isTurn;
    private string gameID;
    private PlayerMove playerMove;
    private bool hasCreatedGame;
    public bool gameOver;
    public bool youWon;

    private string joinLobbyHeader = "PUT /api/lobby/join HTTP/1.1\r\nHost: " + server + "\r\nConnection: keep-alive\r\nContent-Length: ";
    private string joinGameHeader = "POST /api/game/join HTTP/1.1\r\nHost: " + server + "\r\nConnection: keep-alive\r\nContent-Length: ";

    //Set client object to not be destroyed as we transition through scenes in the game (MainMenu, Lobby, and Game)
	private void Awake()
	{
        DontDestroyOnLoad(this);
	}

	void Start () {
        playerName = "";
        ID = "";
        sock = null;
        hasCreatedGame = false;
        gameID = "";
        playerMove = null;
        isTurn = true;
        gameOver = false;
        youWon = false;
    }

    //Getters and Setters
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

    public void setIsTurn(bool isTurn){
        this.isTurn = isTurn;
    }

    public bool getIsTurn(){
        return isTurn;
    }

    /**
    * Method: connectSocket
    * Purpose: Connect socket to server, and request that the client be added into the lobby
    * Parameters: N/A
    * Return Val: N/A
    * */
    public void connectSocket(){
        sock = SocketFactory.createSocket("localhost", 9999);
        string sockRequest = "";
        
        byte[] dataReceived = new byte[1024];
        string reqBody = "----------\r\nContent-Disposition: form-data; name=\"user_id\"\r\n\r\n" + ID + 
                            "\r\n---------";
        sockRequest = joinLobbyHeader + reqBody.Length + "\r\n\r\n" + reqBody;      //PUT request that adds player into game lobby 

        byte[] dataToSend = Encoding.ASCII.GetBytes(sockRequest);
        sock.Send(dataToSend);                                                      //Send request 

        string resp = "";
        int bytes = 0;
        do
        {
            bytes = sock.Receive(dataReceived);
            resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);         //Wait for confirmation from server that the join was successful
        }
        while (sock.Available > 0);

        print("Lobby Connection: " + resp);

    }

    /**
    * Method: joinGame
    * Purpose: Send join game request to server
    * Parameters: gameID -> Game ID of game to join
    * Return Val: N/A
    * */
    public void joinGame(string gameID)
    {
        Socket tempSock = SocketFactory.createSocket("localhost", 9999);
        this.gameID = gameID;
        string body = "----------\r\nContent-Disposition: form-data; name=\"user_id\"\r\n\r\n" + ID +
                            "\r\n---------" + "----------\r\nContent-Disposition: form-data; name=\"game_id\"\r\n\r\n" + gameID +   //Send POST request to server to join game
                            "\r\n---------";
        string sockRequest = joinGameHeader + body.Length + "\r\n\r\n" + body;
        byte[] dataToSend = Encoding.ASCII.GetBytes(sockRequest);
        tempSock.Send(dataToSend);

        string resp = "";
        int bytes = 0;
        byte[] dataReceived = new byte[1024];
        do
        {
            bytes = tempSock.Receive(dataReceived);                                             //Wait for confirmation response from server that join was successful
            resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);
        }
        while (tempSock.Available > 0);
        print("JOINED GAME: " + resp);
        tempSock.Close();
    }

    /**
    * Method: sendMove
    * Purpose: Send latest game move to server
    * Parameters: move -> string in JSON format to send to server
    * Return Val: N/A
    * */
    public void sendMove(string move){
        byte[] jsonFile = Encoding.ASCII.GetBytes(move);
        sock.Send(jsonFile);                            //Send game move to server
        string resp = "";   
        int bytes = 0;
        byte[] dataReceived = new byte[1024];
        do
        {
            bytes = sock.Receive(dataReceived);
            resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);     //Wait to receive opponent move from server
        }
        while (sock.Available > 0);
        if(resp.IndexOf("GAME_MOVE") > -1){
            PlayerMove playerMove = JsonConvert.DeserializeObject<PlayerMove>(resp);        //Deserialize response into PlayerMove, if game won/lost, set gameWon to true
            gameOver = playerMove.game_won;
            if(gameOver){
                youWon = playerMove.user_id == ID;
            }
        }
        isTurn = false;
        print(resp);
    }
}
