# What is Checkers
Checkers is a popular strategy board game for two players. It involves diagonal moves of uniform game pieces and mandatory captures by jumping over opponent pieces.  
In this repo you find a Console and WPF Application which alllow you to play the game.
If you want to learn the game rules have a look at Wikipedia: https://en.wikipedia.org/wiki/Checkers

# How to build
Checkers.WPF is targeting net6.0-windows. Because of net6.0-windows you must have the .NET 6.0 SDK installed on your machine. Visual Studio 2022 Community and up is required for working with the solution.

# Online multiplayer (WPF)
Two players on different computers can play via `Checkers.Server`, a small ASP.NET Core SignalR relay. The server validates every move and forwards it to the opponent.

1. Start the server on a machine both players can reach: `dotnet run --project Checkers.Server --urls http://0.0.0.0:5000` (open the port in the firewall / host it on a public server; use HTTPS behind a reverse proxy for internet play).
2. In the WPF app choose **Play Online**, enter the server address (e.g. `http://my-server:5000`) and your name.
3. Player 1 clicks **Create Game** and shares the 5-character code (plays the light pieces and moves first). Player 2 enters the code and clicks **Join Game**.

Leaving a game or losing the connection forfeits it. Games live in server memory only; there is no reconnect.
