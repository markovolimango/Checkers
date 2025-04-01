using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels;

public partial class SettingsData : ObservableObject
{
    [ObservableProperty] private double _botThinkingTime;
    [ObservableProperty] private string _hintModelName;
    [ObservableProperty] private bool _hintsEnabled;
    [ObservableProperty] private bool _isPlayerRed;
    [ObservableProperty] private bool _soundEffectsEnabled;

    public SettingsData()
    {
        BotThinkingTime = 1.0;
        IsPlayerRed = true;
        SoundEffectsEnabled = true;
        HintsEnabled = false;
        HintModelName = "deepseek-r1";
    }

    public SettingsData(SettingsData data)
    {
        BotThinkingTime = data.BotThinkingTime;
        IsPlayerRed = data.IsPlayerRed;
        SoundEffectsEnabled = data.SoundEffectsEnabled;
        HintsEnabled = data.HintsEnabled;
        HintModelName = data.HintModelName;
    }
}