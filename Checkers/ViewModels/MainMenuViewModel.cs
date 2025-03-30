namespace Checkers.ViewModels;

public class MainMenuViewModel : ViewModelBase
{
    public MainMenuViewModel(MainWindowViewModel mainWindowViewModel)
    {
        MainWindowViewModel = mainWindowViewModel;
    }

    public void LoadGame()
    {
        if (MainWindowViewModel is null) return;
        MainWindowViewModel.LoadGameViewModel();
    }
}