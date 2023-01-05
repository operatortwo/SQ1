Imports System.Collections.ObjectModel
Imports System.Text
Imports SequencerUI.TrackCanvas

Public Class TrackViewSettings

    Private trkel As TrackElement

    'Public Property NoteList As New List(Of NoteItem)
    Public Property NoteList As New ObservableCollection(Of NoteItem)

    Public Class NoteItem
        Inherits ListBoxItem

        Public Property NoteNumber As Integer
        Public Property NoteName As String = ""
        Public Property IsLocked As Boolean              ' always selected, can not be unselected
    End Class

    Public Sub New(TrkElement As TrackElement)
        ' required by the designer
        InitializeComponent()


        trkel = TrkElement

        lbSelectedNotes.ItemsSource = NoteList

    End Sub


    ''' <summary>
    ''' If IsVoice = TRUE thenthe List ist filled with Voice Names, else it is filled with Drum Names
    ''' </summary>
    ''' <param name="IsVoice"></param>
    Private Sub FillLbSelectedNotes(IsVoice As Boolean)


        'lbSelectedNotes.ItemsSource = Nothing
        NoteList.Clear()

        '---- test
        Dim list As New Dictionary(Of Integer, String)
        'list.Clear()
        'list = SequencerBase.GetNoteNameList_88Keys(False, False)
        'list.Clear()
        'list = SequencerBase.GetNoteNameList_88Keys(True, False)
        'list.Clear()
        'list = SequencerBase.GetNoteNameList_88Keys(False, True)
        'list.Clear()
        'list = SequencerBase.GetNoteNameList_88Keys(True, True)
        'list.Clear()
        'list = SequencerBase.Get_GM_DrumVoiceNameList_Map(False, False)
        'list.Clear()
        'list = SequencerBase.Get_GM_DrumVoiceNameList_Map(True, False)
        'list.Clear()
        'list = SequencerBase.Get_GM_DrumVoiceNameList_Map(True, True)
        'list.Clear()
        '--------
        '   Trigger Property=IsMouseOver Value=TRUE
        '       Setter Property=Foreground Value=Blue
        '   Trigger
        '             
        '-------

        'list = SequencerBase.GetNoteNameList_88Keys(True, True)
        'list = SequencerBase.Get_GM_DrumVoiceNameList(30, 40, True, True)


        If IsVoice = True Then
            list = SequencerBase.GetNoteNameList_88Keys(True, True)
        Else
            list = SequencerBase.Get_GM_DrumVoiceNameList_Map(True, True)
        End If


        'list = SequencerBase.GetNoteNameList_88Keys(True, True)

        For Each item In list
            'For Each item In SequencerBase.GM_DrumVoiceNames.Reverse
            Dim ni As New NoteItem
            ni.NoteNumber = item.Key
            ni.NoteName = item.Value
            ni.Content = item.Value
            ni.HorizontalContentAlignment = HorizontalAlignment.Left        ' avoid Binding errors !
            ni.VerticalContentAlignment = VerticalAlignment.Top             ' avoid Binding errors !

            If trkel.TrackCanvas.ListOfUsedNotes.Contains(CByte(item.Key)) Then
                ni.IsLocked = True
                ni.IsSelected = True
                ni.IsEnabled = False
            End If
            NoteList.Add(ni)
        Next

        'lbSelectedNotes.ItemsSource = NoteList


        'lbSelectedNotes.DisplayMemberPath = "NoteName"

        'For Each item As NoteItem In lbSelectedNotes.Items
        '    If item.IsLocked Then
        '        lbSelectedNotes.SelectedItems.Add(item)
        '    End If
        'Next


        'lbSelectedNotes.ItemsSource = NoteList
        'lbSelectedNotes.DisplayMemberPath = "NoteName"

        'lbSelectedNotes.ItemsSource = SequencerBase.NoteNameList.Reverse
        'lbSelectedNotes.ItemsSource = SequencerBase.GM_DrumVoiceNames.Reverse
        'lbSelectedNotes.DisplayMemberPath = "Key"
        'lbSelectedNotes.DisplayMemberPath = "Value"


        'lbSelectedNotes.SelectedItems.Add(lbSelectedNotes.Items(1))


        'lvSelectedNotes.ItemContainerGenerator.ContainerFromIndex(1)


        'For Each item In SequencerBase.NoteNameList.Reverse
        '    Dim lbi As New ListBoxItem
        '    lbi.Content = item.Value
        '    lbSelectedNotes.Items.Add(lbi)
        'Next

        'For i = 1 To lbSelectedNotes.Items.Count
        '    If i > 5 AndAlso i < 10 Then
        '        'lbi = CType(lbSelectedNotes.ItemContainerGenerator.ContainerFromIndex(i - 1), ListBoxItem)
        '        'lbi.IsSelected = True
        '        Dim obj As ListBoxItem = CType(lbSelectedNotes.Items(i - 1), ListBoxItem)
        '        obj.IsSelected = True
        '        obj.IsEnabled = False
        '        obj.Background = Brushes.YellowGreen
        '    End If
        'Next


    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        InitializeWindow()





        'Dim obj As Object = lvSelectedNotes.ItemContainerGenerator.ContainerFromIndex(5)

        'obj.IsSelected = False
        'obj.Background = Brushes.Yellow
        'obj.IsEnabled = False

    End Sub

    Private Sub InitializeWindow()

        Dim sb As New StringBuilder

        trkel.TrackCanvas.AnalyzeTrack()

        Dim tc As TrackCanvas = trkel.TrackCanvas

        sb.Append("Number of Notes = " & tc.ListOfUsedNotes.Count & vbCrLf)

        If tc.ListOfUsedNotes.Count > 1 Then
            Dim FirstNote As Byte = tc.ListOfUsedNotes.First
            Dim LastNote As Byte = tc.ListOfUsedNotes.Last
            sb.Append("Note Range: " & FirstNote & " - " & LastNote & vbCrLf)
        End If

        sb.Append("NoteNumbers: ")
        For Each note In tc.ListOfUsedNotes
            sb.AppendFormat(" {0}", note)
        Next
        sb.Append(vbCrLf)

        sb.Append("NoteNames: ")
        For Each note In tc.ListOfUsedNotes
            sb.AppendFormat(SequencerBase.NoteNumber_to_NoteName(note) & " ")
        Next
        sb.Append(vbCrLf)

        tbMessage.AppendText(sb.ToString)                ' to TextBox

        '--------

        If trkel.TrackCanvas.TrackView = TrackCanvas.TrackViewType.Pattern Then
            rbPattern.IsChecked = True
        ElseIf trkel.TrackCanvas.TrackView = TrackCanvas.TrackViewType.VoiceNotes Then
            rbVoiceNotes.IsChecked = True
            rbNoteRange.IsChecked = True
        ElseIf trkel.TrackCanvas.TrackView = TrackCanvas.TrackViewType.DrumVoiceNotes Then
            rbDrumVoiceNotes.IsChecked = True
            rbSelectedNotes.IsChecked = True
        End If

        'nudNoteRangeStart.MaximumValue = tc.ListOfUsedNotes.First
        'nudNoteRangeStart.Value = tc.ListOfUsedNotes.First
        nudNoteRangeStart.Value = tc.ListOfUsedNotes.FirstOrDefault

        'nudNoteRangeEnd.MinimumValue = tc.ListOfUsedNotes.Last
        'nudNoteRangeEnd.Value = tc.ListOfUsedNotes.Last
        nudNoteRangeEnd.Value = tc.ListOfUsedNotes.LastOrDefault


        '--- selected notes

        'lbSelectedNotes.Items.Clear()

        'Dim lbi As ListBoxItem

        'For i = 127 To 0 Step -1
        '    lbi = New ListBoxItem
        '    lbi.Content = i
        '    lbSelectedNotes.Items.Add(lbi)
        'Next


        For Each item In lbSelectedNotes.Items



        Next




        'For Each note In tc.ListOfUsedNotes
        '    lbi = New ListBoxItem
        '    lbi.Content = note
        '    lbi.IsSelected = True
        '    lbi.IsEnabled = False
        '    lbSelectedNotes.Items.Add(lbi)
        'Next





    End Sub

    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click

        'trkel.TrackView = RadioButtonSelection         ' automatically  TrackCanvas.DrawTrack  

        'Dim lst As IList = lbSelectedNotes.SelectedItems



        If rbPattern.IsChecked = True Then
            trkel.TrackCanvas.TrackView = TrackViewType.Pattern
        ElseIf rbVoiceNotes.IsChecked = True Then
            trkel.TrackCanvas.TrackView = TrackViewType.VoiceNotes
        ElseIf rbDrumVoiceNotes.IsChecked = True Then
            trkel.TrackCanvas.TrackView = TrackViewType.DrumVoiceNotes
        End If

        trkel.TrackCanvas.DrawTrack()

        Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click

    End Sub

    Private Sub rbPattern_Checked(sender As Object, e As RoutedEventArgs) Handles rbPattern.Checked
        GroupBoxNotes.IsEnabled = False
        'GroupBoxNoteRange.IsEnabled = False
    End Sub

    Private Sub rbVoiceNotes_Checked(sender As Object, e As RoutedEventArgs) Handles rbVoiceNotes.Checked
        GroupBoxNotes.IsEnabled = True
        If rbNoteRange.IsChecked = True Then
            GroupBoxNoteRange.IsEnabled = True
            lbSelectedNotes.IsEnabled = False
        Else
            GroupBoxNoteRange.IsEnabled = False
            lbSelectedNotes.IsEnabled = True
        End If

        FillLbSelectedNotes(True)
    End Sub

    Private Sub rbDrumVoiceNotes_Checked(sender As Object, e As RoutedEventArgs) Handles rbDrumVoiceNotes.Checked
        GroupBoxNotes.IsEnabled = True
        If rbNoteRange.IsChecked = True Then
            GroupBoxNoteRange.IsEnabled = True
            lbSelectedNotes.IsEnabled = False
        Else
            GroupBoxNoteRange.IsEnabled = False
            lbSelectedNotes.IsEnabled = True
        End If

        FillLbSelectedNotes(False)
    End Sub

    Private Sub nudNoteRangeEnd_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        nudNoteRangeStart.MaximumValue = Math.Max(nudNoteRangeStart.Value, nudNoteRangeEnd.Value)
        nudNoteRangeEnd.MinimumValue = Math.Min(nudNoteRangeStart.Value, nudNoteRangeEnd.Value)
        lblNoteRangeEnd.Content = SequencerBase.NoteNumber_to_NoteName(CByte(e.NewValue))
    End Sub
    Private Sub nudNoteRangeStart_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        nudNoteRangeStart.MaximumValue = Math.Max(nudNoteRangeStart.Value, nudNoteRangeEnd.Value)
        nudNoteRangeEnd.MinimumValue = Math.Min(nudNoteRangeStart.Value, nudNoteRangeEnd.Value)
        lblNoteRangeStart.Content = SequencerBase.NoteNumber_to_NoteName(CByte(e.NewValue))
    End Sub

    Private Sub GroupBoxNotes_IsEnabledChanged(sender As Object, e As DependencyPropertyChangedEventArgs) Handles GroupBoxNotes.IsEnabledChanged
        If GroupBoxNotes.IsEnabled = True Then
            If rbNoteRange.IsChecked = True Then
                GroupBoxNoteRange.IsEnabled = True
                lbSelectedNotes.IsEnabled = False
            Else
                GroupBoxNoteRange.IsEnabled = False
                lbSelectedNotes.IsEnabled = True
            End If
        Else
            GroupBoxNoteRange.IsEnabled = False
            lbSelectedNotes.IsEnabled = False
        End If
    End Sub

    Private Sub rbNoteRange_Checked(sender As Object, e As RoutedEventArgs) Handles rbNoteRange.Checked
        GroupBoxNoteRange.IsEnabled = True
        lbSelectedNotes.IsEnabled = False
    End Sub

    Private Sub rbSelectedNotes_Checked(sender As Object, e As RoutedEventArgs) Handles rbSelectedNotes.Checked
        GroupBoxNoteRange.IsEnabled = False
        lbSelectedNotes.IsEnabled = True
        lbSelectedNotes.Focus()
    End Sub


End Class
