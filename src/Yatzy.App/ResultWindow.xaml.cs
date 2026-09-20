using System.Windows;
using Yatzy.Core;

namespace Yatzy.App;

public partial class ResultWindow : Window
{
    public ResultWindow(IEnumerable<Player> players)
    {
        InitializeComponent();
        var orderedPlayers = players.OrderByDescending(player => player.ScoreSheet.Total).ToArray();
        var winningScore = orderedPlayers[0].ScoreSheet.Total;
        var winners = orderedPlayers.Where(player => player.ScoreSheet.Total == winningScore).Select(player => player.Name).ToArray();
        WinnersTextBlock.Text = $"Vinder{(winners.Length == 1 ? string.Empty : "e")}: {string.Join(", ", winners)} med {winningScore} point.";
        ResultsList.ItemsSource = orderedPlayers.Select(player => new ResultRow(player.Name, player.ScoreSheet.UpperSubtotal, player.ScoreSheet.Bonus, player.ScoreSheet.Total));
    }

    private void CloseClicked(object sender, RoutedEventArgs e) => Close();

    private sealed record ResultRow(string Name, int Upper, int Bonus, int Total);
}
