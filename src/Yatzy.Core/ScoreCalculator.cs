namespace Yatzy.Core;

public static class ScoreCalculator
{
    public static int Calculate(ScoreCategory category, IEnumerable<int> dice)
    {
        ArgumentNullException.ThrowIfNull(dice);

        var values = dice.ToArray();
        ValidateDice(values);

        var groups = values
            .GroupBy(value => value)
            .ToDictionary(group => group.Key, group => group.Count());

        return category switch
        {
            ScoreCategory.Enere => SumUpperSection(values, 1),
            ScoreCategory.Toere => SumUpperSection(values, 2),
            ScoreCategory.Treere => SumUpperSection(values, 3),
            ScoreCategory.Firere => SumUpperSection(values, 4),
            ScoreCategory.Femmere => SumUpperSection(values, 5),
            ScoreCategory.Seksere => SumUpperSection(values, 6),
            ScoreCategory.EtPar => ScoreOnePair(groups),
            ScoreCategory.ToPar => ScoreTwoPairs(groups),
            ScoreCategory.TreEns => ScoreOfAKind(groups, 3),
            ScoreCategory.FireEns => ScoreOfAKind(groups, 4),
            ScoreCategory.LilleStraight => IsStraight(values, 1) ? 15 : 0,
            ScoreCategory.StorStraight => IsStraight(values, 2) ? 20 : 0,
            ScoreCategory.FuldtHus => ScoreFullHouse(groups),
            ScoreCategory.Chance => values.Sum(),
            ScoreCategory.Yatzy => groups.Values.Any(count => count == 5) ? 50 : 0,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };
    }

    private static void ValidateDice(IReadOnlyCollection<int> dice)
    {
        if (dice.Count != 5)
        {
            throw new ArgumentException("Et kast skal bestå af præcis fem terninger.", nameof(dice));
        }

        if (dice.Any(value => value is < 1 or > 6))
        {
            throw new ArgumentException("En terning skal have en værdi fra 1 til 6.", nameof(dice));
        }
    }

    private static int SumUpperSection(IEnumerable<int> dice, int faceValue) =>
        dice.Where(value => value == faceValue).Sum();

    private static int ScoreOnePair(IReadOnlyDictionary<int, int> groups)
    {
        var pairValue = groups
            .Where(group => group.Value >= 2)
            .Select(group => group.Key)
            .DefaultIfEmpty(0)
            .Max();

        return pairValue * 2;
    }

    private static int ScoreTwoPairs(IReadOnlyDictionary<int, int> groups)
    {
        var pairValues = groups
            .Where(group => group.Value >= 2)
            .Select(group => group.Key)
            .OrderDescending()
            .Take(2)
            .ToArray();

        return pairValues.Length == 2 ? pairValues.Sum() * 2 : 0;
    }

    private static int ScoreOfAKind(IReadOnlyDictionary<int, int> groups, int requiredCount)
    {
        var faceValue = groups
            .Where(group => group.Value >= requiredCount)
            .Select(group => group.Key)
            .DefaultIfEmpty(0)
            .Max();

        return faceValue * requiredCount;
    }

    private static bool IsStraight(IEnumerable<int> dice, int firstValue) =>
        dice.Order().SequenceEqual(Enumerable.Range(firstValue, 5));

    private static int ScoreFullHouse(IReadOnlyDictionary<int, int> groups)
    {
        var hasPair = groups.Values.Any(count => count == 2);
        var hasThreeOfAKind = groups.Values.Any(count => count == 3);

        return groups.Count == 2 && hasPair && hasThreeOfAKind
            ? groups.Sum(group => group.Key * group.Value)
            : 0;
    }
}
