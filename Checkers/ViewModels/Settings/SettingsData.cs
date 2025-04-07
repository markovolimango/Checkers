using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels.Settings;

public partial class SettingsData : ObservableObject
{
    [ObservableProperty] private double _botThinkingTime;
    [ObservableProperty] private string _hintModelName;
    [ObservableProperty] private bool _hintsEnabled;
    [ObservableProperty] private bool _isPlayerRed;

    public SettingsData()
    {
        BotThinkingTime = 1.0;
        IsPlayerRed = true;
        HintsEnabled = false;
        HintModelName = "deepseek-r1";
    }

    public SettingsData(SettingsData data)
    {
        BotThinkingTime = data.BotThinkingTime;
        IsPlayerRed = data.IsPlayerRed;
        HintsEnabled = data.HintsEnabled;
        HintModelName = data.HintModelName;
    }
}