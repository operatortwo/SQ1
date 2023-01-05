Imports System.IO

Public Class DebugMidiOutList
    Private MainWin As MainWindow = Nothing
    Public Sub New(MainWindow As MainWindow)
        InitializeComponent()
        MainWin = MainWindow
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        'MainWin.DebugMidiOutList
        'DataGrid1.DataContext = MainWin.DebugMidiOutList

        DataGrid1.DataContext = MainWin.Sequencer.DebugEventOutList
        Dim x As Integer = MainWin.Sequencer.DebugEventOutList.Count

    End Sub

    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        Me.Close()

    End Sub

    Private Sub DataGrid1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles DataGrid1.SelectionChanged
        If cbPlayEvents.IsChecked = True Then
            Dim dg As DataGrid
            dg = TryCast(sender, DataGrid)
            If dg IsNot Nothing Then

                Dim ev As SequencerBase.Module1.PortTrackEvent
                ev = TryCast(dg.SelectedItem, SequencerBase.Module1.PortTrackEvent)
                If ev IsNot Nothing Then
                    SequencerBase.Module1.DebugMidiOutShort(ev.PortNumber, 0, ev.Status, ev.Data1, ev.Data2)
                End If
            End If
        End If
    End Sub

    Private Sub btnSaveData_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveData.Click
        Dim list = MainWin.Sequencer.DebugEventOutList

        Dim fs As FileStream
        Try
            fs = New FileStream("DebugMidiOut.xml", FileMode.Create)      'create or truncate / write        
            Dim seria As New Xml.Serialization.XmlSerializer(MainWin.Sequencer.DebugEventOutList.GetType)
            seria.Serialize(fs, MainWin.Sequencer.DebugEventOutList)
            MessageBox.Show("XML was saved", "EventOutList save as XML", MessageBoxButton.OK)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "EventOutList save as XML", MessageBoxButton.OK)
        Finally
#Disable Warning BC42104 ' Variable is used before it has been assigned a value
            If fs IsNot Nothing Then fs.Close()
#Enable Warning BC42104 ' Variable is used before it has been assigned a value
        End Try
    End Sub
End Class
