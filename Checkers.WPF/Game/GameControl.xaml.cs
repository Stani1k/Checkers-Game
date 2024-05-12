using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Checkers.WPF.Game;

public partial class GameControl : UserControl
{
    private const int SquareSize = 80;
    
    public GameControl()
    {
        InitializeComponent();
    }

    private void GameControl_OnLoaded(object? sender, EventArgs e)
    {
        DrawGameArea();
    }

    private void DrawGameArea()
    {
        var doneDrawingBackground = false;  
        int nextX = 0, nextY = 0;  
        var rowCounter = 0;  
        var nextIsOdd = false;  

        while(doneDrawingBackground == false)  
        {  
            var rect = new Rectangle  
            {  
                Width = SquareSize,  
                Height = SquareSize,  
                Fill = nextIsOdd ? Brushes.White : Brushes.Black  
            };          
            GameArea.Children.Add(rect);  
            Canvas.SetTop(rect, nextY);
            Canvas.SetLeft(rect, nextX);  

            nextIsOdd = !nextIsOdd;  
            nextX += SquareSize;  
            if(nextX >= GameArea.ActualWidth)  
            {  
                nextX = 0;  
                nextY += SquareSize;  
                rowCounter++;
                nextIsOdd = (rowCounter % 2 != 0);  
            }

            if (nextY >= GameArea.ActualHeight)
            {
                doneDrawingBackground = true;
            }
        }  
    }
}