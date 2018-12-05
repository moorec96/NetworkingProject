/**
 * Class: Socket Factory
 * Purpose: Create sockets 
 * Author: Caleb Moore
 * Date: 12/4/2018
 * */

using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;

public class SocketFactory
{
    /**
     * Method: createSocket
     * Purpose: Create a socket and return it 
     * Parameters: server -> host name to connect socket to, port -> port number to bind to socket
     * Return Val: Socket object
     * */
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
