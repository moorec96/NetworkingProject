using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Grid : MonoBehaviour {

    public static char[,] grid = new char[3,3];
    public Text[] buttonText;
    private Client client;
    public Button[] gridBtns;
    public PlayerMove oppMove;

    private void Start()
    {
        client = GameObject.Find("ClientObj").GetComponent<Client>();
        getTurn();
    }

    public void getTurn(){
        Socket sock = SocketFactory.createSocket("localhost", 9999);
        string req = "GET /api/game?id=" + client.getGameID() + " HTTP/1.1\r\nHost: " + "localhost:9999\r\n\r\n";
        byte[] sockRequest = Encoding.ASCII.GetBytes(req);
        sock.Send(sockRequest);

        string resp = "";
        int bytes = 0;
        byte[] dataReceived = new byte[1024];
        do
        {
            bytes = sock.Receive(dataReceived);
            resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);
        }
        while (sock.Available > 0);
        print(resp);
        int index = resp.IndexOf("\r\n\r\n");
        index += 4;
        resp = resp.Substring(index);
        ServerUpdate updList = JsonConvert.DeserializeObject<ServerUpdate>(resp);
        if(updList.turn == client.getID()){
            toggleGrid(true);
            client.setPlayerNum(1);
            print("My turn");
        }
        else{
            toggleGrid(false);
            client.setPlayerNum(2);
            client.setIsTurn(false);
            print("There turn");
        }
        sock.Close();
    }

    // Use this for initialization
    public void fillGridSpace(int playerNum, int x, int y)
    {
        int index = x * 3 == 0 ? y : x * 3 + y;
        char moveType = playerNum == 1 ? 'X' : 'Y';
        if(playerNum == 1)
        {
            grid[x, y] = moveType;
            buttonText[index].text = "X";
        }
        else
        {
            grid[x, y] = moveType;
            buttonText[index].text = "O";
        }
        convertToJson(moveType, x, y);
    }

    public void buttonClicked(int btnNum)
    {
        int x = btnNum / 3;
        int y = btnNum % 3 == 0 ? 0 : (btnNum % 3);
        fillGridSpace(client.getPlayerNum(), x, y);
        toggleGrid(false);
    }

    public void convertToJson(char moveType, int x, int y){
        PlayerMove newMove = new PlayerMove(x, y, moveType,client.getID(), client.getGameID());
        string jsonFile = JsonConvert.SerializeObject(newMove);
        client.sendMove(jsonFile);

    }

    public void toggleGrid(bool enable)
    {
        if (!enable)
        {
            for (int i = 0; i < gridBtns.Length; i++)
            {
                gridBtns[i].interactable = false;
            }
        }
        else
        {
            for (int i = 0; i < gridBtns.Length; i++)
            {
                gridBtns[i].interactable = true;
            }
        }
    }

    public void Update()
    {
        if (!client.getIsTurn())
        {
            int bytes = 0;
            byte[] receivedData = new byte[1024];
            string resp = "";
            while (client.getSock().Available > 0)
            {
                bytes = client.getSock().Receive(receivedData);
                resp = resp + Encoding.ASCII.GetString(receivedData, 0, bytes);
                client.setIsTurn(true);
            }
            print(resp);
            if (client.getIsTurn())
            {
                oppMove = JsonConvert.DeserializeObject<PlayerMove>(resp);
                if (oppMove.game_won)
                {
                    gameWon();
                }
                int oppNum = client.getPlayerNum() == 1 ? 2 : 1;
                fillGridSpace(oppNum, oppMove.x, oppMove.y);
                toggleGrid(true);
            }
        }
    }

    public void gameWon(){
        
    }

}
