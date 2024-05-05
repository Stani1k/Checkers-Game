using System;
using System.Windows.Controls;

namespace Checkers.WPF.Game;

public partial class GameControl : UserControl
{
    private const int GameSquareSize = 20;
    
    public GameControl()
    {
        InitializeComponent();
    }

    private void GameControl_OnInitialized(object? sender, EventArgs e)
    {
        DrawGameArea();
    }

    private void DrawGameArea()
    {
        throw new NotImplementedException();
    }
}