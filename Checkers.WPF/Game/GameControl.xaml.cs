using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Checkers.WPF.Game;

public partial class GameControl : UserControl
{
    public GameControl()
    {
        InitializeComponent();
    }

    private void Square_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: SquareViewModel square } && DataContext is GameViewModel viewModel)
        {
            viewModel.SquareClickCommand.Execute(square);
        }
    }
}
