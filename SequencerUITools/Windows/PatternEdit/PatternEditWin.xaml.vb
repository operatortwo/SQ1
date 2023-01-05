Imports System.IO
Imports System.Windows.Interop
Imports SequencerBase
Imports SequencerBase.Directplay
Imports SequencerUI

Public Class PatternEditWin
    Private WithEvents Seq As Sequencer = SequencerInstance
    Private Aud As Composition = Seq.Audition
    Private Dplay As Directplay = Seq.DPlay

    Private voice As DirectplayVoice = Dplay.Voices(2)
    Private slot As Slot = Dplay.Voices(2).Slots(0)


    Public Property Pattern1 As Pattern

    Public Sub New()
        ' this call is required for the designer
        InitializeComponent()
        Pattern1 = New Pattern                       ' start with an empty pattern
        Pattern1.Name = "New Pattern"
        Pattern1.Length = Sequencer.TicksPerBeat
        Pattern1.Duration = Pattern1.Length
    End Sub

    Public Sub New(pattern_to_edit As Pattern)
        ' this call is required for the designer
        InitializeComponent()
        Pattern1 = pattern_to_edit.Copy              ' edit a copy of the pattern
        ' decide when closing if the pattern should be replaced, disposed or saved as..
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        If Seq.IsRunning = True Then Seq.Stop_Sequencer()
        If Seq.AuditionIsRunning = True Then Seq.Stop_Audition()
        If Seq.DirectplayIsOn = False Then Seq.DirectplayIsOn = True
        slot.RingPlay = False
        tgbtnRestartAtEnd.IsChecked = slot.RingPlay
        AuditionBpmSlider.SetValueSilent(Seq.AuditionBPM)
        nudAuditionLength.SetValueSilent(Aud.Length / TicksPerBeat)

        AddHandler Module3.ScreenRefreshUITools, AddressOf ScreenRefresh

        SetupPatternEditWin()

        PatternPanel1.sldScaleX.SetValueSilent(2.0)


        'PatternPanel1.KeyPanel.ActualWidth
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        Seq.Stop_Audition()
        RemoveHandler Module3.ScreenRefreshUITools, AddressOf ScreenRefresh
    End Sub

    Public Sub ScreenRefresh()

        Dim reltime As UInteger
        reltime = Seq.AuditionTime
        lblPatternPosition.Content = TimeTo_MBT(reltime)

        'MeasureAdornerLayer.Update()

        'If Seq.AuditionIsRunning = True Then
        PatternPanel1.ScreenRefresh(Seq.AuditionTime)
        'End If

    End Sub

    Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Escape Then Close()
    End Sub

    Private Sub SetupPatternEditWin()
        Seq.Audition.Voices(0).Tracks(0).PatternList.Clear()
        Seq.Audition.Voices(0).InsertPattern(0, Pattern1)
        PatternPanel1.Voice = Seq.Audition.Voices(0)

        lblPattern1Name.Content = Pattern1.Name
        lblPattern1Length.Content = Pattern1.Length / TicksPerBeat
        lblPattern1Duration.Content = Pattern1.Duration / TicksPerBeat

        PatternPanel1.Pattern = Pattern1
        PatternPanel1.UpdatePatternPanel()
    End Sub


    Private Sub btnRestart_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnRestart.PreviewMouseDown
        Command_Restart()
    End Sub

    Private Sub btnStop_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnStop.PreviewMouseDown
        Command_Stop()
    End Sub

    Private Sub btnPlay_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnPlay.PreviewMouseDown
        Command_Play()
    End Sub


    Friend Sub Command_Restart()
        Seq.Set_AuditionTime(0)
    End Sub
    Friend Sub Command_Play()
        btnRestart.IsEnabled = False
        btnStop.IsEnabled = True
        btnPlay.IsEnabled = False

        Seq.Start_Audition()
    End Sub
    Friend Sub Command_Stop()
        btnStop.IsEnabled = False
        btnPlay.IsEnabled = True
        btnRestart.IsEnabled = True

        Seq.Stop_Audition()
    End Sub

    Private Sub tgbtnRestartAtEnd_Checked(sender As Object, e As RoutedEventArgs) Handles tgbtnRestartAtEnd.Checked
        Aud.RestartAtEnd = True
    End Sub

    Private Sub tgbtnRestartAtEnd_Unchecked(sender As Object, e As RoutedEventArgs) Handles tgbtnRestartAtEnd.Unchecked
        Aud.RestartAtEnd = False
    End Sub

    Public Delegate Sub Command_Delegate()
    Private Sub AuditionAtEnd() Handles Seq.Play_Audition_EndReached
        Me.Dispatcher.Invoke(New Command_Delegate(AddressOf Command_Stop))
    End Sub

    Private Sub MiFile_LoadPattern_Click(sender As Object, e As RoutedEventArgs) Handles MiFile_LoadPattern.Click
        If Seq.AuditionIsRunning = True Then
            Command_Stop()
            Command_Restart()
        End If

        Dim ret As Boolean
        Dim lopat As New LoadPatternDialog
        lopat.Multiselect = False
        ret = lopat.ShowDialog(Me)

        If ret = True Then
            Pattern1 = lopat.LoadedPattern
            SetupPatternEditWin()
        End If
    End Sub

    Private Sub MiFile_ImportPattern_Click(sender As Object, e As RoutedEventArgs) Handles MiFile_ImportPattern.Click
        If Seq.AuditionIsRunning = True Then
            Command_Stop()
            Command_Restart()
        End If

        Dim pat As New PatternX
        If LoadPatternFromXML(pat) = True Then
            Pattern1 = pat.ToPattern
            SetupPatternEditWin()
        End If
    End Sub

    Private Sub MiFile_Exit_Click(sender As Object, e As RoutedEventArgs) Handles MiFile_Exit.Click
        Close()
    End Sub

    Private Sub AuditionBpmSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles AuditionBpmSlider.ValueChanged
        If IsLoaded = True Then
            Seq.AuditionBPM = e.NewValue
        End If
    End Sub

    Private Sub nudAuditionLength_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudAuditionLength.ValueChanged
        Aud.Length = nudAuditionLength.Value * TicksPerBeat
    End Sub

    Private Sub rbVoice_Checked(sender As Object, e As RoutedEventArgs) Handles rbVoice.Checked
        Aud.Voices(0).MidiChannel = 0
    End Sub

    Private Sub rbDrum_Checked(sender As Object, e As RoutedEventArgs) Handles rbDrum.Checked
        Aud.Voices(0).MidiChannel = 9
    End Sub

    Private Sub btnVc0_Click(sender As Object, e As RoutedEventArgs) Handles btnVc0.Click
        Dim dlg As New VoicePopup(Aud.Voices(0))
        dlg.Owner = Me
        dlg.ShowDialog()
    End Sub


End Class
