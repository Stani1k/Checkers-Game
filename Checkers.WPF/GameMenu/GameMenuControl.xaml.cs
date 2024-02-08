using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Checkers.WPF.GameMenu;

public partial class GameMenuControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(GameMenuViewModel), typeof(GameMenuControl), new PropertyMetadata(default(GameMenuViewModel)));

    public GameMenuViewModel ViewModel
    {
        get => (GameMenuViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
    
    public GameMenuControl()
    {
        InitializeComponent();
        CreateCheckerBoard();
    }
    
    private void CreateCheckerBoard() 
    {  
        Rectangle chessBoard = new Rectangle
        {
            Width = 300,
            Height = 300
        };
        
        DrawingBrush blackBrush = new DrawingBrush();
        GeometryDrawing backgroundSquare = new GeometryDrawing(Brushes.White, null, new RectangleGeometry(new Rect(0, 0, 400, 400)));  
        // Create a GeometryGroup that will be added to Geometry
        GeometryGroup gGroup = new GeometryGroup();  
        gGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 200, 200)));
        gGroup.Children.Add(new RectangleGeometry(new Rect(200, 200, 200, 200)));
        
        GeometryDrawing checkers = new GeometryDrawing(new SolidColorBrush(Colors.Black), null, gGroup);  
        DrawingGroup checkersDrawingGroup = new DrawingGroup();  
        checkersDrawingGroup.Children.Add(backgroundSquare);  
        checkersDrawingGroup.Children.Add(checkers);
        blackBrush.Drawing = checkersDrawingGroup;  
        // Set Viewport and TimeMode  
        blackBrush.Viewport = new Rect(0, 0, 0.25, 0.25);  
        blackBrush.TileMode = TileMode.Tile;  
        // Fill rectangle with a DrawingBrush  
        chessBoard.Fill = blackBrush;
        LayoutRoot.Children.Add(chessBoard);  
    } 
}