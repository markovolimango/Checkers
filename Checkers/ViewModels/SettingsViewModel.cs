using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty] private SettingsData _data;

    public SettingsViewModel(MainWindowViewModel mainWindowViewModel)
    {
        MainWindowViewModel = mainWindowViewModel;
        Data = new SettingsData();
    }

    public void SaveSettings()
    {
        if (MainWindowViewModel is null) return;
        MainWindowViewModel.SettingsData = new SettingsData(Data);
        MainWindowViewModel.LoadMainMenuViewModel();
    }

    public void Cancel()
    {
        Data = new SettingsData();
        if (MainWindowViewModel is null) return;
        MainWindowViewModel.LoadMainMenuViewModel();
    }
}

public class SettingsData
{
    public SettingsData()
    {
        BotThinkingTime = 1;
        IsPlayerRed = true;
        SoundEffectsEnabled = true;
    }

    public SettingsData(SettingsData data)
    {
        BotThinkingTime = data.BotThinkingTime;
        IsPlayerRed = data.IsPlayerRed;
        SoundEffectsEnabled = data.SoundEffectsEnabled;
    }

    public double BotThinkingTime { get; set; }
    public bool IsPlayerRed { get; set; }
    public bool SoundEffectsEnabled { get; set; }
}