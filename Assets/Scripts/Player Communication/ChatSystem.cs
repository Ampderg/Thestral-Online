using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatSystem : MonoBehaviour
{
    private static ChatSystem instance;
    [SerializeField]
    private TMP_InputField inputField;
    [SerializeField]
    private TMP_Text chatboxText;

    private List<string> sentHistory;
    private bool canSend;

    public static bool IsFocused { get { return instance.canSend; } }

    public enum ChatCategory
    {
        Announcement,
        Say,
        Party,
        Whisper,
        Combat,
        Event,
        System
    }

    public struct ChatBoxItem
    {
        internal ChatCategory category;
        internal string username;
        internal string message;
    }

    public void OnInputSubmit()
    {
        string text = inputField.text;
        sentHistory.Add(text);
        inputField.text = "";
        inputField.interactable = false;
        inputField.interactable = true;
        ServerLink.SendChatMessage(text);
    }

    public static void OnRecieveChatMessage(string channel, string message)
    {
        ChatBoxItem item = new ChatBoxItem();
        item.message = message;
        item.username = "Player";
        try
        {
            item.category = (ChatCategory)Enum.Parse(typeof(ChatCategory), channel, true);
        }
        catch
        {
            item.category = ChatCategory.System;
        }
        instance.OnRecieveChatMessage(item);
    }

    private void OnRecieveChatMessage(ChatBoxItem message)
    {
        //TODO: add culling to improve chat memory usage
        //TODO: add category seperation
        chatboxText.text += MessageToString(message) + Environment.NewLine;
    }

    private string MessageToString(ChatBoxItem item)
    {
        string time = DateTime.Now.ToString("hh:mm:ss tt");
        return $"[{time}] {item.username}: {item.message}";
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        sentHistory = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {
        if(canSend && !string.IsNullOrWhiteSpace(inputField.text) && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            OnInputSubmit();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                inputField.Select();
            else
                canSend = inputField.isFocused;
        }
    }
}
