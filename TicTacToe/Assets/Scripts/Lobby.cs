using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

public class Lobby : MonoBehaviour {
    // Use this for initialization
    private Client client;
    private ArrayList gamesList;
    private Socket sock;
    private string server = "178.128.66.50";

    private class AvailableGame
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

        public ServerUpdate(){
        }
    }
        
	void Start () {
        client = GameObject.Find("ClientObj").GetComponent<Client>();
        sock = SocketFactory.createSocket(server, 80);
        StartCoroutine(retrieveGameList());
	}

    IEnumerator retrieveGameList()
    {
        string request = "GET /api/game/list HTTP/1.1\r\nHost: " + server + "\r\n\r\n";
        //string body = "----------\r\nContent-Disposition: form-data; name=\"name\"\r\n\r\n" + client.getPlayerName() + "\r\n---------";
        //string sockRequest = "PUT /api/users HTTP/1.1\r\nHost: " + server + "\r\nConnection: keep-alive\r\nContent-Length: " + body.Length + "\r\n\r\n" + body;
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
        //ServerUpdate upd = JsonConvert.DeserializeObject<ServerUpdate>(resp);
        //print("Server Update:\nType: " + upd.type + "\nSize: " + upd.size + "\nUser Name: " + upd.user_name + "\nID: " + upd.id + "\n");

        //print(resp);
        yield return 0;
    }

    public string parseHTTP(string resp){
        int index = resp.IndexOf("\r\n\r\n");
        index += 4;
        return resp.Substring(index);
    }
}
