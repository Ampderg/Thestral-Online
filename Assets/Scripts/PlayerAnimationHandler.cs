using System;
using System.Linq;
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
    private SpriteRenderer[] bodyAnims;
    [SerializeField]
    private List<uint> doNotRenderAppearanceIds;
    [SerializeField]
    private List<SpriteRenderer> doNotDestroyOnGenerate;
    [SerializeField]
    private List<Transform> flipShift;

    [SerializeField]
    private Animator bodyAnimator;

    [SerializeField]
    private List<SpriteRenderer> spriteRenderers;

    [SerializeField]
    private float spriteFlipTime = 0;

    private Coroutine spriteFlipCoroutine;

    public const int SpriteLayersPerPixel = 100;

    public const int playerPPU = 10;

    internal bool isWalking;

    private bool headLower;
    private bool tailLower;
    private bool wingLower;

    private void GenerateSprites(PlayerAppearanceContainer c)
    {
        var itemReg = ServerLink.instance.appearanceItemRegistry.registry;
        //clear old items
        flipShift.Clear();
        for(int i = 0; i < spriteRenderers.Count; i++)
        {
            if(!doNotDestroyOnGenerate.Contains(spriteRenderers[i]))
            {
                Destroy(spriteRenderers[i].gameObject);
                spriteRenderers.RemoveAt(i);
                i--;
            }
        }
        //add new items
        for (int i = 0; i < c.itemList.Count; i++)
        {
            var it = itemReg[c.itemList[i]];


            for (int j = 0; j < it.items.Count; j++)
            {
                if (!doNotRenderAppearanceIds.Contains(c.itemList[i]))
                {
                    GameObject sprite = new GameObject();
                    switch (it.items[j].spriteAnchor)
                    {
                        case PlayerAppearanceItem.SpriteAnchor.Body:
                            sprite.transform.parent = transform;
                            break;
                        case PlayerAppearanceItem.SpriteAnchor.Head:
                            sprite.transform.parent = headAnchor;
                            break;
                        case PlayerAppearanceItem.SpriteAnchor.Tail:
                            sprite.transform.parent = tailAnchor;
                            break;
                        case PlayerAppearanceItem.SpriteAnchor.Wings:
                            sprite.transform.parent = wingAnchor;
                            break;
                    }
                    sprite.transform.localPosition = Vector3.zero;
                    sprite.transform.localScale = Vector3.one;
                    SpriteRenderer sr = sprite.AddComponent<SpriteRenderer>();
                    sr.sprite = it.items[j].sprite;
                    if(!string.IsNullOrWhiteSpace(it.items[j].colorTag) && c.colors.ContainsKey(it.items[j].colorTag))
                        sr.color = c.colors[it.items[j].colorTag];
                    sr.sortingOrder = it.items[j].renderOrderOffset + it.renderOrder;
                    if(it.items[j].flipShift)
                    {
                        flipShift.Add(sprite.transform);
                    }
                    sprite.gameObject.name = it.items[j].name;
                    spriteRenderers.Add(sr);
                }
                else
                {
                    switch (it.items[j].colorTag)
                    {
                        case "Body":
                            foreach (var body in bodyAnims)
                            {
                                body.color = c.colors["Body"];
                            }
                            break;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        bodyAnimator.SetBool("Walking", isWalking);
        for(int i = 0; i < spriteRenderers.Count; i++)
        {
            //spriteRenderers[i].sortingOrder = -((int)(transform.position.y * playerPPU) * SpriteLayersPerPixel) + i;
            int order = spriteRenderers[i].sortingOrder % SpriteLayersPerPixel;
            spriteRenderers[i].sortingOrder = -((int)(transform.position.y * playerPPU) * SpriteLayersPerPixel) + order;
        }
    }

    internal void ParseCharacterString(string s)
    {
        PlayerAppearanceContainer c = new PlayerAppearanceContainer(s, ServerLink.playerInfo.unlockedManes);
        GenerateSprites(c);
    }

    public void UpdateDirection(float sign)
    {
        if(Mathf.Sign(transform.localScale.x) != sign)
        {
            if (spriteFlipTime != 0)
            {
                if(spriteFlipCoroutine == null)
                    spriteFlipCoroutine = StartCoroutine(FlipCoroutine());
            }
            else
                Flip();
        }
    }

    public IEnumerator FlipCoroutine()
    {
        float dir = Mathf.Sign(transform.localScale.x);
        Vector3 startScale = transform.localScale;
        Vector3 midScale = new Vector3(0, startScale.y, startScale.z);
        Vector3 targetScale = new Vector3(dir * -1, startScale.y, startScale.z);
        for(float t = 0; t < 1; t += Time.deltaTime * 2 / spriteFlipTime)
        {
            transform.localScale = Vector3.Lerp(startScale, midScale, t * t);
            yield return null;
        }
        for (int i = 0; i < flipShift.Count; i++)
        {
            flipShift[i].localPosition -= Vector3.right * dir / playerPPU;
        }
        transform.localScale = midScale;
        yield return null;
        for (float t = 0; t < 1; t += Time.deltaTime * 2 / spriteFlipTime)
        {
            transform.localScale = Vector3.Lerp(midScale, targetScale, Mathf.Sqrt(t));
            yield return null;
        }
        transform.localScale = targetScale;
        spriteFlipCoroutine = null;
    }

    public void Flip()
    {
        float dir = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(dir * -1, transform.localScale.y, transform.localScale.z);
        for (int i = 0; i < flipShift.Count; i++)
        {
            flipShift[i].localPosition -= Vector3.right * dir / playerPPU;
        }
    }

    public void LowerHead()
    {
        if (headLower) return;
        headAnchor.position += Vector3.down / playerPPU;
        headLower = true;
    }

    public void RaiseHead()
    {
        if (!headLower) return;
        headAnchor.position += Vector3.up / playerPPU;
        headLower = false;
    }

    public void LowerWing()
    {
        if (wingLower) return;
        wingAnchor.position += Vector3.down / playerPPU;
        wingLower = true;
    }

    public void RaiseWing()
    {
        if (!wingLower) return;
        wingAnchor.position += Vector3.up / playerPPU;
        wingLower = false;
    }

    public void LowerTail()
    {
        if (tailLower) return;
        tailAnchor.position += Vector3.down / playerPPU;
        tailLower = true;
    }

    public void RaiseTail()
    {
        if (!tailLower) return;
        tailAnchor.position += Vector3.up / playerPPU;
        tailLower = false;
    }
}
