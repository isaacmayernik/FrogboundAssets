using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

// handles all save data
[System.Serializable]
public class SaveData
{
    public static SaveData Instance { get; private set; } = new SaveData();

    // map data
    public HashSet<string> sceneNames;

    // lilypad data
    public string lilypadSceneName = "";
    public Vector2 lilypadPos;

    // player data
    public int playerHealth;
    public int playerMaxHealth;
    public Vector2 playerPosition;
    public string lastScene;
    public bool playerUnlockedDodge;
    public bool playerDamageUpgrade;

    // scare room triggered
    public bool triggered;

    //king diptera
    public bool KDDefeated;

    private SaveData()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (sceneNames == null)
        {
            sceneNames = new HashSet<string>();
        }
        if (!File.Exists(Application.persistentDataPath + "/save.lilypad.data"))
        {
            BinaryWriter writer = new BinaryWriter(File.Create(Application.persistentDataPath + "/save.lilypad.data"));
        }
        if (!File.Exists(Application.persistentDataPath + "/save.player.data"))
        {
            BinaryWriter writer = new BinaryWriter(File.Create(Application.persistentDataPath + "/save.player.data"));
        }
        if (!File.Exists(Application.persistentDataPath + "/save.boss.data"))
        {
            BinaryWriter writer = new BinaryWriter(File.Create(Application.persistentDataPath + "/save.boss.data"));
        }
    }

    public void SaveLilypad()
    {
        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.persistentDataPath + "/save.lilypad.data")))
        {
            if (!string.IsNullOrEmpty(lilypadSceneName))
            {
                writer.Write(lilypadSceneName);
            }
            else
            {
                Debug.LogWarning("lilypadSceneName is null or empty; skipping save.");
            }
            writer.Write(lilypadPos.x);
            writer.Write(lilypadPos.y);
        }
    }

    public void LoadLilypad()
    {
        if (File.Exists(Application.persistentDataPath + "/save.lilypad.data"))
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(Application.persistentDataPath + "/save.lilypad.data")))
            {
                lilypadSceneName = reader.ReadString();
                lilypadPos.x = reader.ReadSingle();
                lilypadPos.y = reader.ReadSingle();
            }
        }
    }

    public void SaveBoss()
    {
        Debug.Log("Saving boss data...");
        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.persistentDataPath + "/save.boss.data")))
        {
            KDDefeated = GameManager.Instance.KDDefeated;
            writer.Write(KDDefeated);
        }
    }

    public void LoadBoss()
    {
        Debug.Log("Loading boss data...");
        if (File.Exists(Application.persistentDataPath + "/save.boss.data"))
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(Application.persistentDataPath + "/save.boss.data")))
            {
                if (reader.BaseStream.Length > 0)
                {
                    KDDefeated = reader.ReadBoolean();
                    GameManager.Instance.KDDefeated = KDDefeated;
                    Debug.Log($"KDDefeated status loaded: {KDDefeated}");
                }
                else
                {
                    Debug.LogWarning("Boss data file is empty.");
                }
            }
        }
        else
        {
            Debug.LogWarning("Boss data file does not exist.");
        }
    }

    public void SaveTriggerData()
    {
        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.persistentDataPath + "/save.trigger.data")))
        {
            if (RoomTrigger.Instance != null)
            {
                triggered = RoomTrigger.Instance.isTriggered;
                writer.Write(triggered);
                Debug.Log("Saving triggered state: " + triggered);
            }
            else
            {
                // if RoomTrigger is not present, write a default value (false)
                writer.Write(false);
            }
        }
    }

    public void LoadTriggerData()
    {
        string path = Application.persistentDataPath + "/save.trigger.data";
        if (File.Exists(path))
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                if (reader.BaseStream.Length > 0)
                {
                    triggered = reader.ReadBoolean();

                    if (RoomTrigger.Instance != null)
                    {
                        RoomTrigger.Instance.isTriggered = triggered;
                    }
                    else
                    {
                        Debug.LogWarning("RoomTrigger.Instance is null; unable to set isTriggered.");
                    }
                }
            }
        }
        else
        {
            RoomTrigger.Instance.isTriggered = false;
        }
    }

    public void SavePlayerData()
    {
        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.persistentDataPath + "/save.player.data")))
        {
            playerHealth = PlayerController.Instance.Health;
            writer.Write(playerHealth);
            playerMaxHealth = PlayerController.Instance.maxHealth;
            writer.Write(playerMaxHealth);

            playerUnlockedDodge = PlayerController.Instance.unlockedDodge;
            writer.Write(playerUnlockedDodge);

            playerDamageUpgrade = PlayerController.Instance.damageUpgraded;
            writer.Write(playerDamageUpgrade);

            playerPosition = PlayerController.Instance.transform.position;
            writer.Write(playerPosition.x);
            writer.Write(playerPosition.y);

            lastScene = SceneManager.GetActiveScene().name;
            writer.Write(lastScene);
        }
    }

    public void LoadPlayerData()
    {
        string path = Application.persistentDataPath + "/save.player.data";
        if (File.Exists(path))
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                if (reader.BaseStream.Length > 0)
                {
                    playerHealth = reader.ReadInt32();
                    playerMaxHealth = reader.ReadInt32();
                    playerUnlockedDodge = reader.ReadBoolean();
                    playerDamageUpgrade = reader.ReadBoolean();
                    playerPosition.x = reader.ReadSingle();
                    playerPosition.y = reader.ReadSingle();
                    lastScene = reader.ReadString();

                    SceneManager.LoadScene(lastScene);
                    PlayerController.Instance.transform.position = playerPosition;
                    PlayerController.Instance.health = playerHealth;
                    PlayerController.Instance.maxHealth = playerMaxHealth;
                    PlayerController.Instance.unlockedDodge = playerUnlockedDodge;
                    PlayerController.Instance.damageUpgraded = playerDamageUpgrade;
                }
                else
                {
                    Debug.Log("Player data file is empty.");
                }
            }
        }
        else
        {
            Debug.Log("Player data file does not exist.");
            PlayerController.Instance.Health = PlayerController.Instance.maxHealth;
            PlayerController.Instance.unlockedDodge = false;
            PlayerController.Instance.damageUpgraded = false;
            RoomTrigger.Instance.isTriggered = false;
        }
        LoadSceneNames();
    }

    public void SaveSceneNames()
    {
        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.persistentDataPath + "/save.scenes.data")))
        {
            writer.Write(sceneNames.Count);
            foreach (var scene in sceneNames)
            {
                writer.Write(scene);
            }
        }
    }

    public void LoadSceneNames()
    {
        string path = Application.persistentDataPath + "/save.scenes.data";
        if (File.Exists(path))
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                int sceneCount = reader.ReadInt32();
                sceneNames.Clear();
                for (int i = 0; i < sceneCount; i++)
                {
                    string scene = reader.ReadString();
                    sceneNames.Add(scene);
                }
            }
        }
    }

    public void DeleteAllSaveFiles()
    {
        if (File.Exists(Application.persistentDataPath + "/save.player.data"))
        {
            File.Delete(Application.persistentDataPath + "/save.player.data");
        }
        if (File.Exists(Application.persistentDataPath + "/save.boss.data"))
        {
            File.Delete(Application.persistentDataPath + "/save.boss.data");
        }
        if (File.Exists(Application.persistentDataPath + "/save.lilypad.data"))
        {
            File.Delete(Application.persistentDataPath + "/save.lilypad.data");
        }
        if (File.Exists(Application.persistentDataPath + "/save.scenes.data"))
        {
            File.Delete(Application.persistentDataPath + "/save.scenes.data");
        }
        if (File.Exists(Application.persistentDataPath + "/save.trigger.data"))
        {
            File.Delete(Application.persistentDataPath + "/save.trigger.data");
        }
    }

    public void ResetAllData()
    {
        playerHealth = 5;
        playerUnlockedDodge = false;
        PlayerController.Instance.unlockedDodge = false;
        playerDamageUpgrade = false;
        PlayerController.Instance.damageUpgraded = false;
        KDDefeated = false;
        triggered = false;
        sceneNames.Clear();
    }
}
