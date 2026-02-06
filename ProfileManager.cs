using System;
using System.IO;
using System.Text.Json;

public class ProfileManager
{
    private readonly string profilesDirectory;
    private readonly JsonSerializerOptions serializerOptions;

    public ProfileManager()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        profilesDirectory = Path.Combine(localAppData, "Visualizationizer", "Profiles");
        serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        Directory.CreateDirectory(profilesDirectory);
    }

    public void SaveSlot(int slot, AppState profile)
    {
        ValidateSlot(slot);
        string slotPath = GetSlotPath(slot);
        string json = JsonSerializer.Serialize(profile, serializerOptions);
        File.WriteAllText(slotPath, json);
    }

    public bool TryLoadSlot(int slot, out AppState profile)
    {
        ValidateSlot(slot);
        string slotPath = GetSlotPath(slot);
        if (!File.Exists(slotPath))
        {
            profile = null;
            return false;
        }

        try
        {
            string json = File.ReadAllText(slotPath);
            profile = JsonSerializer.Deserialize<AppState>(json, serializerOptions);
            return profile != null;
        }
        catch
        {
            profile = null;
            return false;
        }
    }

    private string GetSlotPath(int slot)
    {
        return Path.Combine(profilesDirectory, $"slot{slot:D2}.json");
    }

    private static void ValidateSlot(int slot)
    {
        if (slot < 1 || slot > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), "Slot must be in range 1-10.");
        }
    }
}
