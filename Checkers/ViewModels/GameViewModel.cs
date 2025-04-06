using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Checkers.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    private const string YourTurnText = "Your Turn", BotTurnText = "Thinking...";

    private readonly Board _board = new();
    private readonly Engine.Engine _engine = new();
    private readonly HintSystem _hintSystem;
    private readonly List<byte> _path = [];

    private readonly SettingsData _settings;

    private Move? _botMove;
    [ObservableProperty] private string _hintText;
    private bool _isBotThinking;
    private List<Move> _moves = [];
    [ObservableProperty] private string _questionText = "";
    [ObservableProperty] private string _turnText = "";

    public GameViewModel(MainWindowViewModel mainWindowViewModel)
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

        MainWindowViewModel = mainWindowViewModel;
        _settings = mainWindowViewModel.SettingsData;
        Console.WriteLine(MainWindowViewModel.SettingsData.BotThinkingTime);
        Console.WriteLine(_settings.BotThinkingTime);

        _hintSystem = new HintSystem(_settings.HintModelName);
        HintText = "";

        if (MainWindowViewModel.SettingsData.IsPlayerRed)
            IsBotThinking = false;
        else
            BotPlayMove(BotTimeLimitMs).Wait();
    }

    public Square[] Squares { get; }

    public string HintModelName => _settings.HintModelName;
    public GridLength HintsGridWidth => _settings.HintsEnabled ? new GridLength(1, GridUnitType.Star) : GridLength.Auto;
    private int BotTimeLimitMs => (int)(_settings.BotThinkingTime * 1000);
    public bool AreHintsEnabled => _settings.HintsEnabled;

    private bool IsBotThinking
    {
        get => _isBotThinking;
        set
        {
            _isBotThinking = value;
            TurnText = _isBotThinking ? BotTurnText : YourTurnText;
        }
    }

    private async void OnSquareClick(Square square)
    {
        if (IsBotThinking)
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
            res = await MovePlayerPieceTo(square.Index);
            if (res == -1)
            {
                Squares[_path[0]].Deselect();
                _path.Clear();
            }

            if (res == 1)
            {
                Squares[_path[0]].Deselect();
                _path.Clear();
                await BotPlayMove(BotTimeLimitMs);
            }

            return;
        }

        if (_board[square.Mask] != Piece.Empty)
            return;
        res = await MovePlayerPieceTo(square.Index);
        if (res == -1 || res == 0)
            return;
        Squares[_path[0]].Deselect();
        _path.Clear();
        Console.WriteLine(BotTimeLimitMs);
        await BotPlayMove(BotTimeLimitMs);
    }

    private async Task BotPlayMove(int timeLimitMs)
    {
        await Task.Run(() =>
        {
            IsBotThinking = true;
            Console.WriteLine($"BotPlayMove: {timeLimitMs}");
            _botMove = _engine.FindBestMoveWithTimeLimit(_board, timeLimitMs);
            Console.WriteLine(_botMove);
        });
        await MoveBotPieceAlong(_botMove);
        IsBotThinking = false;
        if (_board.KingsMoves.Count == 0 && _board.MenMoves.Count == 0 && MainWindowViewModel is not null)
        {
            await Task.Delay(800);
            MainWindowViewModel.LoadEndViewModel(false);
        }
    }

    private async Task<int> MovePlayerPieceTo(byte index)
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
            Squares[last].RemovePiece();
            if (moves[0].Captures != 0)
                Squares[(last + index) / 2].RemovePiece();
            _board.MakeMove(moves[0]);
            Squares[index].PutPiece(_board[moves[0].End]);
            if (_board.KingsMoves.Count == 0 && _board.MenMoves.Count == 0 && MainWindowViewModel is not null)
            {
                await Task.Delay(800);
                MainWindowViewModel.LoadEndViewModel(true);
            }

            return 1;
        }

        Squares[index].PutPiece(_board[_path[0]]);
        Squares[last].RemovePiece();
        Squares[(last + index) / 2].RemovePiece();
        return 0;
    }

    private async Task MoveBotPieceAlong(Move? move)
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

        Squares[move.End].PutPiece(_board[move.End]);
        foreach (var index in Board.GetPieceIndexes(move.Captures))
            Squares[index].RemovePiece();

        Squares[move.Start].Deselect();
    }

    public async void GetHint()
    {
        var questionText = QuestionText;
        QuestionText = "";
        await _hintSystem.GetHint(_board, this, questionText);
    }

    public void ExportBoardState()
    {
        Console.WriteLine($"{_board}");
    }

    public void LoadMainMenu()
    {
        if (MainWindowViewModel is null) return;
        MainWindowViewModel.LoadMainMenuViewModel();
    }
}