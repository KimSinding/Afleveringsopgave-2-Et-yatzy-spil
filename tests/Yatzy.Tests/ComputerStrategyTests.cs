using Yatzy.Core;

namespace Yatzy.Tests;

public class ComputerStrategyTests
{
    [Fact]
    public void Computeren_holder_et_eksisterende_par_foer_nyt_kast()
    {
        var game = CreateComputerGame(6, 6, 2, 3, 4);
        game.Roll();

        var decision = new ComputerStrategy().ChooseDecision(game, new ComputerLearningData());

        Assert.Equal(ComputerActionType.HoldAndRoll, decision.Action);
        Assert.Equal(new[] { 0, 1 }, decision.HeldDiceIndices);
        Assert.Null(decision.ScoreCategory);
    }

    [Fact]
    public void Computeren_vaelger_fire_ens_tidligt()
    {
        var game = CreateComputerGame(6, 6, 6, 6, 1);
        game.Roll();

        var decision = new ComputerStrategy().ChooseDecision(game, new ComputerLearningData());

        Assert.Equal(ComputerActionType.SelectScore, decision.Action);
        Assert.Equal(ScoreCategory.FireEns, decision.ScoreCategory);
    }

    [Fact]
    public void Computeren_vaelger_altid_et_ledigt_felt_efter_tredje_kast()
    {
        var game = CreateComputerGame(1, 2, 3, 4, 6, 1, 2, 3, 4, 6, 1, 2, 3, 4, 6);
        game.Roll();
        game.Roll();
        game.Roll();

        var decision = new ComputerStrategy().ChooseDecision(game, new ComputerLearningData());

        Assert.Equal(ComputerActionType.SelectScore, decision.Action);
        Assert.NotNull(decision.ScoreCategory);
        Assert.True(game.CurrentPlayer.ScoreSheet.IsAvailable(decision.ScoreCategory!.Value));
    }

    [Fact]
    public void Laering_samles_uden_spillerprofiler()
    {
        var strategy = new ComputerStrategy();
        var learningData = new ComputerLearningData();

        strategy.ObserveHumanChoice([6, 6], ScoreCategory.FireEns, learningData);
        strategy.ObserveHumanChoice([6, 6], ScoreCategory.FireEns, learningData);

        Assert.Equal(2, learningData.PatternCounts["hold:6,6"]);
        Assert.Equal(2, learningData.PatternCounts["score:FireEns"]);
        Assert.DoesNotContain(learningData.PatternCounts.Keys, key => key.Contains("Anna"));
    }

    [Fact]
    public void Taenkepause_er_800_ms_foer_observationer_og_begraenses_derefter()
    {
        var calculator = new ThinkingPauseCalculator();
        var learningData = new ComputerLearningData();

        Assert.Equal(TimeSpan.FromMilliseconds(800), calculator.GetPause(ComputerActionType.SelectScore, learningData));

        calculator.ObserveHumanDecision(TimeSpan.FromMilliseconds(3000), ComputerActionType.SelectScore, learningData);

        Assert.Equal(TimeSpan.FromMilliseconds(2000), calculator.GetPause(ComputerActionType.SelectScore, learningData));
    }

    private static Game CreateComputerGame(params int[] values)
    {
        var game = new Game(
            [new Player("Anna"), new Player("Computeren", isComputer: true)],
            new FakeDiceRoller(Enumerable.Repeat(1, 5).Concat(values).ToArray()));
        game.Roll();
        game.SelectScore(ScoreCategory.Chance);
        return game;
    }

    private sealed class FakeDiceRoller(params int[] values) : IDiceRoller
    {
        private readonly Queue<int> _values = new(values);

        public int Roll() => _values.Dequeue();
    }
}
