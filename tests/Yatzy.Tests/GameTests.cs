using Yatzy.Core;

namespace Yatzy.Tests;

public class GameTests
{
    [Fact]
    public void Bonus_er_nul_ved_62_point_i_oevre_sektion()
    {
        var scoreSheet = CreateUpperScoreSheet(62);

        Assert.Equal(62, scoreSheet.UpperSubtotal);
        Assert.Equal(0, scoreSheet.Bonus);
        Assert.Equal(62, scoreSheet.UpperTotal);
    }

    [Fact]
    public void Bonus_er_50_ved_63_point_i_oevre_sektion()
    {
        var scoreSheet = CreateUpperScoreSheet(63);

        Assert.Equal(63, scoreSheet.UpperSubtotal);
        Assert.Equal(50, scoreSheet.Bonus);
        Assert.Equal(113, scoreSheet.UpperTotal);
    }

    [Fact]
    public void Scorefelt_kan_ikke_udfyldes_to_gange()
    {
        var scoreSheet = new ScoreSheet();
        scoreSheet.RecordScore(ScoreCategory.Chance, 20);

        Assert.Throws<InvalidOperationException>(() => scoreSheet.RecordScore(ScoreCategory.Chance, 21));
    }

    [Fact]
    public void Kan_ikke_holde_eller_vaelge_score_foer_foerste_kast()
    {
        var game = CreateGame(new[] { 1, 2, 3, 4, 5 });

        Assert.Throws<InvalidOperationException>(() => game.ToggleHold(0));
        Assert.Throws<InvalidOperationException>(() => game.SelectScore(ScoreCategory.Chance));
    }

    [Fact]
    public void Kan_hoejst_kaste_tre_gange_i_en_tur()
    {
        var game = CreateGame(Enumerable.Repeat(1, 15));

        game.Roll();
        game.Roll();
        game.Roll();

        Assert.Equal(3, game.RollCount);
        Assert.Equal(0, game.RollsRemaining);
        Assert.False(game.CanRoll);
        Assert.Throws<InvalidOperationException>(() => game.Roll());
        Assert.Throws<InvalidOperationException>(() => game.ToggleHold(0));
    }

    [Fact]
    public void Falsk_terningekaster_driver_tur_med_holdte_terninger()
    {
        var game = CreateGame(new[] { 6, 2, 3, 4, 5, 1, 1, 1, 1 });

        game.Roll();
        game.ToggleHold(0);
        game.Roll();

        Assert.Equal(new[] { 6, 1, 1, 1, 1 }, game.Dice.Select(die => die.Value));
        Assert.True(game.Dice[0].IsHeld);
        Assert.Equal(10, game.SelectScore(ScoreCategory.Chance));
        Assert.Equal(0, game.RollCount);
        Assert.All(game.Dice, die =>
        {
            Assert.Equal(0, die.Value);
            Assert.False(die.IsHeld);
        });
    }

    [Fact]
    public void Nul_point_kan_vaelges_som_streg()
    {
        var game = CreateGame(new[] { 1, 2, 3, 4, 6 });

        game.Roll();
        var score = game.SelectScore(ScoreCategory.Yatzy);

        Assert.Equal(0, score);
        Assert.Equal(0, game.Players[0].ScoreSheet.Scores[ScoreCategory.Yatzy]);
    }

    [Fact]
    public void Turen_gaar_til_naeste_spiller_efter_valgt_score()
    {
        var game = new Game(
            [new Player("Anna"), new Player("Bo")],
            new FakeDiceRoller(1, 2, 3, 4, 5));

        game.Roll();
        game.SelectScore(ScoreCategory.Chance);

        Assert.Equal("Bo", game.CurrentPlayer.Name);
    }

    [Fact]
    public void Spillet_er_afsluttet_naar_alle_15_scorefelter_er_udfyldt()
    {
        var game = CreateGame(Enumerable.Repeat(1, 75));

        foreach (var category in Enum.GetValues<ScoreCategory>())
        {
            game.Roll();
            game.SelectScore(category);
        }

        Assert.True(game.IsComplete);
        Assert.False(game.CanRoll);
        Assert.Equal(15, game.Players[0].ScoreSheet.Scores.Count);
    }

    private static ScoreSheet CreateUpperScoreSheet(int sixesScore)
    {
        var scoreSheet = new ScoreSheet();
        scoreSheet.RecordScore(ScoreCategory.Enere, 5);
        scoreSheet.RecordScore(ScoreCategory.Toere, 10);
        scoreSheet.RecordScore(ScoreCategory.Treere, 15);
        scoreSheet.RecordScore(ScoreCategory.Firere, 12);
        scoreSheet.RecordScore(ScoreCategory.Femmere, 2);
        scoreSheet.RecordScore(ScoreCategory.Seksere, sixesScore - 44);
        return scoreSheet;
    }

    private static Game CreateGame(IEnumerable<int> diceValues) =>
        new([new Player("Anna")], new FakeDiceRoller(diceValues));

    private sealed class FakeDiceRoller : IDiceRoller
    {
        private readonly Queue<int> _values;

        public FakeDiceRoller(params int[] values)
            : this((IEnumerable<int>)values)
        {
        }

        public FakeDiceRoller(IEnumerable<int> values) => _values = new Queue<int>(values);

        public int Roll() => _values.Dequeue();
    }
}
