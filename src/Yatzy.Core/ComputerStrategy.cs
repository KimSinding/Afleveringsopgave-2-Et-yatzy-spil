namespace Yatzy.Core;

public enum ComputerActionType
{
    HoldAndRoll,
    SelectScore
}

public sealed record ComputerDecision(
    ComputerActionType Action,
    IReadOnlyList<int> HeldDiceIndices,
    ScoreCategory? ScoreCategory);

public sealed class ComputerStrategy
{
    public ComputerDecision ChooseDecision(Game game, ComputerLearningData learningData)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(learningData);

        if (!game.CurrentPlayer.IsComputer || game.RollCount == 0 || game.IsComplete)
        {
            throw new InvalidOperationException("Computeren kan ikke vælge en handling i den aktuelle spiltilstand.");
        }

        var values = game.Dice.Select(die => die.Value).ToArray();
        var available = game.CurrentPlayer.ScoreSheet.AvailableCategories;
        var scoreCategory = ChooseBestScoreCategory(values, available, learningData);

        if (game.RollCount == 3 || IsStrongCompletedCombination(scoreCategory, values))
        {
            return new ComputerDecision(ComputerActionType.SelectScore, [], scoreCategory);
        }

        return new ComputerDecision(
            ComputerActionType.HoldAndRoll,
            ChooseDiceToHold(values, available, learningData),
            null);
    }

    public void ObserveHumanChoice(
        IEnumerable<int> heldDiceValues,
        ScoreCategory selectedCategory,
        ComputerLearningData learningData)
    {
        ArgumentNullException.ThrowIfNull(heldDiceValues);
        ArgumentNullException.ThrowIfNull(learningData);

        var heldKey = $"hold:{string.Join(',', heldDiceValues.Order())}";
        Increment(learningData.PatternCounts, heldKey);
        Increment(learningData.PatternCounts, $"score:{selectedCategory}");
    }

    private static ScoreCategory ChooseBestScoreCategory(
        IReadOnlyCollection<int> values,
        IReadOnlyCollection<ScoreCategory> available,
        ComputerLearningData learningData) => available
        .Select(category => new
        {
            Category = category,
            Score = ScoreCalculator.Calculate(category, values),
            CombinationRank = IsStrongCompletedCombination(category, values) ? 1 : 0,
            LearnedPreference = learningData.PatternCounts.GetValueOrDefault($"score:{category}")
        })
        .OrderByDescending(candidate => candidate.CombinationRank)
        .ThenByDescending(candidate => candidate.Score)
        .ThenByDescending(candidate => candidate.LearnedPreference)
        .ThenBy(candidate => candidate.Category)
        .First().Category;

    private static IReadOnlyList<int> ChooseDiceToHold(
        IReadOnlyList<int> values,
        IReadOnlyCollection<ScoreCategory> available,
        ComputerLearningData learningData)
    {
        var groupedDice = values
            .Select((value, index) => new { value, index })
            .GroupBy(die => die.value)
            .Where(group => group.Count() >= 2)
            .OrderByDescending(group => group.Count())
            .ThenByDescending(group => group.Key)
            .FirstOrDefault();

        if (groupedDice is not null)
        {
            return groupedDice.Select(die => die.index).ToArray();
        }

        var straightValues = available.Contains(ScoreCategory.LilleStraight)
            ? new HashSet<int>([1, 2, 3, 4, 5])
            : [];
        if (available.Contains(ScoreCategory.StorStraight))
        {
            straightValues.UnionWith([2, 3, 4, 5, 6]);
        }

        var distinctStraightDice = values
            .Select((value, index) => new { value, index })
            .Where(die => straightValues.Contains(die.value))
            .GroupBy(die => die.value)
            .Select(group => group.First())
            .ToArray();
        if (distinctStraightDice.Length >= 3)
        {
            return distinctStraightDice.Select(die => die.index).ToArray();
        }

        var highDice = values
            .Select((value, index) => new { value, index })
            .Where(die => die.value >= 5)
            .OrderByDescending(die => die.value)
            .Select(die => die.index)
            .ToArray();
        return highDice;
    }

    private static bool IsStrongCompletedCombination(ScoreCategory category, IReadOnlyCollection<int> values) =>
        (category is ScoreCategory.Yatzy or ScoreCategory.LilleStraight or ScoreCategory.StorStraight or ScoreCategory.FuldtHus or ScoreCategory.FireEns) &&
        ScoreCalculator.Calculate(category, values) > 0;

    private static void Increment(Dictionary<string, int> counts, string key) =>
        counts[key] = counts.GetValueOrDefault(key) + 1;
}
