using System;

namespace Checkers.ViewModels.Settings;

public class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel(MainWindowViewModel mainWindowViewModel)
    {
        MainWindowViewModel = mainWindowViewModel;
        Data = new SettingsData();
    }

    public SettingsData Data { get; private set; }

    public void SaveSettings()
    {
        if (MainWindowViewModel is null) return;
        Console.WriteLine((double)Data.BotThinkingTime);
        MainWindowViewModel.SettingsData = new SettingsData(Data);
        Console.WriteLine((double)MainWindowViewModel.SettingsData.BotThinkingTime);
        MainWindowViewModel.LoadMainMenuViewModel();
    }

    public void Cancel()
    {
        Data = new SettingsData();
        if (MainWindowViewModel is null) return;
        MainWindowViewModel.LoadMainMenuViewModel();
    }
}