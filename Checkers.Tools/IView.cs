using System.Diagnostics;
using Checkers.Tools.Models;

namespace Checkers.Tools
{
    public interface IView
    {
        int CursorXCoordinate { get; }
        int CursorYCoordinate { get; }
        /// <summary>
        /// SetUp everything related to View like culture, window size etc.
        /// </summary>
        void SetUp();
        void ShowMainMenu();
        int ChooseLanguage();
        Variant GetSelectedGameVariant();
        string GetPlayerName(int playerID);
        string GetStartingSite(string playerName);
        Colors GetSelectedColor(string playerName, int playerNumber);
        void EqualCheckerColorNotAllowed();
        void EqualNamesNotAllowed();
        void CreateGameboard();
        void PrintRowValueNumbers(int columnNumber);
        void CreateRows(int rowNumber);
        void CreateColumns();
        void PrintChecker(Checker checker);
        void ShowSurrenderOption();
        Position? GetPosition(bool isStartCoordinate, Player player);
        void ShowCurrentPosition(char x, int y);
        void MoveChecker(Checker checkerToMove);
        void RemoveChecker(Checker checkerToRemove);
        void TransformCheckerToQueen(Position position);
        void TryMovingEnemyChecker();
        void MoveCheckerOnWhiteField();
        void TryMovingCheckerToFarAway();
        void SelectEmptyField();
        void MoveCheckerOnOccupiedField();
        void MovingNotPossible();
        void ShowPlayerTurn(string playerName, Colors checkerColor);
        void ShowPossibleJump(char x, int y);
        void ShowTwoPossibleJumps(char xRight, int yRight, char xLeft, int yLeft);
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