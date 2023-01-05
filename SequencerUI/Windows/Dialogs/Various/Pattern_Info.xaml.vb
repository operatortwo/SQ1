Imports System.Text
Imports SequencerBase
Public Class Pattern_Info

    Private pat As Pattern
    Private strb As New StringBuilder
    Private NoteList As New List(Of Byte)

    Public Sub New(Pattern As Pattern)
        ' required by the designer
        InitializeComponent()
        pat = Pattern
    End Sub
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        ShowPatternInfo()
    End Sub

    Private Sub ShowPatternInfo()
        tbPatternName.Text = pat.Name
        tbInfo.Clear()

        strb.Clear()
        strb.AppendLine("StartTime = " & pat.StartTime)
        strb.AppendLine("StartOffset = " & pat.StartOffset)
        strb.AppendLine("Length = " & pat.Length)
        strb.AppendLine("Duration = " & pat.Duration)
        'strb.AppendLine("DoLoop = " & pat.DoLoop)
        strb.AppendLine("Ended = " & pat.Ended)
        strb.AppendLine("EventListPtr = " & pat.EventListPtr)
        strb.AppendLine("EventList Count = " & pat.EventList.Count)

        Create_NoteList()

        strb.Append("Number of Notes = " & NoteList.Count & vbCrLf)

        If NoteList.Count > 1 Then
            Dim FirstNote As Byte = NoteList.First
            Dim LastNote As Byte = NoteList.Last
            strb.Append("Note Range: " & FirstNote & " - " & LastNote & vbCrLf)
        End If

        strb.Append("NoteNumbers: ")
        For Each note In NoteList
            strb.AppendFormat(" {0}", note)
        Next
        strb.Append(vbCrLf)

        'NoteNumber to NoteName
        strb.Append("NoteNames: ")
        For Each note In NoteList
            strb.AppendFormat(NoteNumber_to_NoteName(note) & " ")
        Next
        strb.Append(vbCrLf)

        tbInfo.AppendText(strb.ToString)                ' to TextBox

    End Sub





    Private Sub Create_NoteList()
        Dim tev As TrackEvent
        Dim status As Byte
        Dim note As Byte

        NoteList.Clear()

        For i = 1 To pat.EventList.Count
            tev = pat.EventList(i - 1)

            If tev.Type = EventType.MidiEvent Then
                status = CByte(tev.Status And &HF0)
                If (status = &H90) And (tev.Data2 > 0) Then
                    note = tev.Data1
                    If Not NoteList.Contains(note) Then
                        NoteList.Add(note)
                    End If
                End If
            End If
        Next

        NoteList.Sort()

    End Sub


    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        Close()
    End Sub
End Class
