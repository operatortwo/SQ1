Partial Public Class TrackPanelStackHeader

    Public MeasureAdornerLayer As AdornerLayer
    'Public CanvasMousePositionX As Double

    Public PlayPositionAdorner1 As PlayPositionAdorner

    Private Sub InitializePlayPositionAdorner()
        'AdornerArray.Contains = False

        'MeasureAdornerLayer = AdornerLayer.GetAdornerLayer(MeasureCanvas)
        'If MeasureAdornerLayer IsNot Nothing Then
        '    Dim AdornerArray() As Adorner = MeasureAdornerLayer.GetAdorners(MeasureCanvas)
        '    If AdornerArray Is Nothing Then
        '        MeasureAdornerLayer = AdornerLayer.GetAdornerLayer(MeasureCanvas)
        '        PlayPositionAdorner1 = New PlayPositionAdorner(MeasureCanvas)
        '        MeasureAdornerLayer.Add(PlayPositionAdorner1)
        '    End If
        'End If

        MeasureAdornerLayer = AdornerLayer.GetAdornerLayer(MeasureCanvas)
        If MeasureAdornerLayer IsNot Nothing Then
            If PlayPositionAdorner1 Is Nothing Then         ' avoid multiple add's (when returning from other tab)
                PlayPositionAdorner1 = New PlayPositionAdorner(MeasureCanvas)
                MeasureAdornerLayer.Add(PlayPositionAdorner1)
            End If
        End If

    End Sub
End Class


Public Class PlayPositionAdorner
    Inherits Adorner
    Sub New(ByVal adornedElement As UIElement)
        MyBase.New(adornedElement)
        IsHitTestVisible = False             ' important to prevent flicker!
    End Sub

    Public DrawLocator As Boolean
    Public MousePosition As New Point
    Public SeqPanel As SequencerPanel

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

        Dim pen As New Pen(Brushes.Red, 1)
        'pen.DashStyle = DashStyles.Dot

        Dim DashedPen As New Pen(Brushes.Black, 1)
        DashedPen.DashStyle = DashStyles.Dot

        Dim rect As New Rect
        rect.Location = New Point(0, 0)
        rect.Size = AdornedElement.RenderSize

        'If AdornedElement.IsMouseOver Then
        '    Dim pt As Point
        '    pt = Mouse.GetPosition(AdornedElement)
        '    drawingContext.DrawLine(pen, New Point(pt.X, 0), New Point(pt.X, adornedElementRect.Height))
        'End If

        'drawingContext.DrawLine(pen, New Point(20, 0), New Point(20, adornedElementRect.Height))
        'drawingContext.DrawLine(pen, New Point(20, 0), New Point(20, 25))

        ' avoid error in Design-Time
        'If Sequencer IsNot Nothing Then
        '    If SequencerPanel1 IsNot Nothing Then
        '        Dim posx As Double
        '        posx = TicksToPixel(Sequencer.SequencerTime, SequencerPanel1.TracksHeader.ScaleX)
        '        drawingContext.DrawLine(pen, New Point(posx, 0), New Point(posx, 18))
        '    End If
        'End If

        If Sequencer IsNot Nothing Then
            If SeqPanel IsNot Nothing Then
                Dim posx As Double
                posx = TicksToPixel(Sequencer.SequencerTime, SeqPanel.TracksHeader.ScaleX)
                drawingContext.DrawLine(pen, New Point(posx, 0), New Point(posx, 18))
            End If
        End If

    End Sub

    Private Function TicksToPixel(Time As Double, ScaleX As Double) As Double
        Return Time * TicksToPixelFactor * ScaleX
    End Function


End Class
