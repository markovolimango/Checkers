using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels;

public partial class EndViewModel : ViewModelBase
{
    [ObservableProperty] private string _emoji;
    [ObservableProperty] private string _text;
    [ObservableProperty] private string _textColor;

    public EndViewModel(MainWindowViewModel mainWindowViewModel, int isWin)
    {
        MainWindowViewModel = mainWindowViewModel;
        if (isWin == 1)
        {
            Emoji = "🏆";
            Text = "You Won!";
            TextColor = "Green";
        }
        else if (isWin == -1)
        {
            Emoji = "💔";
            Text = "You Lost :(";
            TextColor = "Red";
        }
        else
        {
            Emoji = "🤝";
            Text = "Draw...";
            TextColor = "Black";
        }
    }
}