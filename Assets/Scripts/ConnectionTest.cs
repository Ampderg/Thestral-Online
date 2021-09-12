using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ConnectionTest : MonoBehaviour
{

    [SerializeField]
    private string ipString = "127.0.0.1";
    [SerializeField]
    private int port = 59873;

    // Start is called before the first frame update
    void Start()
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(ipString);
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

        // Create a TCP/IP socket.  
        Socket client = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        try
        {
            client.Connect(remoteEP);
            Debug.Log("Connected!");
        }
        catch (SocketException se)
        {
            Debug.Log(string.Format("SocketException : {0}", se.ToString()));
        }
    }

}
