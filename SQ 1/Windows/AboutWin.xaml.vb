Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices

Public Class AboutWin

    Private NoteStr As String = "this project is still under development"
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        tbDescription.Text = My.Application.Info.Description
        tbVersion.Text = My.Application.Info.Version.ToString
        tbCopyright.Text = My.Application.Info.Copyright
        tbNotes.Text = NoteStr
    End Sub
End Class
