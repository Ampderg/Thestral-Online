using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : Entity
{
    internal bool IsPlayerAssigned { get; private set; }
    internal uint PlayerID { get; private set; }

    protected virtual void Start()
    {
        IsPlayerAssigned = false;
    }

    internal void SetPlayerInfo(uint playerId, string characterName, string[] characterAppearance)
    {
        IsPlayerAssigned = true;
        this.PlayerID = playerId;
    }
}
