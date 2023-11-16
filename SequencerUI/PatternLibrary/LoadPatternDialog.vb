Imports SequencerBase

Public Class LoadPatternDialog

    Public Property Multiselect As Boolean
    Public Property LoadedPattern As Pattern
    Public Property LoadedPatterns As List(Of Pattern)

    Public Function ShowDialog(owner As Window) As Boolean
        OwnerWindow = owner
        If CheckIfPatLibFullnameSettingExists(My.Settings.PatternLibFullname) = False Then Return False
        If CheckIfPatLibFullnameExitst(My.Settings.PatternLibFullname) = False Then Return False
        PatternLibFullname = My.Settings.PatternLibFullname     ' assume this file exists, based on previous checks
        LoadedPattern = Nothing                                 ' clear if any leftovers
        LoadedPatterns = Nothing                                ' clear if any leftovers

        If LoadIndex() = False Then Return False

        'Dim var = Get_DSI_VarValue("PresetVersion")
        'var can be used to check if the PresetPatterns are updated

        Dim win As New LoadPatternWindow
        win.Owner = OwnerWindow
        win.WindowStartupLocation = WindowStartupLocation.CenterOwner
        win.Multiselect = Multiselect
        win.LoadedPattern = LoadedPattern
        win.LoadedPatterns = LoadedPatterns
        Dim ret As Boolean
        ret = CBool(win.ShowDialog)
        DSI.Clear()
        If ret = False Then Return False

        LoadedPattern = win.LoadedPattern
        LoadedPatterns = win.LoadedPatterns
        Return True
    End Function

End Class
