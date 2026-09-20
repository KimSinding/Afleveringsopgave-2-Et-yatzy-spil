using System.Text.Json;

namespace Yatzy.Core;

public sealed class JsonStorage
{
    private const string SettingsFileName = "settings.json";
    private const string LearningFileName = "learning.json";
    private const string GameFileName = "game.json";
    private readonly string _directoryPath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public JsonStorage(string? directoryPath = null)
    {
        _directoryPath = directoryPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Yatzy");
    }

    public AppSettings LoadSettings() => LoadOrDefault<AppSettings>(SettingsFileName);

    public ComputerLearningData LoadLearningData() => LoadOrDefault<ComputerLearningData>(LearningFileName);

    public GameState? LoadGame()
    {
        try
        {
            var path = Path.Combine(_directoryPath, GameFileName);
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                return null;
            }

            return JsonSerializer.Deserialize<GameState>(File.ReadAllText(path), _options);
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public void SaveSettings(AppSettings settings) => Save(SettingsFileName, settings);

    public void SaveLearningData(ComputerLearningData learningData) => Save(LearningFileName, learningData);

    public void SaveGame(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        Save(GameFileName, game.CreateState());
    }

    public void DeleteGame()
    {
        var path = Path.Combine(_directoryPath, GameFileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private T LoadOrDefault<T>(string fileName) where T : new()
    {
        try
        {
            var path = Path.Combine(_directoryPath, fileName);
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                return new T();
            }

            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), _options) ?? new T();
        }
        catch (IOException)
        {
            return new T();
        }
        catch (JsonException)
        {
            return new T();
        }
        catch (UnauthorizedAccessException)
        {
            return new T();
        }
    }

    private void Save<T>(string fileName, T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Directory.CreateDirectory(_directoryPath);
        File.WriteAllText(Path.Combine(_directoryPath, fileName), JsonSerializer.Serialize(value, _options));
    }
}
