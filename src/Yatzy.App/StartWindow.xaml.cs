using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Yatzy.Core;

namespace Yatzy.App;

public partial class StartWindow : Window
{
    private readonly List<TextBox> _humanNameTextBoxes = [];
    private bool _hasAttemptedStart;

    public StartWindow()
    {
        InitializeComponent();
        CreateHumanNameFields();
    }

    private void PlayerCountChanged(object sender, SelectionChangedEventArgs e) => CreateHumanNameFields();

    private void ComputerParticipationChanged(object sender, RoutedEventArgs e)
    {
        if (ComputerPanel is not null)
        {
            ComputerPanel.Visibility = ComputerCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void CreateHumanNameFields()
    {
        if (HumanNamesPanel is null || PlayerCountComboBox.SelectedItem is not ComboBoxItem selectedItem)
        {
            return;
        }

        var existingNames = _humanNameTextBoxes.Select(textBox => textBox.Text).ToArray();
        _humanNameTextBoxes.Clear();
        HumanNamesPanel.Children.Clear();
        var playerCount = int.Parse(selectedItem.Content.ToString()!);

        for (var index = 0; index < playerCount; index++)
        {
            HumanNamesPanel.Children.Add(new TextBlock
            {
                Text = $"Spiller {index + 1}",
                FontSize = 15
            });

            var textBox = new TextBox
            {
                MaxLength = 20,
                Text = index < existingNames.Length ? existingNames[index] : string.Empty
            };
            textBox.TextChanged += NameTextChanged;
            _humanNameTextBoxes.Add(textBox);
            HumanNamesPanel.Children.Add(textBox);
        }

        if (_hasAttemptedStart)
        {
            ValidateNames(showMessage: false);
        }
    }

    private void NameTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_hasAttemptedStart)
        {
            ValidateNames(showMessage: false);
        }
    }

    private void StartGameClicked(object sender, RoutedEventArgs e)
    {
        _hasAttemptedStart = true;
        if (!ValidateNames(showMessage: true))
        {
            return;
        }

        var players = _humanNameTextBoxes
            .Select(textBox => new Player(NormalizeName(textBox.Text)))
            .ToList();

        if (ComputerCheckBox.IsChecked == true)
        {
            var computerName = string.IsNullOrEmpty(NormalizeName(ComputerNameTextBox.Text))
                ? "Computeren"
                : NormalizeName(ComputerNameTextBox.Text);
            players.Add(new Player(computerName, isComputer: true));
        }

        _ = new Game(players, new RandomDiceRoller());
        ValidationMessageTextBlock.Foreground = Brushes.DarkGreen;
        ValidationMessageTextBlock.Text = "Spillet er oprettet. Selve spillevinduet kommer i næste fase.";
        ValidationMessageTextBlock.Visibility = Visibility.Visible;
    }

    private bool ValidateNames(bool showMessage)
    {
        var allValid = true;
        foreach (var textBox in _humanNameTextBoxes)
        {
            allValid &= SetValidationState(textBox, required: true);
        }

        if (ComputerCheckBox.IsChecked == true)
        {
            allValid &= SetValidationState(ComputerNameTextBox, required: false);
        }
        else
        {
            ClearValidationState(ComputerNameTextBox);
        }

        if (allValid)
        {
            ValidationMessageTextBlock.Visibility = Visibility.Collapsed;
        }
        else if (showMessage)
        {
            ValidationMessageTextBlock.Foreground = Brushes.Firebrick;
            ValidationMessageTextBlock.Text = "Hvert spillernavn skal have mindst ét synligt tegn og må højst være 20 gyldige tegn.";
            ValidationMessageTextBlock.Visibility = Visibility.Visible;
        }

        return allValid;
    }

    private static bool SetValidationState(TextBox textBox, bool required)
    {
        var normalizedName = NormalizeName(textBox.Text);
        var valid = !string.IsNullOrEmpty(normalizedName) || !required;
        valid &= normalizedName.All(IsAllowedNameCharacter);

        if (valid)
        {
            ClearValidationState(textBox);
        }
        else
        {
            textBox.BorderBrush = Brushes.Firebrick;
            textBox.BorderThickness = new Thickness(2);
        }

        return valid;
    }

    private static void ClearValidationState(TextBox textBox)
    {
        textBox.ClearValue(BorderBrushProperty);
        textBox.ClearValue(BorderThicknessProperty);
    }

    private static string NormalizeName(string name) => Regex.Replace(name.Trim(), @"\s+", " ");

    private static bool IsAllowedNameCharacter(char character) =>
        char.IsLetterOrDigit(character) || character is ' ' or '-' or '_' or '!';
}
