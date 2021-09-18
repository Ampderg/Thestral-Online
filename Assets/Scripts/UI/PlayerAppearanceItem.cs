using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Player Appearance/Item")]
public class PlayerAppearanceItem : ScriptableObject
{
    public string colorTag;
    public SpriteAnchor spriteAnchor;
    public Sprite sprite;
    public int renderOrderOffset;
    public RectTransform uiEditPrefab;
    public bool flipShift;

    public enum ItemType
    {
        Body,
        Mane,
        Tail,
        Wings
    }

    public enum SpriteAnchor
    {
        Body,
        Head,
        Tail,
        Wings
    }
}
