using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Text;
using System.Net;

public class Registration : MonoBehaviour {

    public Text playerName;
    public Client client;
    private static Socket sock;
    public GameObject regPanel;
	// Use this for initialization
	void Start () {
		
	}
	
    public void registerPlayer()
    {
        client.setPlayerName(playerName.text);
        string resp = connectSocket("localhost:9999", 9999);
        int index = resp.IndexOf("\r\n\r\n");
        index += 4;
        string id = "";
        for (int i = index; i < resp.Length; i++)
        {
            id += resp[i];
        }
                                 
        client.setID(id);
        print(id);
        this.gameObject.SetActive(false);
    }

    private string connectSocket(string server, int port)
    {
        string body = "----------\r\nContent-Disposition: form-data; name=\"name\"\r\n\r\n" + client.getPlayerName() + "\r\n---------";
        string sockRequest = "PUT /api/users HTTP/1.1\r\nHost: " + server + "\r\nConnection: keep-alive\r\nContent-Length: " + body.Length + "\r\n\r\n" + body;
        //print(sockRequest);
        byte[] dataToSend =  Encoding.ASCII.GetBytes(sockRequest);
        byte[] dataReceived = new byte[1024];

        Socket sock = SocketFactory.createSocket("localhost", 9999);
        
        sock.Send(dataToSend);

        int bytes = 0;
        string resp = "Data from server " + server + ":\r\n";

        do
        {
            bytes = sock.Receive(dataReceived);
            resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);
        }
        while (sock.Available > 0);
        sock.Close();
        return resp;
    }

}
