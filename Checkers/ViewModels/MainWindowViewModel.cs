namespace Checkers.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public BoardViewModel BoardViewModel { get; } = new();
}