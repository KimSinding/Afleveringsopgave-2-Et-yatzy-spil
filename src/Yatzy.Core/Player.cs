namespace Yatzy.Core;

public sealed class Player
{
    public Player(string name, bool isComputer = false)
        : this(Guid.NewGuid(), name, isComputer, new ScoreSheet())
    {
    }

    internal Player(Guid id, string name, bool isComputer, ScoreSheet scoreSheet)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("En spiller skal have et navn.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(scoreSheet);

        Id = id;
        Name = name;
        IsComputer = isComputer;
        ScoreSheet = scoreSheet;
    }

    public Guid Id { get; }

    public string Name { get; }

    public bool IsComputer { get; }

    public ScoreSheet ScoreSheet { get; }
}
