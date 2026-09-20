namespace Yatzy.Core;

public sealed record GameState(
    IReadOnlyList<PlayerState> Players,
    IReadOnlyList<DieState> Dice,
    int CurrentPlayerIndex,
    int RollCount,
    int Round);

public sealed record PlayerState(
    Guid Id,
    string Name,
    bool IsComputer,
    IReadOnlyDictionary<ScoreCategory, int> Scores);

public sealed record DieState(int Value, bool IsHeld);

public sealed class AppSettings
{
    public bool RollAnimationEnabled { get; set; } = true;

    public bool SoundEnabled { get; set; }
}

public sealed class ComputerLearningData
{
    public Dictionary<string, int> PatternCounts { get; set; } = [];

    public Dictionary<ComputerActionType, DecisionTiming> TimingByAction { get; set; } = [];
}

public sealed class DecisionTiming
{
    public int ObservationCount { get; set; }

    public double AverageMilliseconds { get; set; }
}
