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
    //private ArrayList gamesList;
    private Socket sock;
    private string server = "localhost:9999";
    private Dictionary<string, GameObject> gamesDict;

    public GameObject gamesPanel;
    //public GameObject availableGamePrefab;

    private class Game
    {
        string gameId;
        string playerName;
    }
    /*
    private class ServerUpdate {
        public string id;
        public List<string> users;
        public string status;
        public List<PlayerMove> moves;
        public string turn;
        public int size;
        public string host_name;
    }
*/
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
        
        client = GameObject.Find("ClientObj").GetComponent<Client>();
        client.connectSocket();
        gamesDict = new Dictionary<string, GameObject>();
        //clearGamesList();
        StartCoroutine(retrieveGameList());
    }

    IEnumerator retrieveGameList()
    {
        sock = SocketFactory.createSocket("localhost", 9999);
        //print("TEst");
        string request = "GET /api/game/list HTTP/1.1\r\nHost: " + server + "\r\n\r\n";
        byte[] byteReq = Encoding.ASCII.GetBytes(request);
        sock.Send(byteReq);


        byte[] dataReceived = new byte[1024];
        int bytes = 0;
        string resp = "";
        //bool hasReceived = false;
        yield return new WaitForSeconds(1.5f);
        while(sock.Available > 0){  
            bytes = sock.Receive(dataReceived);
            resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);
        }
        print(resp);
        resp = parseHTTP(resp);
        List<ServerUpdate> updList = JsonConvert.DeserializeObject<List<ServerUpdate>>(resp);
        for(int i = 0; i < updList.Count; i++)
        {
            print("Game : " + i + "\nID: " + updList[i].id + "\nStatus: " + updList[i].status + "\nTurn: " + updList[i].turn + "\nSize: " + updList[i].size + "\n");
        }
        createGameObjects(updList);
        sock.Close();
        yield return 0;
    }

    private void createGameObjects(List<ServerUpdate> updList)
    {
        //print(updList.Capacity);
        for(int i = 0; i < updList.Count; i++)
        {
            if(updList[i].status == "LOBBY")
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

    public string parseHTTP(string resp){
        int index = resp.IndexOf("\r\n\r\n");
        index += 4;
        return resp.Substring(index);
    }

    public void createGame()
    {
        if (!client.getHasCreatedGame())
        {
            sock = SocketFactory.createSocket("localhost", 9999);
            string body = "----------\r\nContent-Disposition: form-data; name=\"user_id\"\r\n\r\n" + client.getID() + "\r\n----------";
            string req = "PUT /api/game HTTP/1.1\r\nHost: " + server + "\r\nConnection: keep-alive\r\nContent-Length: " + body.Length + "\r\n\r\n" + body;

            byte[] dataToSend = Encoding.ASCII.GetBytes(req);
            sock.Send(dataToSend);
            //StartCoroutine(retrieveGameList());
            client.setHasCreatedGame(true);
            client.setPlayerNum(1);

            string resp = "";
            int bytes = 0;
            byte[] dataReceived = new byte[1024];
            do
            {
                bytes = sock.Receive(dataReceived);
                resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);
            }
            while (sock.Available > 0);

            client.setGameID(getGameID(resp));
            print(resp);
            print("GameID: " + client.getGameID());
            waitingForResponse = true;
        }
        sock.Close();
    }

    /*
    public void joinGame(Text gameID)
    {
        client.joinGame(gameID.text);
    }
    */
    public void clearGamesList()    //For Testing
    {
        string req = "POST /api/clear HTTP/1.1\r\nHost: " + server + "\r\n\r\n";
        byte[] request = Encoding.ASCII.GetBytes(req);
        sock.Send(request);
    }

    public void refreshLobby()
    {
        foreach(string game in gamesDict.Keys){
            Destroy(gamesDict[game]);
        }
        gamesDict.Clear();
        StartCoroutine(retrieveGameList());
    }


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
        if (waitingForResponse)
        {
            joinedResp = "";
            print("waiting");
            while (client.getSock().Available > 0)
            {
                joinedBytes = client.getSock().Receive(joinReceived);
                joinedResp = joinedResp + Encoding.ASCII.GetString(joinReceived, 0, joinedBytes);
                waitingForResponse = false;
            }
            if (!waitingForResponse)
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

    public void cancelGameCreate()
    {
        waitingForResponse = false;
        client.setGameID("");
        string req = "DELETE /api/users?id=" + client.getID() + " HTTP/1.1\r\nHost: " + server + "\r\n\r\n";
        byte[] sockReq = Encoding.ASCII.GetBytes(req);
        sock.Send(sockReq);
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

    public bool checkForGameJoined(List<string> users){
        foreach(string user in users){
            if(user == client.getID()){
                return true;
            }
        }
        return false;
    }
}
