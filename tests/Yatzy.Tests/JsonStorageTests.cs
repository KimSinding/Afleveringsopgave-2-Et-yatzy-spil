using Yatzy.Core;

namespace Yatzy.Tests;

public sealed class JsonStorageTests : IDisposable
{
    private readonly string _directoryPath = Path.Combine(Path.GetTempPath(), "Yatzy.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void Manglende_filer_giver_sikre_standardvaerdier()
    {
        var storage = new JsonStorage(_directoryPath);

        var settings = storage.LoadSettings();
        var learningData = storage.LoadLearningData();

        Assert.True(settings.RollAnimationEnabled);
        Assert.False(settings.SoundEnabled);
        Assert.Empty(learningData.PatternCounts);
        Assert.Null(storage.LoadGame());
    }

    [Fact]
    public void Ugyldig_json_giver_sikre_standardvaerdier_uden_nedbrud()
    {
        Directory.CreateDirectory(_directoryPath);
        File.WriteAllText(Path.Combine(_directoryPath, "settings.json"), "dette er ikke JSON");
        File.WriteAllText(Path.Combine(_directoryPath, "learning.json"), "{");
        File.WriteAllText(Path.Combine(_directoryPath, "game.json"), "[");
        var storage = new JsonStorage(_directoryPath);

        Assert.True(storage.LoadSettings().RollAnimationEnabled);
        Assert.Empty(storage.LoadLearningData().PatternCounts);
        Assert.Null(storage.LoadGame());
    }

    [Fact]
    public void Indstillinger_og_laeringsdata_gemmes_og_indlaeses()
    {
        var storage = new JsonStorage(_directoryPath);
        storage.SaveSettings(new AppSettings { RollAnimationEnabled = false, SoundEnabled = true });
        storage.SaveLearningData(new ComputerLearningData
        {
            PatternCounts = new Dictionary<string, int> { ["hold-6"] = 4 }
        });

        var settings = storage.LoadSettings();
        var learningData = storage.LoadLearningData();

        Assert.False(settings.RollAnimationEnabled);
        Assert.True(settings.SoundEnabled);
        Assert.Equal(4, learningData.PatternCounts["hold-6"]);
    }

    [Fact]
    public void Igangvaerende_spil_kan_gemmes_og_genoptages_med_holdte_terninger()
    {
        var playerOne = new Player("Anna");
        var playerTwo = new Player("Bo");
        var game = new Game([playerOne, playerTwo], new FakeDiceRoller(6, 2, 3, 4, 5));
        game.Roll();
        game.ToggleHold(0);
        var storage = new JsonStorage(_directoryPath);

        storage.SaveGame(game);
        var state = Assert.IsType<GameState>(storage.LoadGame());
        var restoredGame = Game.Restore(state, new FakeDiceRoller());

        Assert.Equal(playerOne.Id, restoredGame.CurrentPlayer.Id);
        Assert.Equal(1, restoredGame.RollCount);
        Assert.Equal(1, restoredGame.Round);
        Assert.Equal(new[] { 6, 2, 3, 4, 5 }, restoredGame.Dice.Select(die => die.Value));
        Assert.True(restoredGame.Dice[0].IsHeld);
        Assert.False(restoredGame.Dice[1].IsHeld);
    }

    [Fact]
    public void Gemt_spil_kan_slettes()
    {
        var storage = new JsonStorage(_directoryPath);
        var game = new Game([new Player("Anna")], new FakeDiceRoller(1, 2, 3, 4, 5));
        storage.SaveGame(game);

        storage.DeleteGame();

        Assert.Null(storage.LoadGame());
    }

    public void Dispose()
    {
        if (Directory.Exists(_directoryPath))
        {
            Directory.Delete(_directoryPath, recursive: true);
        }
    }

    private sealed class FakeDiceRoller(params int[] values) : IDiceRoller
    {
        private readonly Queue<int> _values = new(values);

        public int Roll() => _values.Dequeue();
    }
}
