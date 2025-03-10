using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Checkers.Models;
using CommunityToolkit.Mvvm.Input;

namespace Checkers.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    //private readonly Board _board = new();

    private readonly Board _board = new(new Piece[8, 8]
    {
        //@formatter:off
        { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.RedMan ,Piece.Empty ,Piece.RedMan ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.Empty ,Piece.BlackMan ,Piece.Empty ,Piece.BlackMan ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.Empty ,Piece.BlackMan ,Piece.Empty ,Piece.BlackMan ,Piece.Empty ,Piece.Empty ,Piece.Empty},
        { Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty ,Piece.Empty}
        //@formatter:on
    }, Team.Red);

    private readonly List<SquareViewModel> _path = [];

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
        if (_path.Count == 0)
        {
            if (piece.GetTeam() != _board.CurrentTurn) return;

            _path.Add(square);
            square.Select();
            return;
        }

        if (_path.Count == 1)
        {
            if (_path[0] == square)
            {
                square.Deselect();
                _path.Clear();
                return;
            }


            if (piece.GetTeam() == _board.GetPiece(_path[0].Pos).GetTeam())
            {
                _path[0].Deselect();
                square.Select();
                _path[0] = square;
                return;
            }

            _path.Add(square);
            if (TryToMakeMove(_path) != 0)
            {
                _path[0].Deselect();
                _path.Clear();
            }

            return;
        }

        if (_path.Count >= 2)
        {
            if (piece != Piece.Empty) return;
            _path.Add(square);
            var res = TryToMakeMove(_path);
            if (res == -1)
            {
                _path.RemoveAt(_path.Count - 1);
                return;
            }

            if (res == 0) return;

            if (res == 1)
            {
                _path[0].Deselect();
                _path.Clear();
            }
        }
    }

    private int TryToMakeMove(List<SquareViewModel> path)
    {
        var moves = _board.FindMovesStartingWith(path.Select(square => square.Pos).ToList());
        if (moves.Count == 0) return -1;
        if (moves.Count == 1 && moves[0].Path.Count == path.Count)
        {
            var move = moves[0];
            var piece = _board.GetPiece(path[0].Pos);

            MoveAlongPath(move.Path, piece);
            _board.MakeMove(move);

            return 1;
        }
        else
        {
            var piece = _board.GetPiece(path[0].Pos);
            MoveAlongPath(_path, piece);

            return 0;
        }
    }

    private void MoveAlongPath(List<Pos> path, Piece piece)
    {
        for (var i = 0; i < path.Count - 1; i++)
        {
            GetSquare((path[i] + path[i + 1]) / 2).RemovePiece();
            GetSquare(path[i]).RemovePiece();
        }

        GetSquare(path[path.Count - 1]).PutPiece(piece);
    }

    private void MoveAlongPath(List<SquareViewModel> path, Piece piece)
    {
        for (var i = 0; i < path.Count - 1; i++)
        {
            GetSquare((path[i].Pos + path[i + 1].Pos) / 2).RemovePiece();
            path[i].RemovePiece();
        }

        path[path.Count - 1].PutPiece(piece);
    }

    private SquareViewModel GetSquare(int row, int col)
    {
        if (row < 0 || row >= 8 || col < 0 || col >= 8)
            throw new IndexOutOfRangeException();
        return Squares[row * 8 + col];
    }

    private SquareViewModel GetSquare(Pos pos)
    {
        return GetSquare(pos.Row, pos.Col);
    }
}