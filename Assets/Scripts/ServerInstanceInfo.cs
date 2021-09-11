using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerInstanceInfo : MonoBehaviour
{
    [SerializeField]
    private uint roomId;

    public uint GetRoomId()
    {
        return roomId;
    }
}
