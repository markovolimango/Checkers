using Checkers.ViewModels.Settings;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SettingsViewModel _settingsViewModel;
    [ObservableProperty] private ViewModelBase _currentViewModel;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(MinWidth))]
    private Settings.SettingsData _settingsData;

    public MainWindowViewModel()
    {
        CurrentViewModel = new MainMenuViewModel(this);
        _settingsViewModel = new SettingsViewModel(this);
        SettingsData = new Settings.SettingsData();
    }

    public int MinWidth => SettingsData.HintsEnabled ? 800 : 500;

    public void LoadGameViewModel()
    {
        CurrentViewModel = new Game.GameViewModel(this);
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