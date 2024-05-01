Imports System.Globalization
Imports System.Threading
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media

<TemplatePart(Name:=DateScroller.PartItemsControl, Type:=GetType(ItemsControl))>
<TemplatePart(Name:=DateScroller.PartRect, Type:=GetType(Border))>
<TemplatePart(Name:=DateScroller.PartBorderWeek, Type:=GetType(Border))>
<TemplatePart(Name:=DateScroller.PartItemsControlWeek, Type:=GetType(ItemsControl))>
<TemplatePart(Name:=DateScroller.PartScrollViewer, Type:=GetType(ScrollViewer))>
<TemplatePart(Name:=DateScroller.PartLabel, Type:=GetType(System.Windows.Controls.TextBlock))>
Public Class DateScroller
    Inherits Control

    Public Const PartItemsControl As String = "PART_ItemsControl"
    Public Const PartRect As String = "PART_Rect"
    Public Const PartBorderWeek As String = "PART_Border_Week"
    Public Const PartItemsControlWeek As String = "PART_ItemsControl_Week"
    Public Const PartScrollViewer As String = "PART_ScrollViewer"
    Public Const PartLabel As String = "PART_Label"

    Public Shared ReadOnly FirstDateProperty As DependencyProperty = DependencyProperty.Register("FirstDate", GetType(DateTime), GetType(DateScroller), New FrameworkPropertyMetadata(New DateTime(2022, 1, 1), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnFirstDateChanged))
    Public Shared ReadOnly LastDateProperty As DependencyProperty = DependencyProperty.Register("LastDate", GetType(DateTime), GetType(DateScroller), New FrameworkPropertyMetadata(New DateTime(2022, 12, 31), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnLastDateChanged))
    Public Shared ReadOnly StartDateProperty As DependencyProperty = DependencyProperty.Register("StartDate", GetType(DateTime), GetType(DateScroller), New FrameworkPropertyMetadata(New DateTime(2022, 1, 1), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnStartDateChanged))
    Public Shared ReadOnly EndDateProperty As DependencyProperty = DependencyProperty.Register("EndDate", GetType(DateTime), GetType(DateScroller), New FrameworkPropertyMetadata(New DateTime(2022, 1, 31), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnEndDateChanged))
    Public Shared ReadOnly AmountItemsProperty As DependencyProperty = DependencyProperty.Register("AmountItems", GetType(List(Of AmountItem)), GetType(DateScroller), New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnAmountItemsChanged))

    Private _itemsControl As ItemsControl
    Private _rect As Border
    Private _borderWeek As Border
    Private _itemsControlWeek As ItemsControl
    Private _scrollViewer As ScrollViewer
    Private _label As System.Windows.Controls.TextBlock

    Private _prDown As Point = Nothing
    Private _pDown As Point = Nothing
    Private _isDown As Boolean = False
    Private _isLeft As Boolean = False
    Private _isRight As Boolean = False
    Private _isWithin As Boolean = False
    Private _startLeft As Double = 0
    Protected _isWorking As Boolean = False

    Private _l As List(Of DateItem) = Nothing
    Private _lw As List(Of WeekItem) = Nothing
    Private _weekMargin As Double = 0

    Private _timer As Timer = Nothing

    Public Property ItemsControl As ItemsControl
        Get
            Return _itemsControl
        End Get
        Set(value As ItemsControl)
            _itemsControl = value
        End Set
    End Property

    Public Property Rect As Border
        Get
            Return _rect
        End Get
        Set(value As Border)
            _rect = value
        End Set
    End Property

    Public Property BorderWeek As Border
        Get
            Return _borderWeek
        End Get
        Set(value As Border)
            _borderWeek = value
        End Set
    End Property

    Public Property ScrollViewer As ScrollViewer
        Get
            Return _scrollViewer
        End Get
        Set(value As ScrollViewer)
            _scrollViewer = value
        End Set
    End Property

    Public Property ItemsControlWeek As ItemsControl
        Get
            Return _itemsControlWeek
        End Get
        Set(value As ItemsControl)
            _itemsControlWeek = value
        End Set
    End Property

    Public Property Label As System.Windows.Controls.TextBlock
        Get
            Return _label
        End Get
        Set(value As System.Windows.Controls.TextBlock)
            _label = value
        End Set
    End Property

    Public Property FirstDate() As DateTime
        Get
            Return GetValue(FirstDateProperty)
        End Get

        Set(ByVal value As DateTime)
            SetValue(FirstDateProperty, value)
        End Set
    End Property

    Public Property LastDate() As DateTime
        Get
            Return GetValue(LastDateProperty)
        End Get

        Set(ByVal value As DateTime)
            SetValue(LastDateProperty, value)
        End Set
    End Property

    Public Property StartDate() As DateTime
        Get
            Return GetValue(StartDateProperty)
        End Get

        Set(ByVal value As DateTime)
            SetValue(StartDateProperty, value)
        End Set
    End Property

    Public Property EndDate() As DateTime
        Get
            Return GetValue(EndDateProperty)
        End Get

        Set(ByVal value As DateTime)
            SetValue(EndDateProperty, value)
        End Set
    End Property

    Public Property AmountItems() As List(Of AmountItem)
        Get
            Return GetValue(AmountItemsProperty)
        End Get

        Set(ByVal value As List(Of AmountItem))
            SetValue(AmountItemsProperty, value)
        End Set
    End Property

    Shared Sub OnFirstDateChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim ds As DateScroller = d
        ds.MakeItems(ds.FirstDate, ds.LastDate)
    End Sub

    Shared Sub OnLastDateChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim ds As DateScroller = d
        ds.MakeItems(ds.FirstDate, ds.LastDate)
    End Sub

    Shared Sub OnStartDateChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim ds As DateScroller = d
        If Not ds.Rect Is Nothing AndAlso Not ds._isWorking Then
            Dim rectX As Double = e.NewValue.Subtract(ds.FirstDate).TotalDays * 50
            Dim rectWidth As Double = (ds.EndDate.Subtract(ds.StartDate).TotalDays + 1) * 50
            If rectWidth <= 0 Then
                rectWidth = 50
                ds.EndDate = e.NewValue
            End If
            ds.Rect.Margin = New Thickness(rectX, 0, 0, 0)
            ds.Rect.Width = rectWidth
            ds.setLabel()
            If rectX < ds.ScrollViewer.HorizontalOffset Or rectX > ds.ScrollViewer.HorizontalOffset + ds.ScrollViewer.ActualWidth Then
                ds.ScrollViewer.ScrollToHorizontalOffset(rectX - 50)
            End If
        End If
    End Sub

    Shared Sub OnEndDateChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim ds As DateScroller = d
        If Not ds.Rect Is Nothing AndAlso Not ds._isWorking Then
            Dim rectWidth As Double = (e.NewValue.Subtract(ds.StartDate).TotalDays + 1) * 50
            If rectWidth <= 0 Then
                rectWidth = 50
                ds.EndDate = e.NewValue
            End If
            ds.Rect.Width = rectWidth
            ds.setLabel()
        End If
    End Sub

    Shared Sub OnAmountItemsChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim ds As DateScroller = d
        ds.MakeItems(ds.FirstDate, ds.LastDate)
    End Sub

    Shared Sub New()
        DefaultStyleKeyProperty.OverrideMetadata(GetType(DateScroller), New FrameworkPropertyMetadata(GetType(DateScroller)))
    End Sub

    Public Sub New()
    End Sub

    Public Sub MakeItems(firstDate As DateTime, lastDate As DateTime)
        _l = New List(Of DateItem)()
        _lw = New List(Of WeekItem)()
        Dim d As DateTime = firstDate
        Dim lastWeek As Integer = -1
        Dim days As Integer = 0
        Dim maxAmount As Integer = 0
        If Not Me.AmountItems Is Nothing Then
            maxAmount = Me.AmountItems.Max(Function(i) i.Amount)
        End If
        While d <= lastDate
            Dim height As Integer = 0
            Dim brush As Brush = Brushes.Silver
            If Not Me.AmountItems Is Nothing Then
                Dim ai As AmountItem = Me.AmountItems.FirstOrDefault(Function(i) i.Date = d)
                If Not ai Is Nothing Then
                    brush = ai.Brush
                    height = ai.Amount / maxAmount * 40
                End If
            End If
            _l.Add(New DateItem() With {.Datum = d, .Height = height, .Brush = brush})
            Dim week As Integer = getISO8601WeekOfYear(d)
            If week <> lastWeek Then
                If days <> -1 And lastWeek <> -1 Then
                    _weekMargin = -50 * (7 - days)
                    days = -1
                End If
                _lw.Add(New WeekItem() With {.Tekst = String.Format("Week {0}, {1:MMMM} {1:yyyy}", week, d)})
                lastWeek = week
            End If
            d = d.AddDays(1)
            If days <> -1 Then days += 1
        End While

        If Not Me.ItemsControl Is Nothing Then
            Me.ItemsControl.ItemsSource = _l
        End If
        If Not Me.ItemsControlWeek Is Nothing Then
            Me.ItemsControlWeek.ItemsSource = _lw
            Me.ItemsControlWeek.Margin = New Thickness(_weekMargin, 0, 0, 0)
        End If
    End Sub

    Public Overrides Sub OnApplyTemplate()
        MyBase.OnApplyTemplate()

        Me.ItemsControl = Template.FindName(PartItemsControl, Me)
        Me.Rect = Template.FindName(PartRect, Me)
        Me.BorderWeek = Template.FindName(PartBorderWeek, Me)
        Me.ItemsControlWeek = Template.FindName(PartItemsControlWeek, Me)
        Me.ScrollViewer = Template.FindName(PartScrollViewer, Me)
        Me.Label = Template.FindName(PartLabel, Me)

        AddHandler Me.ItemsControl.MouseMove, AddressOf ItemsControl_MouseMove
        AddHandler Me.ItemsControl.MouseDown, AddressOf ItemsControl_MouseDown
        AddHandler Me.ItemsControl.MouseUp, AddressOf ItemsControl_MouseUp

        If Not _l Is Nothing Then
            Me.ItemsControl.ItemsSource = _l
            Me.ItemsControlWeek.ItemsSource = _lw
            Me.ItemsControlWeek.Margin = New Thickness(_weekMargin, 0, 0, 0)
        Else
            MakeItems(Me.FirstDate, Me.LastDate)
        End If

        Dim rectX As Double = Me.StartDate.Subtract(Me.FirstDate).TotalDays * 50
        Dim rectWidth As Double = (Me.EndDate.Subtract(Me.StartDate).TotalDays + 1) * 50
        Me.Rect.Margin = New Thickness(rectX, 0, 0, 0)
        Me.Rect.Width = rectWidth

        If rectX < Me.ScrollViewer.HorizontalOffset Or rectX > Me.ScrollViewer.HorizontalOffset + Me.ScrollViewer.ActualWidth Then
            Me.ScrollViewer.ScrollToHorizontalOffset(rectX - 50)
        End If

        setLabel()
    End Sub

    Protected Sub ItemsControl_MouseMove(sender As Object, e As MouseEventArgs)
        Dim p As Point = e.GetPosition(Me.ItemsControl)
        Dim pr As Point = e.GetPosition(Me.Rect)
        Dim ps As Point = e.GetPosition(Me.ScrollViewer)
        Dim rectX As Double = 0
        Dim rectWidth As Double = 0

        If _isDown Then
            If ps.X < 0 Then
                If _timer Is Nothing Then
                    _timer = New Timer(
                    Sub()
                        Application.Current.Dispatcher.Invoke(
                            Sub()
                                Me.ScrollViewer.ScrollToHorizontalOffset(Me.ScrollViewer.HorizontalOffset - 50)
                            End Sub)
                    End Sub, Nothing, 0, 125)
                End If
            ElseIf ps.X > Me.ScrollViewer.ActualWidth Then
                If _timer Is Nothing Then
                    _timer = New Timer(
                    Sub()
                        Application.Current.Dispatcher.Invoke(
                            Sub()
                                Me.ScrollViewer.ScrollToHorizontalOffset(Me.ScrollViewer.HorizontalOffset + 50)
                            End Sub)
                    End Sub, Nothing, 0, 125)
                End If
            Else
                If Not _timer Is Nothing Then
                    _timer.Change(Timeout.Infinite, Timeout.Infinite)
                    _timer = Nothing
                End If
            End If

            If _isLeft Then
                rectX = Math.Round(p.X / 50, 0) * 50
                If rectX < 0 Then rectX = 0
                rectWidth = Me.Rect.Width - (rectX - Me.Rect.Margin.Left)
                If rectWidth < 50 Then
                    Me.Rect.Margin = New Thickness(Me.Rect.Margin.Left + Me.Rect.Width - 50, 0, 0, 0)
                    Me.Rect.Width = 50
                Else
                    Me.Rect.Width = rectWidth
                    Me.Rect.Margin = New Thickness(rectX, 0, 0, 0)
                End If
            ElseIf _isRight Then
                rectWidth = Math.Round(pr.X / 50, 0) * 50
                If rectWidth < 50 Then rectWidth = 50
                If Me.Rect.Margin.Left + rectWidth > _l.Count * 50 Then rectWidth = _l.Count * 50 - Me.Rect.Margin.Left
                Me.Rect.Width = rectWidth
            ElseIf _isWithin Then
                rectX = Math.Round((_startLeft + (p.X - _pDown.X)) / 50, 0) * 50
                If rectX < 0 Then rectX = 0
                If rectX + Me.Rect.Width > _l.Count * 50 Then rectX = _l.Count * 50 - Me.Rect.Width
                Me.Rect.Margin = New Thickness(rectX, 0, 0, 0)
            Else
                rectX = Math.Floor(_pDown.X / 50) * 50
                If rectX < 0 Then rectX = 0

                rectWidth = (Math.Ceiling(p.X / 50) * 50) - (Math.Floor(_pDown.X / 50) * 50)
                If rectWidth <= 0 Then
                    rectWidth -= 50
                    rectWidth = -rectWidth
                    rectX = rectX - rectWidth
                    rectWidth += 50
                End If
                If Me.Rect.Margin.Left + rectWidth > _l.Count * 50 Then rectWidth = _l.Count * 50 - Me.Rect.Margin.Left

                Me.Rect.Margin = New Thickness(rectX, 0, 0, 0)
                Me.Rect.Width = rectWidth
            End If

            setLabel()
        Else
            If (pr.X > -5 AndAlso pr.X < 5) OrElse (pr.X > Me.Rect.Width - 5 AndAlso pr.X < Me.Rect.Width + 5) Then
                Me.ItemsControl.Cursor = Cursors.SizeWE
            Else
                Me.ItemsControl.Cursor = Cursors.Hand
            End If
        End If
    End Sub

    Protected Friend Sub setLabel()
        If Not Me.Label Is Nothing Then
            Dim startDate As DateTime = Me.FirstDate.AddDays(Me.Rect.Margin.Left / 50)
            Dim endDate As DateTime = startDate.AddDays((Me.Rect.Width / 50) - 1)
            If startDate = endDate Then
                Me.Label.Text = String.Format("{0:d}", startDate)
            Else
                Me.Label.Text = String.Format("{0:d}-{1:d}", startDate, endDate)
            End If
        End If
    End Sub

    Protected Sub ItemsControl_MouseDown(sender As Object, e As MouseButtonEventArgs)
        If e.LeftButton = MouseButtonState.Pressed Then
            _pDown = e.GetPosition(Me.ItemsControl)
            _prDown = e.GetPosition(Me.Rect)
            _isDown = True

            Dim pr As Point = e.GetPosition(Me.Rect)
            If (pr.X > -5 AndAlso pr.X < 5) Then
                _isLeft = True
            ElseIf (pr.X > Me.Rect.Width - 5 AndAlso pr.X < Me.Rect.Width + 5) Then
                _isRight = True
            ElseIf pr.X > 0 AndAlso pr.X < Me.Rect.Width Then
                _isWithin = True
                _startLeft = Me.Rect.Margin.Left
            End If

            Me.ItemsControl.CaptureMouse()
        End If
    End Sub

    Protected Sub ItemsControl_MouseUp(sender As Object, e As MouseButtonEventArgs)
        If e.LeftButton = MouseButtonState.Released AndAlso _isDown Then
            _isDown = False
            _isLeft = False
            _isRight = False
            _isWithin = False
            Me.ItemsControl.ReleaseMouseCapture()

            _isWorking = True
            Me.StartDate = Me.FirstDate.AddDays(Me.Rect.Margin.Left / 50)
            Me.EndDate = Me.StartDate.AddDays((Me.Rect.Width / 50) - 1)
            _isWorking = False

            If Not _timer Is Nothing Then
                _timer.Change(Timeout.Infinite, Timeout.Infinite)
                _timer = Nothing
            End If
        End If
    End Sub

    ' This presumes that weeks start with Monday.
    ' Week 1 Is the 1st week of the year with a Thursday in it.
    Private Function getISO8601WeekOfYear(dt As DateTime) As Integer
        ' Seriously cheat.  If its Monday, Tuesday Or Wednesday, then it'll 
        ' be the same week# as whatever Thursday, Friday Or Saturday are,
        ' And we always get those right
        Dim day As DayOfWeek = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dt)
        If (day >= DayOfWeek.Monday AndAlso day <= DayOfWeek.Wednesday) Then
            dt = dt.AddDays(3)
        End If

        ' Return the week of our adjusted day
        Return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
    End Function
End Class
