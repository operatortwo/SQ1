Imports SequencerBase

Public Class PatternNotePanel
    Inherits Canvas

    Friend PatternPanel As PatternPanel
    Friend rendercount As Integer

    Private drawlist As New List(Of Nrect)

    Private NotePen As New Pen(Brushes.Black, 1)

    Private GridPen4 As New Pen(Brushes.Black, 1)
    Private GridPen8 As New Pen(Brushes.Gray, 0.7)
    Private GridPen16 As New Pen(Brushes.Gray, 0.7)

    Private NoteBrushFull As New SolidColorBrush(Colors.LightGray)
    'Private NoteBrushVeocity As New SolidColorBrush(Colors.LightGray)
    Private NoteBrushVelocity As New SolidColorBrush(Color.FromRgb(0, 122, 204))

    Private Class Nrect
        Public full As Rect
        Public velocity As Rect
    End Class


    Protected Overrides Sub OnRender(ByVal dc As DrawingContext)
        MyBase.OnRender(dc)                'draw background

        '--- not added to the visual tree

        rendercount += 1
        ' Console.WriteLine("render count " & rendercount)                            'debug

        '---
        '
        If PatternPanel Is Nothing Then Exit Sub
        If PatternPanel.Pattern Is Nothing Then Exit Sub
        If PatternPanel.Pattern.EventList.Count = 0 Then Exit Sub
        If PatternPanel.sldScaleX Is Nothing Then Exit Sub
        If PatternPanel.sldScaleY Is Nothing Then Exit Sub

        DrawGrid(dc)

        Dim evlist As List(Of TrackEvent)
        evlist = PatternPanel.Pattern.EventList
        Dim scaleX As Double = PatternPanel.sldScaleX.Value
        Dim scaleY As Double = PatternPanel.sldScaleY.Value
        Dim ticksToPixelFactor = PatternPanel.TicksToPixelFactor

        drawlist.Clear()

        Dim nrect As Nrect
        Dim noteRow As Integer
        Dim velheight As Integer
        Dim veloffset As Integer

        ' insert note rectangle if note-event
        For Each ev In evlist
            If IsNoteOnEvent(ev) = True Then
                noteRow = GetNoteRow(ev.Data1)
                If noteRow <> -1 Then
                    nrect = New Nrect
                    '--- full rect
                    nrect.full.X = ev.Time * ticksToPixelFactor * scaleX
                    nrect.full.Width = ev.Duration * ticksToPixelFactor * scaleX
                    nrect.full.Y = noteRow * PatternPanel.PixelPerNoteRow * scaleY
                    nrect.full.Height = (PatternPanel.PixelPerNoteRow - 2) * scaleY
                    '--- velocity rect
                    nrect.velocity = nrect.full
                    velheight = nrect.full.Height / 127 * ev.Data2
                    veloffset = nrect.full.Height - velheight
                    nrect.velocity.Height = velheight
                    nrect.velocity.Y += veloffset
                    drawlist.Add(nrect)
                    End If
                End If
        Next

        For Each nrect In drawlist
            dc.DrawRectangle(NoteBrushFull, Nothing, nrect.full)                ' draw full rectangle
            dc.DrawRectangle(NoteBrushVelocity, Nothing, nrect.velocity)        ' draw velocity rectangle            
            If scaleX > 0.4 Then
                dc.DrawRectangle(Nothing, NotePen, nrect.full)                  ' draw border rectangle
            End If
        Next

    End Sub

    Private Sub DrawGrid(dc As DrawingContext)
        Dim Pattern As Pattern = PatternPanel.Pattern
        Dim scaleX As Double = PatternPanel.sldScaleX.Value
        Dim ticksToPixelFactor = PatternPanel.TicksToPixelFactor
        Dim posx As Double
        Dim stepx As Double
        Dim pt0 As New Point
        Dim pt1 As New Point
        Dim panelHeight As Double = PatternPanel.NotePanel.Height
        Dim linecount As Integer
        ' x  0.5 / 1 / 2 /

        '--- draw Quarter lines
        If scaleX >= 0.4 Then
            linecount = Pattern.Length / PatternPanel.TicksPerQuarterNote
            stepx = PatternPanel.TicksPerQuarterNote
            posx = PatternPanel.TicksPerQuarterNote
            pt1.Y = panelHeight

            For i = 1 To linecount
                pt0.X = posx * ticksToPixelFactor * scaleX
                pt1.X = pt0.X
                pt0.Y = 0
                pt1.Y = panelHeight
                dc.DrawLine(GridPen4, pt0, pt1)
                posx += stepx
            Next
        End If

        '--- draw eighth lines
        If scaleX >= 1 Then
            linecount = Pattern.Length / PatternPanel.TicksPerQuarterNote
            stepx = PatternPanel.TicksPerQuarterNote
            posx = PatternPanel.TicksPerQuarterNote / 2
            pt1.Y = panelHeight

            For i = 1 To linecount
                pt0.X = posx * ticksToPixelFactor * scaleX
                pt1.X = pt0.X
                pt0.Y = 0
                pt1.Y = panelHeight
                dc.DrawLine(GridPen8, pt0, pt1)
                posx += stepx
            Next
        End If

        '--- draw sixteenth lines
        If scaleX >= 1.5 Then
            linecount = Pattern.Length / PatternPanel.TicksPerQuarterNote * 2
            stepx = PatternPanel.TicksPerQuarterNote / 2
            posx = PatternPanel.TicksPerQuarterNote / 4
            pt1.Y = panelHeight

            For i = 1 To linecount
                pt0.X = posx * ticksToPixelFactor * scaleX
                pt1.X = pt0.X
                pt0.Y = 0
                pt1.Y = panelHeight
                dc.DrawLine(GridPen16, pt0, pt1)
                posx += stepx
            Next
        End If





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
