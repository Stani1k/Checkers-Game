using System;
using System.Collections.Generic;
using System.Linq;
using Checkers.Console.Views;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;

namespace Checkers.Console
{
    public class GameController
    {
        private static readonly char[] MoveSeparators = { ' ', '\t', '-', 'x', 'X', ':', ',', '>' };

        private readonly View _view = new();

        public void RunApplication()
        {
            try
            {
                _view.SetUp();
                RunMainMenu();
            }
            catch (EndOfInputException)
            {
                // Input stream closed: leave quietly.
            }
        }

        private void RunMainMenu()
        {
            while (true)
            {
                switch (_view.ShowMainMenu())
                {
                    case MenuOption.Play:
                        PlayGame();
                        break;
                    case MenuOption.Help:
                        _view.ShowHelp();
                        break;
                    case MenuOption.About:
                        _view.ShowAbout();
                        break;
                    case MenuOption.Quit:
                        _view.ShowGoodbye();
                        return;
                }
            }
        }

        private void PlayGame()
        {
            var variant = _view.AskVariant();
            var (bottom, top) = CreatePlayers();
            var game = new CheckersGame(variant);

            string? message = null;
            while (game.Status == GameStatus.InProgress)
            {
                _view.ShowGame(game, bottom, top, message);
                message = null;
                var current = game.Turn == Side.Bottom ? bottom : top;
                var opponent = game.Turn == Side.Bottom ? top : bottom;

                var input = _view.ReadLine();
                switch (input.ToLowerInvariant())
                {
                    case "":
                        continue;
                    case "help":
                        _view.ShowHelp();
                        continue;
                    case "moves":
                        _view.ShowLegalMoves(game);
                        _view.WaitForEnter();
                        continue;
                    case "quit":
                    case "exit":
                        if (_view.Confirm("Really abandon this game and return to the menu?"))
                        {
                            return;
                        }

                        continue;
                    case "surrender":
                    case "resign":
                        if (_view.Confirm($"{current.Name}, do you really want to surrender?"))
                        {
                            _view.ShowSurrender(current, opponent);
                            _view.WaitForEnter();
                            return;
                        }

                        continue;
                }

                if (!TryParseSquares(game, input, out var squares, out var parseError))
                {
                    message = parseError;
                }
                else if (game.TryResolveMove(squares, out var move, out var moveError))
                {
                    game.Apply(move!);
                }
                else
                {
                    message = moveError;
                }
            }

            _view.ShowResult(game, bottom, top);
            _view.WaitForEnter();
        }

        private (PlayerInfo Bottom, PlayerInfo Top) CreatePlayers()
        {
            var name1 = _view.AskName(1, null);
            var name2 = _view.AskName(2, name1);
            var color1 = _view.AskColor(name1, Array.Empty<Colors>());
            var color2 = _view.AskColor(name2, new[] { color1 });
            var side1 = _view.AskSide(name1);

            var player1 = new PlayerInfo(name1, color1, side1);
            var player2 = new PlayerInfo(name2, color2, side1 == Side.Bottom ? Side.Top : Side.Bottom);
            return side1 == Side.Bottom ? (player1, player2) : (player2, player1);
        }

        private static bool TryParseSquares(CheckersGame game, string input, out List<Square> squares, out string error)
        {
            squares = new List<Square>();
            error = string.Empty;
            foreach (var token in input.Split(MoveSeparators, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!game.TryParseSquare(token, out var square))
                {
                    error = $"'{token}' is not a valid square. Use a letter and a number, e.g. c3.";
                    return false;
                }

                squares.Add(square);
            }

            if (squares.Count < 2)
            {
                error = "Enter a start and a destination square, e.g. c3 d4. Type 'help' for more information.";
                return false;
            }

            return true;
        }
    }
}
