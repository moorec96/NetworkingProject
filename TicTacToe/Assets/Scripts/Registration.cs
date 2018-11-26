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
	// Use this for initialization
	void Start () {
		
	}
	
    public void registerPlayer()
    {
        client.setPlayerName(playerName.text);
        print(connectSocket("8a2e7691.ngrok.io", 80));

    }

    private string connectSocket(string server, int port)
    {
        string body = "----------\r\nContent-Disposition: form-data; name=\"name\"\r\n" + client.getPlayerName() + "\r\n---------";
        print(body);
        string sockRequest = "PUT /api/users HTTP/1.1\r\nHost: " + server + "\r\nConnection: keep-alive\r\n\r\n" + body;
                            
        byte[] dataToSend =  Encoding.ASCII.GetBytes(sockRequest);
        byte[] dataReceived = new byte[256];

        Socket sock = null;
        IPHostEntry host = Dns.GetHostEntry(server);
        foreach (IPAddress addr in host.AddressList)
        { 
            IPEndPoint ipe = new IPEndPoint(addr, port);
            Socket tempSock = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            tempSock.Connect(ipe);
            if (tempSock.Connected)
            {
                sock = tempSock;
                break;
            }
        }

        if(sock == null)
        {
            return "FAILED CONNECTION";
        }

        
        sock.Send(dataToSend, dataToSend.Length, 0);

        int bytes = 0;
        string page = "Data from server " + server + ":\r\n";

        do
        {
            bytes = sock.Receive(dataReceived, dataReceived.Length, 0);
            page = page + Encoding.ASCII.GetString(dataReceived, 0, bytes);
        }
        while (bytes > 0);
        
        return page;
    }

}
