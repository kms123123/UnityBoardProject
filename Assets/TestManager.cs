using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    public Server server;
    public Client client;

    private void Start()
    {
        server.Init(8005);
        client.Init("127.0.0.1", 8005);
    }
}
