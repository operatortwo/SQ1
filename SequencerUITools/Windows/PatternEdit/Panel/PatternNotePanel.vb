Imports SequencerBase

Public Class PatternNotePanel
    Inherits Canvas

    Friend PatternPanel As PatternPanel
    Friend rendercount As Integer

    Private drawlist As New List(Of Rect)

    Private NotePen As New Pen(Brushes.Black, 1)


    Protected Overrides Sub OnRender(ByVal dc As DrawingContext)
        MyBase.OnRender(dc)                'draw background

        '--- not added to the visual tree

        rendercount += 1
        Console.WriteLine("render count " & rendercount)                            'debug

        '---
        ' x  0.5 / 1 / 2 /



        '---
        If PatternPanel Is Nothing Then Exit Sub
        If PatternPanel.Pattern Is Nothing Then Exit Sub
        If PatternPanel.Pattern.EventList.Count = 0 Then Exit Sub
        If PatternPanel.sldScaleX Is Nothing Then Exit Sub
        If PatternPanel.sldScaleY Is Nothing Then Exit Sub

        Dim evlist As List(Of TrackEvent)
        evlist = PatternPanel.Pattern.EventList
        Dim scaleX As Double = PatternPanel.sldScaleX.Value
        Dim scaleY As Double = PatternPanel.sldScaleY.Value
        Dim ticksToPixelFactor = PatternPanel.TicksToPixelFactor

        drawlist.Clear()

        Dim nrect As Rect
        Dim noteRow As Integer

        ' insert note rectangle if note-event
        For Each ev In evlist
            If IsNoteOnEvent(ev) = True Then
                noteRow = GetNoteRow(ev.Data1)
                If noteRow <> -1 Then
                    nrect = New Rect
                    nrect.X = ev.Time * ticksToPixelFactor * scaleX
                    nrect.Width = ev.Duration * ticksToPixelFactor * scaleX
                    nrect.Y = noteRow * PatternPanel.PixelPerNoteRow * scaleY
                    nrect.Height = (PatternPanel.PixelPerNoteRow - 2) * scaleY
                    drawlist.Add(nrect)
                End If
            End If
        Next

        For Each nrect In drawlist
            dc.DrawRectangle(Brushes.Red, NotePen, nrect)
        Next

    End Sub


    Private Function GetNoteRow(note As Byte) As Integer
        For i = 1 To PatternPanel.NoteList.Count
            If PatternPanel.NoteList(i - 1) = note Then Return PatternPanel.NoteList.Count - i
        Next
        Return -1
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
End Class
