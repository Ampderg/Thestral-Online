using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{

    [SerializeField]
    private Transform headAnchor;
    [SerializeField]
    private Transform wingAnchor;
    [SerializeField]
    private Transform tailAnchor;
    [SerializeField]
    private Transform[] flipShift;

    [SerializeField]
    private Animator bodyAnimator;

    [SerializeField]
    private SpriteRenderer[] spriteRenderOrder;

    public const int SpriteLayersPerPixel = 10;

    internal bool isWalking;

    private bool headLower;
    private bool tailLower;
    private bool wingLower;

    // Update is called once per frame
    void Update()
    {
        bodyAnimator.SetBool("Walking", isWalking);
        for(int i = 0; i < spriteRenderOrder.Length; i++)
        {
            spriteRenderOrder[i].sortingOrder = -((int)(transform.position.y * Game.PixelsPerUnit) * 10) + i;
        }
    }

    public void UpdateDirection(float sign)
    {
        if(Mathf.Sign(transform.localScale.x) != sign)
        {
            Flip();
        }
    }

    public void Flip()
    {
        float dir = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(dir * -1, transform.localScale.y, transform.localScale.z);
        for(int i = 0; i < flipShift.Length; i++)
        {
            flipShift[i].localPosition -= Vector3.right * dir / Game.PixelsPerUnit;
        }
    }

    public void LowerHead()
    {
        if (headLower) return;
        headAnchor.position += Vector3.down / Game.PixelsPerUnit;
        headLower = true;
    }

    public void RaiseHead()
    {
        if (!headLower) return;
        headAnchor.position += Vector3.up / Game.PixelsPerUnit;
        headLower = false;
    }

    public void LowerWing()
    {
        if (wingLower) return;
        wingAnchor.position += Vector3.down / Game.PixelsPerUnit;
        wingLower = true;
    }

    public void RaiseWing()
    {
        if (!wingLower) return;
        wingAnchor.position += Vector3.up / Game.PixelsPerUnit;
        wingLower = false;
    }

    public void LowerTail()
    {
        if (tailLower) return;
        tailAnchor.position += Vector3.down / Game.PixelsPerUnit;
        tailLower = true;
    }

    public void RaiseTail()
    {
        if (!tailLower) return;
        tailAnchor.position += Vector3.up / Game.PixelsPerUnit;
        tailLower = false;
    }
}
