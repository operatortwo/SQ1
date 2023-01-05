Public Class CompositionWin

    Private comp As SequencerBase.Composition

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        'comp = Module1.Sequencer.Composition
        comp = CompositionPanel1.Composition

        tbName.Text = comp.Name
        tbComments.Text = comp.Comments
        nudTempo.Value = comp.Tempo
        '--- read only ---
        tbTPQ.Text = CStr(comp.TicksPerQuarterNote)
        lblTimeSignature.Text = comp.TimeSignature_BeatsPerBar & "/" & comp.TimeSignature_NoteValuePerBeat
        ShowLengthValues()
        Show_LengthInSeconds()

    End Sub

    Private Sub ShowLengthValues()
        lblLength.Text = CStr(comp.Length)
        lblLength_Beats.Text = CStr(comp.Length / comp.TicksPerQuarterNote)     ' assuming NoteValuePerBeat = 4
        lblLength_Bars.Text = CStr(comp.Length / comp.TicksPerQuarterNote / comp.TimeSignature_BeatsPerBar)
    End Sub


    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        If comp IsNot Nothing Then
            comp.Name = tbName.Text
            comp.Comments = tbComments.Text
        End If
    End Sub

    Private Sub nudTempo_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        comp.Tempo = CShort(nudTempo.Value)
        Show_LengthInSeconds()
    End Sub

    Private Sub Show_LengthInSeconds()
        lblLength_Seconds.Text = CStr(Math.Round(comp.Length / comp.TicksPerQuarterNote * 60 / comp.Tempo, 2))
    End Sub

    Private Sub btnChangeCompositionLength_Click(sender As Object, e As RoutedEventArgs) Handles btnChangeCompositionLength.Click
        Dim win As New CompositionLength
        win.Owner = Me
        If win.ShowDialog() = True Then

            For Each panel As TrackPanel In CompositionPanel1.TrackPanelStack.Children
                For Each element As TrackElement In panel.TrackElementStack.Children
                    element.TrackCanvas.Length = comp.Length
                    'element.TrackCanvas.DrawTrack()
                Next
            Next

            ' need to update MeasureCanvas,..
            CompositionPanel1.UpdateScaleX()

            'CompositionPanel

            '--- update Dialog

            ShowLengthValues()
            Show_LengthInSeconds()

        End If

    End Sub
End Class
