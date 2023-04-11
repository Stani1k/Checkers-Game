namespace Checkers.Console
{
    class Program
    {
        // Einstiegspunkt des Programms
        private static void Main(string[] args)
        {
            var controller = new Controller(new View());
            controller.StarteAnwendung();

            System.Console.ReadKey();
        }
    }
}
