/**
 * Class: Lobby
 * Purpose: Handle players creating and joining games
 * Author: Caleb Moore
 * Date: 12/4/2018
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour {
    private Client client;
    private Socket sock;
    private string server = "localhost:9999";
    private Dictionary<string, GameObject> gamesDict;

    public GameObject gamesPanel;
    public GameObject loadingPanel;

    //Private class that can be used for deserializing joined game updates that are in JSON format
    private class JoinedGameUdpate{
        public string type;
	    public string game_id;
	    public List<string> users;
    }

    private bool waitingForResponse = false;
    private string joinedResp = "";
    private int joinedBytes = 0;
    private byte[] joinReceived = new byte[1024];

    
    void Start () {
        loadingPanel.SetActive(true);                                       //Display loading panel
        client = GameObject.Find("ClientObj").GetComponent<Client>();       //Obtain reference to client object
        client.connectSocket();                                             //Connect client socket to the lobby
        client.setHasCreatedGame(false);                                    //Reset client created game variable so that the client can create a new game if they want to
        gamesDict = new Dictionary<string, GameObject>();                   //Dictionary that contains each game in lobby
        //clearGamesList();
        StartCoroutine(retrieveGameList());                                 //Retrieve current games list
    }

    /**
     * Method: retrieveGameList
     * Purpose: Asks the server for all the current games waiting to start in the lobby
     * Parameters: N/A
     * Return Val: N/A
     * */
    IEnumerator retrieveGameList()
    {
        sock = SocketFactory.createSocket("localhost", 9999);           //Connects socket to server
        string request = "GET /api/game/list HTTP/1.1\r\nHost: " + server + "\r\n\r\n"; //GET request to server that asks for current games list
        byte[] byteReq = Encoding.ASCII.GetBytes(request);
        sock.Send(byteReq);                                     


        byte[] dataReceived = new byte[1024];
        int bytes = 0;
        string resp = "";
        yield return new WaitForSeconds(1.5f);
        while(sock.Available > 0){                                                      //Waits for response from server 
            bytes = sock.Receive(dataReceived);
            resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);
        }
        print(resp);
        resp = parseHTTP(resp);
        List<ServerUpdate> updList = JsonConvert.DeserializeObject<List<ServerUpdate>>(resp);   //Deserializes response from server into a list of ServerUpdates
        for(int i = 0; i < updList.Count; i++)
        {
            print("Game : " + i + "\nID: " + updList[i].id + "\nStatus: " + updList[i].status + "\nTurn: " + updList[i].turn + "\nSize: " + updList[i].size + "\n");
        }
        createGameObjects(updList);                 //Create game objects for each game
        sock.Close();                           
        loadingPanel.SetActive(false);              //Turn off loading screen panel
        yield return 0;
    }

    /**
     * Method: createGameObjects
     * Purpose: Create Individual Game objects for the user to see in the lobby
     * Parameters: updList -> List of games in lobby
     * Return Val: N/A
     * */
    private void createGameObjects(List<ServerUpdate> updList)
    {
        //print(updList.Capacity);
        for(int i = 0; i < updList.Count; i++)
        {
            if(updList[i].status == "LOBBY")        //Loops through list, creating a GameObject for each game in lobby
            {   
                
                GameObject game = (GameObject)Instantiate(Resources.Load("AvailableGame"));
                gamesDict.Add(updList[i].users[0], game);
                game.transform.SetParent(gamesPanel.transform,false);
                AvailableGame aGame = game.GetComponent<AvailableGame>();
                aGame.setGameIDText(updList[i].id);
                aGame.setHostNameText(updList[i].host_name); 
            }
        }
    }

    /**
     * Method: parseHTTP 
     * Purpose: Find data section in HTTP response
     * Parameters: resp -> response from server
     * Return Val: string -> data of response
     * */
    public string parseHTTP(string resp){
        int index = resp.IndexOf("\r\n\r\n");
        index += 4;
        return resp.Substring(index);
    }

    /**
     * Method: createGame
     * Purpose: Tell the server the client wants to create a game
     * Parameters: N/A
     * Return Val: N/A
     * */
    public void createGame()
    {
        if (!client.getHasCreatedGame())        //If the client has not created a game already, allow them to create a game 
        {
            sock = SocketFactory.createSocket("localhost", 9999);       //Connect socket to the server
            string body = "----------\r\nContent-Disposition: form-data; name=\"user_id\"\r\n\r\n" + client.getID() + "\r\n----------";             
            string req = "PUT /api/game HTTP/1.1\r\nHost: " + server + "\r\nConnection: keep-alive\r\nContent-Length: " + body.Length + "\r\n\r\n" + body;  //PUT request to add game to server

            byte[] dataToSend = Encoding.ASCII.GetBytes(req);
            sock.Send(dataToSend);
            //StartCoroutine(retrieveGameList());
            client.setHasCreatedGame(true);
            client.setPlayerNum(1);             //Set player num to 1 which makes them the player to start play in the game

            string resp = "";
            int bytes = 0;
            byte[] dataReceived = new byte[1024];
            do
            {
                bytes = sock.Receive(dataReceived);
                resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);     //Receive gameID of newly created game
            }
            while (sock.Available > 0);

            client.setGameID(getGameID(resp));              //Set GameID in client class
            print(resp);
            print("GameID: " + client.getGameID());
            waitingForResponse = true;
        }
        sock.Close();
    }

    /**
     * Method: clearGamesList
     * Purpose: Tell the server to erase games list 
     * Parameters: N/A
     * Return Val: N/A
     * */
    public void clearGamesList()    //For Testing
    {
        string req = "POST /api/clear HTTP/1.1\r\nHost: " + server + "\r\n\r\n";
        byte[] request = Encoding.ASCII.GetBytes(req);
        sock.Send(request);
    }

    /**
     * Method: refreshLobby
     * Purpose: Delete all gameobjects in lobby, and call retrieveGameList again
     * Parameters: N/A
     * Return Val: N/A
     * */
    public void refreshLobby()
    {
        foreach(string game in gamesDict.Keys){
            Destroy(gamesDict[game]);
        }
        gamesDict.Clear();
        StartCoroutine(retrieveGameList());
    }

    /**
     * Method: getGameID
     * Purpose: Get gameID out of HTTP response
     * Parameters: jsonFile -> JSON response from server
     * Return Val: string -> gameID
     * */
    public string getGameID(string jsonFile)
    {
        int index = jsonFile.IndexOf("\r\n\r\n");
        index += 4;
        string id = "";
        for (int i = index; i < jsonFile.Length; i++)
        {
            id += jsonFile[i];
        }
        return id;
    }

    public void Update()
    {
        if (waitingForResponse)                     //Waiting for someone to join created game
        {
            joinedResp = "";
            print("waiting");
            while (client.getSock().Available > 0)
            {
                joinedBytes = client.getSock().Receive(joinReceived);
                joinedResp = joinedResp + Encoding.ASCII.GetString(joinReceived, 0, joinedBytes);
                waitingForResponse = false;
            }
            if (!waitingForResponse)                        //If someone has joined the game, Load the GAME
            {
                print(joinedResp);
                JoinedGameUdpate gameJoined = JsonConvert.DeserializeObject<JoinedGameUdpate>(joinedResp);
                switch(gameJoined.type){
                    case "GAME_JOINED":
                        if(checkForGameJoined(gameJoined.users)){
                            SceneManager.LoadScene("Game");
                        }
                        break;
                    case "GAME_CREATED":
                        print("NEW GAME");
                        waitingForResponse = true;
                        break;
                    default:
                        print("I dont even know");
                        break;
                }
                //print(gameJoined.game_id);
            }
        }
    }

    /**
     * Method: cancelGameCreate
     * Purpose: Allow player to exit out of their created game, and delete game from server
     * Parameters: N/A
     * Return Val: N/A
     * */
    public void cancelGameCreate()
    {
        waitingForResponse = false;
        client.setGameID("");
        string req = "DELETE /api/users?id=" + client.getID() + " HTTP/1.1\r\nHost: " + server + "\r\n\r\n";
        byte[] sockReq = Encoding.ASCII.GetBytes(req);
        sock.Send(sockReq);                                         //Send Delete game request to server
        gamesDict.Remove(client.getID());
        string resp = "";
        int bytes = 0;
        byte[] dataReceived = new byte[1024];
        do
        {
            bytes = sock.Receive(dataReceived);
            resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);
        }
        while (sock.Available > 0);
    }

    /**
     * Method: checkForGameJoined
     * Purpose: When a player joins a game, a broadcast is sent out of all joined games, this returns true if the game joined is the local clients game
     * Parameters: users -> list of user ID's that are hosts of created games
     * Return Val: bool -> true if the local clients game has been joined
     * */
    public bool checkForGameJoined(List<string> users){
        foreach(string user in users){
            if(user == client.getID()){
                return true;
            }
        }
        return false;
    }
}
