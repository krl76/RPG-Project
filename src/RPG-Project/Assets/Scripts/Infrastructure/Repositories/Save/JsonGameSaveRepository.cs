using System;
using System.IO;
using Core.Gameplay.Save.Data;
using UnityEngine;

namespace Infrastructure.Repositories.Save
{
    public sealed class JsonGameSaveRepository : IGameSaveRepository
    {
        private const string SaveFileName = "savegame.json";

        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public bool HasSave() => File.Exists(SavePath);

        public void Save(GameSaveData data)
        {
            string directoryPath = Path.GetDirectoryName(SavePath);
            if (string.IsNullOrEmpty(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[JsonGameSaveRepository] Save written to '{SavePath}'.");
        }

        public bool TryLoad(out GameSaveData data)
        {
            data = null;

            if (HasSave() == false)
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                data = JsonUtility.FromJson<GameSaveData>(json);
                return data?.Player != null;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                data = null;
                return false;
            }
        }
    }
}
