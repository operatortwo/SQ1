Public Class DlgSetPathToLibrary

    Private MsgFullnameEmpty As String = "The Path to the Pattern Library is not set." & vbCrLf &
        "Please set the desired Path."

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
        TbMessage.Text = MsgFullnameEmpty
        TbPatLibFilename.Text = PatternLibFilename
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)

    End Sub

    Private Sub RbDefaultMusic_Checked(sender As Object, e As RoutedEventArgs) Handles RbDefaultMusic.Checked
        SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic, Environment.SpecialFolderOption.DoNotVerify)
        SelectedPath &= "\SQ1_Data"
    End Sub

    Private Sub RbDefaultDocuments_Checked(sender As Object, e As RoutedEventArgs) Handles RbDefaultDocuments.Checked
        SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify)
        SelectedPath &= "\SQ1_Data"
    End Sub

    Private Sub RbUserDefined_Checked(sender As Object, e As RoutedEventArgs) Handles RbUserDefined.Checked
        SelectedPath = ""
        Dim fbd As New Forms.FolderBrowserDialog
        fbd.RootFolder = Environment.SpecialFolder.MyComputer
        fbd.Description = "Set path to Pattern-Library file"
        If fbd.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPath = fbd.SelectedPath
        Else
            RbDefaultMusic.IsChecked = True                 ' set default
        End If
    End Sub

    Private Sub RbToExistingFile_Checked(sender As Object, e As RoutedEventArgs) Handles RbToExistingFile.Checked
        SelectedPath = ""
        Dim ofd As New Microsoft.Win32.OpenFileDialog
        ofd.InitialDirectory = BaseDirectory
        ofd.Filter = "Library file|" & PatternLibFilename
        If ofd.ShowDialog() = True Then
            SelectedPath = ofd.FileName
        Else
            RbDefaultMusic.IsChecked = True                 ' set default
        End If
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As RoutedEventArgs) Handles BtnOk.Click
        If SelectedPath = "" Then
            DialogResult = False
        Else
            IO.Directory.CreateDirectory(SelectedPath)
            If IO.Directory.Exists(SelectedPath) = True Then
                My.Settings.PatternLibFullname = SelectedPath & "\" & PatternLibFilename
                My.Settings.Save()
                DialogResult = True
            Else
                DialogResult = False                    ' failed creating directory
            End If
        End If
        Close()
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As RoutedEventArgs) Handles BtnCancel.Click
        DialogResult = False
    End Sub

End Class
