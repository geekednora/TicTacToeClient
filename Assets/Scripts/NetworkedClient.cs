using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkedClient : MonoBehaviour
{
    private readonly string _IPaddress = "10.0.225.72";
    private readonly int _maxConnections = 1000;
    private readonly int _socketPort = 5492;
    private int _connectionID;
    private byte _error;
    private int _hostID;
    private bool _isConnected;
    private int _ourClientID;
    private int _reliableChannelID;
    private int _unreliableChannelID;
    private string _userLogin, _userPassword;

    // Start is called before the first frame update
    [Obsolete]
    private void Start()
    {
        Connect();
        Debug.Log(LocalIPAddress());
    }

    // Update is called once per frame
    [Obsolete]
    private void Update()
    {
        UpdateNetworkConnection();
    }

    [Obsolete]
    private void UpdateNetworkConnection()
    {
        if (_isConnected)
        {
            int recHostID;
            int recConnectionID;
            int recChannelID;
            var recBuffer = new byte[1024];
            var bufferSize = 1024;
            int dataSize;
            var recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID,
                out recChannelID, recBuffer, bufferSize, out dataSize, out _error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("connected.  " + recConnectionID);
                    _ourClientID = recConnectionID;
                    break;
                case NetworkEventType.DataEvent:
                    var msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    ProcessRecievedMsg(msg, recConnectionID);
                    Debug.Log("got msg = " + msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    _isConnected = false;
                    Debug.Log("disconnected.  " + recConnectionID);
                    break;
            }

            if (Input.GetKeyDown(KeyCode.S)) SendMessageToHost("Hi there!");
        }
    }

    [Obsolete]
    private void Connect()
    {
        Debug.Log("Attempting to connect.\n Host: " + _IPaddress + ":" + _socketPort);
        
        if (!_isConnected)
        {
            Debug.Log("Client is disconnected. Connecting...");

            NetworkTransport.Init(); // init network porotocol

            // init config
            var config = new ConnectionConfig();
            _reliableChannelID = config.AddChannel(QosType.Reliable);
            _unreliableChannelID = config.AddChannel(QosType.Unreliable);

            // init topology
            var topology = new HostTopology(config, _maxConnections);
            _hostID = NetworkTransport.AddHost(topology, 0);
            Debug.Log("Socket open.  Host ID = " + _hostID);

            _connectionID = NetworkTransport.Connect(_hostID, _IPaddress, _socketPort, 0, out _error);

            if (_error == 0)
            {
                _isConnected = true;
                Debug.Log("Successfully connected! Client ID : " + _connectionID);
            }
        }
    }

    [Obsolete]
    public void Disconnect()
    {
        NetworkTransport.Disconnect(_hostID, _connectionID, out _error);
    }

    [Obsolete]
    public void SendMessageToHost(string msg)
    {
        var buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(_hostID, _connectionID, _reliableChannelID, buffer, msg.Length * sizeof(char),
            out _error);
    }

    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);
    }

    public bool IsConnected()
    {
        return _isConnected;
    }
    
    public static string LocalIPAddress()
    {
        IPHostEntry host;
        string localIP = "0.0.0.0";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }
}