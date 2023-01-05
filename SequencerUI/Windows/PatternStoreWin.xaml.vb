Public Class PatternStoreWin
    ' PatternList / PatternStore as TreeView with ability to define Groups, sort, ..

    Private comp As SequencerBase.Composition
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        'comp = Module1.Sequencer.Composition
        comp = CompositionPanel1.Composition

        If comp IsNot Nothing Then
            lbPatternList.ItemsSource = comp.PatternStore
        End If

    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)

    End Sub

    Private Sub btnLoadPattern_Click(sender As Object, e As RoutedEventArgs) Handles btnLoadPattern.Click

        Dim patx As New SequencerBase.PatternX
        If SequencerBase.LoadPatternFromXML(patx) = True Then
            Sequencer.Stop_Audition()
            comp.PatternStore.Add(patx.ToPattern)
            lbPatternList.Items.Refresh()
        End If

    End Sub

    Private Sub lbPatternList_MouseMove(sender As Object, e As MouseEventArgs) Handles lbPatternList.MouseMove
        If e.LeftButton = MouseButtonState.Pressed Then
            Dim listbox = TryCast(sender, ListBox)
            If listbox IsNot Nothing Then
                If listbox.SelectedItem IsNot Nothing Then
                    Dim dataObject As New DataObject
                    dataObject.SetData(GetType(SequencerBase.Pattern), listbox.SelectedItem)     ' format as type
                    DragDrop.DoDragDrop(listbox, dataObject, DragDropEffects.Copy)
                End If
            End If
        End If
    End Sub

    Private Sub lbPatternList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lbPatternList.SelectionChanged
        If cbAudition.IsChecked = True Then
            Dim pat As SequencerBase.Pattern
            pat = TryCast(e.AddedItems(0), SequencerBase.Pattern)
            If pat IsNot Nothing Then
                Sequencer.Stop_Audition()
                If rbVoice.IsChecked = True Then
                    Sequencer.Audition.Voices(0).MidiChannel = 0
                Else
                    Sequencer.Audition.Voices(0).MidiChannel = 9
                End If
                Dim patplay = pat.Copy
                ' need to copy, else Pattern/PatternStore/Composition is changed and will ask for save at AppExit
                Sequencer.Play_Pattern(patplay, False)
            End If
        End If
    End Sub

    Private Sub cbAudition_Unchecked(sender As Object, e As RoutedEventArgs) Handles cbAudition.Unchecked
        Sequencer.Stop_Audition()
    End Sub
End Class
