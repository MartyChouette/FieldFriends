using UnityEngine;
using System.IO;
using FieldFriends.Core;
using FieldFriends.Party;
using FieldFriends.World;

namespace FieldFriends.Save
{
    /// <summary>
    /// Single save slot. Autosave on area change. Manual save optional.
    /// Save copy text: "You rest for a bit."
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        string SavePath => Path.Combine(Application.persistentDataPath, GameConstants.SaveFileName);

        public void AutoSave()
        {
            WriteSave();
        }

        public void ManualSave()
        {
            WriteSave();
            // Display "You rest for a bit." via dialogue
            var dialogueBox = FindFirstObjectByType<UI.DialogueBox>();
            if (dialogueBox != null)
            {
                StartCoroutine(dialogueBox.ShowDialogue("You rest for a bit."));
            }
        }

        void WriteSave()
        {
            var data = GatherSaveData();
            if (data == null) return;

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }

        SaveData GatherSaveData()
        {
            var save = new SaveData();

            // Current area
            var areaManager = FindFirstObjectByType<AreaManager>();
            if (areaManager != null)
                save.CurrentArea = areaManager.CurrentArea;

            // Player grid position
            var player = FindFirstObjectByType<GridMovement>();
            if (player != null)
            {
                save.PlayerGridX = player.GridPosition.x;
                save.PlayerGridY = player.GridPosition.y;
            }

            // Party
            var partyManager = FindFirstObjectByType<PartyManager>();
            if (partyManager != null)
            {
                foreach (var creature in partyManager.Party)
                {
                    save.Party.Add(CreatureSaveData.FromInstance(creature));
                }
            }

            return save;
        }

        public SaveData LoadSave()
        {
            if (!File.Exists(SavePath))
                return null;

            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public bool HasSave()
        {
            return File.Exists(SavePath);
        }

        public void DeleteSave()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
    }
}
