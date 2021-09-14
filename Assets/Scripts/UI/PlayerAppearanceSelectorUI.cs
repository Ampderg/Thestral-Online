using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAppearanceSelectorUI : MonoBehaviour
{

    public CharacterAppearanceMenu menu;
    public PlayerAppearanceItem.ItemType itemType;

    public void Next()
    {
        menu.SelectNextItem(itemType);
    }

    public void Prev()
    {
        menu.SelectPreviousItem(itemType);
    }
}
