using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    internal uint EntityInstanceId { get; private set; }
    [SerializeField]
    private bool hasAuthority;
    internal uint EntityTypeId { get; private set; }

    private void OnDestroy()
    {
        ServerLink.DeregisterEntity(EntityInstanceId);
    }

    internal void Create(uint entityInstanceId, uint entityId, bool hasAuthority)
    {
        this.EntityInstanceId = entityInstanceId;
        this.hasAuthority = hasAuthority;
        this.EntityTypeId = entityId;
        ServerLink.RegisterEntity(this, entityInstanceId);
    }

    internal bool HasAuthority()
    {
        return hasAuthority;
    }
}
