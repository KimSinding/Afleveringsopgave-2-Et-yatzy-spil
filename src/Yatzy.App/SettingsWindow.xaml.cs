using System.Windows;
using Yatzy.Core;

namespace Yatzy.App;

public partial class SettingsWindow : Window
{
    private readonly JsonStorage _storage = new();
    private readonly AppSettings _settings;
    public SettingsWindow()
    {
        InitializeComponent();
        _settings = _storage.LoadSettings();
        AnimationCheckBox.IsChecked = _settings.RollAnimationEnabled;
        SoundCheckBox.IsChecked = _settings.SoundEnabled;
    }
    private void ResetLearningClicked(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Vil du slette computerens læring?", "Yatzy", MessageBoxButton.YesNo) == MessageBoxResult.Yes) _storage.SaveLearningData(new ComputerLearningData());
    }
    private void SaveClicked(object sender, RoutedEventArgs e)
    {
        _settings.RollAnimationEnabled = AnimationCheckBox.IsChecked == true;
        _settings.SoundEnabled = SoundCheckBox.IsChecked == true;
        _storage.SaveSettings(_settings);
        Close();
    }
}
