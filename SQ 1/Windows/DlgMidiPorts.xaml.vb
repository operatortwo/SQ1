
Public Class DlgMidiPorts

    Private MainWin As MainWindow
    Private outportlist As New List(Of String)

    Public Sub New(MainWindow As MainWindow)
        InitializeComponent()
        MainWin = MainWindow
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        tbOutPort0.Text = MainWin.MidiOutPort0_preferred
        tbOutPort1.Text = MainWin.MidiOutPort1_preferred
        tbOutPort2.Text = MainWin.MidiOutPort2_preferred
        tbOutPort3.Text = MainWin.MidiOutPort3_preferred

        For i = 1 To MainWin.MIO.MidiOutPorts.Count
            If MainWin.MIO.MidiOutPorts(i - 1).invalidPort = False Then          'list only valid devices
                outportlist.Add(MainWin.MIO.MidiOutPorts(i - 1).portName)
            End If
        Next

        cmbOutPort0.Items.Add("")
        For Each item In outportlist
            cmbOutPort0.Items.Add(item)
        Next
        cmbOutPort0.SelectedItem = MainWin.MidiOutPort0_selected

        cmbOutPort1.Items.Add("")
        For Each item In outportlist
            cmbOutPort1.Items.Add(item)
        Next
        cmbOutPort1.SelectedItem = MainWin.MidiOutPort1_selected

        cmbOutPort2.Items.Add("")
        For Each item In outportlist
            cmbOutPort2.Items.Add(item)
        Next
        cmbOutPort2.SelectedItem = MainWin.MidiOutPort2_selected

        cmbOutPort3.Items.Add("")
        For Each item In outportlist
            cmbOutPort3.Items.Add(item)
        Next
        cmbOutPort3.SelectedItem = MainWin.MidiOutPort3_selected


        cmbAlternativeOutPort.Items.Add("")
        cmbAlternativeOutPort.Items.Add("first available")
        For Each item In outportlist
            cmbAlternativeOutPort.Items.Add(item)
        Next
        cmbAlternativeOutPort.SelectedItem = MainWin.AlternativeMidiOutPort

    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)

    End Sub


    Private Sub btnOutPort0_asPreferred_Click(sender As Object, e As RoutedEventArgs) Handles btnOutPort0_asPreferred.Click
        tbOutPort0.Text = CStr(cmbOutPort0.SelectedItem)
    End Sub

    Private Sub btnOutPort1_asPreferred_Click(sender As Object, e As RoutedEventArgs) Handles btnOutPort1_asPreferred.Click
        tbOutPort1.Text = CStr(cmbOutPort1.SelectedItem)
    End Sub

    Private Sub btnOutPort2_asPreferred_Click(sender As Object, e As RoutedEventArgs) Handles btnOutPort2_asPreferred.Click
        tbOutPort2.Text = CStr(cmbOutPort2.SelectedItem)
    End Sub

    Private Sub btnOutPort3_asPreferred_Click(sender As Object, e As RoutedEventArgs) Handles btnOutPort3_asPreferred.Click
        tbOutPort3.Text = CStr(cmbOutPort3.SelectedItem)
    End Sub
    Private Sub btnOk_Click(sender As Object, e As RoutedEventArgs) Handles btnOk.Click

        MainWin.MidiOutPort0_preferred = tbOutPort0.Text
        MainWin.MidiOutPort1_preferred = tbOutPort1.Text
        MainWin.MidiOutPort2_preferred = tbOutPort2.Text
        MainWin.MidiOutPort3_preferred = tbOutPort3.Text

        Dim IsUpdated As Boolean

        If MainWin.MidiOutPort0_selected <> CStr(cmbOutPort0.SelectedItem) Then
            MainWin.MidiOutPort0_selected = CStr(cmbOutPort0.SelectedItem)
            IsUpdated = True
        End If

        If MainWin.MidiOutPort1_selected = CStr(cmbOutPort1.SelectedItem) Then
            MainWin.MidiOutPort1_selected = CStr(cmbOutPort1.SelectedItem)
            IsUpdated = True
        End If

        If MainWin.MidiOutPort2_selected = CStr(cmbOutPort2.SelectedItem) Then
            MainWin.MidiOutPort2_selected = CStr(cmbOutPort2.SelectedItem)
            IsUpdated = True
        End If

        If MainWin.MidiOutPort3_selected = CStr(cmbOutPort3.SelectedItem) Then
            MainWin.MidiOutPort3_selected = CStr(cmbOutPort3.SelectedItem)
            IsUpdated = True
        End If

        MainWin.AlternativeMidiOutPort = CStr(cmbAlternativeOutPort.SelectedItem)

        If IsUpdated = True Then
            MainWin.UpdateMidiOutPorts()
        End If

        Me.Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub



End Class
