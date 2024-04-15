using System;
using System.IO;
using System.Text.Json;

namespace BCode.MusicPlayer.WpfPlayer.Shared;
public class SettingsManager
{

    private static string _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"BCode.MusicPlayer");
    private static string _fileName = "settings.json";
    private string _fullFilePath = Path.Combine(_filePath, _fileName);

    public SettingsManager()
    {
        CheckSettingsExists();
    }

    public MusicPlayerSettings GetSettings()
    {
        CheckSettingsExists();

        var fileContent = File.ReadAllText(_fullFilePath);

        return JsonSerializer.Deserialize<MusicPlayerSettings>(fileContent);
    }

    public void UpdateSettings(MusicPlayerSettings settings)
    {
        var updatedSettings = JsonSerializer.Serialize<MusicPlayerSettings>(settings);

        File.WriteAllText(_fullFilePath, updatedSettings);
    }

    private void CheckSettingsExists()
    {
        if (!File.Exists(_fullFilePath))
        {
            Directory.CreateDirectory(_filePath);
            CreateSettingsFile();
        }
    }

    private void CreateSettingsFile()
    {
        var newSettings = JsonSerializer.Serialize<MusicPlayerSettings>(new MusicPlayerSettings());

        File.WriteAllText(_fullFilePath, newSettings);
    }
}
