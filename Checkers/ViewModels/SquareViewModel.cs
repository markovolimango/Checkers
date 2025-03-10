using System;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Checkers.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Checkers.ViewModels;

public partial class SquareViewModel : ViewModelBase
{
    private const string AssetsPath = "avares://Checkers/Assets";
    private readonly string _backgroundImagePath;
    private readonly string _selectedSquarePath = AssetsPath + "/Board/SelectedSquare.png";
    [ObservableProperty] private Bitmap? _backgroundImage;
    [ObservableProperty] private Bitmap? _pieceImage;

    public SquareViewModel(int row, int column, bool isDark, Action<SquareViewModel> onSquareClick)
    {
        Pos = new Pos(row, column);
        _backgroundImagePath = AssetsPath + "/Board/" + (isDark ? "DarkSquare.png" : "LightSquare.png");
        BackgroundImage = new Bitmap(AssetLoader.Open(new Uri(_backgroundImagePath)));
        PieceImage = null;
        ClickCommand = new RelayCommand(() => onSquareClick(this));
    }

    public Pos Pos { get; }
    public ICommand ClickCommand { get; }

    public void PutPiece(Piece piece)
    {
        var pieceImageName = "";
        switch (piece)
        {
            case Piece.BlackMan:
                pieceImageName = "BlackMan.png";
                break;
            case Piece.RedMan:
                pieceImageName = "RedMan.png";
                break;
            case Piece.BlackKing:
                pieceImageName = "BlackKing.png";
                break;
            case Piece.RedKing:
                pieceImageName = "RedKing.png";
                break;
        }

        if (pieceImageName == "")
        {
            PieceImage = null;
            return;
        }

        var pieceImagePath = AssetsPath + "/Pieces/" + pieceImageName;
        PieceImage = new Bitmap(AssetLoader.Open(new Uri(pieceImagePath)));
    }

    public void RemovePiece()
    {
        PutPiece(Piece.Empty);
    }

    public void Select()
    {
        BackgroundImage = new Bitmap(AssetLoader.Open(new Uri(_selectedSquarePath)));
    }

    public void Deselect()
    {
        BackgroundImage = new Bitmap(AssetLoader.Open(new Uri(_backgroundImagePath)));
    }
}