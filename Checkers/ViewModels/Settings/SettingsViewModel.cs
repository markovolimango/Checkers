namespace Checkers.ViewModels.Settings;

public class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel(MainWindowViewModel mainWindowViewModel)
    {
        MainWindowViewModel = mainWindowViewModel;
        Data = new SettingsData();
    }

    public SettingsData Data { get; private set; }

    public void SaveSettings()
    {
        if (MainWindowViewModel is null) return;
        MainWindowViewModel.SettingsData = new SettingsData(Data);
        LoadMainMenu();
    }

    public void Cancel()
    {
        Data = new SettingsData();
        if (MainWindowViewModel is null) return;
        LoadMainMenu();
    }
}