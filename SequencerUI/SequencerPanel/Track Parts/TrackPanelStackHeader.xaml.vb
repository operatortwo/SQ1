
Imports System.ComponentModel
Imports System.Windows.Media.Animation

Public Class TrackPanelStackHeader
    Private SeqPanel As SequencerPanel
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        InitializePlayPositionAdorner()
        InitializeLoopCursorAdorner()
        Dim obj As Object = FindLogicalParent(Me, GetType(SequencerPanel))
        SeqPanel = TryCast(obj, SequencerPanel)
        PlayPositionAdorner1.SeqPanel = SeqPanel
        LoopCursorAdorner1.SeqPanel = SeqPanel
    End Sub

    Public Event RowHeaderSizeChanged(sender As Object, e As SizeChangedEventArgs)

    Private Sub GridLeft_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles RowHeader.SizeChanged
        RaiseEvent RowHeaderSizeChanged(sender, e)
    End Sub

#Region "Measure Canvas"

    Private Sub MeasureCanvas_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles MeasureCanvas.SizeChanged
        DrawMeasure()
    End Sub

    Private GridLineBarBrush As Brush = Brushes.Black
    Private GridLineBeatBrush As Brush = Brushes.Gray
    Private Const GridY1 = 5                                   ' y-start of measure lines (15)
    Private Const GridLineBarHeight = 20
    Private Const GridLineBeatHeight = 10

    Private _ScaleX As Double = 1.0
    Public Property ScaleX As Double
        Get
            Return _ScaleX
        End Get
        Set(value As Double)
            _ScaleX = value
        End Set
    End Property

    Private Sub DrawMeasure()
        MeasureCanvas.Children.Clear()

        If Sequencer IsNot Nothing Then
            'Dim comp As SequencerBase.Composition = Sequencer.Composition
            Dim comp As SequencerBase.Composition = SeqPanel.Composition
            Dim tpq As Short = comp.TicksPerQuarterNote
            Dim beatcount As Integer = CInt(comp.Length / tpq)

            Dim time As UInteger
            Dim barcount As Byte

            For i = 0 To beatcount
                Dim line As New Line
                If barcount = 0 Then
                    line.Stroke = GridLineBarBrush
                    line.Y2 = GridY1 + GridLineBarHeight

                    If i < beatcount Then           ' don't draw last Beat-Number
                        If SequencerBase.MBT_0_based = True Then
                            DrawText(CStr(i), TimeToPx(time) + Math.Max(10 * ScaleX, 4), GridY1 + GridLineBeatHeight)
                        Else
                            DrawText(CStr(i + 1), TimeToPx(time) + Math.Max(10 * ScaleX, 4), GridY1 + GridLineBeatHeight)
                        End If
                    End If
                Else
                    line.Stroke = GridLineBeatBrush
                    'line.StrokeThickness = 0.5
                    line.Y2 = GridY1 + GridLineBeatHeight
                End If

                line.X1 = time * TicksToPixelFactor * ScaleX
                line.X2 = line.X1
                line.Y1 = GridY1

                line.IsHitTestVisible = False
                MeasureCanvas.Children.Add(line)
                time = CUInt(time + tpq)
                barcount = CByte(barcount + 1)
                If barcount >= 4 Then barcount = 0
            Next
        End If
    End Sub

    Private Sub DrawText(text As String, left As Double, top As Double)
        Dim tb As New TextBlock
        tb.Text = text
        Canvas.SetLeft(tb, left)
        Canvas.SetTop(tb, top)
        tb.IsHitTestVisible = False
        MeasureCanvas.Children.Add(tb)
    End Sub

    ''' <summary>
    ''' Converts SequencerTime units to Pixel units for Drawing on X-Axis. (Time * TicksToPixelFactor * ScaleX)
    ''' </summary>
    ''' <param name="Time">SequencerTicks</param>
    ''' <returns></returns>
    Private Function TimeToPx(Time As Double) As Double
        Return Time * TicksToPixelFactor * ScaleX
    End Function

    Private Sub MeasureCanvas_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles MeasureCanvas.MouseLeftButtonDown
        'If Sequencer.IsRunning = False Then

        ' calc position
        Dim pt As Point = Mouse.GetPosition(MeasureCanvas)

        'beat 4 =120 @Scale=1
        Dim newtime As Double
        newtime = pt.X * PixelToTicksFactor / ScaleX
        Sequencer.Set_SequencerTime(newtime)
        'SequencerPanel1.ScrollIntoView(newtime)
        SeqPanel.ScrollIntoView(newtime)

        'End If
    End Sub

#End Region

#Region "Loop Canvas"

#Region "Adorner"
    Public LoopCanvasAdornerLayer As AdornerLayer
    Public LoopCursorAdorner1 As LoopCursorAdorner

    Private Sub InitializeLoopCursorAdorner()
        LoopCanvasAdornerLayer = AdornerLayer.GetAdornerLayer(LoopCanvas)
        If LoopCanvasAdornerLayer IsNot Nothing Then
            If LoopCursorAdorner1 Is Nothing Then         ' avoid multiple add's (when returning from other tab)
                LoopCursorAdorner1 = New LoopCursorAdorner(LoopCanvas)
                LoopCanvasAdornerLayer.Add(LoopCursorAdorner1)
            End If
        End If
    End Sub

    Public Class LoopCursorAdorner
        Inherits Adorner
        Sub New(ByVal adornedElement As UIElement)
            MyBase.New(adornedElement)
            IsHitTestVisible = False             ' important to prevent flicker!
        End Sub

        Public DrawLocator As Boolean
        Public MousePosition As New Point
        Public SeqPanel As SequencerPanel

        'Private LoopStartGeometry As Geometry = Geometry.Parse("M 250,40 L200,20 L200,60 Z")
        'Private LoopEndGeometry As Geometry = Geometry.Parse("M 250,40 L200,20 L200,60 Z")

        Protected Overrides Sub OnRender(ByVal drawingContext As System.Windows.Media.DrawingContext)
            MyBase.OnRender(drawingContext)
            'Dim adornedElementRect As New Rect(AdornedElement.DesiredSize)
            Dim adornedElementRect As New Rect(AdornedElement.RenderSize)
            Dim renderBrush As New SolidColorBrush(Colors.Green)
            renderBrush.Opacity = 0.2
            Dim renderPen As New Pen(New SolidColorBrush(Colors.Navy), 1.5)
            Dim renderRadius As Double
            renderRadius = 5.0

            'Draw a circle at each corner.
            'drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius)
            'drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius)
            'drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius)
            'drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius)

            Dim pen As New Pen(Brushes.Blue, 1)
            'pen.DashStyle = DashStyles.Dot

            Dim DashedPen As New Pen(Brushes.Black, 1)
            DashedPen.DashStyle = DashStyles.Dot

            'drawingContext.DrawRectangle(Nothing, pen, adornedElementRect)

            'Draw Text

            Dim rect As New Rect
            rect.Location = New Point(0, 0)
            rect.Size = AdornedElement.RenderSize

            '---- Draw current Loop Select Cursor

            If AdornedElement.IsMouseOver Then
                Dim pt As Point
                pt = Mouse.GetPosition(AdornedElement)
                'pt.X = RoundToBeat(pt.X)
                pt.X = RoundToBeat(pt.X, SeqPanel.TracksHeader.ScaleX)

                'drawingContext.DrawLine(pen, New Point(MousePosition.X, 0), New Point(MousePosition.X, adornedElementRect.Height))
                drawingContext.DrawLine(pen, New Point(pt.X, 0), New Point(pt.X, adornedElementRect.Height))
            End If

            '--- Draw Loop Start Mark

            '--- Draw Loop End Mark

            '--- Draw Loop Line

            'Dim sData As String = "M 250,40 L200,20 L200,60 Z"
            'drawingContext.DrawGeometry(renderBrush, pen, Geometry.Parse(sData))

            'drawingContext.DrawGeometry(renderBrush, pen, LoopStartGeometry)

            '--- or define DependecyProperties in LoopCanvas: LoopStart and LoopEnd (in Ticks)
            '--- 


        End Sub

        'Private Function RoundToBeat(MousePosition As Double) As Double
        '    Dim TickPosition As Double = MousePosition * PixelToTicksFactor / SequencerPanel1.TracksHeader.ScaleX
        '    Dim RoundedTickPosition As Double = RoundToStep(TickPosition, SequencerBase.Sequencer.TPQ)
        '    Dim RoundedPosition As Double = RoundedTickPosition * TicksToPixelFactor * SequencerPanel1.TracksHeader.ScaleX

        '    Return RoundedPosition
        'End Function

    End Class

#End Region

    Private Sub LoopCanvas_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles LoopCanvas.SizeChanged
        DrawLoopCanvas()
    End Sub

    Private Sub LoopCanvas_MouseMove(sender As Object, e As MouseEventArgs) Handles LoopCanvas.MouseMove
        Dim pt As Point = e.GetPosition(Me.LoopCanvas)
        'LoopCursorAdorner1.MousePosition = pt
        'LoopCanvasAdornerLayer.Update()
    End Sub

    Private Sub LoopCanvas_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles LoopCanvas.MouseLeftButtonDown
        Dim pt As Point = e.GetPosition(Me.LoopCanvas)
        'Sequencer.Composition.LoopStart = CUInt(pt.X * PixelToTicksFactor / SequencerPanel1.TracksHeader.ScaleX)
        'Dim TickPosition As Double = pt.X * PixelToTicksFactor / SequencerPanel1.TracksHeader.ScaleX
        Dim TickPosition As Double = pt.X * PixelToTicksFactor / SeqPanel.TracksHeader.ScaleX
        Dim RoundedTickPosition As Double = RoundToStep(TickPosition, SequencerBase.Sequencer.TPQ)
        'Sequencer.Composition.LoopStart = CUInt(RoundedTickPosition)
        SeqPanel.Composition.LoopStart = CUInt(RoundedTickPosition)
        DrawLoopCanvas()
    End Sub

    Private Sub LoopCanvas_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs) Handles LoopCanvas.MouseRightButtonDown
        Dim pt As Point = e.GetPosition(Me.LoopCanvas)

        'Dim TickPosition As Double = pt.X * PixelToTicksFactor / SequencerPanel1.TracksHeader.ScaleX
        Dim TickPosition As Double = pt.X * PixelToTicksFactor / SeqPanel.TracksHeader.ScaleX
        Dim RoundedTickPosition As Double = RoundToStep(TickPosition, SequencerBase.Sequencer.TPQ)

        'Sequencer.Composition.LoopEnd = CUInt(RoundedTickPosition)
        SeqPanel.Composition.LoopEnd = CUInt(RoundedTickPosition)
        DrawLoopCanvas()
    End Sub

    Private LoopStartGeometry As Geometry = Geometry.Parse("M 250,0 L200,20 L200,60 Z")

    Private Sub DrawLoopCanvas()

        If Sequencer IsNot Nothing Then
            'Dim comp As SequencerBase.Composition = Sequencer.Composition
            Dim comp As SequencerBase.Composition = SeqPanel.Composition

            Dim LoopStartPosition As Double = comp.LoopStart * TicksToPixelFactor * ScaleX
                Dim LoopEndPosition As Double = comp.LoopEnd * TicksToPixelFactor * ScaleX

                LoopCanvas.Children.Clear()

                Dim LineBrush = New SolidColorBrush(Color.FromArgb(&HFF, &H6A, &H93, &HF2))
                Dim MarkBrush = New SolidColorBrush(Color.FromArgb(&HFF, &H6A, &H93, &HF2))

                Dim LineHeight As Integer = 3

                Dim MarkWidth As Integer = 15
                Dim MarkHeight As Integer = 10

            '--- Line ---

            If LoopStartPosition < LoopEndPosition Then

                Dim pa1 As New Path
                pa1.Fill = LineBrush

                Dim rect As New Rect()
                rect.X = LoopStartPosition
                rect.Y = 0
                rect.Width = LoopEndPosition - LoopStartPosition
                rect.Height = LineHeight

                Dim rg As New RectangleGeometry(rect)
                rg.Freeze()

                pa1.Data = rg
                LoopCanvas.Children.Add(pa1)

            End If

            '--- Mark left ---

            Dim pa2 As New Path
                pa2.Fill = MarkBrush

                Dim geometry2 As New StreamGeometry()

                Using ctx As StreamGeometryContext = geometry2.Open()

                    ctx.BeginFigure(New Point(LoopStartPosition, 0), True, True) ' is closed  -  is filled 
                    ctx.LineTo(New Point(LoopStartPosition + MarkWidth, 0), True, True)
                    ctx.LineTo(New Point(LoopStartPosition, MarkHeight), True, True)

                End Using

                ' Freeze the geometry (make it unmodifiable)
                ' for additional performance benefits.
                geometry2.Freeze()


                pa2.Data = geometry2

                LoopCanvas.Children.Add(pa2)


                '--- Mark right ---

                Dim pa3 As New Path
                pa3.Fill = MarkBrush

                Dim geometry3 As New StreamGeometry()

                Using ctx As StreamGeometryContext = geometry3.Open()

                    ctx.BeginFigure(New Point(LoopEndPosition, 0), True, True) ' is closed  -  is filled 
                    ctx.LineTo(New Point(LoopEndPosition - MarkWidth, 0), True, True)
                    ctx.LineTo(New Point(LoopEndPosition, MarkHeight), True, True)

                End Using

                ' Freeze the geometry (make it unmodifiable)
                ' for additional performance benefits.
                geometry3.Freeze()


                pa3.Data = geometry3

                LoopCanvas.Children.Add(pa3)

            End If


    End Sub



#End Region
End Class
