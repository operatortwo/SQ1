Imports SequencerBase

Partial Public Class MainWindow



    Private Sub ShowPointers()
        'Sequencer.Composition.voi
        Dim str As String = ""
        Dim pat As Pattern

        DbgMsg("--- Show pointers ---")
        DbgMsg("Time: " & CUInt(Sequencer.SequencerTime))

        For Each voice In Sequencer.Composition.Voices
            DbgMsg("Voice: " & voice.Title)

            For Each track In voice.Tracks
                str = "   Track: " & track.Title & " - Pat.Ptr: " & track.PatternListPtr
                DbgMsg(str)

                If track.PatternListPtr < track.PatternList.Count Then
                    pat = track.PatternList(track.PatternListPtr)
                    If pat IsNot Nothing Then
                        'pat.EventListPtr


                        DbgMsg("      Pat StartTime: " & pat.StartTime & "   EndTime: " & pat.StartTime + pat.Duration)
                        DbgMsg("      Ev.ListPtr: " & pat.EventListPtr)
                        DbgMsg("      Ev.ListCount: " & pat.EventList.Count)



                    End If
                End If

            Next
        Next

        DbgMsg("")
        tbDebugOut.ScrollToEnd()

    End Sub

    Private Sub DbgMsg(str As String)
        If tbDebugOut.LineCount > 1000 Then
            'tbDebugOut.Select(0, 100)
            tbDebugOut.Clear()
        End If

        tbDebugOut.AppendText(str & vbCrLf)
    End Sub


End Class
