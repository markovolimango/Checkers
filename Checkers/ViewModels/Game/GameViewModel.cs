using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Checkers.Models;
using Checkers.Models.Board;
using Checkers.ViewModels.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Checkers.ViewModels.Game;

public partial class GameViewModel : ViewModelBase
{
    private const string YourTurnText = "Your Turn", BotTurnText = "Thinking...";
    
    private readonly Board _board=new(
        "Row 0: ., w, ., w, ., ., ., w\n" +
        "Row 1: r, ., w, ., ., ., W, .\n" +
        "Row 2: ., ., ., ., ., ., ., .\n" +
        "Row 3: W, ., ., ., ., ., ., .\n" +
        "Row 4: ., ., ., ., ., ., ., w\n" +
        "Row 5: ., ., ., ., R, ., ., .\n" +
        "Row 6: ., ., ., ., ., ., ., .\n" +
        "Row 7: R, ., ., ., ., ., ., .\n"
        ,false);

    //private readonly Board _board = new();
    private readonly Engine _engine = new();

    private readonly List<byte> _path = [];
    private Move? _botMove;

    private bool _isBotThinking;
    private List<Move> _moves = [];
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
        SettingsData = mainWindowViewModel.SettingsData;

        HintSystemViewModel = new HintSystemViewModel(SettingsData.HintModelName);
        GetHintCommand = new AsyncRelayCommand(() => HintSystemViewModel.GetHint(_board));

        if (MainWindowViewModel.SettingsData.IsPlayerRed)
            IsBotThinking = false;
        else
            BotPlayMove(BotTimeLimitMs).Wait();
    }

    public Square[] Squares { get; }
    public SettingsData SettingsData { get; }
    public HintSystemViewModel HintSystemViewModel { get; }
    public ICommand GetHintCommand { get; }

    public GridLength HintsGridWidth =>
        SettingsData.HintsEnabled ? new GridLength(1, GridUnitType.Star) : GridLength.Auto;

    private int BotTimeLimitMs => (int)(SettingsData.BotThinkingTime * 1000);

    private bool IsBotThinking
    {
        get => _isBotThinking;
        set
        {
            _isBotThinking = value;
            TurnText = _isBotThinking ? BotTurnText : YourTurnText;
        }
    }

    /// <summary>
    ///     Handles selecting / deselecting squares and checking if moves are legal
    /// </summary>
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
        await BotPlayMove(BotTimeLimitMs);
    }

    /// <summary>
    ///     Finds the best move and plays it
    /// </summary>
    private async Task BotPlayMove(int timeLimitMs)
    {
        await Task.Run(() =>
        {
            IsBotThinking = true;
            _botMove = _engine.FindBestMoveWithTimeLimit(_board, timeLimitMs);
        });
        await MoveBotPieceAlong(_botMove);
        IsBotThinking = false;
        await CheckForWin(false);
    }

    /// <summary>
    ///     Moves a piece to the clicked square
    /// </summary>
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
            await CheckForWin(true);

            return 1;
        }

        Squares[index].PutPiece(_board[_path[0]]);
        Squares[last].RemovePiece();
        Squares[(last + index) / 2].RemovePiece();
        return 0;
    }

    /// <summary>
    ///     Loads the end screen if someone has won
    /// </summary>
    private async Task CheckForWin(bool isPlayerTurn)
    {
        if (_board.KingsMoves.Count == 0 && _board.MenMoves.Count == 0 && MainWindowViewModel is not null)
        {
            await Task.Delay(800);
            MainWindowViewModel.LoadEndViewModel(isPlayerTurn);
        }
    }

    /// <summary>
    ///     Moves a piece along a given path
    /// </summary>
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

    public void ExportBoardState()
    {
        Console.WriteLine($"{_board}");
    }
}