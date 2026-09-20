using Yatzy.Core;

namespace Yatzy.Tests;

public class ScoreCalculatorTests
{
    public static IEnumerable<object[]> ScoringCases =>
    [
        Case(ScoreCategory.Enere, 3, 1, 1, 2, 3, 1),
        Case(ScoreCategory.Femmere, 15, 5, 5, 5, 2, 1),
        Case(ScoreCategory.Seksere, 0, 1, 2, 3, 4, 5),
        Case(ScoreCategory.EtPar, 10, 3, 3, 5, 5, 1),
        Case(ScoreCategory.EtPar, 4, 2, 2, 2, 1, 1),
        Case(ScoreCategory.EtPar, 0, 1, 2, 3, 4, 6),
        Case(ScoreCategory.ToPar, 16, 3, 3, 5, 5, 1),
        Case(ScoreCategory.ToPar, 10, 2, 2, 3, 3, 3),
        Case(ScoreCategory.ToPar, 0, 4, 4, 4, 4, 1),
        Case(ScoreCategory.TreEns, 12, 4, 4, 4, 1, 2),
        Case(ScoreCategory.TreEns, 18, 6, 6, 6, 6, 6),
        Case(ScoreCategory.TreEns, 0, 1, 2, 3, 4, 6),
        Case(ScoreCategory.FireEns, 12, 3, 3, 3, 3, 5),
        Case(ScoreCategory.FireEns, 8, 2, 2, 2, 2, 2),
        Case(ScoreCategory.FireEns, 0, 3, 3, 3, 5, 5),
        Case(ScoreCategory.LilleStraight, 15, 1, 2, 3, 4, 5),
        Case(ScoreCategory.LilleStraight, 0, 2, 3, 4, 5, 6),
        Case(ScoreCategory.StorStraight, 20, 2, 3, 4, 5, 6),
        Case(ScoreCategory.StorStraight, 0, 1, 2, 3, 4, 5),
        Case(ScoreCategory.FuldtHus, 13, 2, 2, 3, 3, 3),
        Case(ScoreCategory.FuldtHus, 0, 4, 4, 4, 4, 4),
        Case(ScoreCategory.FuldtHus, 0, 2, 2, 3, 3, 4),
        Case(ScoreCategory.Chance, 17, 1, 2, 3, 5, 6),
        Case(ScoreCategory.Yatzy, 50, 4, 4, 4, 4, 4),
        Case(ScoreCategory.Yatzy, 0, 4, 4, 4, 4, 3)
    ];

    [Theory]
    [MemberData(nameof(ScoringCases))]
    public void Beregner_forventet_point(ScoreCategory category, int expectedScore, int[] dice)
    {
        var actualScore = ScoreCalculator.Calculate(category, dice);

        Assert.Equal(expectedScore, actualScore);
    }

    [Theory]
    [MemberData(nameof(ScoringCases))]
    public void Terningernes_raekkefolge_aendrer_ikke_pointene(
        ScoreCategory category,
        int expectedScore,
        int[] dice)
    {
        var actualScore = ScoreCalculator.Calculate(category, dice.Reverse());

        Assert.Equal(expectedScore, actualScore);
    }

    private static object[] Case(ScoreCategory category, int expectedScore, params int[] dice) =>
        [category, expectedScore, dice];
}
