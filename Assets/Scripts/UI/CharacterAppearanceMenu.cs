using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System;
using DevionGames.UIWidgets;

public class CharacterAppearanceMenu : MonoBehaviour
{
    [SerializeField]
    private Transform contentParent;
    [SerializeField]
    private Transform previewRendererParent;

    private struct CharacterPreviewItem
    {
        public string colorTag;
        public RawImage rawImage;
    }

    
    [SerializeField]
    private PlayerAppearanceContainer appearance;

    private Dictionary<string, FlexibleColorPicker> colorLookups = new Dictionary<string, FlexibleColorPicker>();
    private List<CharacterPreviewItem> previewItems = new List<CharacterPreviewItem>();

    [SerializeField]
    private GameObject previewItemPrefab;

    [SerializeField]
    private TMP_Text maneNameText;

    [SerializeField]
    private UIWidget uiWidget;

    private Dictionary<uint, PlayerAppearanceItemList> registeredAppearanceItems;

   

    // Start is called before the first frame update
    void Start()
    {
        registeredAppearanceItems = new Dictionary<uint, PlayerAppearanceItemList>();
        for(uint i = 0; i < ServerLink.instance.appearanceItemRegistry.registry.Length; i++)
        {
            registeredAppearanceItems[i] = ServerLink.instance.appearanceItemRegistry.registry[i];
        }

        
    }

    private void Awake()
    {
        Action<string> loadChar = SetCharacterFromString;
        ServerLink.instance.LoadCharacterString(0, loadChar);
    }

    #region Serialization
    public void SetCharacterFromString(string s)
    {
        PlayerAppearanceContainer c = new PlayerAppearanceContainer(s, ServerLink.playerInfo.unlockedManes);
        if (c.itemList.Count > 0)
        {
            this.appearance = c;
            SetSelectedMane(ServerLink.playerInfo.unlockedManes[c.currentSelectedManeIndex], false);
            RefreshAppearanceList(false);
        }
    }

    public string GetCharacterString()
    {
        if (appearance == null || appearance.itemList.Count == 0) return "";

        StringBuilder sb = new StringBuilder();
        bool first = true;
        for (int i = 0; i < appearance.itemList.Count; i++)
        {
            sb.Append((first ? "" : ",") + appearance.itemList[i]);
            first = false;
        }
        sb.Append('/');
        sb.Append($"m,{ServerLink.playerInfo.unlockedManes[appearance.currentSelectedManeIndex]}");
        sb.Append('/');
        first = true;
        foreach(var pair in colorLookups)
        {
            Color c = pair.Value.GetColor();
            sb.Append((first ? "" : ",") + $"{pair.Key},{Mathf.RoundToInt(c.r*255f)},{Mathf.RoundToInt(c.g * 255f)},{Mathf.RoundToInt(c.b * 255f)}");
            first = false;
        }
        return sb.ToString();
    }

    public void SaveToClipboard()
    {
        string s = GetCharacterString();
        GUIUtility.systemCopyBuffer = s;
    }

    public void LoadFromClipboard()
    {
        string s = GUIUtility.systemCopyBuffer;
        SetCharacterFromString(s);
    }

    public void SaveAndApply()
    {
        string s = GetCharacterString();
        ServerLink.instance.SaveCharacterString(s);
        CloseWindow();
    }

    public void CloseWindow()
    {
        uiWidget.Close();
    }
    #endregion

    #region Item Selection
    public void SelectNextItem(PlayerAppearanceItem.ItemType type)
    {
        switch(type)
        {
            case PlayerAppearanceItem.ItemType.Mane:
                SelectNextMane();
                break;
        }
    }

    public void SelectPreviousItem(PlayerAppearanceItem.ItemType type)
    {
        switch (type)
        {
            case PlayerAppearanceItem.ItemType.Mane:
                SelectPreviousMane();
                break;
        }
    }

    private void SelectNextMane()
    {
        if(appearance.currentSelectedManeIndex < ServerLink.playerInfo.unlockedManes.Count - 1)
        {
            appearance.currentSelectedManeIndex++;
            SetSelectedMane(ServerLink.playerInfo.unlockedManes[appearance.currentSelectedManeIndex]);
        }
    }
    private void SelectPreviousMane()
    {
        if (appearance.currentSelectedManeIndex > 0)
        {
            appearance.currentSelectedManeIndex--;
            SetSelectedMane(ServerLink.playerInfo.unlockedManes[appearance.currentSelectedManeIndex]);
        }
    }

    private void SetSelectedMane(uint id, bool refresh = true)
    {
        if(registeredAppearanceItems.ContainsKey(id))
        { 
            PlayerAppearanceItemList l = registeredAppearanceItems[id];
            appearance.itemList.RemoveAll(e => registeredAppearanceItems[e].itemType == PlayerAppearanceItem.ItemType.Mane);
            appearance.itemList.Add(id);
            maneNameText.text = l.itemName;
            if(refresh) RefreshAppearanceList();
        }
    }
    #endregion

    #region Graphics
    public void UpdateColorTag(string tag)
    {
        Debug.Log("updating: " + tag);
        for(int i = 0; i < previewItems.Count; i++)
        {
            if(previewItems[i].colorTag == tag)
            {
                previewItems[i].rawImage.color = colorLookups[tag].GetColor();
            }
        }
    }

    public void RefreshAppearanceList(bool storeLookup = true)
    {
        if (storeLookup)
        {
            foreach (var pair in colorLookups)
            {
                appearance.colors[pair.Key] = pair.Value.GetColor();
            }
        }

        foreach (Transform t in contentParent)
        {
            Destroy(t.gameObject);
        }
        foreach (Transform t in previewRendererParent)
        {
            Destroy(t.gameObject);
        }



        colorLookups.Clear();
        previewItems.Clear();
        var list = appearance.itemList.OrderBy(e => registeredAppearanceItems[e].renderOrder).ToList();
        float y = 0;
        foreach (var pair in list)
        {
            registeredAppearanceItems[pair].items = registeredAppearanceItems[pair].items.OrderBy(e => e.renderOrderOffset).ToList();
            for (int i = 0; i < registeredAppearanceItems[pair].items.Count; i++)
            {
                PlayerAppearanceItem item = registeredAppearanceItems[pair].items[i];
                CharacterPreviewItem prev = new CharacterPreviewItem();
                prev.rawImage = Instantiate(previewItemPrefab, previewRendererParent).GetComponent<RawImage>();
                prev.rawImage.texture = item.sprite.texture;

                if (!string.IsNullOrWhiteSpace(item.colorTag))
                {
                    if (!colorLookups.ContainsKey(item.colorTag))
                    {
                        GameObject o = Instantiate(item.uiEditPrefab.gameObject, contentParent);
                        RectTransform t = o.GetComponent<RectTransform>();
                        t.localPosition = new Vector2(0, y);
                        t.Find("Element Name").GetComponent<TMP_Text>().text = item.colorTag;
                        colorLookups[item.colorTag] = t.Find("FlexibleColorPicker").GetComponent<FlexibleColorPicker>();

                        if (appearance.colors.ContainsKey(item.colorTag))
                            colorLookups[item.colorTag].SetColor(appearance.colors[item.colorTag]);

                        y -= item.uiEditPrefab.rect.height;
                    }

                    prev.colorTag = item.colorTag;
                    colorLookups[item.colorTag].appearanceMenu = this;
                    colorLookups[item.colorTag].colorTag = item.colorTag;
                    previewItems.Add(prev);

                }
            }
        }
    }
    #endregion
}
