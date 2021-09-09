using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerLink : MonoBehaviour
{

    protected static ServerLink instance;

    //Server info
    [SerializeField]
    private string ipString = "127.0.0.1";
    [SerializeField]
    private int port = 59873;
    private Socket socket;

    //User info
    [SerializeField]
    private string userName = "Player";
    
    //Message handling
    private IdAssigner messageId;
    private Dictionary<uint, string> messagesSent;

    //Entity handling
    private Dictionary<uint, Entity> networkedEntities;
    private Transform spawnedEntityParent;
    [SerializeField]
    protected SpawnableEntityLibrary spawnableEntityLibrary;

    //Coroutine communication
    private Coroutine connectionCoroutine;
    private string validMessage = null;

    // Start is called before the first frame update
    void Start()
    {
        if(instance != null)
        {
            Debug.LogError("More than one instance of ServerLink detected.");
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        messageId = new IdAssigner();
        messagesSent = new Dictionary<uint, string>();
        networkedEntities = new Dictionary<uint, Entity>();
        connectionCoroutine = StartCoroutine(CrtConnect());
    }

    private void OnDestroy()
    {
        if(socket != null)
            Disconnect();
    }

    

    #region Joining

    private IEnumerator CrtConnect()
    {
        if (JoinServer(ipString, port))
        {
            validMessage = null;
            ValidateUserInfo();
            while (validMessage == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            if (validMessage.ToLower() == "valid")
            {
                JoinScene(1);
            }
        }
        connectionCoroutine = null;
    }

    private void Disconnect()
    {
        SendString("/disconnect");
        if (connectionCoroutine != null)
            StopCoroutine(connectionCoroutine);
        socket.Disconnect(false);
    }

    private bool JoinServer(string ipString, int port)
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
            this.socket = client;
            return true;
        }
        catch (SocketException se)
        {
            Debug.Log(string.Format("SocketException : {0}", se.ToString()));
            return false;
        }
    }

    private void ValidateUserInfo()
    {
        SendString(string.Format("/validate|{0}", userName));
    }

    private void JoinScene(int sceneId)
    {
        //display loading screen
        SceneManager.LoadScene(sceneId);
        SendString(string.Format("/joinScene|{0}", sceneId));
    }

    #endregion

    private void SendString(string s)
    {
        uint id = messageId.GetFreeID();
        messagesSent[id] = s;
        string message = string.Format("{0}:{1}\n", id, s);
        Debug.Log("Sending Message: " + message);
        socket.Send(Encoding.ASCII.GetBytes(message));
    }

    private void SendStringWithoutId(string s)
    {
        string message = string.Format(":{0}\n", s);
        //Debug.Log("Sending Message: " + message);
        socket.Send(Encoding.ASCII.GetBytes(message));
    }

    private void ProcessMessageRecieved(string s)
    {
        Debug.Log("Recieved message: " + s);
        //Split the message on the colon to get the message id
        string[] tokensColon = s.Split(new char[] { ':' }, 2);
        try
        {
            uint msgId;
            bool hasId = uint.TryParse(tokensColon[0], out msgId);
            //split the message on slashes to get the commands
            string[] cmdTokens = tokensColon[1].Split('/');
            for (int i = 1; i < cmdTokens.Length; i++)
            {
                if (cmdTokens[i].Length > 0)
                {
                    //split the command on pipes to get the parameters
                    string[] msgTokens = cmdTokens[i].Split('|');
                    try
                    {
                        switch (msgTokens[0])
                        {
                            case "validate":
                                ValidationResultRecieved(msgTokens[1]);
                                break;
                            case "createEntity":
                                CreateEntityRecieved(uint.Parse(msgTokens[5]), uint.Parse(msgTokens[1]), uint.Parse(msgTokens[6]), bool.Parse(msgTokens[4]), 
                                    int.Parse(msgTokens[2]), int.Parse(msgTokens[3]));
                                break;
                            case "destroyEntity":
                                DestroyEntityRecieved(uint.Parse(msgTokens[1]));
                                break;
                            case "m":
                            case "move":
                                if(msgTokens.Length > 4)
                                    MoveRecieved(uint.Parse(msgTokens[1]), int.Parse(msgTokens[2]), int.Parse(msgTokens[3]),
                                        int.Parse(msgTokens[4]), int.Parse(msgTokens[5]), true);
                                else
                                    MoveRecieved(uint.Parse(msgTokens[1]), int.Parse(msgTokens[2]), int.Parse(msgTokens[3]));
                                break;
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError("Encountered Exception within command execution:" + Environment.NewLine + e + Environment.NewLine + 
                            "Message: " + s);
                    }
                }
            }

            if (hasId && messagesSent.ContainsKey(msgId))
            {
                messageId.UnassignID(msgId);
                messagesSent.Remove(msgId);
            }
        }
        catch (IndexOutOfRangeException)
        {
            Debug.LogError("Recieved a message from the server with a bad format! " + s);
        }
    }

    #region Entities
    internal static void RegisterEntity(Entity entity, uint entityInstanceId)
    {
        instance.networkedEntities[entityInstanceId] = entity;
    }

    internal static void DeregisterEntity(uint entityInstanceId)
    {
        instance.networkedEntities.Remove(entityInstanceId);
    }

    internal static void SendMovement(uint entityInstanceId, Vector3 position)
    {
        int pixelX = Mathf.RoundToInt(position.x * Game.PixelsPerUnit);
        int pixelY = Mathf.RoundToInt(position.y * Game.PixelsPerUnit);
        if (instance != null)
            instance.SendStringWithoutId($"/m|{entityInstanceId}|{pixelX}|{pixelY}");
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        if(socket != null && socket.Available > 0)
        {
            //if socket has data available
            byte[] b = new byte[socket.Available];
            int bytesRecieved = socket.Receive(b);
            string s = Encoding.ASCII.GetString(b,0,bytesRecieved);
            string[] cmd = s.Split('\n');
            for(int i = 0; i < cmd.Length - 1; i++)
                ProcessMessageRecieved(cmd[i]);
        }
        else
        {
            //disconnect local client
        }
    }

    #region Server Messages
    private void ValidationResultRecieved(string message)
    {
        Debug.Log("Validation Result: " + message);
        validMessage = message;
    }

    private void CreateEntityRecieved(uint instanceId, uint entityId, uint entityInstanceId, bool hasAuthority, int pixelX, int pixelY)
    {
        Debug.Log($"Creating entity with id {entityId} at position [{pixelX}, {pixelY}]");
        if(spawnedEntityParent == null)
        {
            GameObject parent = GameObject.Find("SpawnedEntityParent");
            if (parent == null)
                parent = new GameObject("SpawnedEntityParent");
            spawnedEntityParent = parent.transform;
        }
        GameObject o = Instantiate(spawnableEntityLibrary.GetEntity(entityId), spawnedEntityParent);
        o.transform.position = new Vector3((float)pixelX / Game.PixelsPerUnit, (float)pixelY / Game.PixelsPerUnit, 0f);
        Entity e = o.AddComponent<Entity>();
        e.Create(entityInstanceId, entityId, hasAuthority);
    }

    private void DestroyEntityRecieved(uint entityInstanceId)
    {
        Destroy(networkedEntities[entityInstanceId].gameObject);
        networkedEntities.Remove(entityInstanceId);
    }

    private void JoinSceneRecieved()
    {
        //when the result of this is recieved, it means all of the entity loading is complete
        //disable loading screen
    }

    private void MoveRecieved(uint entityInstanceId, int pixelX, int pixelY, int hundredVelX = 0, int hundredVelY = 0, bool hasVel = false)
    {
        if(networkedEntities.ContainsKey(entityInstanceId))
        {
            Transform t = networkedEntities[entityInstanceId].transform;
            Vector3 target = new Vector3((float)pixelX / Game.PixelsPerUnit, (float)pixelY / Game.PixelsPerUnit, t.position.z);
            EntityMove m = t.GetComponent<EntityMove>();
            if (m != null)
            {
                m.targetPosition = target;
                if(hasVel)
                {
                    m.SetVelocity(hundredVelX / 100f * Game.PixelsPerUnit, hundredVelY / 100f * Game.PixelsPerUnit);
                }
            }
            else
            {
                t.position = target;
            }
        }
    }
    #endregion
}
