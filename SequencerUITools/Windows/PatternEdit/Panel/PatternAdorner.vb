Partial Public Class PatternPanel

    Public MeasureAdornerLayer As AdornerLayer
    Public PlayPositionAdorner1 As MyAdorner

    Private Sub InitializePatternAdorner()

        'PlayPosition Adorner
        MeasureAdornerLayer = AdornerLayer.GetAdornerLayer(MeasurePanel)
        If MeasureAdornerLayer IsNot Nothing Then
            If PlayPositionAdorner1 Is Nothing Then         ' avoid multiple add's (when returning from other tab)
                PlayPositionAdorner1 = New MyAdorner(MeasurePanel)
                MeasureAdornerLayer.Add(PlayPositionAdorner1)
            End If
        End If

    End Sub

End Class
Public Class MyAdorner
    Inherits Adorner
    Sub New(ByVal adornedElement As UIElement)
        MyBase.New(adornedElement)
        IsHitTestVisible = False             ' important to prevent flicker!
    End Sub

    Public PatternTime As UInteger
    Public ScaleX As Single = 1.0
    'Public SeqPanel As SequencerPanel

    Protected Overrides Sub OnRender(ByVal drawingContext As System.Windows.Media.DrawingContext)
        MyBase.OnRender(drawingContext)
        'Dim adornedElementRect As New Rect(AdornedElement.DesiredSize)
        Dim adornedElementRect As New Rect(AdornedElement.RenderSize)
        Dim renderBrush As New SolidColorBrush(Colors.Green)
        renderBrush.Opacity = 0.2
        Dim renderPen As New Pen(New SolidColorBrush(Colors.Navy), 1.5)
        Dim renderRadius As Double
        renderRadius = 5.0

        Dim pen As New Pen(Brushes.Red, 1)

        Dim posx As Double = 100

        posx = TicksToPixel(PatternTime, ScaleX)
        posx += 1                                                   ' for better visibility at Pos.0
        drawingContext.DrawLine(pen, New Point(posx, 0), New Point(posx, 18))

    End Sub

    Private Function TicksToPixel(Time As Double, ScaleX As Double) As Double
        Return Time * PatternPanel.TicksToPixelFactor * ScaleX

    End Function

End Class