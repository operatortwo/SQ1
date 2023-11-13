Public Class ManagePatternLibraryDialog

    Public Function ShowDialog(owner As Window) As Boolean
        OwnerWindow = owner
        If CheckIfPatLibFullnameSettingExists(My.Settings.PatternLibFullname) = False Then Return False
        If CheckIfPatLibFullnameExitst(My.Settings.PatternLibFullname) = False Then Return False
        PatternLibFullname = My.Settings.PatternLibFullname     ' assume this file exists, based on previous checks

        If LoadIndex() = False Then Return False

        Dim win As New ManagePatternLibraryWindow
        win.Owner = OwnerWindow
        win.WindowStartupLocation = WindowStartupLocation.CenterOwner
        Dim ret As Boolean
        ret = CBool(win.ShowDialog)

        If ret = True Then
            If DSI.HasChanges Then
                SaveIndex()
            End If
        End If

        DSI.Clear()

        Return ret
    End Function

End Class
