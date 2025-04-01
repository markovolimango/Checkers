using System;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Checkers.Models;

public class HintSystem
{
    private const string Prompt = "boy";
    private readonly IChatClient _chatClient;

    public HintSystem(string modelName)
    {
        _chatClient = new OllamaChatClient(new Uri("http://localhost:11434/"), modelName);
    }

    public async Task<string> GetHint(Board board)
    {
        var prompt =
            "You are an expert checkers player. I will provide you with the board state and a list of all legal moves" +
            " for the current position. Your task is to select the best move based on the given options." +
            "\nyou will get the board state represented as an 8x8 matrix" +
            "\n The coordinates of the top left corner are (0, 0) and the bottom right (7, 7). Remember this when " +
            "reading the available moves." +
            "\n\n**Rules:**" +
            "\n- Only choose from the moves I provide. Do NOT invent new moves." +
            "\n- If multiple jumps exist, choose the one that captures the most pieces." +
            "\n- If no jumps exist, pick the move that provides the best positional advantage." +
            "\n- Consider factors like kinging opportunities, board control, and safety." +
            "\n\n**Board State**" +
            "\n" + board +
            "\n" + (board.IsWhiteTurn ? "White" : "Red") + "'s turn" +
            "\n\n**Legal Moves:**";
        foreach (var move in board.KingsMoves)
            prompt += "\n" + move;
        foreach (var move in board.MenMoves)
            prompt += "\n" + move;
        prompt += "\n\n**Response Format:**" +
                  "\n1.Your response should be \"Best move: \" and then only one of the moves, written the same " +
                  "way as it was given to you. Nothing else, no explanations." +
                  "\n\nOutput Examples:" +
                  "\n**First Example: Best Move: (5, 2) -> (4, 3)" +
                  "\n**Second Example: Best Move: (1, 2) -> (3, 4), capturing (2, 3)";
        try
        {
            var message = new ChatMessage(ChatRole.User, prompt);
            Console.WriteLine(prompt);
            Console.WriteLine("Thinking...");
            var item = await _chatClient.GetResponseAsync(message);
            Console.WriteLine(item.Text);
            return item.Text;
        }
        catch (Exception e)
        {
            return "To get hints install deepsek-r1 with ollama.";
        }
    }
}