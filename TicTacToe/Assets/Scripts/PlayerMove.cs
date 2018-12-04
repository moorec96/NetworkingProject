/**
 * Class: 
 * Purpose: 
 * Author: 
 * Date: 
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove{
    public string type = "GAME_MOVE";
    public int x;
    public int y;
    public char move_type;
    public string user_id;
    public string game_id;
    public bool game_won;

    /**
     * Method: 
     * Purpose: 
     * Parameters: 
     * Return Val: 
     * */
    public PlayerMove(int x, int y, char move_type, string user_id, string game_id){
        this.x = x;
        this.y = y;
        this.move_type = move_type;
        this.user_id = user_id;
        this.game_id = game_id;
        game_won = false;
    }
}
