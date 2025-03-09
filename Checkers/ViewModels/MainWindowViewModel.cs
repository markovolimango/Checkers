using System.Collections.ObjectModel;
using Checkers.Models;

namespace Checkers.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly Board _board = new();
    private SquareViewModel? _from, _to;

    public MainWindowViewModel()
    {
        Squares = new ObservableCollection<SquareViewModel>();
        var isDark = true;
        for (var row = 0; row < 8; row++)
        {
            isDark = !isDark;
            for (var col = 0; col < 8; col++)
            {
                Squares.Add(new SquareViewModel(row, col, isDark, OnSquareClick));
                isDark = !isDark;
            }
        }

        for (var row = 0; row < 3; row++)
        for (var col = 1 - row % 2; col < 8; col += 2)
            GetSquare(row, col).PutPiece(Piece.Red);

        for (var row = 5; row < 8; row++)
        for (var col = 1 - row % 2; col < 8; col += 2)
            GetSquare(row, col).PutPiece(Piece.Black);
    }

    public ObservableCollection<SquareViewModel> Squares { get; }

    private void OnSquareClick(SquareViewModel square)
    {
        if (_from == null)
        {
            if (_board.GetPiece(square.Pos) == Piece.Empty)
            {
                _from = null;
                return;
            }

            _from = square;
            square.Select();
            return;
        }

        if (_from == square)
        {
            _from = null;
            square.Deselect();
            return;
        }

        _to = square;
        MovePiece(_from, _to);
        _from.Deselect();
        _from = _to = null;
    }

    private void MovePiece(SquareViewModel from, SquareViewModel to)
    {
        var move = _board.FindMove(from.Pos, to.Pos);
        if (move is not null)
        {
            var piece = _board.GetPiece(from.Pos);

            _board.MakeMove(move);
            foreach (var pos in move.Captures)
                GetSquare(pos).RemovePiece();
            from.RemovePiece();
            to.PutPiece(piece);
        }
    }

    private SquareViewModel GetSquare(int row, int col)
    {
        return Squares[row * 8 + col];
    }

    private SquareViewModel GetSquare(Pos pos)
    {
        return GetSquare(pos.Row, pos.Col);
    }
}