using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAppearanceContainer
{
    public List<uint> itemList;

    public int currentSelectedManeIndex;

    public Dictionary<string, Color> colors;

    public PlayerAppearanceContainer()
    {
        itemList = new List<uint>();
        currentSelectedManeIndex = 0;
        colors = new Dictionary<string, Color>();
    }

    public PlayerAppearanceContainer(string s, List<uint> unlockedManes)
    {
        itemList = new List<uint>();
        currentSelectedManeIndex = 0;
        colors = new Dictionary<string, Color>();

        try
        {
            string[] lines = s.Split('/');

            //list ids
            string[] tokens = lines[0].Split(',');
            for (int i = 0; i < tokens.Length; i++)
            {
                itemList.Add(uint.Parse(tokens[i]));
            }

            //equipped bits
            tokens = lines[1].Split(',');
            for (int i = 0; i < tokens.Length - 1; i += 2)
            {
                string type = tokens[i];
                switch (type)
                {
                    case "m":
                        currentSelectedManeIndex = unlockedManes.FindIndex(e => e == uint.Parse(tokens[i + 1]));
                        break;
                }
            }

            //colors
            tokens = lines[2].Split(',');
            for (int i = 0; i < tokens.Length - 1; i += 4)
            {
                colors[tokens[i]] = new Color(int.Parse(tokens[i + 1]) / 255f,
                    int.Parse(tokens[i + 2]) / 255f, int.Parse(tokens[i + 3]) / 255f);
            }
        }
        catch (Exception ex) when (ex is FormatException || ex is IndexOutOfRangeException)
        {
            Debug.Log("String invalid!");
            itemList = new List<uint>();
            currentSelectedManeIndex = 0;
            colors = new Dictionary<string, Color>();
        }
    }
}
