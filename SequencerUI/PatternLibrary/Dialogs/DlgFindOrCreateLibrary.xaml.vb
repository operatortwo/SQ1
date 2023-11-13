Imports System.IO

Public Class DlgFindOrCreateLibrary

    Private MsgFullnameNotFound As String = "The Pattern-Library was not found at the specified location." & vbCrLf &
        "Please change the path to an existing file or create a new Pattern-Library."

    Private ExistingFileSelected As Boolean     ' user selected an existing file, need confirm + update Setting

    Private _SelectedPath As String = ""
    Private Property SelectedPath As String
        Get
            Return _SelectedPath
        End Get
        Set(value As String)
            _SelectedPath = value
            TbPatlibPath.Text = _SelectedPath
        End Set
    End Property

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        TbMessage.Text = MsgFullnameNotFound
        TbPatLibFilename.Text = PatternLibFilename
        SelectedPath = My.Settings.PatternLibFullname
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)

    End Sub

    Private Sub BtnFindExistingFile_Click(sender As Object, e As RoutedEventArgs) Handles BtnFindExistingFile.Click
        Dim ofd As New Microsoft.Win32.OpenFileDialog
        ofd.InitialDirectory = BaseDirectory
        ofd.Filter = "Library file|" & PatternLibFilename
        If ofd.ShowDialog() = True Then
            SelectedPath = ofd.FileName
            ExistingFileSelected = True
            tbSuccess.Text = "Library found, please confirm with [ OK ] to use this file."
            tbSuccess.Background = Brushes.LightGreen
            BtnCreateNewLib.IsEnabled = False
            cbImportPresets.IsEnabled = False
        End If
    End Sub

    Private Sub BtnCreateNewLib_Click(sender As Object, e As RoutedEventArgs) Handles BtnCreateNewLib.Click

        Try
            Dim dir As String = Path.GetDirectoryName(My.Settings.PatternLibFullname)
            If Directory.Exists(dir) = False Then
                Directory.CreateDirectory(dir)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Exit Sub
        End Try

        If CreateNewPatternLibrary(My.Settings.PatternLibFullname) = True Then
            Dim str As String = "The new library was created successfully"
            tbSuccess.Text = str
            tbSuccess.Background = Brushes.LightGreen
            BtnCancel.IsEnabled = False
            BtnFindExistingFile.IsEnabled = False
            BtnCreateNewLib.IsEnabled = False
            cbImportPresets.IsEnabled = False
            If cbImportPresets.IsChecked = True Then
                If ImportPresetPatterns(My.Settings.PatternLibFullname, PresetPatternLibFullname) = True Then
                    str &= " and the Preset Patterns were imported."
                    tbSuccess.Text = str
                Else
                    str &= ", but there was an error while importing the Preset Patterns."
                    tbSuccess.Background = Brushes.LightYellow
                End If
            End If
        Else
            MessageBox.Show("creation failed", "Create new Pattern Library")
        End If
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As RoutedEventArgs) Handles BtnOk.Click
        If ExistingFileSelected = True Then
            My.Settings.PatternLibFullname = SelectedPath
            My.Settings.Save()
            Close()
        End If

        If IO.File.Exists(SelectedPath) Then
            DialogResult = True
        Else
            DialogResult = False
        End If
        Close()
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As RoutedEventArgs) Handles BtnCancel.Click
        DialogResult = False
    End Sub

End Class

