using System.Windows;
using System.Windows.Controls;
using Yatzy.Core;

namespace Yatzy.App;

public partial class ResultWindow : Window
{
    private readonly Player[] _players;

    public ResultWindow(IEnumerable<Player> players)
    {
        _players = players.OrderByDescending(player => player.ScoreSheet.Total).ToArray();
        InitializeComponent();
        var winningScore = _players[0].ScoreSheet.Total;
        var winners = _players.Where(player => player.ScoreSheet.Total == winningScore).Select(player => player.Name).ToArray();
        WinnersTextBlock.Text = $"Vinder{(winners.Length == 1 ? string.Empty : "e")}: {string.Join(", ", winners)} med {winningScore} point.";
        BuildScoreboard();
    }

    private void BuildScoreboard()
    {
        ScorePanel.Children.Clear();
        var availableWidth = ActualWidth > 0 ? ActualWidth : Width;
        var fontSize = Math.Clamp(availableWidth / (_players.Length * 13.0), 10, 16);
        var header = CreateGrid();
        AddCell(header, new TextBlock { Text = "Felt", FontSize = fontSize, FontWeight = FontWeights.Bold }, 0);
        for (var index = 0; index < _players.Length; index++)
        {
            AddCell(header, new TextBlock { Text = _players[index].Name, FontSize = fontSize, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis }, index + 1);
        }
        ScorePanel.Children.Add(header);

        foreach (var category in Enum.GetValues<ScoreCategory>())
        {
            var row = CreateGrid();
            AddCell(row, new TextBlock { Text = CategoryName(category), FontSize = fontSize, TextTrimming = TextTrimming.CharacterEllipsis }, 0);
            for (var index = 0; index < _players.Length; index++)
            {
                AddCell(row, new TextBlock { Text = _players[index].ScoreSheet.Scores[category].ToString(), FontSize = fontSize, TextAlignment = TextAlignment.Center }, index + 1);
            }
            ScorePanel.Children.Add(row);
        }

        AddSummaryRow("Øvre subtotal", player => player.ScoreSheet.UpperSubtotal, fontSize);
        AddSummaryRow("Bonus", player => player.ScoreSheet.Bonus, fontSize);
        AddSummaryRow("Øvre total", player => player.ScoreSheet.UpperTotal, fontSize);
        AddSummaryRow("Nedre subtotal", player => player.ScoreSheet.LowerSubtotal, fontSize);
        AddSummaryRow("Samlet score", player => player.ScoreSheet.Total, fontSize);
    }

    private void AddSummaryRow(string label, Func<Player, int> valueSelector, double fontSize)
    {
        var row = CreateGrid();
        AddCell(row, new TextBlock { Text = label, FontSize = fontSize, FontWeight = FontWeights.Bold, TextTrimming = TextTrimming.CharacterEllipsis }, 0);
        for (var index = 0; index < _players.Length; index++)
        {
            AddCell(row, new TextBlock { Text = valueSelector(_players[index]).ToString(), FontSize = fontSize, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }, index + 1);
        }
        ScorePanel.Children.Add(row);
    }

    private Grid CreateGrid()
    {
        var grid = new Grid { Margin = new Thickness(3) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.8, GridUnitType.Star) });
        foreach (var _ in _players)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }
        return grid;
    }

    private static void AddCell(Grid grid, UIElement element, int column)
    {
        Grid.SetColumn(element, column);
        grid.Children.Add(element);
    }

    private void CloseClicked(object sender, RoutedEventArgs e) => Close();

    private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ScorePanel is not null)
        {
            BuildScoreboard();
        }
    }

    private static string CategoryName(ScoreCategory category) => category switch
    {
        ScoreCategory.EtPar => "Ét par", ScoreCategory.ToPar => "To par", ScoreCategory.TreEns => "Tre ens", ScoreCategory.FireEns => "Fire ens", ScoreCategory.LilleStraight => "Lille straight", ScoreCategory.StorStraight => "Stor straight", ScoreCategory.FuldtHus => "Fuldt hus", _ => category.ToString()
    };
}
