Imports SequencerBase

Partial Public Class PatternPanel

    'Friend Const TicksToPixelFactor = 0.03125       ' ( 1 / 32 )
    'Friend Const PixelToTicksFactor = 32            ' ( 1 / TicksToPixelFactor )

    Friend Const TicksToPixelFactor = 0.0625        ' ( 1 / 16 )
    Friend Const PixelToTicksFactor = 16            ' ( 1 / TicksToPixelFactor )

    Private Shared TicksPerBeat = 960


    Private Sub SetupPanel()
        'DrawMeasureStrip()  ev. via size_changed
        'DrawPattern()
    End Sub

    Private drawcount As Integer

    Private Sub DrawPattern()

        drawcount += 1
        ' Console.WriteLine("drawcount " & drawcount)

        ' NotePanel.rendercount

        '--- added to the visual tree

        Dim awidth As Double
        'Aud.Length
        'awidth = Aud.Length * TicksToPixelFactor * sldScaleX.Value
        awidth = Pattern.Length * TicksToPixelFactor * sldScaleX.Value

        MeasurePanel.Width = awidth
        NotePanel.Width = awidth



        '--- above rendered drawings
        'NotePanel.Children.Clear()

        'Dim left As Double = 10
        'Dim top As Double = 20

        'Dim rc As New Rectangle
        'rc.Stroke = Brushes.Black
        'rc.Stroke = Brushes.Black
        'rc.Fill = Brushes.SkyBlue
        'rc.HorizontalAlignment = HorizontalAlignment.Left
        'rc.VerticalAlignment = VerticalAlignment.Center
        'rc.Height = 50
        'rc.Width = 50
        'Canvas.SetLeft(rc, left)
        'Canvas.SetTop(rc, top)
        'rc.IsHitTestVisible = False
        'NotePanel.Children.Add(rc)


    End Sub

    Private GridLineBarBrush As Brush = Brushes.Black
    Private GridLineBeatBrush As Brush = Brushes.Gray
    Private Const GridY1 = 5                                   ' y-start of measure lines (15)
    Private Const GridLineBarHeight = 20
    Private Const GridLineBeatHeight = 10


    Private Sub DrawMeasure()
        MeasurePanel.Children.Clear()

        Dim scaleX As Double = sldScaleX.Value

        If Pattern IsNot Nothing Then
            'Dim comp As SequencerBase.Composition = Sequencer.Composition
            Dim pat As Pattern = Pattern
            Dim tpq As Short = TicksPerQuarterNote
            Dim beatcount As Integer = CInt(pat.Length / tpq)

            Dim time As UInteger
            Dim barcount As Byte

            For i = 0 To beatcount
                Dim line As New Line
                If barcount = 0 Then
                    line.Stroke = GridLineBarBrush
                    line.Y2 = GridY1 + GridLineBarHeight

                    If i < beatcount Then           ' don't draw last Beat-Number
                        If SequencerBase.MBT_0_based = True Then
                            DrawText(CStr(i), TimeToPx(time) + Math.Max(10 * scaleX, 4), GridY1 + GridLineBeatHeight)
                        Else
                            DrawText(CStr(i + 1), TimeToPx(time) + Math.Max(10 * scaleX, 4), GridY1 + GridLineBeatHeight)
                        End If
                    End If
                Else
                    line.Stroke = GridLineBeatBrush
                    'line.StrokeThickness = 0.5
                    line.Y2 = GridY1 + GridLineBeatHeight
                End If

                line.X1 = time * TicksToPixelFactor * scaleX
                line.X2 = line.X1
                line.Y1 = GridY1

                line.IsHitTestVisible = False
                MeasurePanel.Children.Add(line)
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
        MeasurePanel.Children.Add(tb)
    End Sub

    Friend Function PosXYtoTrackEvent(pat As Pattern, posx As Integer, posy As Integer) As TrackEvent
        If pat Is Nothing Then Return Nothing
        If pat.EventList.Count = 0 Then Return Nothing
        Dim notenumber As Integer = PosYtoNoteNumber(posy)
        If notenumber = -1 Then Return Nothing
        Dim time As UInteger = PixelToTicks(posx)
        Dim evlist As List(Of TrackEvent) = pat.EventList


        For Each ev In evlist
            If time >= ev.Time AndAlso time <= ev.Time + ev.Duration Then
                If IsNoteOnEvent(ev) Then
                    If ev.Data1 = notenumber Then Return ev
                End If
            End If
        Next


        Return Nothing
    End Function


    Private Function IsNoteOnEvent(trev As TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.MidiEvent Then
            Dim stat As Byte = trev.Status And &HF0
            If stat = &H90 Then
                If trev.Data2 > 0 Then Return True
            End If
        End If
        Return False
    End Function
    Friend Function PosYtoNoteNumber(posY As Integer) As Integer
        Dim rownum As Integer = Fix(posY / (PixelPerNoteRow) / sldScaleY.Value)
        Dim notenum As Integer

        If NoteList.Count = 0 Then Return -1

        If rownum < NoteList.Count Then
            notenum = NoteList(NoteList.Count - 1 - rownum)
        Else
            notenum = -1
        End If

        Return notenum
    End Function

    ''' <summary>
    ''' Converts SequencerTime units to Pixel units for Drawing on X-Axis. (Time * TicksToPixelFactor * ScaleX)
    ''' </summary>
    ''' <param name="Time">SequencerTicks</param>
    ''' <returns></returns>
    Private Function TimeToPx(Time As Double) As Double
        Return Time * TicksToPixelFactor * sldScaleX.Value
    End Function

    ''' <summary>
    ''' Converts Pixel units to SequencerTime units. (Position / TicksToPixelFactor / ScaleX)
    ''' </summary>
    ''' <param name="Position">Relative position in UIElement or IInputElement</param>
    ''' <returns></returns>
    Private Function PixelToTicks(Position As Double) As UInteger
        Return CUInt(Position / TicksToPixelFactor / sldScaleX.Value)
    End Function



End Class
