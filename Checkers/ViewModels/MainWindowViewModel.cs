using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase _currentViewModel;

    public MainWindowViewModel()
    {
        CurrentViewModel = new MainMenuViewModel(this);
    }

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
}