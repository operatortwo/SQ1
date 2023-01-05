Public Class VoicePopup

    Private vc As SequencerBase.Voice

    Public Sub New(Voice As SequencerBase.Voice)

        ' required by the designer
        InitializeComponent()

        vc = Voice
        VoiceElements1.CompositionVoice = Voice

    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        With VoiceElements1
            .nudPortNumber.SetValueSilent(vc.PortNumber)
            .nudMidiChannel.SetValueSilent(vc.MidiChannel)
            .nudGmVoice.SetValueSilent(vc.VoiceNumberGM)
            .nudNTransp.SetValueSilent(vc.NoteTranspose)
            .nudVcMSB.SetValueSilent(vc.BankSelectMSB)
            .nudVcLSB.SetValueSilent(vc.BankSelectLSB)

            .ssldVolume.SetValueSilent(vc.Volume)
            .ssldPan.SetValueSilent(vc.Pan)

            .tbGmVoiceName.Text = SequencerBase.Get_GM_VoiceName(vc.VoiceNumberGM)


            '.nudVcNum

            '.tbVoiceName
        End With
    End Sub

    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        Close()
    End Sub
End Class
