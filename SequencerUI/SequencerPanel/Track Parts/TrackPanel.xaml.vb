Public Class TrackPanel

    Public Property CompositionVoice As SequencerBase.Voice

    Public Property sibling As VoicePanel

    Private SeqPanel As SequencerPanel

    Public Sub New()

        InitializeComponent()

        TrackElementStack.Children.Clear()         ' removed TrackElements Control needed in design-time
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        Dim obj As Object = FindLogicalParent(Me, GetType(SequencerPanel))
        SeqPanel = TryCast(obj, SequencerPanel)
    End Sub

    Private Sub ScrollViewer_ScrollChanged(sender As Object, e As ScrollChangedEventArgs)
        e.Handled = True
    End Sub

    Private Sub ctxMi_AddTrack_Click(sender As Object, e As RoutedEventArgs) Handles ctxMi_AddTrack.Click
        If CompositionVoice IsNot Nothing Then

            Dim trk As New SequencerBase.Track
            trk.Title = CStr(CompositionVoice.Tracks.Count)
            CompositionVoice.Tracks.Add(trk)

            Dim trkel As New TrackElement
            trkel.CompositionTrack = trk
            trkel.tbTrackTitle.Text = trk.Title

            'trkel.TrackCanvas.Composition = Sequencer.Composition
            'trkel.TrackCanvas.Length = Sequencer.Composition.Length
            trkel.TrackCanvas.Composition = SeqPanel.Composition
            trkel.TrackCanvas.Length = SeqPanel.Composition.Length

            trkel.TrackCanvas.VoiceNumber = Me.sibling.VoiceNumber
            trkel.TrackCanvas.TrackNumber = CompositionVoice.Tracks.Count - 1       ' track was alreay added

            'trkel.TrackCanvas.ScaleX = SequencerPanel1.TracksFooter.sldScaleX.Value
            trkel.TrackCanvas.ScaleX = SeqPanel.TracksFooter.sldScaleX.Value

            'trkel.TrackRowHeader.Width = 100
            trkel.LeftPart.Width = SeqPanel.TracksHeader.RowHeader.ActualWidth

            Me.TrackElementStack.Children.Add(trkel)


            Me.UpdateLayout()

        End If
    End Sub


End Class
