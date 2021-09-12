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

    public static void AddFromEntity(GameObject o, Entity e)
    {
        EntityPlayer p = o.AddComponent<EntityPlayer>();
        p.EntityInstanceId = e.EntityInstanceId;
        p.EntityTypeId = e.EntityTypeId;
        p.hasAuthority = e.HasAuthority();
        p.DisplayName = e.DisplayName;
        Destroy(e);
    }
}
