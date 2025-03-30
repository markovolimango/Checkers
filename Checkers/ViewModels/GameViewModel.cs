using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Checkers.Models;
using CommunityToolkit.Mvvm.Input;

namespace Checkers.ViewModels;

public class GameViewModel : ViewModelBase
{
    private readonly Board _board = new();
    private readonly Engine.Engine _engine = new();
    private readonly List<byte> _path = [];

    private Move? _botMove;
    /*private readonly Board _board = new(new byte[,]
    {
        { 0, 0, 0, 3, 0, 3, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 3, 0, 0 },
        { 0, 0, 1, 0, 0, 0, 3, 0 },
        { 0, 3, 0, 0, 0, 3, 0, 0 },
        { 0, 0, 1, 0, 1, 0, 0, 0 },
        { 0, 0, 0, 1, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0 }
    }, true);*/

    private readonly int _botTimeLimitMs;

    private bool _isBotThinking;
    private List<Move> _moves = [];

    public GameViewModel(MainWindowViewModel mainWindowViewModel) : this()
    {
        MainWindowViewModel = mainWindowViewModel;
    }

    public GameViewModel()
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

        _botTimeLimitMs = 500;

        ExportCommand = new RelayCommand(ExportBoardState);
    }

    public Square[] Squares { get; }
    public ICommand ExportCommand { get; }

    private async void OnSquareClick(Square square)
    {
        if (_isBotThinking)
            return;

        var piece = _board[square.Mask];
        if (_path.Count == 0)
        {
            if (piece == Piece.Empty || (piece == Piece.WhiteMan || piece == Piece.WhiteKing) != _board.IsWhiteTurn)
                return;
            _path.Add(square.Index);
            square.Select();
            return;
        }

        int res;
        if (_path.Count == 1)
        {
            if (_board[square.Mask] != Piece.Empty)
            {
                Squares[_path[0]].Deselect();
                _path.Clear();
                return;
            }

            _moves = _board.FindMovesStartingWith(_path);
            res = MovePlayerPieceTo(square.Index);
            if (res == -1)
            {
                Squares[_path[0]].Deselect();
                _path.Clear();
            }

            if (res == 1)
            {
                Squares[_path[0]].Deselect();
                _path.Clear();
                await BotPlayMove(_botTimeLimitMs);
            }

            return;
        }

        if (_board[square.Mask] != Piece.Empty)
            return;
        res = MovePlayerPieceTo(square.Index);
        if (res == -1 || res == 0)
            return;
        Squares[_path[0]].Deselect();
        _path.Clear();
        await BotPlayMove(_botTimeLimitMs);
    }

    private async Task BotPlayMove(int timeLimitMs)
    {
        await Task.Run(() =>
        {
            _isBotThinking = true;
            _botMove = _engine.FindBestMoveWithTimeLimit(_board, timeLimitMs);
            Console.WriteLine(_botMove);
        });
        MoveBotPieceAlong(_botMove);
        _isBotThinking = false;
    }

    private int MovePlayerPieceTo(byte index)
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

    private async void MoveBotPieceAlong(Move? move)
    {
        if (move is null)
            return;

        Squares[move.Start].Select();
        var piece = _board[move.Start];
        _board.MakeMove(move);
        for (var i = 0; i < move.Path.Count - 1; i++)
        {
            await Task.Delay(800);
            Squares[move.Path[i]].RemovePiece();
            Squares[move.Path[i + 1]].PutPiece(piece);
        }

        foreach (var index in Board.GetPieceIndexes(move.Captures))
            Squares[index].RemovePiece();

        Squares[move.Start].Deselect();
    }

    private void ExportBoardState()
    {
        Console.WriteLine($"{_board}");
    }
}