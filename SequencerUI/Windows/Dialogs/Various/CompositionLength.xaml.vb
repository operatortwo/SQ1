Imports SequencerBase

Public Class CompositionLength

    Private Const TPQ As Integer = SequencerBase.Sequencer.TPQ
    Private OriginalLength As UInteger
    Private CurrentLength As UInteger
    Private MinLength As UInteger
    Private MaxLength As UInteger = 1000 * 4 * TPQ

    Private comp As Composition

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        comp = CompositionPanel1.Composition

        'OriginalLength = Sequencer.Composition.Length
        OriginalLength = comp.Length
        CurrentLength = OriginalLength
        ShowCurrentBeatsBars()

        MinLength = GetMinLength()
        lblBeatsMin.Content = MinLength / TPQ

        lblBeatsMax.Content = MaxLength / TPQ

    End Sub

    Private Sub ShowCurrentBeatsBars()
        lblBeatsCurrent.Content = CurrentLength / TPQ       ' assuming NoteValuePerBeat = 4
        lblBarsCurrent.Content = CurrentLength / comp.TicksPerQuarterNote / comp.TimeSignature_BeatsPerBar
    End Sub

    Private Function GetMinLength() As UInteger
        Dim MinLength As UInteger = 4 * TPQ
        Dim PatternEnd As UInteger

        If comp IsNot Nothing Then
            '  find last pattern end
            For Each voice In comp.Voices
                For Each track In voice.Tracks
                    For Each pattern In track.PatternList
                        PatternEnd = pattern.StartTime + pattern.Duration
                        If PatternEnd > MinLength Then
                            MinLength = PatternEnd
                        End If
                    Next
                Next
            Next
        End If

        Return MinLength
    End Function

    Private Sub repBtnPositionDec_Click(sender As Object, e As RoutedEventArgs) Handles repBtnPositionDec.Click
        If CurrentLength > MinLength Then
            CurrentLength = CUInt(CurrentLength - TPQ)
            ShowCurrentBeatsBars()
        End If
    End Sub

    Private Sub repBtnPositionInc_Click(sender As Object, e As RoutedEventArgs) Handles repBtnPositionInc.Click
        If CurrentLength < MaxLength Then
            CurrentLength = CUInt(CurrentLength + TPQ)
            ShowCurrentBeatsBars()
        End If
    End Sub

    Private Sub btnReset_Click(sender As Object, e As RoutedEventArgs) Handles btnReset.Click
        CurrentLength = OriginalLength
        ShowCurrentBeatsBars()
    End Sub

    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        If CurrentLength <> OriginalLength Then
            comp.Length = CurrentLength
            DialogResult = True
        Else
            DialogResult = False
        End If
    End Sub


End Class
