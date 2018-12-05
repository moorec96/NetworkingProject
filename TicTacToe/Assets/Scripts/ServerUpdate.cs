/**
 * Class: ServerUpdate
 * Purpose: Contains data about the current game data that can be serialized/desieralized to/from JSON format for communicating with the server
 * Author: Caleb Moore
 * Date: 12/04/2018
 * */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerUpdate
{
    public string id;
    public List<string> users;
    public string status;
    public List<PlayerMove> moves;
    public string turn;
    public int size;
    public string host_name;
}
