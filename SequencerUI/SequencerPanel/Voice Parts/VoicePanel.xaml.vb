Public Class VoicePanel

    Public Property CompositionVoice As SequencerBase.Voice
    Public Property Sibling As TrackPanel                        ' sibling
    ''' <summary>
    ''' Voice Number in Composition.Voices
    ''' </summary>
    ''' <returns></returns>
    Public Property VoiceNumber As Integer

    Private SeqPanel As SequencerPanel

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        Dim obj As Object = FindLogicalParent(Me, GetType(SequencerPanel))
        SeqPanel = TryCast(obj, SequencerPanel)
    End Sub

    Private Sub tbVoiceTitle_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbVoiceTitle.TextChanged
        If CompositionVoice IsNot Nothing Then
            If SkipChangedEvent = False Then
                Dim srcTextBox As TextBox
                srcTextBox = CType(e.Source, TextBox)
                CompositionVoice.Title = srcTextBox.Text
            End If
        End If

    End Sub

    Private Sub tgbtnMute_Checked(sender As Object, e As RoutedEventArgs) Handles tgbtnMute.Checked
        If CompositionVoice IsNot Nothing Then
            CompositionVoice.Mute = True
        End If
    End Sub

    Private Sub tgbtnMute_Unchecked(sender As Object, e As RoutedEventArgs) Handles tgbtnMute.Unchecked
        CompositionVoice.Mute = False
    End Sub

    Private Sub UserControl_SizeChanged(sender As Object, e As SizeChangedEventArgs)
        If e.HeightChanged Then

            If Sibling IsNot Nothing Then
                Sibling.Height = e.NewSize.Height
            End If

        End If
    End Sub

    Private Sub ctxMi_DeleteVoice_Click(sender As Object, e As RoutedEventArgs) Handles ctxMi_DeleteVoice.Click
        If MessageBox.Show("Delete this Voice and dispose all of it's Data ?", "Delete Voice",
                       MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                       MessageBoxResult.Cancel) = MessageBoxResult.Yes Then

            Dim voiceNumber As Integer = Me.VoiceNumber
            'Dim comp As SequencerBase.Composition = Sequencer.Composition
            Dim comp As SequencerBase.Composition = SeqPanel.Composition

            If Sibling IsNot Nothing Then
                Me.Sibling.TrackElementStack.Children.Clear()
            End If
            'SequencerPanel1.TrackPanelStack.UpdateLayout()
            SeqPanel.TrackPanelStack.UpdateLayout()

            If comp IsNot Nothing Then
                    If voiceNumber < comp.Voices.Count Then
                        comp.Voices.RemoveAt(voiceNumber)

                        Dim stk As StackPanel = TryCast(Me.Parent, StackPanel)
                        If stk IsNot Nothing Then
                            If stk.Children.Contains(Me) Then
                                stk.Children.Remove(Me)
                                stk.UpdateLayout()
                            End If
                        End If

                    End If
                End If
            End If

    End Sub


End Class
