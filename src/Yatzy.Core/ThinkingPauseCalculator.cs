namespace Yatzy.Core;

public sealed class ThinkingPauseCalculator
{
    public const int DefaultMilliseconds = 800;
    public const int MinimumMilliseconds = 400;
    public const int MaximumMilliseconds = 2000;

    public TimeSpan GetPause(ComputerActionType action, ComputerLearningData learningData)
    {
        ArgumentNullException.ThrowIfNull(learningData);
        var timing = learningData.TimingByAction.GetValueOrDefault(action);
        var milliseconds = timing is null || timing.ObservationCount == 0
            ? DefaultMilliseconds
            : Math.Clamp(timing.AverageMilliseconds, MinimumMilliseconds, MaximumMilliseconds);
        return TimeSpan.FromMilliseconds(milliseconds);
    }

    public void ObserveHumanDecision(TimeSpan elapsed, ComputerActionType action, ComputerLearningData learningData)
    {
        ArgumentNullException.ThrowIfNull(learningData);

        var timing = learningData.TimingByAction.GetValueOrDefault(action) ?? new DecisionTiming();
        timing.AverageMilliseconds = (timing.AverageMilliseconds * timing.ObservationCount + elapsed.TotalMilliseconds) /
                                   (timing.ObservationCount + 1);
        timing.ObservationCount++;
        learningData.TimingByAction[action] = timing;
    }
}
