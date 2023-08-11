Imports System.Runtime.Remoting.Messaging
Imports System.Text
Imports SequencerBase
Imports SequencerUITools.PatternPanel

Public Class KeyModeDlg
    Private PatternPanel As PatternPanel
    Private Pattern As Pattern
    Private PatternInfo As PatternInfo
    Public Sub New(panel As PatternPanel)
        InitializeComponent()
        PatternPanel = panel
        Pattern = panel.Pattern
        PatternInfo = New PatternInfo(Pattern)
    End Sub
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        cmbKeyMode.ItemsSource = [Enum].GetValues(GetType(KeyMode))
        cmbKeyMode.SelectedItem = PatternPanel.PanelKeyMode

        ShowPatternInfo()

    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)

    End Sub

    Private Sub btnOk_Click(sender As Object, e As RoutedEventArgs) Handles btnOk.Click
        PatternPanel.selBtnKeyMode.Value = cmbKeyMode.SelectedIndex
        Close()
    End Sub

    Private Sub ShowPatternInfo()
        Dim sb As New StringBuilder
        sb.Append("Number of used Notes = " & PatternInfo.ListOfUsedNotes.Count & vbCrLf)

        If PatternInfo.ListOfUsedNotes.Count > 1 Then
            Dim FirstNote As Byte = PatternInfo.ListOfUsedNotes.First
            Dim LastNote As Byte = PatternInfo.ListOfUsedNotes.Last
            sb.Append("Note Range: " & FirstNote & " - " & LastNote & vbCrLf)
        End If

        sb.Append("NoteNumbers: ")
        For Each note In PatternInfo.ListOfUsedNotes
            sb.AppendFormat(" {0}", note)
        Next
        sb.Append(vbCrLf)

        sb.Append("NoteNames: ")
        For Each note In PatternInfo.ListOfUsedNotes
            sb.AppendFormat(SequencerBase.NoteNumber_to_NoteName(note) & " ")
        Next
        sb.Append(vbCrLf)

        tbPatInfo.AppendText(sb.ToString)                ' to TextBox
    End Sub


End Class
