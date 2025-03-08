using System.Collections.ObjectModel;

namespace Checkers.ViewModels;

public class BoardViewModel
{
    public BoardViewModel()
    {
        Squares = new ObservableCollection<SquareViewModel>();
        var isDark = true;
        for (var row = 0; row < 8; row++)
        {
            isDark = !isDark;
            for (var col = 0; col < 8; col++)
            {
                Squares.Add(new SquareViewModel(row, col, isDark));
                isDark = !isDark;
            }
        }
    }

    public ObservableCollection<SquareViewModel> Squares { get; }
}