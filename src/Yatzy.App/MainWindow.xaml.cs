using System.Diagnostics;
using System.ComponentModel;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Yatzy.Core;

namespace Yatzy.App;

public partial class MainWindow : Window
{
    private readonly Game _game;
    private readonly Button[] _diceButtons;
    private readonly ComputerStrategy _computerStrategy = new();
    private readonly ThinkingPauseCalculator _thinkingPauseCalculator = new();
    private readonly JsonStorage _storage = new();
    private readonly ComputerLearningData _learningData;
    private readonly Stopwatch _humanDecisionStopwatch = new();
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private AppSettings _settings;
    private bool _isComputerTurn;
    private bool _isAnimating;
    private bool _isClosingForNewGame;
    private static readonly string[] Faces = ["", "⚀", "⚁", "⚂", "⚃", "⚄", "⚅"];

    public MainWindow(Game game)
    {
        _game = game;
        _learningData = _storage.LoadLearningData();
        _settings = _storage.LoadSettings();
        InitializeComponent();
        _diceButtons = [Die0, Die1, Die2, Die3, Die4];
        UpdateView();
        _ = BeginComputerTurnIfNeededAsync();
    }

    private async void RollClicked(object sender, RoutedEventArgs e)
    {
        ObserveHumanDecision(ComputerActionType.HoldAndRoll);
        try
        {
            await AnimateDiceAsync(_lifetimeCancellation.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (_lifetimeCancellation.IsCancellationRequested)
        {
            return;
        }
        _game.Roll();
        PlayRollSound();
        _storage.SaveGame(_game);
        StatusTextBlock.Text = "Vælg terninger, du vil holde, eller vælg et scorefelt.";
        UpdateView();
    }

    private void DieClicked(object sender, RoutedEventArgs e)
    {
        ObserveHumanDecision(ComputerActionType.HoldAndRoll);
        _game.ToggleHold(int.Parse(((Button)sender).Tag.ToString()!));
        _storage.SaveGame(_game);
        UpdateView();
    }

    private async void ScoreClicked(object sender, RoutedEventArgs e)
    {
        var category = (ScoreCategory)((Button)sender).Tag;
        var score = ScoreCalculator.Calculate(category, _game.Dice.Select(die => die.Value));
        if (score == 0 && MessageBox.Show($"Vil du stryge {CategoryName(category)}?", "Yatzy", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        ObserveHumanDecision(ComputerActionType.SelectScore);
        _computerStrategy.ObserveHumanChoice(_game.Dice.Where(die => die.IsHeld).Select(die => die.Value), category, _learningData);
        _storage.SaveLearningData(_learningData);
        _game.SelectScore(category);
        _storage.SaveGame(_game);
        if (_game.IsComplete)
        {
            _storage.DeleteGame();
            ShowResults();
            UpdateView();
            return;
        }
        UpdateView();
        await BeginComputerTurnIfNeededAsync();
    }

    private void UpdateView()
    {
        TurnTextBlock.Text = $"{_game.CurrentPlayer.Name} – runde {_game.Round} – {_game.RollsRemaining} kast tilbage";
        var humanCanAct = !_isComputerTurn && !_isAnimating && !_game.IsComplete;
        RollButton.IsEnabled = humanCanAct && _game.CanRoll;
        for (var index = 0; index < 5; index++)
        {
            var die = _game.Dice[index];
            _diceButtons[index].Content = Faces[die.Value];
            _diceButtons[index].IsEnabled = humanCanAct && _game.RollCount is > 0 and < 3;
            _diceButtons[index].Background = die.IsHeld ? Brushes.Goldenrod : Brushes.White;
        }
        var scoreFontSize = Math.Clamp(ActualWidth / (_game.Players.Count * 13.0), 10, 16);
        ScorePanel.Children.Clear();
        var header = CreateScoreGrid();
        AddScoreCell(header, new TextBlock { Text = "Felt", FontSize = scoreFontSize, FontWeight = FontWeights.Bold }, 0);
        for (var playerIndex = 0; playerIndex < _game.Players.Count; playerIndex++)
        {
            AddScoreCell(header, new TextBlock { Text = _game.Players[playerIndex].Name, FontSize = scoreFontSize, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis }, playerIndex + 1);
        }
        ScorePanel.Children.Add(header);
        foreach (var category in Enum.GetValues<ScoreCategory>())
        {
            var row = CreateScoreGrid();
            AddScoreCell(row, new TextBlock { Text = CategoryName(category), FontSize = scoreFontSize, TextTrimming = TextTrimming.CharacterEllipsis }, 0);
            for (var playerIndex = 0; playerIndex < _game.Players.Count; playerIndex++)
            {
                var player = _game.Players[playerIndex];
                var button = new Button { Tag = category, Margin = new Thickness(3), FontSize = scoreFontSize };
                if (player.ScoreSheet.Scores.TryGetValue(category, out var saved)) button.Content = saved;
                else if (player == _game.CurrentPlayer && _game.CanSelectScore) { button.Content = ScoreCalculator.Calculate(category, _game.Dice.Select(d => d.Value)); button.Click += ScoreClicked; }
                else button.Content = "–";
                button.IsEnabled = humanCanAct && player == _game.CurrentPlayer && player.ScoreSheet.IsAvailable(category) && _game.CanSelectScore;
                AddScoreCell(row, button, playerIndex + 1);
            }
            ScorePanel.Children.Add(row);
        }
        AddSummaryRow("Øvre subtotal", player => player.ScoreSheet.UpperSubtotal);
        AddSummaryRow("Bonus", player => player.ScoreSheet.Bonus);
        AddSummaryRow("Øvre total", player => player.ScoreSheet.UpperTotal);
        AddSummaryRow("Nedre subtotal", player => player.ScoreSheet.LowerSubtotal);
        AddSummaryRow("Samlet score", player => player.ScoreSheet.Total);
    }

    private void AddSummaryRow(string label, Func<Player, int> valueSelector)
    {
        var scoreFontSize = Math.Clamp(ActualWidth / (_game.Players.Count * 13.0), 10, 16);
        var row = CreateScoreGrid();
        AddScoreCell(row, new TextBlock { Text = label, FontSize = scoreFontSize, FontWeight = FontWeights.Bold, TextTrimming = TextTrimming.CharacterEllipsis }, 0);
        for (var playerIndex = 0; playerIndex < _game.Players.Count; playerIndex++)
        {
            AddScoreCell(row, new TextBlock { Text = valueSelector(_game.Players[playerIndex]).ToString(), FontSize = scoreFontSize, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }, playerIndex + 1);
        }
        ScorePanel.Children.Add(row);
    }

    private Grid CreateScoreGrid()
    {
        var grid = new Grid { Margin = new Thickness(3) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.8, GridUnitType.Star) });
        foreach (var _ in _game.Players)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        return grid;
    }

    private static void AddScoreCell(Grid grid, UIElement element, int column)
    {
        Grid.SetColumn(element, column);
        grid.Children.Add(element);
    }

    private void NewGameClicked(object sender, RoutedEventArgs e)
    {
        if (!_game.IsComplete && MessageBox.Show("Er du sikker på, at du vil afbryde spillet og starte et nyt?", "Yatzy", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        _isClosingForNewGame = true;
        _lifetimeCancellation.Cancel();
        _storage.DeleteGame();
        new StartWindow().Show();
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isClosingForNewGame && !_game.IsComplete)
        {
            if (MessageBox.Show("Er du sikker på, at du vil afslutte? Det igangværende spil gemmes og kan fortsættes senere.", "Yatzy", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            _storage.SaveGame(_game);
        }

        _lifetimeCancellation.Cancel();

        base.OnClosing(e);
    }

    private async Task BeginComputerTurnIfNeededAsync()
    {
        if (_game.IsComplete || !_game.CurrentPlayer.IsComputer)
        {
            if (!_game.IsComplete)
            {
                _humanDecisionStopwatch.Restart();
            }

            return;
        }

        _isComputerTurn = true;
        UpdateView();
        while (!_game.IsComplete && _game.CurrentPlayer.IsComputer)
        {
            StatusTextBlock.Text = "Computeren tænker…";
            try
            {
                await Task.Delay(_thinkingPauseCalculator.GetPause(ComputerActionType.HoldAndRoll, _learningData), _lifetimeCancellation.Token);
                await AnimateDiceAsync(_lifetimeCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (_lifetimeCancellation.IsCancellationRequested)
            {
                return;
            }
            _game.Roll();
            PlayRollSound();
            _storage.SaveGame(_game);
            StatusTextBlock.Text = "Computeren har kastet.";
            UpdateView();

            var decision = _computerStrategy.ChooseDecision(_game, _learningData);
            try
            {
                await Task.Delay(_thinkingPauseCalculator.GetPause(decision.Action, _learningData), _lifetimeCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            if (decision.Action == ComputerActionType.HoldAndRoll)
            {
                foreach (var index in decision.HeldDiceIndices.Where(index => !_game.Dice[index].IsHeld))
                {
                    _game.ToggleHold(index);
                }

                StatusTextBlock.Text = "Computeren holder terninger.";
                _storage.SaveGame(_game);
                UpdateView();
                continue;
            }

            _game.SelectScore(decision.ScoreCategory!.Value);
            _storage.SaveGame(_game);
            StatusTextBlock.Text = "Computeren har valgt et scorefelt.";
            break;
        }

        _isComputerTurn = false;
        if (_game.IsComplete)
        {
            _storage.DeleteGame();
            ShowResults();
        }
        UpdateView();
        if (!_game.IsComplete)
        {
            _humanDecisionStopwatch.Restart();
        }
    }

    private void ObserveHumanDecision(ComputerActionType action)
    {
        if (_humanDecisionStopwatch.IsRunning)
        {
            _thinkingPauseCalculator.ObserveHumanDecision(_humanDecisionStopwatch.Elapsed, action, _learningData);
            _storage.SaveLearningData(_learningData);
        }

        _humanDecisionStopwatch.Restart();
    }

    private async Task AnimateDiceAsync(CancellationToken cancellationToken)
    {
        if (!_settings.RollAnimationEnabled)
        {
            return;
        }

        _isAnimating = true;
        UpdateView();
        for (var frame = 0; frame < 7; frame++)
        {
            for (var index = 0; index < _diceButtons.Length; index++)
            {
                if (!_game.Dice[index].IsHeld)
                {
                    _diceButtons[index].Content = Faces[Random.Shared.Next(1, 7)];
                }
            }

            await Task.Delay(55, cancellationToken);
        }

        _isAnimating = false;
    }

    private void PlayRollSound()
    {
        if (_settings.SoundEnabled)
        {
            SystemSounds.Asterisk.Play();
        }
    }

    private void SettingsClicked(object sender, RoutedEventArgs e)
    {
        var window = new SettingsWindow { Owner = this };
        window.ShowDialog();
        _settings = _storage.LoadSettings();
    }

    private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_diceButtons is not null)
        {
            UpdateView();
        }
    }

    private void ShowResults()
    {
        if (_settings.SoundEnabled && _game.Players.Any(player => player.ScoreSheet.Scores.TryGetValue(ScoreCategory.Yatzy, out var score) && score == 50))
        {
            SystemSounds.Exclamation.Play();
        }

        new ResultWindow(_game.Players) { Owner = this }.ShowDialog();
    }

    private static string CategoryName(ScoreCategory category) => category switch
    {
        ScoreCategory.EtPar => "Ét par", ScoreCategory.ToPar => "To par", ScoreCategory.TreEns => "Tre ens", ScoreCategory.FireEns => "Fire ens", ScoreCategory.LilleStraight => "Lille straight", ScoreCategory.StorStraight => "Stor straight", ScoreCategory.FuldtHus => "Fuldt hus", _ => category.ToString()
    };
}
