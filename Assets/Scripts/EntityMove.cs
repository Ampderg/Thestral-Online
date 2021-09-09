using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMove : MonoBehaviour
{
    [System.Serializable]
    public class MoveParameters
    {
        public float walkSpeed = 4;
    }

    [System.Serializable]
    public class MoveInput
    {
        public Vector2 velocity;
        public float timeWithoutVelUpdate;
    }

    public static Vector3 GlobalMoveMultiplier = new Vector3(1, 1, 1);

    [SerializeField]
    protected MoveParameters p;

    [SerializeField]
    protected MoveInput i;

    [SerializeField]
    protected Transform moveTransform;

    protected Entity entity;
    private Vector3 lastUpdatePosition;

    private const int UpdatesPerSecond = 20;
    private float updateTimer;

    internal Vector3 targetPosition;

    // Start is called before the first frame update
    void Awake()
    {
        if (moveTransform == null) moveTransform = transform;
        entity = GetComponent<Entity>();
        targetPosition = transform.position;
    }

    protected virtual void Update()
    {
        i.timeWithoutVelUpdate += Time.deltaTime;
    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {
        ProcessMovementTick();
        if (updateTimer > 0f)
        {
            updateTimer -= Time.fixedDeltaTime;
        }
        else if(HasAuthority() && ShouldUpdateMovement())
        {
            //send update to server
            ServerLink.SendMovement(entity.EntityInstanceId, moveTransform.position);
            lastUpdatePosition = moveTransform.position;
            updateTimer = 1f / UpdatesPerSecond;
        }
    }

    protected void ProcessMovementTick()
    {
        if (HasAuthority())
        {
            Vector3 moveVector = i.velocity; //(Vector3)(i.moveAxis.normalized * p.walkSpeed);
            moveVector.x *= GlobalMoveMultiplier.x;
            moveVector.y *= GlobalMoveMultiplier.y;
            moveVector.z *= GlobalMoveMultiplier.z;
            moveTransform.position += moveVector * Time.deltaTime;
        }
        else
        {
            if(i.timeWithoutVelUpdate > 0.1f)
            {
                i.velocity = Vector2.MoveTowards(i.velocity, Vector2.zero, 50 * Time.deltaTime);
            }
            moveTransform.position = Vector3.MoveTowards(moveTransform.position, targetPosition, Time.fixedDeltaTime * p.walkSpeed);
        }
    }

    private bool ShouldUpdateMovement()
    {
        return (Mathf.Abs(moveTransform.position.x - lastUpdatePosition.x) > (1f / Game.PixelsPerUnit)) // x distance greater than a pixel
            || (Mathf.Abs(moveTransform.position.y - lastUpdatePosition.y) > (1f / Game.PixelsPerUnit)); // y distance greater than a pixel
    }

    public static Vector3 SnapToPixelGrid(Vector3 v, int pixelsPerUnit = 10)
    {
        return v;
    }

    protected bool HasAuthority()
    {
        Entity e = GetEntity();
        return e != null && e.HasAuthority();
    }

    protected Entity GetEntity()
    {
        if(entity == null)
        {
            entity = GetComponent<Entity>();
        }
        return entity;
    }

    internal void SetVelocity(float x, float y)
    {
        i.velocity = new Vector2(x, y);
        i.timeWithoutVelUpdate = 0;
    }
}
