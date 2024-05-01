Class MainWindow
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        dateScroller.FirstDate = New DateTime(DateTime.Now.Year, 1, 1)
        dateScroller.LastDate = New DateTime(DateTime.Now.Year, 12, 31)
        dateScroller.StartDate = DateTime.Now.Date
        dateScroller.EndDate = DateTime.Now.Date.AddDays(3)
        DateScroller.AmountItems = New List(Of AmountItem)() From {
            New AmountItem() With {.[Date] = DateTime.Now.Date, .Amount = 100, .Brush = New SolidColorBrush(Colors.Red)},
            New AmountItem() With {.[Date] = DateTime.Now.Date.AddDays(1), .Amount = 75, .Brush = New SolidColorBrush(Colors.Yellow)},
            New AmountItem() With {.[Date] = DateTime.Now.Date.AddDays(2), .Amount = 50, .Brush = New SolidColorBrush(Colors.Green)}
        }
    End Sub
End Class
