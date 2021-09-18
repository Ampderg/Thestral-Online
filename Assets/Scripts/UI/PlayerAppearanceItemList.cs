using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/Player Appearance/Item Cluster")]
public class PlayerAppearanceItemList : ScriptableObject
{
    public string itemName;
    public PlayerAppearanceItem.ItemType itemType;
    public int renderOrder;
    public List<PlayerAppearanceItem> items = new List<PlayerAppearanceItem>();
}