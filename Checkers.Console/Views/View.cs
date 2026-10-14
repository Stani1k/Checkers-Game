using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;

namespace Checkers.Console.Views
{
    /// <summary>
    /// Plain, linear console UI. It never positions the cursor absolutely, so it works in any terminal or IDE console.
    /// </summary>
    public sealed class View
    {
        private const int CellWidth = 6;
        private const int CellHeight = 2;
        private const string Separator = "------------------------------------------------------------";

        private static readonly Colors[] SelectableColors = { Colors.Red, Colors.Green, Colors.Yellow, Colors.Pink, Colors.Blue };

        public void SetUp()
        {
            ResetColors();
            ClearScreen();
        }

        public static ConsoleColor ToConsoleColor(Colors color) => color switch
        {
            Colors.Red => ConsoleColor.Red,
            Colors.Green => ConsoleColor.Green,
            Colors.Yellow => ConsoleColor.Yellow,
            Colors.Pink => ConsoleColor.Magenta,
            _ => ConsoleColor.Cyan
        };

        private static string ColorName(Colors color) => color.ToString();

        private static void ResetColors()
        {
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = ConsoleColor.White;
        }

        private static void ClearScreen()
        {
            try
            {
                System.Console.Clear();
            }
            catch (IOException)
            {
                // Output is redirected; nothing to clear.
            }
        }

        public string ReadLine()
        {
            var line = System.Console.ReadLine();
            if (line == null)
            {
                throw new EndOfInputException();
            }

            return line.Trim();
        }

        private string Ask(string prompt)
        {
            System.Console.Write(prompt);
            return ReadLine();
        }

        public void ShowMessage(string message)
        {
            System.Console.WriteLine(message);
        }

        public void ShowError(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine(message);
            ResetColors();
        }

        public void WaitForEnter()
        {
            System.Console.Write("Press Enter to continue...");
            ReadLine();
        }

        public MenuOption ShowMainMenu()
        {
            while (true)
            {
                ClearScreen();
                System.Console.WriteLine(Separator);
                System.Console.WriteLine("  Welcome to the game Checkers!");
                System.Console.WriteLine(Separator);
                System.Console.WriteLine("[1] Start Game");
                System.Console.WriteLine("[2] Help");
                System.Console.WriteLine("[3] About");
                System.Console.WriteLine("[4] Quit");
                System.Console.WriteLine();

                switch (Ask("Please choose: "))
                {
                    case "1": return MenuOption.Play;
                    case "2": return MenuOption.Help;
                    case "3": return MenuOption.About;
                    case "4": return MenuOption.Quit;
                }

                ShowError("Please make a valid selection!");
                WaitForEnter();
            }
        }

        public Variant AskVariant()
        {
            ClearScreen();
            System.Console.WriteLine("Choose a variant:");
            System.Console.WriteLine("[1] American  - 8x8 board, kings move one square, captures are mandatory");
            System.Console.WriteLine("[2] European  - 10x10 board, flying kings, men capture backwards, the longest capture must be taken");
            while (true)
            {
                switch (Ask("Variant (1/2): "))
                {
                    case "1": return Variant.American;
                    case "2": return Variant.European;
                }

                ShowError("Please enter 1 or 2.");
            }
        }

        public string AskName(int playerNumber, string? takenName)
        {
            while (true)
            {
                var name = Ask($"Player {playerNumber}, please enter your name: ");
                if (string.IsNullOrWhiteSpace(name))
                {
                    ShowError("The name must not be empty.");
                }
                else if (string.Equals(name, takenName, StringComparison.OrdinalIgnoreCase))
                {
                    ShowError("Both players need different names.");
                }
                else
                {
                    return name;
                }
            }
        }

        public Colors AskColor(string playerName, IReadOnlyCollection<Colors> takenColors)
        {
            var available = SelectableColors.Where(c => !takenColors.Contains(c)).ToList();
            System.Console.WriteLine($"{playerName}, choose the color of your checkers:");
            for (var i = 0; i < available.Count; i++)
            {
                System.Console.Write($"[{i + 1}] ");
                System.Console.ForegroundColor = ToConsoleColor(available[i]);
                System.Console.WriteLine(ColorName(available[i]));
                ResetColors();
            }

            while (true)
            {
                var input = Ask("Color: ");
                if (int.TryParse(input, out var index) && index >= 1 && index <= available.Count)
                {
                    return available[index - 1];
                }

                var byName = available.Where(c => string.Equals(ColorName(c), input, StringComparison.OrdinalIgnoreCase)).ToList();
                if (byName.Count == 1)
                {
                    return byName[0];
                }

                ShowError("Invalid color.");
            }
        }

        public Side AskSide(string playerName)
        {
            System.Console.WriteLine($"{playerName}, which side do you want to play?");
            System.Console.WriteLine("[1] Bottom (moves first)");
            System.Console.WriteLine("[2] Top");
            while (true)
            {
                switch (Ask("Side (1/2): "))
                {
                    case "1": return Side.Bottom;
                    case "2": return Side.Top;
                }

                ShowError("Please enter 1 or 2.");
            }
        }

        public void ShowGame(CheckersGame game, PlayerInfo bottom, PlayerInfo top, string? message)
        {
            ClearScreen();
            DrawBoard(game, bottom, top);
            System.Console.WriteLine();
            DrawPlayerLine(game, top, "Top   ");
            DrawPlayerLine(game, bottom, "Bottom");
            if (game.LastMove != null)
            {
                System.Console.WriteLine($"Last move: {FormatMove(game, game.LastMove)}");
            }

            if (!string.IsNullOrEmpty(message))
            {
                ShowError(message);
            }

            if (game.Status != GameStatus.InProgress)
            {
                return;
            }

            var turnPlayer = game.Turn == Side.Bottom ? bottom : top;
            System.Console.WriteLine(Separator);
            System.Console.Write("Turn: ");
            System.Console.ForegroundColor = ToConsoleColor(turnPlayer.Color);
            System.Console.WriteLine($"{turnPlayer.Name} ({ColorName(turnPlayer.Color)})");
            ResetColors();
            if (game.GetLegalMoves().Any(m => m.IsCapture))
            {
                System.Console.WriteLine("You must capture!");
            }

            System.Console.WriteLine("Enter a move like c3 d4 (jumps: c3 e5 g7). Commands: moves, help, surrender, quit");
        }

        private void DrawPlayerLine(CheckersGame game, PlayerInfo player, string label)
        {
            System.Console.Write($"{label}: ");
            System.Console.ForegroundColor = ToConsoleColor(player.Color);
            System.Console.Write($"{player.Name} ({ColorName(player.Color)})");
            ResetColors();
            System.Console.WriteLine($" - {game.CountPieces(player.Side)} pieces");
        }

        private void DrawBoard(CheckersGame game, PlayerInfo bottom, PlayerInfo top)
        {
            for (var row = 0; row < game.Size; row++)
            {
                for (var line = 0; line < CellHeight; line++)
                {
                    ResetColors();
                    System.Console.Write(line == 0 ? $"{game.Size - row,2} " : "   ");
                    for (var col = 0; col < game.Size; col++)
                    {
                        DrawCell(game, new Square(col, row), line, bottom, top);
                    }

                    ResetColors();
                    System.Console.WriteLine();
                }
            }

            ResetColors();
            System.Console.Write("   ");
            for (var col = 0; col < game.Size; col++)
            {
                var letter = (char)('a' + col);
                System.Console.Write(new string(' ', CellWidth / 2 - 1) + letter + new string(' ', CellWidth - CellWidth / 2));
            }

            System.Console.WriteLine();
        }

        private static void DrawCell(CheckersGame game, Square square, int line, PlayerInfo bottom, PlayerInfo top)
        {
            var dark = game.IsDarkSquare(square);
            System.Console.BackgroundColor = dark ? ConsoleColor.DarkGray : ConsoleColor.Gray;
            var piece = game.PieceAt(square);
            if (piece == null || line != 0)
            {
                System.Console.Write(new string(' ', CellWidth));
                return;
            }

            var owner = piece.Value.Side == Side.Bottom ? bottom : top;
            System.Console.ForegroundColor = ToConsoleColor(owner.Color);
            var glyph = piece.Value.IsKing ? "K" : "O";
            System.Console.Write(new string(' ', CellWidth / 2 - 1) + glyph + new string(' ', CellWidth - CellWidth / 2));
        }

        public static string FormatMove(CheckersGame game, Move move)
        {
            var separator = move.IsCapture ? " x " : " - ";
            return string.Join(separator, move.Path.Select(game.ToNotation));
        }

        public void ShowLegalMoves(CheckersGame game)
        {
            var moves = game.GetLegalMoves();
            System.Console.WriteLine($"Possible moves ({moves.Count}):");
            System.Console.WriteLine(string.Join("   ", moves.Select(m => FormatMove(game, m))));
        }

        public void ShowHelp()
        {
            ClearScreen();
            System.Console.WriteLine(Separator);
            System.Console.WriteLine("  Help");
            System.Console.WriteLine(Separator);
            System.Console.WriteLine("Goal: capture all pieces of your opponent or leave him without a legal move.");
            System.Console.WriteLine();
            System.Console.WriteLine("Entering moves: type the squares, e.g.  c3 d4   or  c3-d4.");
            System.Console.WriteLine("For multiple jumps type every landing square, e.g.  c3 e5 g7");
            System.Console.WriteLine("(start and end alone are enough when they are unambiguous).");
            System.Console.WriteLine("Commands: moves = list your possible moves, help, surrender, quit (back to menu).");
            System.Console.WriteLine();
            System.Console.WriteLine("Common rules:");
            System.Console.WriteLine(" - Pieces move diagonally on the dark squares; the bottom player starts.");
            System.Console.WriteLine(" - Captures are mandatory. A piece cannot be jumped twice in one turn.");
            System.Console.WriteLine(" - A man reaching the far row becomes a king (K).");
            System.Console.WriteLine();
            System.Console.WriteLine("American (8x8): men move and capture forward only; kings move and capture one square");
            System.Console.WriteLine("  in any direction. You may choose any capture sequence. A man that is crowned while");
            System.Console.WriteLine("  capturing ends its turn.");
            System.Console.WriteLine("European (10x10): men capture forward and backward; kings fly over any number of free");
            System.Console.WriteLine("  squares; you must take the capture sequence with the most pieces.");
            System.Console.WriteLine("A game is a draw after a long series of king-only moves without a capture.");
            System.Console.WriteLine();
            WaitForEnter();
        }

        public void ShowAbout()
        {
            ClearScreen();
            System.Console.WriteLine(Separator);
            System.Console.WriteLine("  Checkers - console edition");
            System.Console.WriteLine("  American and European draughts for two players.");
            System.Console.WriteLine(Separator);
            WaitForEnter();
        }

        public bool Confirm(string question)
        {
            while (true)
            {
                switch (Ask($"{question} (y/n): ").ToLowerInvariant())
                {
                    case "y":
                    case "yes":
                        return true;
                    case "n":
                    case "no":
                        return false;
                }
            }
        }

        public void ShowResult(CheckersGame game, PlayerInfo bottom, PlayerInfo top, string? message = null)
        {
            ShowGame(game, bottom, top, message);
            System.Console.WriteLine(Separator);
            switch (game.Status)
            {
                case GameStatus.BottomWins:
                    ShowMessage($"Congratulations {bottom.Name}, you have won the game!");
                    break;
                case GameStatus.TopWins:
                    ShowMessage($"Congratulations {top.Name}, you have won the game!");
                    break;
                case GameStatus.Draw:
                    ShowMessage("The game ended in a draw.");
                    break;
            }
        }

        public void ShowSurrender(PlayerInfo loser, PlayerInfo winner)
        {
            ShowMessage($"{loser.Name} has surrendered. {winner.Name} wins the game!");
        }

        public void ShowGoodbye()
        {
            ShowMessage("Thanks for playing. Goodbye!");
        }
    }
}
