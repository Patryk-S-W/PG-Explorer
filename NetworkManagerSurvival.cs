
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Mirror;
public enum NetworkState {Offline, Handshake, Lobby, World}
[RequireComponent(typeof(Database))]
public class NetworkManagerSurvival : NetworkManager
{
    public NetworkState state = NetworkState.Offline;
    public Dictionary<NetworkConnection, string> lobby = new Dictionary<NetworkConnection, string>();
    [Header("UI")]
    public UIPopup uiPopup;
    [Serializable]
    public class ServerInfo
    {
        public string name;
        public string ip;
    }
    public List<ServerInfo> serverList = new List<ServerInfo>()
    {
        new ServerInfo{name="Testowy", ip="164.90.176.169"}
    };
    [Header("Logout")]
    [Tooltip("Players shouldn't be able to log out instantly to flee combat. There should be a delay.")]
    public float combatLogoutDelay = 5;
    [Header("Database")]
    public int characterLimit = 4;
    public int characterNameMaxLength = 16;
    public float saveInterval = 60f; 
    [Header("Debug")]
    public bool showDebugGUI = true;
    [HideInInspector] public List<GameObject> playerClasses = new List<GameObject>();
    [HideInInspector] public CharactersAvailableMsg charactersAvailableMsg;
    public bool IsAllowedCharacterName(string characterName)
    {
        return characterName.Length <= characterNameMaxLength &&
               Regex.IsMatch(characterName, @"^[a-zA-Z0-9_]+$");
    }
    public List<GameObject> FindPlayerClasses()
    {
        List<GameObject> classes = new List<GameObject>();
        foreach (GameObject go in spawnPrefabs)
            if (go.GetComponent<Player>() != null)
                classes.Add(go);
        return classes;
    }
    public override void Awake()
    {
        base.Awake();
        playerClasses = FindPlayerClasses();
    }
    void Update()
    {
        if (ClientScene.localPlayer != null)
            state = NetworkState.World;
    }
    public void ServerSendError(NetworkConnection conn, string error, bool disconnect)
    {
        conn.Send(new ErrorMsg{text=error, causesDisconnect=disconnect});
    }
    void OnClientError(NetworkConnection conn, ErrorMsg message)
    {
        print("OnClientError: " + message.text);
        uiPopup.Show(message.text);
        if (message.causesDisconnect)
        {
            conn.Disconnect();
            if (NetworkServer.active) StopHost();
        }
    }
    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<ErrorMsg>(OnClientError, false); 
        NetworkClient.RegisterHandler<CharactersAvailableMsg>(OnClientCharactersAvailable);
    }
    public override void OnStartServer()
    {
        Database.singleton.Connect();
        NetworkServer.RegisterHandler<CharacterCreateMsg>(OnServerCharacterCreate);
        NetworkServer.RegisterHandler<CharacterSelectMsg>(OnServerCharacterSelect);
        NetworkServer.RegisterHandler<CharacterDeleteMsg>(OnServerCharacterDelete);
        Database.singleton.LoadStructures();
        InvokeRepeating(nameof(Save), saveInterval, saveInterval);
    }
    public override void OnStopServer()
    {
        print("OnStopServer");
        CancelInvoke(nameof(Save));
    }
    public bool IsConnecting() => NetworkClient.active && !ClientScene.ready;
    public override void OnClientConnect(NetworkConnection conn)
    {
    }
    public override void OnServerConnect(NetworkConnection conn)
    {
        string account = lobby[conn];
        conn.Send(MakeCharactersAvailableMessage(account));
    }
    public override void OnClientSceneChanged(NetworkConnection conn) {}
    CharactersAvailableMsg MakeCharactersAvailableMessage(string account)
    {
        List<string> names = Database.singleton.CharactersForAccount(account);
        List<Player> characters = new List<Player>();
        foreach (string character in names)
        {
            GameObject player = Database.singleton.CharacterLoad(character, playerClasses, true);
            characters.Add(player.GetComponent<Player>());
        }
        CharactersAvailableMsg message = new CharactersAvailableMsg();
        message.Load(characters);
        characters.ForEach(player => Destroy(player.gameObject));
        return message;
    }
    void OnClientCharactersAvailable(NetworkConnection conn, CharactersAvailableMsg message)
    {
        charactersAvailableMsg = message;
        print("characters available:" + charactersAvailableMsg.characters.Length);
        state = NetworkState.Lobby;
    }
    Player CreateCharacter(GameObject classPrefab, string characterName, string account)
    {
        Player player = Instantiate(classPrefab).GetComponent<Player>();
        player.name = characterName;
        player.account = account;
        player.className = classPrefab.name;
        player.transform.position = GetStartPosition().position;
        for (int i = 0; i < player.inventory.size; ++i)
        {
            player.inventory.slots.Add(i < player.inventory.defaultItems.Length ? new ItemSlot(new Item(player.inventory.defaultItems[i].item), player.inventory.defaultItems[i].amount) : new ItemSlot());
        }
        for (int i = 0; i < player.equipment.slotInfo.Length; ++i)
        {
            EquipmentInfo info = player.equipment.slotInfo[i];
            player.equipment.slots.Add(info.defaultItem.item != null ? new ItemSlot( new Item(info.defaultItem.item), info.defaultItem.amount) : new ItemSlot());
        }
        for (int i = 0; i < player.hotbar.size; ++i)
        {
            player.hotbar.slots.Add(i < player.hotbar.defaultItems.Length ? new ItemSlot(new Item(player.hotbar.defaultItems[i])) : new ItemSlot());
        }
        foreach (Energy energy in player.GetComponents<Energy>())
            energy.current = energy.max;
        return player;
    }
    void OnServerCharacterCreate(NetworkConnection conn, CharacterCreateMsg message)
    {
        if (lobby.ContainsKey(conn))
        {
            if (IsAllowedCharacterName(message.name))
            {
                string account = lobby[conn];
                if (!Database.singleton.CharacterExists(message.name))
                {
                    if (Database.singleton.CharactersForAccount(account).Count < characterLimit)
                    {
                        if (0 <= message.classIndex && message.classIndex < playerClasses.Count)
                        {
                            Player player = CreateCharacter(playerClasses[message.classIndex], message.name, account);
                            Database.singleton.CharacterSave(player, false);
                            Destroy(player.gameObject);
                            conn.Send(MakeCharactersAvailableMessage(account));
                        }
                        else
                        {
                            ServerSendError(conn, "character invalid class", false);
                        }
                    }
                    else
                    {
                        ServerSendError(conn, "character limit reached", false);
                    }
                }
                else
                {
                    ServerSendError(conn, "name already exists", false);
                }
            }
            else
            {
                ServerSendError(conn, "character name not allowed", false);
            }
        }
        else
        {
            ServerSendError(conn, "CharacterCreate: not in lobby", true);
        }
    }
    public override void OnServerAddPlayer(NetworkConnection conn) { Debug.LogWarning("Use the CharacterSelectMsg instead"); }
    void OnServerCharacterSelect(NetworkConnection conn, CharacterSelectMsg message)
    {
        if (lobby.ContainsKey(conn))
        {
            string account = lobby[conn];
            List<string> characters = Database.singleton.CharactersForAccount(account);
            if (0 <= message.value && message.value < characters.Count)
            {
                GameObject go = Database.singleton.CharacterLoad(characters[message.value], playerClasses, false);
                NetworkServer.AddPlayerForConnection(conn, go);
                lobby.Remove(conn);
            }
            else
            {
                print("invalid character index: " + account + " " + message.value);
                ServerSendError(conn, "invalid character index", false);
            }
        }
        else
        {
            print("AddPlayer: not in lobby" + conn);
            ServerSendError(conn, "AddPlayer: not in lobby", true);
        }
    }
    void OnServerCharacterDelete(NetworkConnection conn, CharacterDeleteMsg message)
    {
        if (lobby.ContainsKey(conn))
        {
            string account = lobby[conn];
            List<string> characters = Database.singleton.CharactersForAccount(account);
            if (0 <= message.value && message.value < characters.Count)
            {
                print("delete character: " + characters[message.value]);
                Database.singleton.CharacterDelete(characters[message.value]);
                conn.Send(MakeCharactersAvailableMessage(account));
            }
            else
            {
                print("invalid character index: " + account + " " + message.value);
                ServerSendError(conn, "invalid character index", false);
            }
        }
        else
        {
            print("CharacterDelete: not in lobby: " + conn);
            ServerSendError(conn, "CharacterDelete: not in lobby", true);
        }
    }
    void Save()
    {
        Database.singleton.CharacterSaveMany(Player.onlinePlayers.Values);
        if (Player.onlinePlayers.Count > 0) Debug.Log("saved " + Player.onlinePlayers.Count + " player(s)");
        Database.singleton.SaveStorages(Storage.storages.Values);
        if (Storage.storages.Count > 0) Debug.Log("saved " + Storage.storages.Count + " storage(s)");
        Database.singleton.SaveFurnaces(Furnace.furnaces.Values);
        if (Furnace.furnaces.Count > 0) Debug.Log("saved " + Furnace.furnaces.Count + " furnace(s)");
        Database.singleton.SaveStructures(Structure.structures);
        if (Structure.structures.Count > 0) Debug.Log("saved " + Structure.structures.Count + " structure(s)");
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        print("OnServerDisconnect " + conn);
        float delay = 0;
        if (conn.identity != null)
        {
            Player player = conn.identity.GetComponent<Player>();
            delay = (float)player.remainingLogoutTime;
        }
        StartCoroutine(DoServerDisconnect(conn, delay));
    }
    IEnumerator<WaitForSeconds> DoServerDisconnect(NetworkConnection conn, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (conn.identity != null)
        {
            Database.singleton.CharacterSave(conn.identity.GetComponent<Player>(), false);
            print("saved:" + conn.identity.name);
        }
        lobby.Remove(conn); 
        base.OnServerDisconnect(conn);
    }
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        print("OnClientDisconnect");
        Camera mainCamera = Camera.main;
        if (mainCamera.transform.parent != null)
            mainCamera.transform.SetParent(null);
        uiPopup.Show("Disconnected.");
        base.OnClientDisconnect(conn);
        state = NetworkState.Offline;
    }
    public static void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public override void OnValidate()
    {
        base.OnValidate();
        if (!Application.isPlaying && networkAddress != "")
            networkAddress = "Use the Server List below!";
    }
}
