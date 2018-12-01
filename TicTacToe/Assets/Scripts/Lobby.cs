using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

public class Lobby : MonoBehaviour {
    private Client client;
    //private ArrayList gamesList;
    private Socket sock;
    private string server = "178.128.66.50";
    private Dictionary<string, ServerUpdate> gamesDict;

    public GameObject gamesPanel;
    //public GameObject availableGamePrefab;

    private class Game
    {
        string gameId;
        string playerName;
    }

    private class ServerUpdate{
        public string id;
        public List<string> users;
        public string status;
        public List<PlayerMove> moves;
        public string turn;
        public int size; 
    }
        
	void Start () {
        
        client = GameObject.Find("ClientObj").GetComponent<Client>();
        sock = SocketFactory.createSocket(server, 80);
        gamesDict = new Dictionary<string, ServerUpdate>();
        //clearGamesList();
        StartCoroutine(retrieveGameList());
    }

    IEnumerator retrieveGameList()
    {
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
        yield return 0;
    }

    private void createGameObjects(List<ServerUpdate> updList)
    {
        //print(updList.Capacity);
        for(int i = 0; i < updList.Count; i++)
        {
            if(updList[i].status == "LOBBY")
            {
                gamesDict.Add(updList[i].users[0], updList[i]);
                GameObject game = (GameObject)Instantiate(Resources.Load("AvailableGame"));
                game.transform.SetParent(gamesPanel.transform,false);
                AvailableGame aGame = game.GetComponent<AvailableGame>();
                aGame.setGameIDText(updList[i].id);
                aGame.setHostNameText(updList[i].users[0]);
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
            string body = "----------\r\nContent-Disposition: form-data; name=\"user_id\"\r\n\r\n" + client.getID() + "\r\n----------";
            string req = "PUT /api/game HTTP/1.1\r\nHost: " + server + "\r\nConnection: keep-alive\r\nContent-Length: " + body.Length + "\r\n\r\n" + body;

            byte[] dataToSend = Encoding.ASCII.GetBytes(req);
            sock.Send(dataToSend);
            StartCoroutine(retrieveGameList());
            client.setHasCreatedGame(true);
        }
    }

    public void joinGame(Text gameID)
    {
        client.connectSocket(1, gameID.text);
    }

    public void clearGamesList()    //For Testing
    {
        string req = "POST /api/clear HTTP/1.1\r\nHost: " + server + "\r\n\r\n";
        byte[] request = Encoding.ASCII.GetBytes(req);
        sock.Send(request);
    }
}
