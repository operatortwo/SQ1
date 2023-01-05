Imports SequencerBase

Public Class TrackPanelStackFooter

    Friend SeqPanel As SequencerPanel

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        'Dim obj As Object = FindLogicalParent(Me, GetType(SequencerPanel))
        'SeqPanel = TryCast(obj, SequencerPanel)
    End Sub

    Public Sub UpdateScaleX()
        If sldScaleX IsNot Nothing Then
            UpdateTrackCanvasScaleX(sldScaleX.Value)
            UpdateMasterScrollValues()
        End If
    End Sub


    Private Sub sldScaleX_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If sldScaleX IsNot Nothing Then
            UpdateTrackCanvasScaleX(sldScaleX.Value)
            UpdateMasterScrollValues()
        End If
    End Sub

    Private Sub UpdateTrackCanvasScaleX(newValue As Double)
        'If SequencerPanel1 IsNot Nothing Then
        '    For Each panel As TrackPanel In SequencerPanel1.TrackPanelStack.Children
        '        For Each element As TrackElements In panel.TrackElementsStack.Children
        '            element.TrackCanvas.ScaleX = newValue
        '        Next
        '    Next
        '    SequencerPanel1.TracksHeader.ScaleX = newValue
        'End If



        If SeqPanel IsNot Nothing Then
            For Each panel As TrackPanel In SeqPanel.TrackPanelStack.Children
                For Each element As TrackElement In panel.TrackElementStack.Children
                    element.TrackCanvas.ScaleX = newValue
                Next
            Next
            SeqPanel.TracksHeader.ScaleX = newValue
        End If
    End Sub

    Private Sub TrackCanvasMasterScroll_Scroll(sender As Object, e As Primitives.ScrollEventArgs) Handles TrackCanvasMasterScroll.Scroll
        'If SequencerPanel1 IsNot Nothing Then
        '    For Each panel As TrackPanel In SequencerPanel1.TrackPanelStack.Children
        '        For Each element As TrackElements In panel.TrackElementsStack.Children
        '            element.TrackCanvasScroll.ScrollToHorizontalOffset(e.NewValue)
        '        Next
        '    Next

        '    SequencerPanel1.TracksHeader.MeasureCanvasScroll.ScrollToHorizontalOffset(e.NewValue)
        'End If

        If SeqPanel IsNot Nothing Then
            For Each panel As TrackPanel In SeqPanel.TrackPanelStack.Children
                For Each element As TrackElement In panel.TrackElementStack.Children
                    element.TrackCanvasScroll.ScrollToHorizontalOffset(e.NewValue)
                Next
            Next

            SeqPanel.TracksHeader.MeasureCanvasScroll.ScrollToHorizontalOffset(e.NewValue)
        End If
    End Sub

    ''' <summary>    
    ''' If the given time (position) is within the visible part then return without changes. Else scroll
    ''' </summary>
    ''' <param name="time"></param>
    Public Sub ScrollIntoView(time As Double)
        Dim scale As Double = sldScaleX.Value

        ' check is visible

        ' Between ScrollValue + ScrollValue + ViewPort

        '--- scaled with timeToPx + ScaleX
        'TrackCanvasMasterScroll.Value
        'TrackCanvasMasterScroll.ViewportSize

        Dim timeLeft As Double = TrackCanvasMasterScroll.Value * PixelToTicksFactor / sldScaleX.Value
        Dim timeRight As Double = timeLeft + TrackCanvasMasterScroll.ViewportSize * PixelToTicksFactor / sldScaleX.Value

        If time > timeLeft Then
            If time < timeRight Then
                Exit Sub                ' is Visiible (in Viewport)
            End If
        End If

        ' if outside: scroll

        Dim val As Double
        val = time * TicksToPixelFactor * scale
        val -= TrackCanvasMasterScroll.ViewportSize / 2

        SetScroll(val)

    End Sub

    Private Sub SetScroll(value As Double)
        Dim args As New Primitives.ScrollEventArgs(Primitives.ScrollEventType.ThumbTrack, value)
        TrackCanvasMasterScroll.Value = value
        TrackCanvasMasterScroll_Scroll(Nothing, args)
    End Sub

    Private Sub TrackCanvasMasterScroll_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles TrackCanvasMasterScroll.SizeChanged
        UpdateMasterScrollValues()
    End Sub

    Public Sub UpdateMasterScrollValues()
        If Sequencer IsNot Nothing Then
            Dim CanvasLength As Double
            Dim Maximum As Double
            Dim ActualWidth As Double

            'CanvasLength = Sequencer.Composition.Length * TicksToPixelFactor * sldScaleX.Value
            CanvasLength = SeqPanel.Composition.Length * TicksToPixelFactor * sldScaleX.Value
            ActualWidth = TrackCanvasMasterScroll.ActualWidth
            Maximum = CanvasLength - ActualWidth
            If Maximum < 0 Then Maximum = 0

            TrackCanvasMasterScroll.Maximum = CanvasLength - ActualWidth
            TrackCanvasMasterScroll.ViewportSize = ActualWidth

            TrackCanvasMasterScroll.SmallChange = Math.Round(0.02 * Maximum, 0)
            TrackCanvasMasterScroll.LargeChange = Math.Round(0.1 * Maximum, 0)

            'SequencerPanel1.TracksHeader.MeasureCanvas.Width = CanvasLength
            'SequencerPanel1.TracksHeader.LoopCanvas.Width = CanvasLength
            'SequencerPanel1.TracksHeader.SectionCanvas.Width = CanvasLength

            If SeqPanel IsNot Nothing Then
                SeqPanel.TracksHeader.MeasureCanvas.Width = CanvasLength
                SeqPanel.TracksHeader.LoopCanvas.Width = CanvasLength
                SeqPanel.TracksHeader.SectionCanvas.Width = CanvasLength
            End If
        End If
    End Sub


End Class
