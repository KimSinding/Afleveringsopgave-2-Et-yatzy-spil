namespace Yatzy.Core;

public sealed class Die
{
    public int Value { get; private set; }

    public bool IsHeld { get; private set; }

    internal void Roll(IDiceRoller diceRoller)
    {
        ArgumentNullException.ThrowIfNull(diceRoller);

        var value = diceRoller.Roll();
        if (value is < 1 or > 6)
        {
            throw new InvalidOperationException("Terningekasteren skal returnere en værdi fra 1 til 6.");
        }

        Value = value;
    }

    internal void ToggleHeld() => IsHeld = !IsHeld;

    internal void Reset()
    {
        Value = 0;
        IsHeld = false;
    }

    internal void Restore(int value, bool isHeld)
    {
        if (value is < 0 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        Value = value;
        IsHeld = isHeld;
    }
}
