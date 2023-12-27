using Checkers.Console.Models;
using Checkers.Console.Views;

namespace Checkers.Console
{
    internal static class Program
    {
        /// <summary>
        /// Application Entry point
        /// </summary>
        private static void Main()
        {
            var controller = new GameController();
            controller.RunApplication();

            System.Console.ReadKey();
        }
    }
}
