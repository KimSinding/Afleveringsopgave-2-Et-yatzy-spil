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
