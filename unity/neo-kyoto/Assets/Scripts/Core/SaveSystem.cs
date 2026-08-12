using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoKyoto.Core
{
    [Serializable]
    public class ScriptEntry
    {
        public string contractId;
        public string code;
    }

    [Serializable]
    public class SaveData
    {
        public int version = SaveSystem.CurrentVersion;
        public List<string> completedContracts = new List<string>();
        public List<string> unlockedFeatures = new List<string>();
        public List<string> retiredCommands = new List<string>();
        public List<string> debriefed = new List<string>();
        public List<string> followUpDebriefed = new List<string>();
        public List<ScriptEntry> scripts = new List<ScriptEntry>();
    }

    /// <summary>
    /// Progress lives in PlayerPrefs, which maps to IndexedDB on WebGL. Losing a
    /// half-written script to a refreshed browser tab is the worst possible bug in
    /// a game about writing code, so scripts are saved alongside progress.
    /// </summary>
    public static class SaveSystem
    {
        public const int CurrentVersion = 1;
        private const string Key = "neokyoto.save";

        public static bool HasSave { get { return PlayerPrefs.HasKey(Key); } }

        public static void Save(SaveData data)
        {
            if (data == null) return;
            data.version = CurrentVersion;
            try
            {
                PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
                // WebGL only flushes to IndexedDB on an explicit Save().
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Could not save progress: " + e.Message);
            }
        }

        public static SaveData Load()
        {
            if (!HasSave) return null;
            try
            {
                string json = PlayerPrefs.GetString(Key, null);
                if (string.IsNullOrEmpty(json)) return null;

                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return null;

                // Nothing to migrate yet; discard anything newer than we understand.
                if (data.version > CurrentVersion)
                {
                    Debug.LogWarning("Save is from a newer version; ignoring it.");
                    return null;
                }
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Could not read save, starting fresh: " + e.Message);
                return null;
            }
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }
}
