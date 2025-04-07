using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels;

public class ViewModelBase : ObservableObject
{
    protected MainWindowViewModel? MainWindowViewModel;

    public void LoadMainMenu()
    {
        if (MainWindowViewModel is null) return;
        MainWindowViewModel.LoadMainMenuViewModel();
    }

    public void LoadGame()
    {
        if (MainWindowViewModel is null) return;
        MainWindowViewModel.LoadGameViewModel();
    }

    public void LoadSettings()
    {
        if (MainWindowViewModel is null) return;
        MainWindowViewModel.LoadSettingsViewModel();
    }
}