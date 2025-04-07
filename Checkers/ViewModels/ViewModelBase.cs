using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels;

public class ViewModelBase : ObservableObject
{
    protected MainWindowViewModel? MainWindowViewModel;

    public void LoadMainMenu()
    {
        if (MainWindowViewModel is null) return;
        MainWindowViewModel.LoadMainMenu();
    }
}