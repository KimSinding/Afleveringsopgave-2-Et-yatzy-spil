namespace Yatzy.Core;

public sealed class RandomDiceRoller : IDiceRoller
{
    public int Roll() => Random.Shared.Next(1, 7);
}
