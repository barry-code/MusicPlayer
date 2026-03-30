using System;
using System.IO;
using System.Text.Json;

namespace BCode.MusicPlayer.WpfPlayer.Shared;
public class SettingsManager : ISettingsManager
{

    private static string _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"BCode.MusicPlayer");
    private static string _fileName = "settings.json";
    private string _fullFilePath = Path.Combine(_filePath, _fileName);
    private MusicPlayerSettings _currentSettings;

    public SettingsManager()
    {
        Configure();
    }

    public event EventHandler SettingsChanged;

    public MusicPlayerSettings CurrentSettings => GetSettings();

    public void UpdateSettings(MusicPlayerSettings settings)
    {
        SaveSettings(settings);
    }

    private MusicPlayerSettings GetSettings()
    {
        if (_currentSettings is null)
        {
            var fileContent = File.ReadAllText(_fullFilePath);
            _currentSettings = JsonSerializer.Deserialize<MusicPlayerSettings>(fileContent);
        }

        return _currentSettings;
    }

    private void SaveSettings(MusicPlayerSettings settings)
    {
        _currentSettings = settings;
        var updatedSettings = JsonSerializer.Serialize<MusicPlayerSettings>(settings);
        File.WriteAllText(_fullFilePath, updatedSettings);
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Configure()
    {
        if (!File.Exists(_fullFilePath))
        {
            Directory.CreateDirectory(_filePath);
            SaveSettings(new MusicPlayerSettings());
        }
    }
}
