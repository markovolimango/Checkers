using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels;

public partial class EndViewModel : ViewModelBase
{
    [ObservableProperty] private string _emoji;
    [ObservableProperty] private string _text;
    [ObservableProperty] private string _textColor;

    public EndViewModel(MainWindowViewModel mainWindowViewModel, bool isWin)
    {
        MainWindowViewModel = mainWindowViewModel;
        if (isWin)
        {
            Emoji = "🏆";
            Text = "You Won!";
            TextColor = "Green";
        }
        else
        {
            Emoji = "💔";
            Text = "You Lost :(";
            TextColor = "Red";
        }
    }

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
}