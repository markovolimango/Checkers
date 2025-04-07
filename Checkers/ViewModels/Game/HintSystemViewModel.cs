using System.Threading.Tasks;
using Checkers.Models;
using Checkers.Models.Board;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Checkers.ViewModels.Game;

public partial class HintSystemViewModel : ObservableObject
{
    private readonly HintSystem _hintSystem;
    [ObservableProperty] private string _question;
    [ObservableProperty] private string _response;

    public HintSystemViewModel(string modelName)
    {
        _hintSystem = new HintSystem(modelName);
        Response = "";
        Question = "";
    }

    public async Task GetHint(Board board)
    {
        Response += $"\n{Question}\n";
        Question = "";
        await _hintSystem.GetHint(board, Question, UpdateText);
    }

    private void UpdateText(string response)
    {
        Response += response;
    }
}