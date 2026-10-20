using System;
using Checkers.Tools.Logging;
using Serilog;

namespace Checkers.Console
{
    internal static class Program
    {
        /// <summary>
        /// Application Entry point
        /// </summary>
        private static void Main()
        {
            LoggingSetup.Initialize("Checkers.Console");
            Log.Information("Console application started");
            try
            {
                new GameController().RunApplication();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Console application terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.Information("Console application exiting");
                LoggingSetup.Shutdown();
            }
        }
    }
}
