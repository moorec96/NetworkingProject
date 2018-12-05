/**
 * Class: Grid  
 * Purpose: Handles in-game moves and keeping track of tic-tac-toe grid
 * Author: Caleb Moore
 * Date: 12/04/2018
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.SceneManagement;

public class Grid : MonoBehaviour {

    public static char[,] grid = new char[3,3];
    public Text[] buttonText;
    private Client client;
    public Button[] gridBtns;
    public bool[] btnsSet = new bool[9];
    public PlayerMove oppMove;
    public Text gameOverText;
    public GameObject gameOverPanel;
    public GameObject quitPanel;

    //Find client object in hierarchy, and call getTurn
    private void Start()
    {
        client = GameObject.Find("ClientObj").GetComponent<Client>();
        getTurn();
    }

    /**
     * Method: getTurn
     * Purpose: Asks the server whose turn it is 
     * Parameters: N/A
     * Return Val: N/A
     * */
    public void getTurn(){
        client.youWon = false;                                  //Resets clients win status for new game
        client.gameOver = false;                                //Resets clients game over status for new game
        Socket sock = SocketFactory.createSocket("localhost", 9999);        //Initializes socket 
        string req = "GET /api/game?id=" + client.getGameID() + " HTTP/1.1\r\nHost: " + "localhost:9999\r\n\r\n";       //Asks for current game info
        byte[] sockRequest = Encoding.ASCII.GetBytes(req);
        sock.Send(sockRequest);

        string resp = "";
        int bytes = 0;
        byte[] dataReceived = new byte[1024];
        do
        {
            bytes = sock.Receive(dataReceived);                                 //Waits for response from server
            resp = resp + Encoding.ASCII.GetString(dataReceived, 0, bytes);
        }
        while (sock.Available > 0);
        print(resp);
        int index = resp.IndexOf("\r\n\r\n");       //Finds location of the body of the header sent back from server
        index += 4;
        resp = resp.Substring(index);
        ServerUpdate updList = JsonConvert.DeserializeObject<ServerUpdate>(resp);       //Object that contains game data
        if(updList.turn == client.getID()){                                             //If it is the local clients turn, then turn the grid on, and set the clients player num to 1
            toggleGrid(true);
            client.setPlayerNum(1);
            print("My turn");
        }
        else{                                                                           //If it is not the local clients turn, then turn the grid off, and set the clients number to 2
            toggleGrid(false);
            client.setPlayerNum(2);
            client.setIsTurn(false);
            print("There turn");
        }
        sock.Close();
    }

    /**
     * Method: fillGridSpace
     * Purpose: Take in coordinates of last move, and display them on the grid
     * Parameters:  playerNum -> player who just went, x and y -> coordinates
     * Return Val: N/A
     * */
    public void fillGridSpace(int playerNum, int x, int y)
    {
        int index = x * 3 == 0 ? y : x * 3 + y;
        char moveType = playerNum == 1 ? 'X' : 'O';
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
        //gridBtns[index].interactable = false;
        btnsSet[index] = true;
    }

    /**
     * Method: buttonClicked
     * Purpose:  Gets coordinates of button just clicked on the board, calls fillGridSpace, calls convertToJson, and then toggles the grid
     * Parameters: btnNum -> Number of button clicked
     * Return Val: N/A
     * */
    public void buttonClicked(int btnNum)
    {
        int x = btnNum / 3;
        int y = btnNum % 3 == 0 ? 0 : (btnNum % 3);
        fillGridSpace(client.getPlayerNum(), x, y);
        convertToJson(grid[x,y], x, y);
        toggleGrid(false);
    }

    /**
     * Method: convertToJson
     * Purpose: Convert move into a PlayerMove object, convert to JSON, and then send to server
     * Parameters: moveType -> either X or O, x and y -> coordinates
     * Return Val: N/A
     * */
    public void convertToJson(char moveType, int x, int y){
        PlayerMove newMove = new PlayerMove(x, y, moveType,client.getID(), client.getGameID());
        string jsonFile = JsonConvert.SerializeObject(newMove);
        client.sendMove(jsonFile);

    }

    /**
     * Method: toggleGrid
     * Purpose: Turns buttons off when it isn't the players turn so that they can't click on the board  
     * Parameters: enable -> Whether or not the grid should be turned on
     * Return Val: N/A
     * */
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
                if (!btnsSet[i])
                {
                    gridBtns[i].interactable = true;
                }
            }
        }
    }

    public void Update()
    {
        if (!client.getIsTurn())        //If it isn't the players turn, wait for a response from the server 
        {
            if(client.gameOver){            //If game over, then call gameWon()
                gameWon(client.youWon);
            }
            int bytes = 0;
            byte[] receivedData = new byte[1024];
            string resp = "";
            while (client.getSock().Available > 0)      //Wait for a response from the server
            {
                bytes = client.getSock().Receive(receivedData);
                resp = resp + Encoding.ASCII.GetString(receivedData, 0, bytes);
                client.setIsTurn(true);
                print("TEST");
            }
            //print(resp);
            if (client.getIsTurn())                         //If client has received a messaage, continue
            {   
                if (resp.IndexOf("GAME_CANCELLED") > -1)    //If opponent has left, call gameCancelled
                {
                    gameCancelled();
                }
                else if(resp.IndexOf("GAME_MOVE") > -1)     //Deserialize response into a PlayerMove, and display it on the grid.
                {
                    print("RECEIVED MOVE");
                    oppMove = JsonConvert.DeserializeObject<PlayerMove>(resp);
                    if (oppMove.game_won)
                    {
                        if(oppMove.user_id == client.getID()){
                            gameWon(true);
                        }
                        else{
                            gameWon(false);
                        }

                    }
                    int oppNum = client.getPlayerNum() == 1 ? 2 : 1;
                    print("Num: " + oppNum + "   X: " + oppMove.x + "   Y: " + oppMove.y);
                    fillGridSpace(oppNum, oppMove.x, oppMove.y);
                    toggleGrid(true);
                }
                else{
                    client.setIsTurn(false);
                }
            }
        }
    }

    /**
     * Method: gameWon
     * Purpose: Display to the screen the win status of the player
     * Parameters: youWon -> Whether or not the player won the game
     * Return Val: N/A 
     * */
    public void gameWon(bool youWon){
        gameOverPanel.SetActive(true);
        if (youWon)
        {
            gameOverText.text = "YOU WON";
        }
        else{
            gameOverText.text = "YOU LOST";
        }
    }

    /**
     * Method: gameCancelled
     * Purpose: Display opponent quit panel
     * Parameters:  N/A
     * Return Val: N/A
     * */
    public void gameCancelled(){
        quitPanel.SetActive(true);
    }

    /**
     * Method: leaveGame
     * Purpose: Let the server know client is leaving game, and go back to Main Menu 
     * Parameters: N/A
     * Return Val: N/A
     * */
    public void leaveGame(){
        Socket tempSock = SocketFactory.createSocket("localhost", 9999);
        string req = "DELETE /api/users?id=" + client.getID() + " HTTP/1.1\r\nHost: localhost:9999\r\n\r\n";
        byte[] dataToSend = Encoding.ASCII.GetBytes(req);
        tempSock.Send(dataToSend);
        //Destroy(client);
        SceneManager.LoadScene("MainMenu");
        //Application.Quit();
    }

}
