namespace Yatzy.Core;

public sealed class ScoreSheet
{
    private static readonly ScoreCategory[] UpperCategories =
    [
        ScoreCategory.Enere,
        ScoreCategory.Toere,
        ScoreCategory.Treere,
        ScoreCategory.Firere,
        ScoreCategory.Femmere,
        ScoreCategory.Seksere
    ];

    private readonly Dictionary<ScoreCategory, int> _scores = [];

    public IReadOnlyDictionary<ScoreCategory, int> Scores => _scores;

    public IReadOnlyCollection<ScoreCategory> AvailableCategories =>
        Enum.GetValues<ScoreCategory>().Where(category => !_scores.ContainsKey(category)).ToArray();

    public bool IsComplete => _scores.Count == Enum.GetValues<ScoreCategory>().Length;

    public int UpperSubtotal => UpperCategories.Sum(category => _scores.GetValueOrDefault(category));

    public int Bonus => UpperSubtotal >= 63 ? 50 : 0;

    public int UpperTotal => UpperSubtotal + Bonus;

    public int LowerSubtotal => _scores
        .Where(score => !UpperCategories.Contains(score.Key))
        .Sum(score => score.Value);

    public int Total => UpperTotal + LowerSubtotal;

    public bool IsAvailable(ScoreCategory category) => !_scores.ContainsKey(category);

    public void RecordScore(ScoreCategory category, int score)
    {
        if (!IsAvailable(category))
        {
            throw new InvalidOperationException("Scorefeltet er allerede udfyldt.");
        }

        if (score < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(score), "En score kan ikke være negativ.");
        }

        _scores.Add(category, score);
    }
}
