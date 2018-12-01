using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
public class Grid : MonoBehaviour {

    public static char[,] grid = new char[3,3];
    public Text[] buttonText;
    private Client client;
    public Button[] gridBtns;

    private void Start()
    {
        client = GameObject.Find("ClientObj").GetComponent<Client>();
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
            buttonText[index].text = "Y";
        }
        convertToJson(moveType, x, y);
    }
	
    public void buttonClicked(int btnNum)
    {
        int x = btnNum / 3;
        int y = btnNum % 3 == 0 ? 0 : (btnNum % 3);
        fillGridSpace(1, x, y);
        toggleGrid(true);
    }

    public void convertToJson(char moveType, int x, int y){
        PlayerMove newMove = new PlayerMove(x, y, moveType, "abc", "def");
        string jsonFile = JsonConvert.SerializeObject(newMove);
        print(jsonFile);
        client.sendMove(jsonFile);
        string resp = client.receiveMove();
        print(resp);
    }

    public void toggleGrid(bool disable)
    {
        if (disable)
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


}
