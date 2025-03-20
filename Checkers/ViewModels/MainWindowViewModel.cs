using System;
using System.Collections.Generic;
using System.Windows.Input;
using Checkers.Models;
using CommunityToolkit.Mvvm.Input;

namespace Checkers.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    /*private readonly Board _board = new(new byte[,]
    {
        { 0, 3, 0, 3, 0, 1, 0, 3 },
        { 3, 0, 0, 0, 3, 0, 3, 0 },
        { 0, 3, 0, 0, 0, 3, 0, 3 },
        { 0, 0, 0, 0, 3, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 1, 0, 0 },
        { 1, 0, 1, 0, 3, 0, 1, 0 },
        { 0, 0, 0, 1, 0, 0, 0, 1 },
        { 1, 0, 1, 0, 1, 0, 1, 0 }
    }, false);*/

    private readonly Board _board = new();
    private readonly Bot.Bot _bot;
    private readonly List<byte> _path = new(4);
    private List<Move> _moves = new(10);

    public MainWindowViewModel()
    {
        Squares = new Square[64];
        var isDark = true;
        for (var row = 0; row < 8; row++)
        {
            isDark = !isDark;
            for (var col = 0; col < 8; col++)
            {
                var square = new Square(row, col, isDark, OnSquareClick);
                Squares[row * 8 + col] = square;
                square.PutPiece(_board[row, col]);
                isDark = !isDark;
            }
        }

        ExportCommand = new RelayCommand(ExportBoardState);
        _bot = new Bot.Bot();
    }

    public Square[] Squares { get; }
    public ICommand ExportCommand { get; }

    private void OnSquareClick(Square square)
    {
        var piece = _board[square.Mask];
        if (_path.Count == 0)
        {
            if (piece == Piece.Empty || piece.IsBlack() != _board.IsBlackTurn)
                return;
            _path.Add(square.Index);
            square.Select();
            return;
        }

        var res = 0;
        if (_path.Count == 1)
        {
            if (_board[square.Mask] != Piece.Empty)
            {
                Squares[_path[0]].Deselect();
                _path.Clear();
                return;
            }

            _moves = _board.FindMovesStartingWith(_path);
            res = MoveTo(square.Index);
            if (res == -1)
            {
                Squares[_path[0]].Deselect();
                _path.Clear();
            }

            if (res == 1)
            {
                Squares[_path[0]].Deselect();
                _path.Clear();
                Console.WriteLine($"Evaluation: {_bot.Evaluate(_board, 6)}");
            }

            return;
        }

        if (_board[square.Mask] != Piece.Empty)
            return;
        res = MoveTo(square.Index);
        if (res == -1 || res == 0)
            return;
        Squares[_path[0]].Deselect();
        _path.Clear();
        Console.WriteLine($"Evaluation: {_bot.Evaluate(_board, 6)}");
    }

    private int MoveTo(byte index)
    {
        var last = _path[^1];
        _path.Add(index);
        var moves = _board.FindMovesStartingWith(_path, _moves);
        if (moves.Count == 0)
        {
            _path.RemoveAt(_path.Count - 1);
            return -1;
        }

        _moves = moves;
        if (_moves[0].Path.Count == _path.Count)
        {
            Squares[index].PutPiece(_board[_path[0]]);
            Squares[last].RemovePiece();
            if (moves[0].Captures != 0)
                Squares[(last + index) / 2].RemovePiece();
            _board.MakeMove(moves[0]);
            return 1;
        }

        Squares[index].PutPiece(_board[_path[0]]);
        Squares[last].RemovePiece();
        Squares[(last + index) / 2].RemovePiece();
        return 0;
    }

    private void ExportBoardState()
    {
        Console.WriteLine($"{_board}");
    }
}