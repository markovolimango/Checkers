using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Checkers.Models;
using CommunityToolkit.Mvvm.Input;

namespace Checkers.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly Board _board = new();
    /*
    private readonly Board _board = new(new Piece[8, 8]
    {
        //@formatter:off
        { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.RedMan ,Piece.Empty ,Piece.RedMan ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.Empty ,Piece.BlackMan ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.Empty ,Piece.BlackMan ,Piece.Empty ,Piece.BlackMan ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty}
        //@formatter:on
    }, Team.Red);
    */

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
                var square = new SquareViewModel(row, col, isDark, OnSquareClick);
                Squares.Add(square);
                square.PutPiece(_board.GetPiece(row, col));
                isDark = !isDark;
            }
        }

        ExportCommand = new RelayCommand(() => ExportBoardState());
    }

    public ObservableCollection<SquareViewModel> Squares { get; }
    public ICommand ExportCommand { get; }

    private void ExportBoardState()
    {
        Console.WriteLine($"{_board}");
    }

    private void OnSquareClick(SquareViewModel square)
    {
        var piece = _board.GetPiece(square.Pos);
        if (_from == null)
        {
            if (piece.GetTeam() != _board.CurrentTurn) return;

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

        if (piece.GetTeam() == _board.GetPiece(_from.Pos).GetTeam())
        {
            _from.Deselect();
            square.Select();
            _from = square;
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
            GetSquare(move.End).PutPiece(piece);
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