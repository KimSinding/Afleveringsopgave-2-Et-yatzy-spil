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
        await AnimateDiceAsync();
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
        ScorePanel.Children.Clear();
        var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(3) };
        header.Children.Add(new TextBlock { Text = "Felt", Width = 150, FontSize = 16, FontWeight = FontWeights.Bold });
        foreach (var player in _game.Players)
        {
            header.Children.Add(new TextBlock { Text = player.Name, Width = 90, FontSize = 16, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
        }
        ScorePanel.Children.Add(header);
        foreach (var category in Enum.GetValues<ScoreCategory>())
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(3) };
            row.Children.Add(new TextBlock { Text = CategoryName(category), Width = 150, FontSize = 16 });
            foreach (var player in _game.Players)
            {
                var button = new Button { Width = 90, Tag = category, Margin = new Thickness(3) };
                if (player.ScoreSheet.Scores.TryGetValue(category, out var saved)) button.Content = saved;
                else if (player == _game.CurrentPlayer && _game.CanSelectScore) { button.Content = ScoreCalculator.Calculate(category, _game.Dice.Select(d => d.Value)); button.Click += ScoreClicked; }
                else button.Content = "–";
                button.IsEnabled = humanCanAct && player == _game.CurrentPlayer && player.ScoreSheet.IsAvailable(category) && _game.CanSelectScore;
                row.Children.Add(button);
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
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(3) };
        row.Children.Add(new TextBlock { Text = label, Width = 150, FontSize = 16, FontWeight = FontWeights.Bold });
        foreach (var player in _game.Players)
        {
            row.Children.Add(new TextBlock { Text = valueSelector(player).ToString(), Width = 90, FontSize = 16, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
        }
        ScorePanel.Children.Add(row);
    }

    private void NewGameClicked(object sender, RoutedEventArgs e)
    {
        if (!_game.IsComplete && MessageBox.Show("Er du sikker på, at du vil afbryde spillet og starte et nyt?", "Yatzy", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        _storage.DeleteGame();
        _isClosingForNewGame = true;
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
            await Task.Delay(_thinkingPauseCalculator.GetPause(ComputerActionType.HoldAndRoll, _learningData));
            await AnimateDiceAsync();
            _game.Roll();
            PlayRollSound();
            _storage.SaveGame(_game);
            StatusTextBlock.Text = "Computeren har kastet.";
            UpdateView();

            var decision = _computerStrategy.ChooseDecision(_game, _learningData);
            await Task.Delay(_thinkingPauseCalculator.GetPause(decision.Action, _learningData));
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

    private async Task AnimateDiceAsync()
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

            await Task.Delay(55);
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
