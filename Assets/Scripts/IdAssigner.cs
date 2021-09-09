using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class IdAssigner
{
    public Queue<uint> nextIds = new Queue<uint>();
    public uint highestId = 0;

    public uint GetFreeID()
    {
        if (nextIds.Count > 0)
        {
            return nextIds.Dequeue();
        }
        else
        {
            highestId++;
            return highestId;
        }
    }

    public void UnassignID(uint id)
    {
        nextIds.Enqueue(id);
    }
}

