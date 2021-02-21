
using System;
using UnityEngine;
using Mirror;
[Serializable]
public class ChannelInfo
{
    public string command; 
    public string identifierOut; 
    public string identifierIn; 
    public GameObject textPrefab;
    public ChannelInfo(string command, string identifierOut, string identifierIn, GameObject textPrefab)
    {
        this.command = command;
        this.identifierOut = identifierOut;
        this.identifierIn = identifierIn;
        this.textPrefab = textPrefab;
    }
}
[Serializable]
public struct ChatMessage
{
    public string sender;
    public string identifier;
    public string message;
    public string replyPrefix; 
    public GameObject textPrefab;
    public ChatMessage(string sender, string identifier, string message, string replyPrefix, GameObject textPrefab)
    {
        this.sender = sender;
        this.identifier = identifier;
        this.message = message;
        this.replyPrefix = replyPrefix;
        this.textPrefab = textPrefab;
    }
    public string Construct()
    {
        return "<b>" + sender + identifier + ":</b> " + message;
    }
}
public class PlayerChat : NetworkBehaviourNonAlloc
{
    [Header("Channels")]
    public ChannelInfo whisper = new ChannelInfo("/w", "(TO)", "(FROM)", null);
    public ChannelInfo local = new ChannelInfo("", "", "", null);
    public ChannelInfo info = new ChannelInfo("", "(Info)", "(Info)", null);
    [Header("Other")]
    public int maxLength = 70;
    public override void OnStartLocalPlayer()
    {
    }
    [Client]
    public string OnSubmit(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            string lastcommand = "";
            if (text.StartsWith(whisper.command))
            {
                string[] parsed = ParsePM(whisper.command, text);
                string user = parsed[0];
                string msg = parsed[1];
                if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(msg))
                {
                    if (user != name)
                    {
                        lastcommand = whisper.command + " " + user + " ";
                        CmdMsgWhisper(user, msg);
                    }
                    else print("cant whisper to self");
                }
                else print("invalid whisper format: " + user + "/" + msg);
            }
            else if (!text.StartsWith("/"))
            {
                lastcommand = "";
                CmdMsgLocal(text);
            }
            return lastcommand;
        }
        return "";
    }
    static string ParseGeneral(string command, string msg)
    {
		
        return msg.StartsWith(command + " ") ? msg.Substring(command.Length + 1) : "";
    }
    static string[] ParsePM(string command, string pm)
    {
        string content = ParseGeneral(command, pm);
        if (content != "")
        {
            int i = content.IndexOf(" ");
            if (i >= 0)
            {
                string user = content.Substring(0, i);
                string msg = content.Substring(i+1);
                return new string[] {user, msg};
            }
        }
        return new string[] {"", ""};
    }
    [Command]
    void CmdMsgLocal(string message)
    {
        if (message.Length > maxLength) return;
        RpcMsgLocal(name, message);
    }
    [Command]
    void CmdMsgWhisper(string playerName, string message)
    {
        if (message.Length > maxLength) return;
        if (Player.onlinePlayers.TryGetValue(playerName, out Player player))
        {
            player.chat.TargetMsgWhisperFrom(name, message);
            TargetMsgWhisperTo(playerName, message);
        }
    }
    [Server]
    public void SendGlobalMessage(string message)
    {
        foreach (Player player in Player.onlinePlayers.Values)
            player.chat.TargetMsgInfo(message);
    }
    [TargetRpc]
    public void TargetMsgWhisperFrom(string sender, string message)
    {
        string identifier = whisper.identifierIn;
        string reply = whisper.command + " " + sender + " "; 
        UIChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, reply, whisper.textPrefab));
    }
    [TargetRpc]
    public void TargetMsgWhisperTo(string receiver, string message)
    {
        string identifier = whisper.identifierOut;
        string reply = whisper.command + " " + receiver + " "; 
        UIChat.singleton.AddMessage(new ChatMessage(receiver, identifier, message, reply, whisper.textPrefab));
    }
    [ClientRpc]
    public void RpcMsgLocal(string sender, string message)
    {
        string identifier = sender != name ? local.identifierIn : local.identifierOut;
        string reply = whisper.command + " " + sender + " "; 
        UIChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, reply, local.textPrefab));
    }
    [TargetRpc]
    public void TargetMsgInfo(string message)
    {
        UIChat.singleton.AddMessage(new ChatMessage("", info.identifierIn, message, "", info.textPrefab));
    }
}
