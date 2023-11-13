Imports System.Data
Imports SequencerBase

Public Class SavePatternWindow

    Public Property OverwriteExisting As Boolean        ' TRUE: if a Pattern with the same name exists, it will be replaced

    Public Property PatternsToSave As List(Of Pattern)
    Private CurrentPatternIndex As Integer                  ' index in PatternsToSave

    Private NumPatternsToSave As Integer                    ' remaining
    Private NumPatternsSaved As Integer
    Private NumErrorCount As Integer

    Public IndexView As DataView            ' for filtering / sorting
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        ' set local format for Datagrid DateTime column     --> already made in MainWindow
        ' LanguageProperty.OverrideMetadata(GetType(FrameworkElement), New FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)))

        Dim odp As ObjectDataProvider = CType(TryFindResource("Dsi1"), ObjectDataProvider)
        If odp IsNot Nothing Then
            odp.ObjectInstance = DSI
        End If

        IndexView = DSI.DT_Main.DefaultView

        '--- Filter

        FillInsertBoxes()
        FillFilterBoxes()
        ResetAllFilters()

        Dim patcount As Integer = PatternsToSave.Count

        If patcount = 0 Then MessageBox.Show("Pattern savelist is empty")
        If IsNothing(PatternsToSave(0)) Then
            MessageBox.Show("Pattern is Nothing")
        End If

        CurrentPatternIndex = 0
        NumPatternsToSave = patcount            ' initialize
        NumPatternsSaved = 0
        NumErrorCount = 0

        ShowSaveState()

        ' hide Button 'SaveAll' if count is 0 or 1
        If patcount < 2 Then
            btnSaveAll.Visibility = Visibility.Hidden
        End If


        ' show name of first pattern
        If patcount > 0 Then
            tbName.Text = PatternsToSave(CurrentPatternIndex).Name
        End If

        ' check name, considering checkbox 'Overwrite existing'
        ValidateName()

    End Sub

    Private Sub FillInsertBoxes()

        '--- Category
        cmbCategoryForSave.DisplayMemberPath = "Value"
        cmbCategoryForSave.SelectedValuePath = "Key"
        For Each item In DSI.DT_Categories
            cmbCategoryForSave.Items.Add(New KeyValuePair(Of Integer, String)(item.ID, item.Name))
        Next
        '--- Category2
        cmbCategory2ForSave.DisplayMemberPath = "Value"
        cmbCategory2ForSave.SelectedValuePath = "Key"
        For Each item In DSI.DT_Categories2
            cmbCategory2ForSave.Items.Add(New KeyValuePair(Of Integer, String)(item.ID, item.Name))
        Next
        '--- VoiceType
        cmbVoiceTypeForSave.DisplayMemberPath = "Value"
        cmbVoiceTypeForSave.SelectedValuePath = "Key"
        For Each item In DSI.DT_VoiceTypes
            cmbVoiceTypeForSave.Items.Add(New KeyValuePair(Of Integer, String)(item.ID, item.Name))
        Next
        '--- Source
        cmbSourceForSave.DisplayMemberPath = "Value"
        cmbSourceForSave.SelectedValuePath = "Key"
        For Each item In DSI.DT_Sources
            cmbSourceForSave.Items.Add(New KeyValuePair(Of Integer, String)(item.ID, item.Name))
        Next

    End Sub
    Private Sub FillFilterBoxes()
        'cmbCategoryFilter.ItemsSource = DSI.DT_Categories
        'cmbCategoryFilter.DisplayMemberPath = "Name"
        'cmbCategoryFilter.SelectedValuePath = "ID"

        '--- Category
        cmbCategoryFilter.DisplayMemberPath = "Value"
        cmbCategoryFilter.SelectedValuePath = "Key"
        cmbCategoryFilter.Items.Add(New KeyValuePair(Of Integer, String)(0, "*"))
        For Each item In DSI.DT_Categories
            cmbCategoryFilter.Items.Add(New KeyValuePair(Of Integer, String)(item.ID, item.Name))
        Next
        '--- Category 2
        cmbCategory2Filter.DisplayMemberPath = "Value"
        cmbCategory2Filter.SelectedValuePath = "Key"
        cmbCategory2Filter.Items.Add(New KeyValuePair(Of Integer, String)(0, "*"))
        For Each item In DSI.DT_Categories2
            cmbCategory2Filter.Items.Add(New KeyValuePair(Of Integer, String)(item.ID, item.Name))
        Next
        '--- Voice Type
        cmbVoiceTypeFilter.DisplayMemberPath = "Value"
        cmbVoiceTypeFilter.SelectedValuePath = "Key"
        cmbVoiceTypeFilter.Items.Add(New KeyValuePair(Of Integer, String)(0, "*"))
        For Each item In DSI.DT_VoiceTypes
            cmbVoiceTypeFilter.Items.Add(New KeyValuePair(Of Integer, String)(item.ID, item.Name))
        Next
        '--- Source
        cmbSourceFilter.DisplayMemberPath = "Value"
        cmbSourceFilter.SelectedValuePath = "Key"
        cmbSourceFilter.Items.Add(New KeyValuePair(Of Integer, String)(0, "*"))
        For Each item In DSI.DT_Sources
            cmbSourceFilter.Items.Add(New KeyValuePair(Of Integer, String)(item.ID, item.Name))
        Next
        '--- Drum / Voice
        cmbDrumVoiceFilter.Items.Add("*")
        cmbDrumVoiceFilter.Items.Add("Drum")
        cmbDrumVoiceFilter.Items.Add("Voice")

        '--- Preset / User
        cmbPresetUserFilter.Items.Add("*")
        cmbPresetUserFilter.Items.Add("Preset")
        cmbPresetUserFilter.Items.Add("User")

    End Sub
    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)

    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        DialogResult = False
    End Sub

    Private Sub btnOk_Click(sender As Object, e As RoutedEventArgs) Handles btnOk.Click
        Close()
    End Sub

    Private Sub btnResetAllFilters_Click(sender As Object, e As RoutedEventArgs) Handles btnResetAllFilters.Click
        ResetAllFilters()
    End Sub

    Private Sub ShowViewCount()
        sbiViewCount.Content = IndexView.Count
        sbiTotalCount.Content = IndexView.Table.Rows.Count
    End Sub

    Private Sub ResetAllFilters()
        cmbCategoryFilter.SelectedIndex = 0
        cmbCategory2Filter.SelectedIndex = 0
        cmbVoiceTypeFilter.SelectedIndex = 0
        cmbSourceFilter.SelectedIndex = 0
        cmbDrumVoiceFilter.SelectedIndex = 0
        cmbPresetUserFilter.SelectedIndex = 0
    End Sub

    Private Sub cmbCategoryFilter_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbCategoryFilter.SelectionChanged
        UpdateRowFilter()
    End Sub

    Private Sub cmbCategory2Filter_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbCategory2Filter.SelectionChanged
        UpdateRowFilter()
    End Sub

    Private Sub cmbVoiceTypeFilter_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbVoiceTypeFilter.SelectionChanged
        UpdateRowFilter()
    End Sub

    Private Sub cmbSourceFilter_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbSourceFilter.SelectionChanged
        UpdateRowFilter()
    End Sub

    Private Sub cmbDrumVoiceFilter_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbDrumVoiceFilter.SelectionChanged
        UpdateRowFilter()
    End Sub

    Private Sub cmbPresetUserFilter_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbPresetUserFilter.SelectionChanged
        UpdateRowFilter()
    End Sub

    Private Sub UpdateRowFilter()
        Dim filter As String = ""

        If cmbCategoryFilter.SelectedIndex > 0 Then
            filter &= "ID_Category = " & CStr(cmbCategoryFilter.SelectedValue)
        End If

        If cmbCategory2Filter.SelectedIndex > 0 Then
            If filter <> "" Then filter &= " AND "
            filter &= "ID_Category2 = " & CStr(cmbCategory2Filter.SelectedValue)
        End If

        If cmbVoiceTypeFilter.SelectedIndex > 0 Then
            If filter <> "" Then filter &= " AND "
            filter &= "ID_VoiceType = " & CStr(cmbVoiceTypeFilter.SelectedValue)
        End If

        If cmbSourceFilter.SelectedIndex > 0 Then
            If filter <> "" Then filter &= " AND "
            filter &= "ID_Source = " & CStr(cmbSourceFilter.SelectedValue)
        End If

        If cmbDrumVoiceFilter.SelectedIndex > 0 Then
            If filter <> "" Then filter &= " AND "
            If cmbDrumVoiceFilter.SelectedItem Is "Drum" Then
                filter &= "IsDrumPattern = True"
            Else
                filter &= "IsDrumPattern = False"
            End If
        End If

        If cmbPresetUserFilter.SelectedIndex > 0 Then
            If filter <> "" Then filter &= " AND "
            If cmbPresetUserFilter.SelectedItem Is "Preset" Then
                filter &= "IsPresetPattern = True"
            Else
                filter &= "IsPresetPattern = False"
            End If
        End If

        IndexView.RowFilter = filter        ' set filter like: dv.RowFilter = "Category = 'Cp' AND Bank_Name = 'GM'" 
        ShowViewCount()                     ' 'Viewing x of x Pattern'
    End Sub

    Private Sub btnSaveThis_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveThis.Click

        If CurrentPatternIndex >= PatternsToSave.Count Then
            btnSaveThis.IsEnabled = False
            Exit Sub
        End If

        If IsNothing(PatternsToSave(CurrentPatternIndex)) Then
            CurrentPatternIndex += 1
            NumPatternsToSave -= 1
            NumErrorCount += 1
            ShowSaveState()
            lblLastError.Content = "Pattern was Nothing"
            Exit Sub
        End If

        If tbName.Text = "" Then
            ValidateName()                  ' show 'invalid name'
            Exit Sub
        End If

        Dim row As DataSetIndex.DT_MainRow
        row = DSI.DT_Main.NewDT_MainRow

        '--- Category
        If cmbCategoryForSave.SelectedIndex >= 0 Then
            row.ID_Category = CInt(cmbCategoryForSave.SelectedValue)          ' use ID
        ElseIf cmbCategoryForSave.Text <> "" Then
            row.ID_Category = FindOrInsertCategory(cmbCategoryForSave.Text)
        End If

        '--- Category2
        If cmbCategory2ForSave.SelectedIndex >= 0 Then
            row.ID_Category2 = CInt(cmbCategory2ForSave.SelectedValue)          ' use ID
        ElseIf cmbCategory2ForSave.Text <> "" Then
            row.ID_Category2 = FindOrInsertCategory2(cmbCategory2ForSave.Text)
        End If

        '--- VoiceType
        If cmbVoiceTypeForSave.SelectedIndex >= 0 Then
            row.ID_VoiceType = CInt(cmbVoiceTypeForSave.SelectedValue)          ' use ID
        ElseIf cmbVoiceTypeForSave.Text <> "" Then
            row.ID_VoiceType = FindOrInsertVoiceType(cmbVoiceTypeForSave.Text)
        End If

        '--- Source
        If cmbSourceForSave.SelectedIndex >= 0 Then
            row.ID_Source = CInt(cmbSourceForSave.SelectedValue)          ' use ID
        ElseIf cmbSourceForSave.Text <> "" Then
            row.ID_Source = FindOrInsertSource(cmbSourceForSave.Text)
        End If

        '--- IsDrumPattern
        If cbIsDrum.IsChecked = True Then
            row.IsDrumPattern = True
        End If

        '--- Remarks

        If tbRemarks.Text <> "" Then
            row.Remarks = tbRemarks.Text
        End If

        '--- BPM

        If tbBPM.Text <> "" Then
            If IsNumeric(tbBPM.Text) Then
                Dim val As Double = CDbl(tbBPM.Text)
                If val >= 30 AndAlso val <= 300 Then
                    row.BPM = CUShort(Int(val))
                End If
            End If
        End If

        row.Length = CUInt(PatternsToSave(CurrentPatternIndex).Length / 960)           ' Lenght in Beats

        '--- initialize Position (for sorting)
        row.Position = CUInt(row.ID)

        row.DateTime = Now

        '--- try to save the pattern

        Dim ret As String
        ret = SavePatternToArchive(tbName.Text, PatternsToSave(CurrentPatternIndex), CBool(cbOverwriteExisting.IsChecked))

        If ret IsNot Nothing Then
            row.Pattern_Name = ret                      ' entry name in archive for index
            InsertOrOverwriteIndexRow(ret, row, CBool(cbOverwriteExisting.IsChecked))
            NumPatternsToSave -= 1
            NumPatternsSaved += 1
        Else
            NumPatternsToSave -= 1
            NumErrorCount += 1
            lblLastError.Content = "Save Pattern failed"
        End If

        ShowSaveState()                         ' update stae on screen
        ShowViewCount()
        CurrentPatternIndex += 1                ' in any case: to next pattern, if any
        If NumPatternsToSave > 0 Then
            If CurrentPatternIndex < PatternsToSave.Count Then
                If PatternsToSave(CurrentPatternIndex) IsNot Nothing Then
                    tbName.Text = PatternsToSave(CurrentPatternIndex).Name
                    ValidateName()
                End If
            End If
        End If

        '---

        'if no more patterns, disable saveButtons and cancel button and enable OK button

        If NumPatternsToSave <= 0 Then
            btnOk.IsEnabled = True
            btnCancel.IsEnabled = False
            ' Input makes no sense anymore, disable all controls in this Grid
            InputGrid.IsEnabled = False
        End If

    End Sub

    Private Sub btnSaveAll_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveAll.Click
        Beep()
    End Sub

    Private Sub btnCheckName_Click(sender As Object, e As RoutedEventArgs) Handles btnCheckName.Click
        ValidateName()
    End Sub

    Private ReadOnly NameRegexPattern As String = "^[A-z0-9-_ #()]+$"
    Private Function ValidateName() As Boolean

        If tbName.Text = "" Then
            tbName.BorderBrush = Brushes.Red
            lblNameValidationMessage.Content = "Name must not be empty."
            Return False
        End If

        Try
            Dim Regex1 As Text.RegularExpressions.Regex
            Regex1 = New Text.RegularExpressions.Regex(NameRegexPattern)

            If Regex1.IsMatch(tbName.Text) Then
                tbName.BorderBrush = Brushes.Green
                lblNameValidationMessage.Content = ""
                If cbOverwriteExisting.IsChecked = False Then
                    ' check unique name
                    CreateUniqueIndexName(tbName.Text)
                End If
                Return True
            Else
                tbName.BorderBrush = Brushes.Red
                lblNameValidationMessage.Content = "Invalid Name"
                Return False
            End If
        Catch
            tbName.BorderBrush = Brushes.Gray
            lblNameValidationMessage.Content = "Validate error"
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Show NumPatternsToSave, NumPatternsSaved, NumErrors
    ''' </summary>
    Private Sub ShowSaveState()

        If NumPatternsToSave < 2 Then
            lblNumPatternsToSave.Content = NumPatternsToSave & " Pattern to save"
        Else
            lblNumPatternsToSave.Content = NumPatternsToSave & " Patterns to save"
        End If

        If NumPatternsSaved < 2 Then
            lblNumPatternsSaved.Content = NumPatternsSaved & " Pattern saved"
        Else
            lblNumPatternsSaved.Content = NumPatternsSaved & " Patterns saved"
        End If

        If NumErrorCount = 0 Then
            lblNumSaveErors.Content = "No Error"
            lblLastError.Content = ""
        ElseIf NumErrorCount = 1 Then
            lblNumSaveErors.Content = "1 Error"
        Else
            lblNumSaveErors.Content = NumErrorCount & " Errors"
        End If


    End Sub


End Class