using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grid : MonoBehaviour {

    public static char[,] grid = new char[3,3];
    public Text[] buttonText;
    private Client client;

    private void Start()
    {
        client = GameObject.Find("Client").GetComponent<Client>();
    }

    // Use this for initialization
    public void fillGridSpace(int playerNum, int x, int y)
    {
        int index = x * 3 == 0 ? y : x * 3 + y;

        if(playerNum == 1)
        {
            grid[x, y] = 'X';
            buttonText[index].text = "X";
        }
        else
        {
            grid[x, y] = 'Y';
            buttonText[index].text = "Y";
        }
    }
	
    public void buttonClicked(int btnNum)
    {
        int x = btnNum / 3;
        int y = btnNum % 3 == 0 ? 0 : (btnNum % 3);
        print(x + " " + y);
        fillGridSpace(1, x, y);
    }

}
