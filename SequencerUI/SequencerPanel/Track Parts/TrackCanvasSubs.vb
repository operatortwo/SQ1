Imports SequencerBase
Partial Public Class TrackCanvas


    Private GridLineBarBrush As Brush = Brushes.Black
    Private GridLineBeatBrush As Brush = Brushes.Gray
    Private Const GridY1 = 50
    Private Const GridLineBarHeight = 30
    Private Const GridLineBeatHeight = 20


    Private Const PatternTop = 2        ' 10
    Private Const PatternHeigth = 30  '40


    Private PatternRectFillBrush As Brush = New SolidColorBrush(Color.FromArgb(&H7F, &H6B, &HB5, &H43))
    Private PatternRectBlockLineBrush As Brush = Brushes.Green


    Private Sub DrawPattern(pattern As Pattern)
        Dim rect As New PatternRectangle(Me)
        Dim left As Double
        Dim top As Double = PatternTop
        Dim width As Double

        '--- Rectangle

        left = pattern.StartTime * TicksToPixelFactor * ScaleX
        width = pattern.Duration * TicksToPixelFactor * ScaleX

        Canvas.SetLeft(rect, left)
        Canvas.SetTop(rect, top)
        rect.Height = PatternHeigth
        rect.Width = width
        'rect.Fill = PatternRectFillBrush
        'rect.Stroke = Brushes.Black

        rect.Text = pattern.Name
        rect.ToolTip = pattern.Name
        rect.Pattern = pattern

        Canvas1.Children.Add(rect)

        Exit Sub


        '--- dividing Line if Duration > Length

        If pattern.Duration > pattern.Length Then
            Dim line As New Line

            line.Stroke = PatternRectBlockLineBrush
            line.StrokeThickness = 2

            line.X1 = left + (pattern.Length * TicksToPixelFactor * ScaleX)
            line.X2 = line.X1
            line.Y1 = top
            line.Y2 = top + PatternHeigth

            line.IsHitTestVisible = False
            Canvas1.Children.Add(line)

        End If

    End Sub


    Private Overloads Sub DrawText(text As String)
        Dim tb As New TextBlock
        tb.Text = text
        tb.IsHitTestVisible = False
        Canvas1.Children.Add(tb)
    End Sub
    Private Overloads Sub DrawText(text As String, left As Double, top As Double)
        Dim tb As New TextBlock
        tb.Text = text
        Canvas.SetLeft(tb, left)
        Canvas.SetTop(tb, top)
        tb.IsHitTestVisible = False
        Canvas1.Children.Add(tb)
    End Sub

    Private Overloads Sub DrawText(text As String, left As Double, top As Double, Foreground As SolidColorBrush)
        Dim tb As New TextBlock
        tb.Text = text
        tb.Foreground = Foreground
        Canvas.SetLeft(tb, left)
        Canvas.SetTop(tb, top)
        tb.IsHitTestVisible = False
        Canvas1.Children.Add(tb)
    End Sub

    Private Overloads Sub DrawText(text As String, left As Double, top As Double, FontSize As Double)
        Dim tb As New TextBlock
        tb.Text = text
        tb.FontSize = FontSize
        Canvas.SetLeft(tb, left)
        Canvas.SetTop(tb, top)
        tb.IsHitTestVisible = False
        Canvas1.Children.Add(tb)
    End Sub

    Private Overloads Sub DrawText(text As String, left As Double, top As Double, FontSize As Double, FontWeight As FontWeight)
        Dim tb As New TextBlock
        tb.Text = text
        tb.FontSize = FontSize
        tb.FontWeight = FontWeight
        Canvas.SetLeft(tb, left)
        Canvas.SetTop(tb, top)
        tb.IsHitTestVisible = False
        Canvas1.Children.Add(tb)
    End Sub


    Private Sub InsertPattern(time As UInteger, Pattern As Pattern)

        Dim newPattern As Pattern = Pattern.Copy

        Dim beats As UInteger = CUInt(time \ Sequencer.TPQ)
        time = CUInt(beats * Sequencer.TPQ)                     ' round to beat-start
        newPattern.StartTime = time

        Dim comp As Composition = Me.Composition
        If comp IsNot Nothing Then
            If VoiceNumber < comp.Voices.Count Then
                If TrackNumber < comp.Voices(VoiceNumber).Tracks.Count Then
                    Dim trk As Track = comp.Voices(VoiceNumber).Tracks(TrackNumber)

                    '--- find index of insert point
                    Dim ndx As Integer

                    ndx = trk.PatternList.FindLastIndex(Function(x) (x.StartTime + x.Duration) <= time)

                    'Dim maxEndTime As UInteger = Sequencer.Composition.Length       ' not behind end
                    Dim maxEndTime As UInteger = Composition.Length       ' not behind end
                    If ndx + 1 < trk.PatternList.Count Then
                        maxEndTime = trk.PatternList(ndx + 1).StartTime     ' not behind start of next pattern
                    End If

                    If maxEndTime < newPattern.StartTime Then
                        Beep()
                        Console.WriteLine("Error: maxEndTime < newPattern.StartTime")
                        Exit Sub
                    End If

                    Dim maxDuration As UInteger = maxEndTime - newPattern.StartTime

                    If newPattern.Duration > maxDuration Then
                        newPattern.Duration = maxDuration
                    End If

                    'Beep()
                    trk.PatternList.Insert(ndx + 1, newPattern)

                    DrawTrack()


                    '--- Debug

                    'Console.WriteLine("-----")
                    'Console.WriteLine("ndx: " & ndx + 1)
                    'Console.WriteLine("InsertTime: " & time)
                    'Console.WriteLine("StartTime" & " - Duration" & " - EndTime")

                    'For Each Pattern In trk.PatternList
                    '    Console.WriteLine(Pattern.StartTime & " - " & Pattern.Duration & " - " & Pattern.StartTime + Pattern.Duration)
                    'Next


                End If
            End If
        End If

    End Sub

    ''' <summary>
    ''' Check if the Position (x) is on a Pattern 
    ''' </summary>
    ''' <param name="Time">Position in Sequencer Ticks on Track-Canvas</param>
    ''' <returns></returns>
    Private Function IsOnPattern(Time As UInteger) As Boolean
        Dim comp As Composition = Me.Composition
        If comp IsNot Nothing Then
            If VoiceNumber < comp.Voices.Count Then
                If TrackNumber < comp.Voices(VoiceNumber).Tracks.Count Then
                    Dim trk As Track = comp.Voices(VoiceNumber).Tracks(TrackNumber)
                    'Dim time As UInteger = PixelToTicks(Position)
                    For Each pattern In trk.PatternList
                        If Time >= pattern.StartTime Then
                            If Time <= pattern.StartTime + pattern.Length Then Return True
                        End If
                    Next
                End If
            End If
        End If
        Return False
    End Function

    ''' <summary>
    ''' Converts SequencerTime units to Pixel units for Drawing on X-Axis. (Time * TicksToPixelFactor * ScaleX)
    ''' </summary>
    ''' <param name="Time">SequencerTicks</param>
    ''' <returns></returns>
    Private Function TicksToPixel(Time As Double) As Double
        Return Time * TicksToPixelFactor * ScaleX
    End Function
    ''' <summary>
    ''' Converts Pixel units to SequencerTime units. (Position / TicksToPixelFactor / ScaleX)
    ''' </summary>
    ''' <param name="Position">Relative position in UIElement or IInputElement</param>
    ''' <returns></returns>
    Private Function PixelToTicks(Position As Double) As UInteger
        Return CUInt(Position / TicksToPixelFactor / ScaleX)
    End Function


End Class
