using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Checkers.Models;
using CommunityToolkit.Mvvm.Input;

namespace Checkers.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly Board _board = new();
    private readonly Bot.Bot _bot = new();

    /*private readonly Board _board = new(new Piece[8, 8]
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
    }, Team.Red);*/

    private readonly List<Pos> _currentPath = [];
    private List<Move> _possibleMoves = [];

    public MainWindowViewModel()
    {
        Squares = [];
        var isDark = true;
        for (var row = 0; row < 8; row++)
        {
            isDark = !isDark;
            for (var col = 0; col < 8; col++)
            {
                var square = new Square(row, col, isDark, OnSquareClick);
                Squares.Add(square);
                square.PutPiece(_board.GetPiece(row, col));
                isDark = !isDark;
            }
        }

        ExportCommand = new RelayCommand(ExportBoardState);
    }

    public ObservableCollection<Square> Squares { get; }
    public ICommand ExportCommand { get; }

    private Square SelectedSquare => GetSquare(_currentPath[0]);
    private Piece SelectedPiece => _board.GetPiece(_currentPath[0]);

    private Square GetSquare(int row, int col)
    {
        if (row < 0 || row >= 8 || col < 0 || col >= 8)
            throw new IndexOutOfRangeException();
        return Squares[row * 8 + col];
    }

    private Square GetSquare(Pos pos)
    {
        return GetSquare(pos.Row, pos.Col);
    }

    private void OnSquareClick(Square square)
    {
        var pos = square.Pos;
        var piece = _board.GetPiece(square.Pos);
        if (_currentPath.Count == 0)
        {
            if (piece.GetTeam() != _board.CurrentTurn) return;

            _currentPath.Add(pos);
            square.Select();
            return;
        }

        if (_currentPath.Count == 1)
        {
            if (_currentPath[0] == pos)
            {
                square.Deselect();
                _currentPath.Clear();
                return;
            }


            if (piece.GetTeam() == _board.GetPiece(_currentPath[0]).GetTeam())
            {
                SelectedSquare.Deselect();
                square.Select();
                _currentPath[0] = pos;
                return;
            }

            _possibleMoves = _board.FindMovesStartingWith(_currentPath);
            if (MoveTo(pos) != 0)
            {
                SelectedSquare.Deselect();
                _currentPath.Clear();
                Console.WriteLine($"{_bot.Evaluate(_board, 6)}");
            }

            return;
        }

        if (piece != Piece.Empty) return;
        var res = MoveTo(pos);
        if (res == -1)
        {
            _currentPath.RemoveAt(_currentPath.Count - 1);
            return;
        }

        if (res == 0) return;

        if (res == 1)
        {
            SelectedSquare.Deselect();
            _currentPath.Clear();
            Console.WriteLine($"{_bot.Evaluate(_board, 6)}");
        }
    }

    private int MoveTo(Pos pos)
    {
        var last = _currentPath[^1];
        _currentPath.Add(pos);
        var moves = _board.FindMovesStartingWith(_currentPath, _possibleMoves);
        if (moves.Count == 0)
        {
            _currentPath.RemoveAt(_currentPath.Count - 1);
            return -1;
        }

        _possibleMoves = moves;

        if (moves[0].Path.Count == _currentPath.Count)
        {
            GetSquare(pos).PutPiece(pos.IsInKingsRow ? SelectedPiece.Promote() : SelectedPiece);
            GetSquare(last).RemovePiece();
            if (moves[0].Captures.Count != 0)
                GetSquare((last + pos) / 2).RemovePiece();
            _board.MakeMove(moves[0]);
            return 1;
        }

        GetSquare(_currentPath[^1]).PutPiece(SelectedPiece);
        GetSquare(_currentPath[^2]).RemovePiece();
        GetSquare((last + pos) / 2).RemovePiece();
        return 0;
    }

    private void ExportBoardState()
    {
        Console.WriteLine($"{_board}");
    }
}