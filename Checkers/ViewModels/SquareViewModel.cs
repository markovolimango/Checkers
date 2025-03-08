using System;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;

namespace Checkers.ViewModels;

public class SquareViewModel : ViewModelBase
{
    public SquareViewModel(int row, int column, bool isDark)
    {
        Row = row;
        Column = column;
        var imagePath = isDark ? "avares://Checkers/Assets/DarkSquare.png" : "avares://Checkers/Assets/LightSquare.png";
        Image = new Bitmap(AssetLoader.Open(new Uri(imagePath)));
        ClickCommand = new RelayCommand(SquareClicked);
    }

    public Bitmap? Image { get; }

    public int Row { get; }
    public int Column { get; }
    public ICommand ClickCommand { get; }

    private void SquareClicked()
    {
        Console.WriteLine($"Square clicked at {Row}, {Column}");
    }
}