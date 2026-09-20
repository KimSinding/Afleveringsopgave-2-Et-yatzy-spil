namespace Yatzy.Core;

public sealed class Game
{
    private const int DiceCount = 5;
    private const int MaximumRollsPerTurn = 3;
    private readonly IDiceRoller _diceRoller;
    private readonly List<Player> _players;
    private readonly List<Die> _dice = Enumerable.Range(0, DiceCount).Select(_ => new Die()).ToList();
    private int _currentPlayerIndex;

    public Game(IEnumerable<Player> players, IDiceRoller diceRoller)
    {
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(diceRoller);

        _players = players.ToList();
        ValidatePlayers(_players);
        _diceRoller = diceRoller;
    }

    public IReadOnlyList<Player> Players => _players;

    public IReadOnlyList<Die> Dice => _dice;

    public Player CurrentPlayer => _players[_currentPlayerIndex];

    public int RollCount { get; private set; }

    public int RollsRemaining => MaximumRollsPerTurn - RollCount;

    public int Round => Math.Min(_players.Min(player => player.ScoreSheet.Scores.Count) + 1, 15);

    public bool CanRoll => !IsComplete && RollCount < MaximumRollsPerTurn;

    public bool CanSelectScore => !IsComplete && RollCount > 0;

    public bool IsComplete => _players.All(player => player.ScoreSheet.IsComplete);

    public void Roll()
    {
        if (!CanRoll)
        {
            throw new InvalidOperationException("Der kan ikke kastes flere gange i denne tur.");
        }

        foreach (var die in _dice.Where(die => !die.IsHeld))
        {
            die.Roll(_diceRoller);
        }

        RollCount++;
    }

    public void ToggleHold(int dieIndex)
    {
        if (RollCount is 0 or MaximumRollsPerTurn || IsComplete)
        {
            throw new InvalidOperationException("Terninger kan kun holdes efter første eller andet kast.");
        }

        if (dieIndex is < 0 or >= DiceCount)
        {
            throw new ArgumentOutOfRangeException(nameof(dieIndex));
        }

        _dice[dieIndex].ToggleHeld();
    }

    public int SelectScore(ScoreCategory category)
    {
        if (!CanSelectScore)
        {
            throw new InvalidOperationException("Et scorefelt kan først vælges efter et kast.");
        }

        if (!CurrentPlayer.ScoreSheet.IsAvailable(category))
        {
            throw new InvalidOperationException("Scorefeltet er allerede udfyldt.");
        }

        var score = ScoreCalculator.Calculate(category, _dice.Select(die => die.Value));
        CurrentPlayer.ScoreSheet.RecordScore(category, score);

        foreach (var die in _dice)
        {
            die.Reset();
        }

        RollCount = 0;
        MoveToNextPlayer();
        return score;
    }

    public GameState CreateState() => new(
        _players.Select(player => new PlayerState(
            player.Id,
            player.Name,
            player.IsComputer,
            new Dictionary<ScoreCategory, int>(player.ScoreSheet.Scores))).ToArray(),
        _dice.Select(die => new DieState(die.Value, die.IsHeld)).ToArray(),
        _currentPlayerIndex,
        RollCount,
        Round);

    public static Game Restore(GameState state, IDiceRoller diceRoller)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(diceRoller);

        if (state.Players is null || state.Dice is null || state.Dice.Count != DiceCount)
        {
            throw new ArgumentException("Den gemte spiltilstand er ugyldig.", nameof(state));
        }

        var players = state.Players
            .Select(player => new Player(
                player.Id,
                player.Name,
                player.IsComputer,
                ScoreSheet.Restore(player.Scores ?? new Dictionary<ScoreCategory, int>())))
            .ToArray();
        var game = new Game(players, diceRoller);

        if (state.CurrentPlayerIndex < 0 ||
            state.CurrentPlayerIndex >= players.Length ||
            state.RollCount < 0 ||
            state.RollCount > MaximumRollsPerTurn)
        {
            throw new ArgumentException("Den gemte spiltilstand er ugyldig.", nameof(state));
        }

        for (var index = 0; index < DiceCount; index++)
        {
            game._dice[index].Restore(state.Dice[index].Value, state.Dice[index].IsHeld);
        }

        game._currentPlayerIndex = state.CurrentPlayerIndex;
        game.RollCount = state.RollCount;
        return game;
    }

    private static void ValidatePlayers(IReadOnlyCollection<Player> players)
    {
        if (players.Count is < 1 or > 7)
        {
            throw new ArgumentException("Et spil skal have mellem 1 og 7 spillere.", nameof(players));
        }

        if (players.Count(player => !player.IsComputer) is < 1 or > 6 || players.Count(player => player.IsComputer) > 1)
        {
            throw new ArgumentException("Et spil skal have 1 til 6 menneskelige spillere og højst én computer.", nameof(players));
        }
    }

    private void MoveToNextPlayer()
    {
        if (!IsComplete)
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        }
    }
}
