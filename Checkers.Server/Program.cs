using Checkers.Server;
using Checkers.Tools.Online;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GameRoomManager>();
builder.Services.AddSignalR();

var app = builder.Build();
app.MapGet("/", () => "Checkers multiplayer server is running.");
app.MapHub<GameHub>(OnlineProtocol.HubPath);
app.Run();
