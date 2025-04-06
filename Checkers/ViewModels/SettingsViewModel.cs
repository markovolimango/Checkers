using System;

namespace Checkers.ViewModels;

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
        Console.WriteLine(Data.BotThinkingTime);
        MainWindowViewModel.SettingsData = new SettingsData(Data);
        Console.WriteLine(MainWindowViewModel.SettingsData.BotThinkingTime);
        MainWindowViewModel.LoadMainMenuViewModel();
    }

    public void Cancel()
    {
        Data = new SettingsData();
        if (MainWindowViewModel is null) return;
        MainWindowViewModel.LoadMainMenuViewModel();
    }
}