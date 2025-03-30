using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SettingsViewModel _settingsViewModel;
    [ObservableProperty] private ViewModelBase _currentViewModel;

    public MainWindowViewModel()
    {
        CurrentViewModel = new MainMenuViewModel(this);
        _settingsViewModel = new SettingsViewModel(this);
        SettingsData = new SettingsData();
    }

    public SettingsData SettingsData { get; set; }

    public void LoadGameViewModel()
    {
        CurrentViewModel = new GameViewModel(this);
    }

    public void LoadEndViewModel(bool isWin)
    {
        CurrentViewModel = new EndViewModel(this, isWin);
    }

    public void LoadMainMenuViewModel()
    {
        CurrentViewModel = new MainMenuViewModel(this);
    }

    public void LoadSettingsViewModel()
    {
        CurrentViewModel = _settingsViewModel;
    }
}