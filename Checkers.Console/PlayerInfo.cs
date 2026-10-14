using System;
using Checkers.Tools.Engine;
using Checkers.Tools.Models;

namespace Checkers.Console
{
    public sealed record PlayerInfo(string Name, Colors Color, Side Side);

    /// <summary>Thrown when standard input ends (e.g. Ctrl+Z / Ctrl+D or redirected input is exhausted).</summary>
    public sealed class EndOfInputException : Exception
    {
    }
}
