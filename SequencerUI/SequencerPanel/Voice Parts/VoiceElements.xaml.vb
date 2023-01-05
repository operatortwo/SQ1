Public Class VoiceElements
    Private Const GridSplitterHeight = 6

    Friend Property CompositionVoice As SequencerBase.Voice             ' same as in VoicePanel

    Public Sub New()

        ' Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent()

        ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

    End Sub

#Region "Grid Splitter"
    Private Sub GridSplitter1_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles GridSplitter1.PreviewMouseLeftButtonDown
        Dim el As UIElement = CType(sender, UIElement)
        el.CaptureMouse()

        'Dim curpos As Point
        'curpos = e.GetPosition(Me)              ' get current (rel) position

        ' capture will raise MouseMove, set initial values before
    End Sub

    Private Sub GridSplitter1_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles GridSplitter1.PreviewMouseLeftButtonUp
        Dim el As UIElement = CType(sender, UIElement)
        If el.IsMouseCaptured Then
            el.ReleaseMouseCapture()
        End If
    End Sub

    Private Sub GridSplitter1_PreviewMouseMove(sender As Object, e As MouseEventArgs) Handles GridSplitter1.PreviewMouseMove
        Dim el As UIElement = CType(sender, UIElement)
        If el.IsMouseCaptured Then

            If e.LeftButton = MouseButtonState.Pressed Then

                Dim pt As Point
                pt = e.GetPosition(Me)

                If pt.Y < GridSplitterHeight Then pt.Y = GridSplitterHeight

                If pt.Y > MaxHeight Then pt.Y = MaxHeight       ' defined in (Voice) UserControl property
                Me.Height = pt.Y

            End If

        End If
    End Sub


#End Region


    Private Sub nudPortNumber_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudPortNumber.ValueChanged
        If CompositionVoice IsNot Nothing Then
            If SkipChangedEvent = False Then
                CompositionVoice.PortNumber = CByte(e.NewValue)
            End If
        End If
    End Sub
    Private Sub nudMidiChannel_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudMidiChannel.ValueChanged
        If CompositionVoice IsNot Nothing Then
            If SkipChangedEvent = False Then
                CompositionVoice.MidiChannel = CByte(e.NewValue)
            End If
        End If
    End Sub

    Private Sub nudGmVoice_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudGmVoice.ValueChanged
        If CompositionVoice IsNot Nothing Then
            If SkipChangedEvent = False Then
                CompositionVoice.VoiceNumberGM = CByte(e.NewValue)
                PlayEvent(&HC0, CByte(e.NewValue), 0)                   ' Program Change
                tbGmVoiceName.Text = SequencerBase.Get_GM_VoiceName(CByte(e.NewValue))
            End If
        End If
    End Sub

    Private Sub nudNTransp_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudNTransp.ValueChanged
        If CompositionVoice IsNot Nothing Then
            CompositionVoice.NoteTranspose = CShort(e.NewValue)
        End If
    End Sub

    Private Sub nudVcMSB_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudVcMSB.ValueChanged
        If CompositionVoice IsNot Nothing Then
            If SkipChangedEvent = False Then
                CompositionVoice.BankSelectMSB = CByte(e.NewValue)
            End If
        End If
    End Sub

    Private Sub nudVcLSB_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudVcLSB.ValueChanged
        If CompositionVoice IsNot Nothing Then
            If SkipChangedEvent = False Then
                CompositionVoice.BankSelectLSB = CByte(e.NewValue)
            End If
        End If
    End Sub

    Private Sub nudVcNum_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudVcNum.ValueChanged
        If CompositionVoice IsNot Nothing Then
            If SkipChangedEvent = False Then
                CompositionVoice.VoiceNumber = CByte(e.NewValue)
            End If
        End If
    End Sub
    Private Sub ssldVolume_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles ssldVolume.ValueChanged
        If CompositionVoice IsNot Nothing Then
            If SkipChangedEvent = False Then
                CompositionVoice.Volume = CByte(e.NewValue)
                PlayEvent(&HB0, 7, CByte(e.NewValue))           ' Channel volume coarse
            End If
        End If
    End Sub

    Private Sub ssldPan_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles ssldPan.ValueChanged
        If CompositionVoice IsNot Nothing Then
            If SkipChangedEvent = False Then
                CompositionVoice.Pan = CByte(e.NewValue)
                PlayEvent(&HB0, 10, CByte(e.NewValue))          ' Panorama MSB
            End If
        End If

    End Sub

    Private Sub btnTap_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles btnTap.PreviewMouseLeftButtonDown
        PlayEvent(&H90, &H40, 100)
    End Sub

    Private Sub btnTap_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles btnTap.PreviewMouseLeftButtonUp
        'PlayEvent(&H90, &H40, 0)
        CompositionVoice.AllRunningNotesOff(CUInt(Sequencer.SequencerTime))
    End Sub

    Private Sub PlayEvent(status As Byte, data1 As Byte, data2 As Byte)
        Dim vc As SequencerBase.Voice = CompositionVoice
        'vc.PlaySingleNote(CUInt(Sequencer.SequencerTime), vc.MidiChannel, &H40, 100, 480)
        Dim tev As New SequencerBase.TrackEvent
        tev.Type = SequencerBase.EventType.MidiEvent
        tev.Time = CUInt(Sequencer.SequencerTime)
        tev.Status = CByte(status Or vc.MidiChannel)
        tev.Data1 = data1
        tev.Data2 = data2
        vc.PlayEvent(CUInt(Sequencer.SequencerTime), CUInt(Sequencer.SequencerTime), tev)
    End Sub


End Class
