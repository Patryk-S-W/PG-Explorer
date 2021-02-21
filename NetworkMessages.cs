
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class LoginMsg : MessageBase
{
    public string account;
    public string password;
    public string version;
}
public class CharacterCreateMsg : MessageBase
{
    public string name;
    public int classIndex;
}
public partial class CharacterSelectMsg : MessageBase
{
    public int value;
}
public partial class CharacterDeleteMsg : MessageBase
{
    public int value;
}
public class ErrorMsg : MessageBase
{
    public string text;
    public bool causesDisconnect;
}
public partial class LoginSuccessMsg : MessageBase
{
}
public class CharactersAvailableMsg : MessageBase
{
    public struct CharacterPreview
    {
        public string name;
        public string className; 
    }
    public CharacterPreview[] characters;
    public void Load(List<Player> players)
    {
        characters = new CharacterPreview[players.Count];
        for (int i = 0; i < players.Count; ++i)
        {
            Player player = players[i];
            characters[i] = new CharacterPreview
            {
                name = player.name,
                className = player.className
            };
        }
    }
}