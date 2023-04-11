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
            var controller = new Controller(new View());
            controller.StarteAnwendung();

            System.Console.ReadKey();
        }
    }
}
