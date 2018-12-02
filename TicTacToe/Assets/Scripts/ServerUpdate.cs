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
