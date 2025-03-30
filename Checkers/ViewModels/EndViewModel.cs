using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels;

public partial class EndViewModel : ViewModelBase
{
    [ObservableProperty] private string _text;

    public EndViewModel(MainWindowViewModel mainWindowViewModel, bool isWin)
    {
        MainWindowViewModel = mainWindowViewModel;
        if (isWin)
            Text = "You Won :)";
        else
            Text = "You Lost :(";
    }
}