using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;

public class SocketFactory
{
    public static Socket createSocket(string server, int port){
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
        return sock != null ? sock : null;
    }
}
