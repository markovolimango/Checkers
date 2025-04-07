namespace Checkers.ViewModels;

public class MainMenuViewModel : ViewModelBase
{
    public MainMenuViewModel(MainWindowViewModel mainWindowViewModel)
    {
        MainWindowViewModel = mainWindowViewModel;
    }
}