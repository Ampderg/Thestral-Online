using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Player Appearance/Item Registry")]
public class PlayerAppearanceItemRegistry : ScriptableObject
{
    public PlayerAppearanceItemList[] registry;
}
