using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Checkers.ViewModels;
using Microsoft.Extensions.AI;

namespace Checkers.Models;

public class HintSystem
{
    private readonly IChatClient _chatClient;
    private readonly List<ChatMessage> _chatHistory;
    private readonly string _modelName;

    public HintSystem(string modelName)
    {
        _modelName = modelName;
        _chatClient = new OllamaChatClient(new Uri("http://localhost:11434/"), modelName);
        var intro =
            "You are an expert checkers player. I will provide you with the board state and a list of all legal moves for the current position. Your task is to analyze the position and answer player questions." +
            "\n\n**Board Representation:**" +
            "\n- The board is an 8x8 matrix where coordinates start at (0,0) in the top left" +
            "\n- Red pieces are represented as 'r' (regular) and 'R' (kings)" +
            "\n- White pieces are represented as 'w' (regular) and 'W' (kings)" +
            "\n- Empty squares are represented as '.'" +
            "\n\n**Move Format:**" +
            "\nMoves are represented as a list of coordinates the piece visits, followed by captured pieces when applicable:" +
            "\n- Simple move example: (2, 1) -> (3, 2) - piece moves from (2,1) to (3,2)" +
            "\n- Capture move example: (2, 1) -> (4, 3) capturing (3, 2) - piece moves from (2,1) to (4,3) capturing opponent's piece at (3,2)" +
            "\n- Multiple capture example: (2, 1) -> (4, 3) -> (6, 5) capturing (3, 2), (5, 4) - piece moves from (2,1) to (6,5) capturing pieces at (3,2) and (5,4)" +
            "\n\n**Rules:**" +
            "\n- Do NOT invent new moves. The moves you'll be given are the only legal moves in the position." +
            "\n\n**Game Rules Reminder:**" +
            "\n- Regular pieces move diagonally forward one square" +
            "\n- Kings can move diagonally forward or backward one square" +
            "\n- Captures are mandatory and occur by jumping over an opponent's piece" +
            "\n- Multiple captures in sequence are required when possible" +
            "\n- Regular pieces that reach the opponent's back row become kings" +
            "\n- If capture moves are possible only they will be listed." +
            "\n\n**Response Rules:**" +
            "\nKeep your response brief. Only provide necessary information. Do not yap. Answer the player's question directly and clearly." +
            "\nDO NOT talk about anything other than checkers. Answer questions if they're related to checkers, if not do not answer.";
        _chatHistory = [new ChatMessage(ChatRole.User, intro)];
    }

    public async Task GetHint(Board board, GameViewModel gameViewModel, string question)
    {
        if (question.Length < 2)
        {
            await GetHint(board, gameViewModel);
            return;
        }

        var prompt = GeneratePrompt(board);
        prompt += $"\n{question}";
        _chatHistory.Add(new ChatMessage(ChatRole.User, prompt));
        try
        {
            Console.WriteLine(prompt);
            gameViewModel.HintText += "\n" + question + "\n";
            var response = "";
            await foreach (var item in _chatClient.GetStreamingResponseAsync(_chatHistory))
            {
                Console.Write(item.Text);
                gameViewModel.HintText += item.Text;
                response += item.Text;
            }

            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
        }
        catch
        {
            gameViewModel.HintText = $"Couldn't find {_modelName} with ollama";
        }
    }

    public async Task GetHint(Board board, GameViewModel gameViewModel)
    {
        var question = "\nWhat's the best move in this position?";
        await GetHint(board, gameViewModel, question);
    }

    private string GeneratePrompt(Board board)
    {
        var res =
            "\n\n**Board State:**" +
            "\n" + board +
            "\n" + (board.IsWhiteTurn ? "White" : "Red") + "'s turn" +
            "\n\n**Legal Moves:**";
        foreach (var move in board.KingsMoves)
            res += "\n" + move;
        foreach (var move in board.MenMoves)
            res += "\n" + move;
        res += "\n\n**Reminders:**" +
               "\nKeep your response brief. Only provide necessary information. Do not yap." +
               "\nDo NOT talk about anything other than checkers. Under any circumstances DO NOT TALK ABOUT ANYTHING OTHER THAN CHECKERS." +
               "\nDo NOT invent new moves. The moves you were given are the only legal moves in the position."+
               "\n*The Question: (This is your task, answer what you're given here)*";
        return res;
    }
}