Imports SequencerBase

Public Class SavePatternDialog
    Public Property OverwriteExisting As Boolean        ' TRUE: if a Pattern with the same name exists, it will be replaced    
    Public Property PatternsToSave As New List(Of Pattern)

    Public Function ShowDialog(owner As Window, pattern As Pattern) As Boolean
        If pattern Is Nothing Then Return False
        PatternsToSave.Clear()
        PatternsToSave.Add(pattern)
        Return Show(owner)
    End Function

    Public Function ShowDialog(owner As Window, patternList As List(Of Pattern)) As Boolean
        If patternList Is Nothing Then Return False
        If patternList.Count = 0 Then Return False
        PatternsToSave = patternList
        Return Show(owner)
    End Function

    Private Function Show(owner As Window) As Boolean
        OwnerWindow = owner
        If CheckIfPatLibFullnameSettingExists(My.Settings.PatternLibFullname) = False Then Return False
        If CheckIfPatLibFullnameExitst(My.Settings.PatternLibFullname) = False Then Return False
        PatternLibFullname = My.Settings.PatternLibFullname     ' assume this file exists, based on previous checks

        If LoadIndex() = False Then Return False

        Dim win As New SavePatternWindow
        win.Owner = OwnerWindow
        win.WindowStartupLocation = WindowStartupLocation.CenterOwner
        win.OverwriteExisting = OverwriteExisting
        win.PatternsToSave = PatternsToSave
        Dim ret As Boolean
        ret = CBool(win.ShowDialog)
        ' ret is not used, use HasChanges directly
        If DSI.HasChanges Then
            SaveIndex()
        End If

        DSI.Clear()
        Return ret
    End Function
End Class
