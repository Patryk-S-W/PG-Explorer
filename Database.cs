
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;
using Mirror;
using SQLite;
public class Database : MonoBehaviour
{
    public static Database singleton;
    public string databaseFile = "Database.sqlite";
    public SQLiteConnection connection;
    class accounts
    {
        [PrimaryKey] 
        public string name { get; set; }
        public string password { get; set; }
        public DateTime created { get; set; }
        public DateTime lastlogin { get; set; }
        public bool banned { get; set; }
    }
    class characters
    {
        [PrimaryKey] 
        [Collation("NOCASE")] 
        public string name { get; set; }
        [Indexed] 
        public string account { get; set; }
        public string classname { get; set; } 
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float yrotation { get; set; }
        public int health { get; set; }
        public bool online { get; set; }
        public DateTime lastsaved { get; set; }
        public bool deleted { get; set; }
    }
    class character_inventory
    {
        public string character { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
        public int ammo { get; set; }
        public int durability { get; set; }
    }
    class character_equipment : character_inventory
    {
    }
    class character_hotbar : character_inventory
    {
    }
    class character_hotbar_selection
    {
        [PrimaryKey] 
        public string character { get; set; }
        public int selection { get; set; }
    }
    class storages
    {
        public string storage { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
        public int ammo { get; set; }
        public int durability { get; set; }
    }
    class furnaces
    {
        public string furnace { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
        public int ammo { get; set; }
        public int durability { get; set; }
    }
    class structures
    {
        public string name { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float xrotation { get; set; }
        public float yrotation { get; set; }
        public float zrotation { get; set; }
    }
    [Header("Events")]
    public UnityEvent onConnected;
    public UnityEventPlayer onCharacterLoad;
    public UnityEventPlayer onCharacterSave;
    void Awake()
    {
        if (singleton == null) singleton = this;
    }
    public void Connect()
    {
        if (singleton == null) singleton = this;
#if UNITY_EDITOR
        string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, databaseFile);
#elif UNITY_ANDROID
        string path = Path.Combine(Application.persistentDataPath, databaseFile);
#elif UNITY_IOS
        string path = Path.Combine(Application.persistentDataPath, databaseFile);
#else
        string path = Path.Combine(Application.dataPath, databaseFile);
#endif
        connection = new SQLiteConnection(path);
        connection.CreateTable<accounts>();
        connection.CreateTable<characters>();
        connection.CreateTable<character_inventory>();
        connection.CreateIndex(nameof(character_inventory), new []{"character", "slot"});
        connection.CreateTable<character_equipment>();
        connection.CreateIndex(nameof(character_equipment), new []{"character", "slot"});
        connection.CreateTable<character_hotbar>();
        connection.CreateIndex(nameof(character_hotbar), new []{"character", "slot"});
        connection.CreateTable<character_hotbar_selection>();
        connection.CreateTable<storages>();
        connection.CreateIndex(nameof(storages), new []{"storage", "slot"});
        connection.CreateTable<furnaces>();
        connection.CreateIndex(nameof(furnaces), new []{"furnace", "slot"});
        connection.CreateTable<structures>();
        onConnected.Invoke();
        Debug.Log("connected to database");
    }
    void OnApplicationQuit()
    {
        connection?.Close();
    }
    public bool TryLogin(string account, string password)
    {
        
        
        if (!string.IsNullOrWhiteSpace(account) && !string.IsNullOrWhiteSpace(password))
        {
            if (connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE name=?", account) == null)
                connection.Insert(new accounts{ name=account, password=password, created=DateTime.UtcNow, lastlogin=DateTime.Now, banned=false});
            if (connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE name=? AND password=? and banned=0", account, password) != null)
            {
                connection.Execute("UPDATE accounts SET lastlogin=? WHERE name=?", DateTime.UtcNow, account);
                return true;
            }
        }
        return false;
    }
    public bool CharacterExists(string characterName)
    {
        return connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name=?", characterName) != null;
    }
    public void CharacterDelete(string characterName)
    {
        connection.Execute("UPDATE characters SET deleted=1 WHERE name=?", characterName);
    }
    public List<string> CharactersForAccount(string account)
    {
        List<string> result = new List<string>();
        foreach (characters character in connection.Query<characters>("SELECT * FROM characters WHERE account=? AND deleted=0", account))
            result.Add(character.name);
        return result;
    }
    void LoadInventory(PlayerInventory inventory)
    {
        for (int i = 0; i < inventory.size; ++i)
            inventory.slots.Add(new ItemSlot());
        foreach (character_inventory row in connection.Query<character_inventory>("SELECT * FROM character_inventory WHERE character=?", inventory.name))
        {
            if (row.slot < inventory.size)
            {
                if (ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
                {
                    Item item = new Item(itemData);
                    item.ammo = row.ammo;
                    item.durability = Mathf.Min(row.durability, item.maxDurability);
                    inventory.slots[row.slot] = new ItemSlot(item, row.amount);
                }
                else Debug.LogWarning("LoadInventory: skipped item " + row.name + " for " + inventory.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
            }
            else Debug.LogWarning("LoadInventory: skipped slot " + row.slot + " for " + inventory.name + " because it's bigger than size " + inventory.size);
        }
    }
    void LoadEquipment(PlayerEquipment equipment)
    {
        for (int i = 0; i < equipment.slotInfo.Length; ++i)
            equipment.slots.Add(new ItemSlot());
        foreach (character_equipment row in connection.Query<character_equipment>("SELECT * FROM character_equipment WHERE character=?", equipment.name))
        {
            if (row.slot < equipment.slotInfo.Length)
            {
                if (ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
                {
                    Item item = new Item(itemData);
                    item.ammo = row.ammo;
                    item.durability = Mathf.Min(row.durability, item.maxDurability);
                    equipment.slots[row.slot] = new ItemSlot(item, row.amount);
                }
                else Debug.LogWarning("LoadEquipment: skipped item " + row.name + " for " + equipment.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
            }
            else Debug.LogWarning("LoadEquipment: skipped slot " + row.slot + " for " + equipment.name + " because it's bigger than size " + equipment.slotInfo.Length);
        }
    }
    void LoadHotbar(PlayerHotbar hotbar)
    {
        for (int i = 0; i < hotbar.size; ++i)
            hotbar.slots.Add(new ItemSlot());
        foreach (character_hotbar row in connection.Query<character_hotbar>("SELECT * FROM character_hotbar WHERE character=?", hotbar.name))
        {
            if (row.slot < hotbar.size)
            {
                if (ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
                {
                    Item item = new Item(itemData);
                    item.ammo = row.ammo;
                    item.durability = Mathf.Min(row.durability, item.maxDurability);
                    hotbar.slots[row.slot] = new ItemSlot(item, row.amount);
                }
                else Debug.LogWarning("LoadHotbar: skipped item " + row.name + " for " + hotbar.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
            }
            else Debug.LogWarning("LoadHotbar: skipped slot " + row.slot + " for " + hotbar.name + " because it's bigger than size " + hotbar.size);
        }
    }
    void LoadHotbarSelection(PlayerHotbar hotbar)
    {
        character_hotbar_selection row = connection.FindWithQuery<character_hotbar_selection>("SELECT * FROM character_hotbar_selection WHERE character=?", hotbar.name);
        if (row != null)
        {
            hotbar.selection = row.selection;
        }
    }
    public GameObject CharacterLoad(string characterName, List<GameObject> prefabs, bool isPreview)
    {
        characters row = connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name=? AND deleted=0", characterName);
        if (row != null)
        {
            GameObject prefab = prefabs.Find(p => p.name == row.classname);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab.gameObject);
                Player player = go.GetComponent<Player>();
                player.name               = row.name;
                player.account            = row.account;
                player.className          = row.classname;
                player.transform.position = new Vector3(row.x, row.y, row.z);
                player.transform.rotation = Quaternion.Euler(0, row.yrotation, 0);
                LoadInventory(player.inventory);
                LoadEquipment(player.equipment);
                LoadHotbar(player.hotbar);
                LoadHotbarSelection(player.hotbar);
                player.health.current = row.health;
                
                if (!isPreview)
                    connection.Execute("UPDATE characters SET online=1, lastsaved=? WHERE name=?", characterName, DateTime.UtcNow);
                onCharacterLoad.Invoke(player);
                return go;
            }
            else Debug.LogError("no prefab found for class: " + row.classname);
        }
        return null;
    }
    void SaveInventory(PlayerInventory inventory)
    {
        connection.Execute("DELETE FROM character_inventory WHERE character=?", inventory.name);
        for (int i = 0; i < inventory.slots.Count; ++i)
        {
            ItemSlot slot = inventory.slots[i];
            if (slot.amount > 0) 
            {
                connection.InsertOrReplace(new character_inventory{
                    character = inventory.name,
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount,
                    ammo = slot.item.ammo,
                    durability = slot.item.durability
                });
            }
        }
    }
    void SaveEquipment(PlayerEquipment equipment)
    {
        connection.Execute("DELETE FROM character_equipment WHERE character=?", equipment.name);
        for (int i = 0; i < equipment.slots.Count; ++i)
        {
            ItemSlot slot = equipment.slots[i];
            if (slot.amount > 0) 
            {
                connection.InsertOrReplace(new character_equipment{
                    character = equipment.name,
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount,
                    ammo = slot.item.ammo,
                    durability = slot.item.durability
                });
            }
        }
    }
    void SaveHotbar(PlayerHotbar hotbar)
    {
        connection.Execute("DELETE FROM character_hotbar WHERE character=?", hotbar.name);
        for (int i = 0; i < hotbar.slots.Count; ++i)
        {
            ItemSlot slot = hotbar.slots[i];
            if (slot.amount > 0) 
            {
                connection.InsertOrReplace(new character_hotbar{
                    character = hotbar.name,
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount,
                    ammo = slot.item.ammo,
                    durability = slot.item.durability
                });
            }
        }
    }
    void SaveHotbarSelection(PlayerHotbar hotbar)
    {
        connection.InsertOrReplace(new character_hotbar_selection{character=hotbar.name, selection=hotbar.selection});
    }
    public void CharacterSave(Player player, bool online, bool useTransaction = true)
    {
        if (useTransaction) connection.BeginTransaction();
        connection.InsertOrReplace(new characters{
            name = player.name,
            account = player.account,
            classname = player.className,
            x = player.transform.position.x,
            y = player.transform.position.y,
            z = player.transform.position.z,
            yrotation = player.transform.rotation.eulerAngles.y,
            health = player.health.current,
            online = online,
            lastsaved = DateTime.UtcNow
        });
        SaveInventory(player.inventory);
        SaveEquipment(player.equipment);
        SaveHotbar(player.hotbar);
        SaveHotbarSelection(player.hotbar);
        onCharacterSave.Invoke(player);
        if (useTransaction) connection.Commit();
    }
    public void CharacterSaveMany(IEnumerable<Player> players, bool online = true)
    {
        connection.BeginTransaction(); 
        foreach (Player player in players)
            CharacterSave(player, online, false);
        connection.Commit(); 
    }
    public void LoadStorage(Storage storage)
    {
        for (int i = 0; i < storage.size; ++i)
            storage.slots.Add(new ItemSlot());
        foreach (storages row in connection.Query<storages>("SELECT * FROM storages WHERE storage=?", storage.name))
        {
            if (row.slot < storage.size &&
                ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = new Item(itemData);
                item.ammo = row.ammo;
                item.durability = Mathf.Min(row.durability, item.maxDurability);
                storage.slots[row.slot] = new ItemSlot(item, row.amount);
            }
        }
    }
    void SaveStorage(Storage storage, bool useTransaction = true)
    {
        if (useTransaction) connection.BeginTransaction();
        connection.Execute("DELETE FROM storages WHERE storage=?", storage.name);
        for (int i = 0; i < storage.slots.Count; ++i)
        {
            ItemSlot slot = storage.slots[i];
            if (slot.amount > 0) 
            {
                connection.InsertOrReplace(new storages{
                    storage = storage.name,
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount,
                    ammo = slot.item.ammo,
                    durability = slot.item.durability
                });
            }
        }
        if (useTransaction) connection.Commit();
    }
    public void SaveStorages(IEnumerable<Storage> storages)
    {
        connection.BeginTransaction(); 
        foreach (Storage storage in storages)
            SaveStorage(storage, false);
        connection.Commit(); 
    }
    public void LoadFurnace(Furnace furnace)
    {
        foreach (furnaces row in connection.Query<furnaces>("SELECT * FROM furnaces WHERE furnace=?", furnace.name))
        {
            if (ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = new Item(itemData);
                item.ammo = row.ammo;
                item.durability = Mathf.Min(row.durability, item.maxDurability);
                if (row.slot == 0)
                    furnace.ingredientSlot = new ItemSlot(item, row.amount);
                else if (row.slot == 1)
                    furnace.fuelSlot = new ItemSlot(item, row.amount);
                else if (row.slot == 2)
                    furnace.resultSlot = new ItemSlot(item, row.amount);
            }
        }
    }
    void SaveFurnace(Furnace furnace, bool useTransaction = true)
    {
        if (useTransaction) connection.BeginTransaction();
        connection.Execute("DELETE FROM furnaces WHERE furnace=?", furnace.name);
        if (furnace.ingredientSlot.amount > 0) 
        {
            connection.InsertOrReplace(new furnaces{
                furnace = furnace.name,
                slot = 0,
                name = furnace.ingredientSlot.item.name,
                amount = furnace.ingredientSlot.amount,
                ammo = furnace.ingredientSlot.item.ammo,
                durability = furnace.ingredientSlot.item.durability
            });
        }
        if (furnace.fuelSlot.amount > 0) 
        {
            connection.InsertOrReplace(new furnaces{
                furnace = furnace.name,
                slot = 1,
                name = furnace.fuelSlot.item.name,
                amount = furnace.fuelSlot.amount,
                ammo = furnace.fuelSlot.item.ammo,
                durability = furnace.fuelSlot.item.durability
            });
        }
        if (furnace.resultSlot.amount > 0) 
        {
            connection.InsertOrReplace(new furnaces{
                furnace = furnace.name,
                slot = 2,
                name = furnace.resultSlot.item.name,
                amount = furnace.resultSlot.amount,
                ammo = furnace.resultSlot.item.ammo,
                durability = furnace.resultSlot.item.durability
            });
        }
        if (useTransaction) connection.Commit();
    }
    public void SaveFurnaces(IEnumerable<Furnace> furnaces)
    {
        connection.BeginTransaction(); 
        foreach (Furnace furnace in furnaces)
            SaveFurnace(furnace, false);
        connection.Commit(); 
    }
    void SaveStructure(Structure structure, bool useTransaction = true)
    {
        if (useTransaction) connection.BeginTransaction();
        Vector3 position = structure.transform.position;
        Vector3 rotation = structure.transform.rotation.eulerAngles;
        connection.Insert(new structures{
            name = structure.name,
            x = position.x,
            y = position.y,
            z = position.z,
            xrotation = rotation.x,
            yrotation = rotation.y,
            zrotation = rotation.z
        });
        if (useTransaction) connection.Commit();
    }
    public void SaveStructures(HashSet<Structure> structures)
    {
        connection.BeginTransaction(); 
        connection.DeleteAll<structures>();
        foreach (Structure structure in structures)
            SaveStructure(structure, false);
        connection.Commit(); 
    }
    public void LoadStructures()
    {
        Dictionary<string, GameObject> spawnable = NetworkManager.singleton.spawnPrefabs
                                                     .Where(p => p.CompareTag("Structure"))
                                                     .ToDictionary(p => p.name, p => p);
        foreach (structures row in connection.Query<structures>("SELECT * FROM structures"))
        {
            if (spawnable.ContainsKey(row.name))
            {
                Vector3 position = new Vector3(row.x, row.y, row.z);
                Quaternion rotation = Quaternion.Euler(row.xrotation, row.yrotation, row.zrotation);
                GameObject prefab = spawnable[row.name];
                GameObject go = Instantiate(prefab, position, rotation);
                go.name = prefab.name; 
                NetworkServer.Spawn(go);
            }
        }
    }
}