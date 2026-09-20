namespace Yatzy.Core;

public sealed class Player
{
    public Player(string name, bool isComputer = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("En spiller skal have et navn.", nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name;
        IsComputer = isComputer;
        ScoreSheet = new ScoreSheet();
    }

    public Guid Id { get; }

    public string Name { get; }

    public bool IsComputer { get; }

    public ScoreSheet ScoreSheet { get; }
}
