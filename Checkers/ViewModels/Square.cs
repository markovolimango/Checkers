using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Checkers.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Checkers.ViewModels;

public partial class Square : ViewModelBase
{
    private const string AssetsPath = "avares://Checkers/Assets/";
    private const string BoardAssetsPath = AssetsPath + "Board/", PieceAssetsPath = AssetsPath + "Pieces/";

    private static readonly Dictionary<Piece, Bitmap?> PieceImages = new()
    {
        { Piece.Empty, null },
        { Piece.BlackMan, new Bitmap(AssetLoader.Open(new Uri(PieceAssetsPath + "BlackMan.png"))) },
        { Piece.BlackKing, new Bitmap(AssetLoader.Open(new Uri(PieceAssetsPath + "BlackKing.png"))) },
        { Piece.RedMan, new Bitmap(AssetLoader.Open(new Uri(PieceAssetsPath + "RedMan.png"))) },
        { Piece.RedKing, new Bitmap(AssetLoader.Open(new Uri(PieceAssetsPath + "RedKing.png"))) }
    };

    private static readonly Bitmap
        DarkSquareImage = new(AssetLoader.Open(new Uri(BoardAssetsPath + "DarkSquare.png"))),
        LightSquareImage = new(AssetLoader.Open(new Uri(BoardAssetsPath + "LightSquare.png"))),
        SelectedSquareImage = new(AssetLoader.Open(new Uri(BoardAssetsPath + "SelectedSquare.png")));

    private readonly Bitmap _squareImage;

    [ObservableProperty] private Bitmap _backgroundImage;
    [ObservableProperty] private Bitmap? _pieceImage;

    public Square(int row, int col, bool isDark, Action<Square> onSquareClick)
    {
        Row = row;
        Col = col;
        Index = (byte)(row * 8 + col);
        Mask = 1UL << Index;

        ClickCommand = new RelayCommand(() => onSquareClick(this));

        _squareImage = isDark ? DarkSquareImage : LightSquareImage;
        BackgroundImage = _squareImage;
    }

    public int Row { get; }
    public int Col { get; }
    public byte Index { get; }
    public ulong Mask { get; }

    public ICommand ClickCommand { get; }

    public void PutPiece(Piece piece)
    {
        PieceImage = PieceImages[piece];
    }

    public void RemovePiece()
    {
        PutPiece(Piece.Empty);
    }

    public void Select()
    {
        BackgroundImage = SelectedSquareImage;
    }

    public void Deselect()
    {
        BackgroundImage = _squareImage;
    }
}