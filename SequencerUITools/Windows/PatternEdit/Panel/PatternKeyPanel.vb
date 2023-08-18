Imports System.IO
Imports SequencerBase
Imports SequencerUI

Partial Public Class PatternPanel

    Private KeyNoteNumber_playing As Byte = 128           ' currently playing Note initiated from user. > 127 = none

    Friend Const PixelPerNoteRow = 20
    Private PatternInfo1 As PatternInfo
    Private NumberOfNoteRows As Integer = 128
    Friend NoteList As New List(Of Byte)

    Friend ListOfDesiredNotes As New List(Of Byte)
    Friend DesiredNoteRangeStart As Integer
    Friend DesiredNoteRangeEnd As Integer

    Private KeyTextOption As KeyTextOptions

    Private _PanelKeyMode As KeyMode = KeyMode.UsedRange
    Friend Property PanelKeyMode As KeyMode
        Get
            Return _PanelKeyMode
        End Get
        Set(value As KeyMode)
            If _PanelKeyMode <> value Then
                _PanelKeyMode = value
                CreateNoteList()
                SetVscrollValues()
                DrawKeys()
            End If
        End Set
    End Property

    Private Sub DrawKeys()

        Dim scaleY As Double = sldScaleY.Value
        KeyPanel.Children.Clear()
        KeyPanel.Height = NumberOfNoteRows * PixelPerNoteRow * scaleY

        Dim rowHeight As Integer = PixelPerNoteRow * scaleY
        Dim rowAlign As Integer = (rowHeight - 16) / 2      ' FontSize:12, ActualHeight=16, see also tblk.arrange
        Dim rowsToDraw As Integer = 128
        Dim rowNumber As Integer
        Dim noteNumber As Integer
        Dim line As Line

        ' top line

        Dim lineWidth As Double = CenterGrid.ColumnDefinitions(0).MaxWidth

        line = New Line With {.Stroke = Brushes.Gray, .Y1 = 0, .Y2 = 0, .X1 = 0, .X2 = lineWidth}
        KeyPanel.Children.Add(line)

        For i = NoteList.Count - 1 To 0 Step -1

            noteNumber = 127 - rowNumber

            line = New Line
            line.Stroke = Brushes.Gray
            line.Y1 = PixelPerNoteRow * (rowNumber + 1) * scaleY
            line.Y2 = line.Y1
            line.X1 = 4
            line.X2 = lineWidth

            line.IsHitTestVisible = False
            KeyPanel.Children.Add(line)

            Dim keytext As String

            Select Case KeyTextOption
                Case KeyTextOptions.NoteName
                    keytext = NoteNumber_to_NoteName(NoteList(i))
                Case KeyTextOptions.DrumVoiceNameGM
                    keytext = Get_GM_DrumVoiceName(NoteList(i))
                Case Else
                    keytext = NoteList(i)
            End Select

            'InsertText(KeyPanel, rowNumber & " " & keytext, 2, (rowNumber * rowHeight) + rowAlign)
            InsertText(KeyPanel, keytext, 5, (rowNumber * rowHeight) + rowAlign)

            rowNumber += 1

        Next

    End Sub


    Private Sub InsertText(panel As Canvas, text As String, left As Double, top As Double)
        Dim tb As New TextBlock
        tb.Text = text
        Canvas.SetLeft(tb, left)
        Canvas.SetTop(tb, top)
        tb.IsHitTestVisible = False
        panel.Children.Add(tb)
    End Sub

    Private Sub CreateNoteList()

        NoteList.Clear()

        Select Case PanelKeyMode
            Case KeyMode.FullRange
                For i = 0 To 127
                    NoteList.Add(i)
                Next
            Case KeyMode.UsedRange
                For i = PatternInfo1.NoteRangeStart To PatternInfo1.NoteRangeEnd
                    NoteList.Add(i)
                Next
            Case KeyMode.UserDefinedRange
                For i = DesiredNoteRangeStart To DesiredNoteRangeEnd
                    NoteList.Add(i)
                Next
            Case KeyMode.UsedNotes
                For i = 1 To PatternInfo1.ListOfUsedNotes.Count
                    NoteList.Add(PatternInfo1.ListOfUsedNotes(i - 1))
                Next
            Case KeyMode.UserDefinedNotes
                For i = 1 To ListOfDesiredNotes.Count
                    NoteList.Add(PatternInfo1.ListOfUsedNotes(i - 1))
                Next
            Case Else
                For i = 0 To 127
                    NoteList.Add(i)
                Next
        End Select

        NumberOfNoteRows = NoteList.Count
    End Sub


    Private Enum KeyTextOptions
        NoteNumber
        NoteName
        DrumVoiceNameGM
    End Enum


#Region "Pattern Info"

    Private Sub InitializePatternInfo(pat As Pattern)
        Dim pi As New PatternInfo(pat)
        PatternInfo1 = pi

        DesiredNoteRangeStart = pi.NoteRangeStart
        DesiredNoteRangeEnd = pi.NoteRangeEnd

        ListOfDesiredNotes.Clear()

        For Each note In pi.ListOfUsedNotes
            ListOfDesiredNotes.Add(note)
        Next

        ListOfDesiredNotes.Sort()
    End Sub

    Friend Class PatternInfo
        Private Pattern As Pattern

        Friend NumberOfNotes As Integer
        Friend ListOfUsedNotes As New List(Of Byte)
        Friend NoteRangeStart As Integer
        Friend NoteRangeEnd As Integer

        Public Sub New(pat As Pattern)
            Pattern = pat
            FillData()
        End Sub

        Public Sub GetInfo(pat As Pattern)
            Pattern = pat
            FillData()
        End Sub

        Private Sub FillData()
            Create_NoteLists()
            NumberOfNotes = ListOfUsedNotes.Count
            NoteRangeStart = ListOfUsedNotes.FirstOrDefault
            NoteRangeEnd = ListOfUsedNotes.LastOrDefault
        End Sub

        Private Sub Create_NoteLists()
            ListOfUsedNotes.Clear()

            If Pattern IsNot Nothing Then
                Dim status As Byte
                Dim note As Byte

                For Each tev In Pattern.EventList

                    If tev.Type = EventType.MidiEvent Then
                        status = CByte(tev.Status And &HF0)
                        If (status = &H90) And (tev.Data2 > 0) Then
                            note = tev.Data1
                            If Not ListOfUsedNotes.Contains(note) Then
                                ListOfUsedNotes.Add(note)
                            End If
                        End If
                    End If
                Next
                ListOfUsedNotes.Sort()
            End If
        End Sub

    End Class

    Public Enum KeyMode
        FullRange
        UsedRange
        UserDefinedRange
        UsedNotes
        UserDefinedNotes
    End Enum

    Private Function GetKeyModeName(mode As KeyMode) As String
        Select Case mode
            Case KeyMode.FullRange
                Return "Full Range"
            Case KeyMode.UsedRange
                Return "Used Range"
            Case KeyMode.UserDefinedRange
                Return "User defined Range"
            Case KeyMode.UsedNotes
                Return "Used Notes"
            Case KeyMode.UserDefinedNotes
                Return "User defined Notes"
        End Select
        Return ""
    End Function

#End Region


End Class
