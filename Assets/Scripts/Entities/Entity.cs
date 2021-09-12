using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public uint EntityInstanceId { get; protected set; }
    [SerializeField]
    protected bool hasAuthority;
    public uint EntityTypeId { get; protected set; }
    public string DisplayName { get; set; }


    protected virtual void OnDestroy()
    {
        ServerLink.DeregisterEntity(EntityInstanceId);
    }

    internal virtual void Create(uint entityInstanceId, uint entityId, bool hasAuthority, string displayName)
    {
        this.EntityInstanceId = entityInstanceId;
        this.hasAuthority = hasAuthority;
        this.EntityTypeId = entityId;
        this.DisplayName = displayName;
        ServerLink.RegisterEntity(this, entityInstanceId);
    }

    internal virtual bool HasAuthority()
    {
        return hasAuthority;
    }
}
