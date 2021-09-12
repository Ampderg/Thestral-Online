using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : EntityMove
{
    [SerializeField]
    private PlayerAnimationHandler anims;

    public bool CanMove { get { return !ChatSystem.IsFocused; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (HasAuthority())
        {
            if(CanMove)
                i.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * p.walkSpeed;
        }
        if (anims != null)
        {
            if(i.velocity.x != 0)
                anims.UpdateDirection(-Mathf.Sign(i.velocity.x));
            anims.isWalking = i.velocity.sqrMagnitude > 0.1f;
        }
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
