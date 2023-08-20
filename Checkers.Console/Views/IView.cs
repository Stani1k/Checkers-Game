using System;
using System.Collections.Generic;
using System.Diagnostics;
using Checkers.Console.Models;

namespace Checkers.Console.Views
{
    public interface IView
    {
        int CursorXCoordinate { get; }
        int CursorYCoordinate { get; }
        /// <summary>
        /// SetUp everything related to View like culture, window size etc.
        /// </summary>
        void SetUp();
        void GreetPlayer();
        int ChooseLanguage();
        Variant GetSelectedGameVariant();
        string GetPlayerName(int playerID);
        string GetStartingSite(string playerName);
        ConsoleColor GetSelectedColor(string spielername, int spielerNummer);
        void EqualCheckerColorNotAllowed();
        void EqualNamesNotAllowed();
        void CreateGameboard();
        void CreateColumns(int columnNumber);
        void CreateRows(int rowNumber);
        void CreateXAxis();
        void PrintChecker(Checker checker);
        void ShowSurrenderOption();
        Position GetPosition(bool isStartingCoordinate, Player player);
        void ShowCurrentPosition(char x, int y);
        void MoveChecker(Checker checker);
        void RemoveChecker(Checker checkerToRemove);
        void TransformCheckerToQueen(Position position);
        void TryMovingEnemyChecker();
        void MoveCheckerOnWhiteField();
        void TryMovingCheckerToFarAway();
        void SelectEmptyField();
        void MoveCheckerOnOccupiedField();
        void MovingNotPossible();
        void ShowPlayerTurn(string playerName, ConsoleColor color);
        void ShowPossibleJump(char x, int y);
        void ShowTwoPossibleJumps(char xKoordinateRechts, int yKoordinateRechts, char xKoordinateLinks, int yKoordinateLinks);
        void ShowPossibleJumpCount(List<Checker> checkers);
        void CheckerHasLessJumps(Checker spielstein);
        void ForceJump(List<Position> availableJumps);
        void BeatChecker();
        void ShowCurrentCheckerAmount(string playerName1, int checkerAmount1, string playerName2, int checkerAmount2);
        void ShowPassedTime(Stopwatch stopwatch);
        void ClearText(int x, int y);
        void SetGameColors();
        void PlayerHasSurrendered(Player loser, Player winner);
        void ShowWinner(string winnerName);
        void QuitGame();
    }
}