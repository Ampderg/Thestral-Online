using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System;

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
    private List<uint> list;

    private Dictionary<string, Color> previousColorLookup = new Dictionary<string, Color>();
    private Dictionary<string, FlexibleColorPicker> colorLookups = new Dictionary<string, FlexibleColorPicker>();
    private List<CharacterPreviewItem> previewItems = new List<CharacterPreviewItem>();

    [SerializeField]
    private List<uint> unlockedManes;
    [SerializeField]
    private List<uint> unlockedTails;

    [SerializeField]
    private GameObject previewItemPrefab;

    private int currentSelectedManeIndex = 0;
    [SerializeField]
    private TMP_Text maneNameText;

    private Dictionary<uint, PlayerAppearanceItemList> registeredAppearanceItems;

    [SerializeField]
    private PlayerAppearanceItemList[] appearenceItemsToRegister;

    // Start is called before the first frame update
    void Start()
    {
        registeredAppearanceItems = new Dictionary<uint, PlayerAppearanceItemList>();
        previousColorLookup = new Dictionary<string, Color>();
        for(uint i = 0; i < appearenceItemsToRegister.Length; i++)
        {
            registeredAppearanceItems[i] = appearenceItemsToRegister[i];
        }
        appearenceItemsToRegister = null;
        RefreshAppearanceList();
    }

    #region Serialization
    public void SetCharacterFromString(string s)
    {
        try
        {
            list.Clear();
            string[] lines = s.Split('\n');

            //list ids
            string[] tokens = lines[0].Split('|');
            for (int i = 0; i < tokens.Length; i++)
            {
                list.Add(uint.Parse(tokens[i]));
            }

            //equipped bits
            tokens = lines[1].Split('|');
            for(int i = 0; i < tokens.Length - 1; i += 2)
            {
                string type = tokens[i];
                switch(type)
                {
                    case "m":
                        currentSelectedManeIndex = unlockedManes.FindIndex(e => e == uint.Parse(tokens[i + 1]));
                        SetSelectedMane(unlockedManes[currentSelectedManeIndex], false);
                        break;
                }
            }

            //colors
            tokens = lines[2].Split('|');
            for (int i = 0; i < tokens.Length - 1; i += 4)
            {
                previousColorLookup[tokens[i]] = new Color(int.Parse(tokens[i+1])/ 255f,
                    int.Parse(tokens[i + 2]) / 255f, int.Parse(tokens[i + 3]) / 255f);
            }
        }
        catch(Exception ex) when (ex is FormatException || ex is IndexOutOfRangeException)
        {
            Debug.Log("String invalid!");
        }
        RefreshAppearanceList(false);
    }

    public string GetCharacterString()
    {
        if (list == null || list.Count == 0) return "";

        StringBuilder sb = new StringBuilder();
        bool first = true;
        for (int i = 0; i < list.Count; i++)
        {
            sb.Append((first ? "" : "|") + list[i]);
            first = false;
        }
        sb.Append('\n');
        sb.Append($"m|{unlockedManes[currentSelectedManeIndex]}");
        sb.Append('\n');
        first = true;
        foreach(var pair in colorLookups)
        {
            Color c = pair.Value.GetColor();
            sb.Append((first ? "" : "|") + $"{pair.Key}|{Mathf.RoundToInt(c.r*255f)}|{Mathf.RoundToInt(c.g * 255f)}|{Mathf.RoundToInt(c.b * 255f)}");
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
        if(currentSelectedManeIndex < unlockedManes.Count - 1)
        {
            currentSelectedManeIndex++;
            SetSelectedMane(unlockedManes[currentSelectedManeIndex]);
        }
    }
    private void SelectPreviousMane()
    {
        if (currentSelectedManeIndex > 0)
        {
            currentSelectedManeIndex--;
            SetSelectedMane(unlockedManes[currentSelectedManeIndex]);
        }
    }

    private void SetSelectedMane(uint id, bool refresh = true)
    {
        if(registeredAppearanceItems.ContainsKey(id))
        { 
            PlayerAppearanceItemList l = registeredAppearanceItems[id];
            list.RemoveAll(e => registeredAppearanceItems[e].itemType == PlayerAppearanceItem.ItemType.Mane);
            list.Add(id);
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
                previousColorLookup[pair.Key] = pair.Value.GetColor();
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
        list = list.OrderBy(e => registeredAppearanceItems[e].renderOrder).ToList();
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

                        if (previousColorLookup.ContainsKey(item.colorTag))
                            colorLookups[item.colorTag].SetColor(previousColorLookup[item.colorTag]);

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
