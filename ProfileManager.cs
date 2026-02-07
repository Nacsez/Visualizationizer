using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class ProfileManager
{
    private readonly string profilesDirectory;
    private readonly string bundlesDirectory;
    private readonly JsonSerializerOptions serializerOptions;

    public ProfileManager()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        profilesDirectory = Path.Combine(localAppData, "Visualizationizer", "Profiles");
        bundlesDirectory = Path.Combine(profilesDirectory, "Bundles");
        serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        Directory.CreateDirectory(profilesDirectory);
        Directory.CreateDirectory(bundlesDirectory);
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

    public bool HasSlot(int slot)
    {
        ValidateSlot(slot);
        return File.Exists(GetSlotPath(slot));
    }

    public bool SaveNamedSlotProfile(string profileName)
    {
        string trimmedName = profileName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return false;
        }

        try
        {
            var package = new SlotProfilePackage
            {
                Name = trimmedName,
                SavedAtUtc = DateTime.UtcNow
            };

            for (int slot = 1; slot <= 10; slot++)
            {
                if (TryLoadSlot(slot, out AppState state) && state != null)
                {
                    package.Slots[slot] = state;
                }
            }

            string path = GetBundlePath(trimmedName);
            string json = JsonSerializer.Serialize(package, serializerOptions);
            File.WriteAllText(path, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string[] ListNamedSlotProfiles()
    {
        try
        {
            var names = new List<string>();
            foreach (string path in Directory.GetFiles(bundlesDirectory, "*.json"))
            {
                if (TryReadPackage(path, out SlotProfilePackage package) && !string.IsNullOrWhiteSpace(package.Name))
                {
                    names.Add(package.Name);
                }
                else
                {
                    names.Add(Path.GetFileNameWithoutExtension(path));
                }
            }

            return names
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public bool TryLoadNamedSlotProfile(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            return false;
        }

        try
        {
            string requestedName = profileName.Trim();
            SlotProfilePackage package = null;

            // Fast path: sanitized file name match.
            string directPath = GetBundlePath(requestedName);
            if (File.Exists(directPath))
            {
                TryReadPackage(directPath, out package);
            }

            // Fallback path: scan package display names.
            if (package == null)
            {
                foreach (string path in Directory.GetFiles(bundlesDirectory, "*.json"))
                {
                    if (TryReadPackage(path, out SlotProfilePackage candidate)
                        && string.Equals(candidate.Name, requestedName, StringComparison.OrdinalIgnoreCase))
                    {
                        package = candidate;
                        break;
                    }
                }
            }

            if (package == null)
            {
                return false;
            }

            for (int slot = 1; slot <= 10; slot++)
            {
                if (package.Slots.TryGetValue(slot, out AppState state) && state != null)
                {
                    SaveSlot(slot, state);
                }
                else
                {
                    string slotPath = GetSlotPath(slot);
                    if (File.Exists(slotPath))
                    {
                        File.Delete(slotPath);
                    }
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool TryReadPackage(string path, out SlotProfilePackage package)
    {
        try
        {
            string json = File.ReadAllText(path);
            package = JsonSerializer.Deserialize<SlotProfilePackage>(json, serializerOptions);
            if (package?.Slots == null)
            {
                package = null;
                return false;
            }
            return true;
        }
        catch
        {
            package = null;
            return false;
        }
    }

    private string GetBundlePath(string profileName)
    {
        string sanitized = SanitizeFileName(profileName);
        return Path.Combine(bundlesDirectory, $"{sanitized}.json");
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var chars = fileName
            .Where(ch => !invalidChars.Contains(ch))
            .ToArray();
        string sanitized = new string(chars).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "UnnamedProfile" : sanitized;
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

    private sealed class SlotProfilePackage
    {
        public string Name { get; set; } = string.Empty;
        public DateTime SavedAtUtc { get; set; }
        public Dictionary<int, AppState> Slots { get; set; } = new Dictionary<int, AppState>();
    }
}
