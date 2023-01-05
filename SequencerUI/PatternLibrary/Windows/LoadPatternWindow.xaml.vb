Imports System.Data
Imports SequencerBase

Public Class LoadPatternWindow
    Public Property Multiselect As Boolean
    Public Property LoadedPattern As Pattern
    Public Property LoadedPatterns As List(Of Pattern)


    Public IndexView As DataView            ' for filtering / sorting
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        ' set local format for Datagrid DateTime column     --> already made in MainWindow
        ' LanguageProperty.OverrideMetadata(GetType(FrameworkElement), New FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)))

        Dim odp As ObjectDataProvider = CType(TryFindResource("Dsi1"), ObjectDataProvider)
        If odp IsNot Nothing Then
            odp.ObjectInstance = DSI
        End If

        IndexView = DSI.DT_Main.DefaultView

        If Multiselect = True Then
            DgMain.SelectionMode = DataGridSelectionMode.Extended
        Else
            DgMain.SelectionMode = DataGridSelectionMode.Single
        End If

        '--- Filter

        FillFilterBoxes()
        ResetAllFilters()

        '--- StatusBar ---

        If Multiselect = True Then
            sbiSelectMode.Content = "MultiSelect Mode"
        Else
            sbiSelectMode.Content = "SingleSelect Mode"
        End If


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
        ReturnSelectedItems()
    End Sub

    Private Sub DgMain_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgMain.MouseDoubleClick
        ReturnSelectedItems()
    End Sub

    Private Sub ReturnSelectedItems()
        Dim drv As DataRowView
        Dim row As DataSetIndex.DT_MainRow
        Dim arr As New ArrayList

        For Each item In DgMain.SelectedItems
            drv = TryCast(item, DataRowView)
            If drv IsNot Nothing Then
                row = TryCast(drv.Row, DataSetIndex.DT_MainRow)
                If row IsNot Nothing Then
                    arr.Add(row.Pattern_Name)
                End If
            End If
        Next

        If arr.Count > 0 Then
            If Multiselect = False Then
                LoadedPattern = GetPatternFromArchive(CStr(arr(0)))
                If LoadedPattern IsNot Nothing Then
                    DialogResult = True                             ' a pattern was loaded
                Else
                    DialogResult = False                            ' something went wrong
                End If
            Else
                LoadedPatterns = GetPatternsFromArchive(arr)
                If LoadedPatterns IsNot Nothing Then
                    DialogResult = True                             ' one ore more patterns were loaded
                Else
                    DialogResult = False                            ' something went wrong
                End If
            End If
        Else
            DialogResult = False                                    ' no archive names
        End If

        Close()
    End Sub

    Private Sub DgMain_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles DgMain.SelectionChanged
        sbiSelectionCount.Content = DgMain.SelectedItems.Count
    End Sub

    Private Sub ShowViewCount()
        sbiViewCount.Content = IndexView.Count
        sbiTotalCount.Content = IndexView.Table.Rows.Count
    End Sub

    Private Sub btnResetAllFilters_Click(sender As Object, e As RoutedEventArgs) Handles btnResetAllFilters.Click
        ResetAllFilters()
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


End Class

