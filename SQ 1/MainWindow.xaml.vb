Imports SequencerBase
Imports SequencerBase.Module1
Imports SequencerBase.Sequencer
Imports SequencerUI
Imports SequencerUITools

Class MainWindow

    Public WithEvents Sequencer As New SequencerBase.Sequencer

    Public Property WindowComposition As New SequencerUI.CompositionWin
    Public Property WindowPatternStore As New SequencerUI.PatternStoreWin
    Public Property WindowSections As New SequencerUI.SectionsWin
    Public Property WindowArrange As New SequencerUI.ArrangeWin

    Public Shared ScreenRefreshTimer As New Timers.Timer(50)       ' 80 ms Screen Timer (= 12.5 FPS)
    ' smooth scrolling of position Adorner needs at least 20 FPS (50 ms)

    Public WithEvents Mkbd As New Midi_Keyboard.Main()      'screen Keyboard
    Private DebugWindow1 As DebugWindow
    Private RemoteControl1 As RemoteControl

    Private WindowTitle_Base As String

    Private Const DefaultNameForNewComposition = "New Composition.xml"


    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        '--- make sure the windows will be closed on exit. SidePanel will also set owner of Associated Windows
        WindowComposition.Owner = Me
        WindowPatternStore.Owner = Me
        WindowSections.Owner = Me
        WindowArrange.Owner = Me

        SequencerUI.Sequencer = Sequencer               ' assign Sequencer Instance

        WindowTitle_Base = Me.Title

        '--- in case the Application Version has changed, Upgrade the settings ---

        If My.Settings.UpgradeRequired = True Then
            My.Settings.Upgrade()
            My.Settings.UpgradeRequired = False
            My.Settings.Save()
        End If

        '---
        LastPatternDirectory = My.Settings.LastPatternDirectory

        '--- try to set Last Midi Output ports

        MidiOutPort0_preferred = My.Settings.LastMidiOutPort0
        MidiOutPort1_preferred = My.Settings.LastMidiOutPort1
        MidiOutPort2_preferred = My.Settings.LastMidiOutPort2
        MidiOutPort3_preferred = My.Settings.LastMidiOutPort3
        AlternativeMidiOutPort = My.Settings.LastAlternativeMidiOutPort

        '--- for first start
        If MidiOutPort0_preferred = "" Then MidiOutPort0_preferred = "init"
        If AlternativeMidiOutPort = "" Then AlternativeMidiOutPort = "first available"
        '---

        OpenMidiOutPorts()

        '---

        CreateAuditionTestData()
        Dim vc1 As New SequencerBase.Voice
        vc1.Tracks.Add(New SequencerBase.Track)
        Sequencer.Audition.Voices.Add(vc1)

        AddHandler SequencerBase.MidiOutShortMsg, AddressOf MidiOutShortMsg

        AddHandler ScreenRefreshTimer.Elapsed, AddressOf ScreenRefreshTimer_Tick
        ScreenRefreshTimer.Start()
        Sequencer.Start_Timer()

        '--- add pattern examples for DirectPlay

        Sequencer.DPlay.PatternStore.Add(Pattern1.Copy)
        Sequencer.DPlay.PatternStore.Add(Pattern2.Copy)

        'For Each pattern In Sequencer.DPlay.PatternStore
        '    lbDpPatternStore.Items.Add(pattern.Label)
        'Next

        '---

        'CompositionPanel.UpdateScaleX()                 ' initialize first

        Dim filename As String = My.Settings.LastCompositionName
        If filename <> "" Then
            If IO.File.Exists(filename) Then
                If SequencerBase.LoadCompositionFromXML(filename, Sequencer.Composition) = True Then
                    'CompositionPanel.CompositionWasLoaded(Sequencer.Composition)    ' set the values in UI
                    'CompositionPanel.CompositionWasLoaded(Sequencer.Audition)    ' set the values in UI    xxxx debug
                    'CompositionPanel.CompositionWasLoaded(Nothing)    ' set the values in UI    xxxx debug
                    CompositionPanel.Composition = Sequencer.Composition

                    Sequencer.BPM = Sequencer.Composition.Tempo
                    Set_LoopRestartState()
                    ShowCurrentCompsitionName()
                End If
            End If
        End If

    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        ScreenRefreshTimer.Stop()
        Sequencer.Stop_Timer()

        SidePanel1.CloseOpenWindow()    ' close open Window if any and update values before they were saved
        'If WindowComposition IsNot Nothing Then WindowComposition = Nothing
        'If WindowPatternStore IsNot Nothing Then WindowPatternStore = Nothing
        'If WindowSections IsNot Nothing Then WindowSections = Nothing
        'If WindowArrange IsNot Nothing Then WindowArrange = Nothing

        If SequencerBase.CurrentCompositionFileName <> "" Then

            SaveCompositionIfChanged()

            My.Settings.LastCompositionName = SequencerBase.CurrentCompositionFileName
        End If

        My.Settings.LastMidiOutPort0 = MidiOutPort0_preferred
        My.Settings.LastMidiOutPort1 = MidiOutPort1_preferred
        My.Settings.LastMidiOutPort2 = MidiOutPort2_preferred
        My.Settings.LastMidiOutPort3 = MidiOutPort3_preferred
        My.Settings.LastAlternativeMidiOutPort = AlternativeMidiOutPort

        My.Settings.LastPatternDirectory = LastPatternDirectory

        My.Settings.Save()

        MIO._End()                          ' close all MIDI-Ports
    End Sub

    Private Sub SaveCompositionIfChanged()
        '--- check if the current composition must be saved to disk
        Dim saveComposition As Boolean = True
        Dim diskComp As New Composition

        Sequencer.Reset_AllCompositionPointers()            ' -> reset pointers, else Compositions are not equal
        Sequencer.Reset_AllCompositionVuVelocityValues()    ' -> reset VU Velocity values, else Compositions are not equal

        If IO.File.Exists(CurrentCompositionFileName) Then
            If SequencerBase.LoadCompositionFromXML(CurrentCompositionFileName, diskComp) = True Then
                If IsEqual(diskComp, Sequencer.Composition) = True Then
                    saveComposition = False             ' nothing changed -> no need to write
                Else
                    Dim str As String = "Composition was changed. Do you want to save the changes ?"
                    Dim result As MessageBoxResult
                    result = MessageBox.Show(str, "Save Composition", MessageBoxButton.YesNo)
                    If result <> MessageBoxResult.Yes Then
                        saveComposition = False         ' user don't want to save
                    End If
                End If
            End If
        End If

        If saveComposition = True Then
            SequencerBase.SaveComposition_as_XML(SequencerBase.CurrentCompositionFileName, Sequencer.Composition)
        End If
        '---
    End Sub

    Private Sub Set_LoopRestartState()
        If Sequencer.Composition.LoopMode = True Then
            tgbtnLoopMode.IsChecked = True
        Else
            tgbtnLoopMode.IsChecked = False
        End If

        If Sequencer.Composition.RestartAtEnd = True Then
            tgbtnRestartAtEnd.IsChecked = True
        Else
            tgbtnRestartAtEnd.IsChecked = False
        End If
    End Sub

    Private Sub Mi_File_Exit_Click(sender As Object, e As RoutedEventArgs) Handles Mi_File_Exit.Click
        Me.Close()
    End Sub


    Private Sub btnKeyboard_Click(sender As Object, e As RoutedEventArgs) Handles btnKeyboard.Click
        Mkbd.show("Keyboard")
    End Sub

    Private Sub KeyboardEvent(status As Integer, channel As Integer, data1 As Integer, data2 As Integer) Handles Mkbd.kbEvent
        If status = &H90 Then
            'Sequencer.Audition.Voices(0).NoteTranspose = CShort(data1 - 40)

            nudVc0Transpose.Value = data1 - 57      ' A3 =center

        End If
    End Sub
    Private Sub btnRestart_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnRestart.PreviewMouseDown
        Command_Restart()

    End Sub
    Private Sub btnPlay_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnPlay.PreviewMouseDown
        Command_Play()
    End Sub

    Private Sub btnStop_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnStop.PreviewMouseDown
        Command_Stop()
    End Sub

    Friend Sub Command_Restart()
        Sequencer.Set_SequencerTime(0)
        Sequencer.Reset_AllCompositionPointers()
        'SequencerUI.SequencerPanel1.ScrollIntoView(0)
        CompositionPanel.ScrollIntoView(0)
    End Sub
    Friend Sub Command_Play()
        Sequencer.Start_Sequencer()
        btnRestart.IsEnabled = False
        btnStop.IsEnabled = True
        btnPlay.IsEnabled = False

        If IsWindowOpen(RemoteControl1) Then
            RemoteControl1.btnRestart.IsEnabled = False
            RemoteControl1.btnStop.IsEnabled = True
            RemoteControl1.btnPlay.IsEnabled = False
        End If
    End Sub
    Friend Sub Command_Stop()
        Sequencer.Stop_Sequencer()
        btnStop.IsEnabled = False
        btnPlay.IsEnabled = True
        btnRestart.IsEnabled = True

        If IsWindowOpen(RemoteControl1) Then
            RemoteControl1.btnStop.IsEnabled = False
            RemoteControl1.btnPlay.IsEnabled = True
            RemoteControl1.btnRestart.IsEnabled = True
        End If
    End Sub


    Private Sub CompositionEndReached() Handles Sequencer.Play_Sequence_EndReached
        'If Me.Dispatcher.CheckAccess Then
        'ScreenRefresh()
        'Else
        Me.Dispatcher.Invoke(New SetSequencerButtons_StopState_Delegate(AddressOf SetSequencerButtons_StopState))
        'End If
    End Sub

    Public Delegate Sub SetSequencerButtons_StopState_Delegate()
    Private Sub SetSequencerButtons_StopState()
        Command_Stop()
    End Sub

    Private Sub btnRemoteControl_Click(sender As Object, e As RoutedEventArgs) Handles btnRemoteControl.Click

        If (RemoteControl1 Is Nothing) OrElse (RemoteControl1.IsLoaded = False) Then
            RemoteControl1 = New RemoteControl(Me)
            RemoteControl1.Owner = Me
            If btnRestart.IsEnabled = False Then RemoteControl1.btnRestart.IsEnabled = False
            If btnStop.IsEnabled = False Then RemoteControl1.btnStop.IsEnabled = False
            If btnPlay.IsEnabled = False Then RemoteControl1.btnPlay.IsEnabled = False
            RemoteControl1.Show()
        Else
            RemoteControl1.WindowState = WindowState.Normal
            RemoteControl1.Activate()
        End If

    End Sub

    Private Function IsWindowOpen(win As Window) As Boolean
        If IsNothing(win) OrElse win.IsLoaded = False Then
            Return False
        Else
            Return True
        End If
    End Function

    Private Sub BpmSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles BpmSlider.ValueChanged
        Sequencer.BPM = CInt(BpmSlider.Value)
    End Sub

    Private Sub MainVolumeSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles MainVolumeSlider.ValueChanged
        ' Master Volume for GM Instruments, Universal Real Time SysEx
        'F0 7F 7F 04 01 ll mm F7        ' ll = volume LSB, mm = volume MSB

        Dim MasterVolume_sysx As Byte() = {&HF0, &H7F, &H7F, &H4, &H1, &H0, &H64, &HF7}

        MasterVolume_sysx(6) = CByte(MainVolumeSlider.Value)
        MidiOutLongMsg(0, MasterVolume_sysx)
    End Sub


    Private Sub ScreenRefreshTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        'If Me.Dispatcher.CheckAccess Then
        'ScreenRefresh()
        'Else
        Me.Dispatcher.Invoke(New ScreenRefresh_Delegate(AddressOf ScreenRefresh))
        'End If
    End Sub

    Public Delegate Sub ScreenRefresh_Delegate()
    Private Sub ScreenRefresh()

        'SequencerUI.SequencerPanel1.ScreenRefresh()
        CompositionPanel.ScreenRefresh()

        '--- Sequencer Time (Ticks)
        Dim time As Long = CLng(Sequencer.SequencerTime)
        lblSequencerPosition.Content = TimeTo_MBT(time)

        If Sequencer.BPM <> BpmSlider.Value Then
            BpmSlider.Value = Sequencer.BPM
        End If

        '--- Audition Tab ---

        'lblAuditionTime.Content = Sequencer.AuditionTime
        lblAuditionTime.Content = TimeTo_MBT(CLng(Sequencer.AuditionTime))

        If Sequencer.PlayAuditionErrors > 0 Then
            Status_AuditionErrorCount.Text = CStr(Sequencer.PlayAuditionErrors)
        End If

        lblAuditionTime2.Content = CLng(Sequencer.AuditionTime)
        lblAuditionTime2.Content = Math.Round(Sequencer.AuditionTime, 0)

        If Sequencer.Audition.Voices(0).NoteOffList.Count > 0 Then
            Dim txt As String
            txt = CStr(Sequencer.Audition.Voices(0).NoteOffList.Count)
            txt = txt & " 1:  " & Sequencer.Audition.Voices(0).NoteOffList(0).Time
            Status_NoteOffListCount.Text = txt
        Else
            Status_NoteOffListCount.Text = "0"
        End If

        Status_NumberOfTimedEvents.Text = CStr(Sequencer.TimedEvents.Count)

        '--- DirectPlay Tab ---

        If tiDirectplay.IsSelected Then
            If Sequencer.DirectplayIsOn = True Then
                lblDirectPlayTime.Content = TimeTo_MBT(CLng(Sequencer.DirectplayTime))
            End If
        End If



        If Sequencer.DirectplayErrors > 0 Then
            Status_DirectplayErrorCount.Text = CStr(Sequencer.DirectplayErrors)
        End If

        'Sequencer.DPlay.Voices(0).MidiChannel = 0
        'Sequencer.DPlay.Voices(0).Queues.Item(0).Dequeue()

        '--- debug Tab

        lblMidiOutShortMsg_Count.Content = MidiOutShortMsg_Counter

        '--- debug window ---

        If DebugWindow1 IsNot Nothing Then
            If DebugWindow1.IsLoaded Then
                If DebugWindow1.WindowState <> WindowState.Minimized Then
                    If Sequencer.IsRunning OrElse Sequencer.AuditionIsRunning Then
                        DebugWindow1.ScreenRefresh()
                    End If
                End If
            End If
        End If

    End Sub

    Private Sub btnStartAudition_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnStartAudition.PreviewMouseDown
        ' or Sequencer.PlayPattern(Voice,Pattern) ?
    End Sub

    Private Sub btnStartAudition_Click(sender As Object, e As RoutedEventArgs) Handles btnStartAudition.Click
        Dim aud As SequencerBase.Composition = Sequencer.Audition
        For v = 1 To aud.Voices.Count
            For t = 1 To aud.Voices(v - 1).Tracks.Count
                For p = 1 To aud.Voices(v - 1).Tracks(t - 1).PatternList.Count
                    aud.Voices(v - 1).Tracks(t - 1).PatternList(p - 1).Ended = False
                    aud.Voices(v - 1).Tracks(t - 1).PatternList(p - 1).StartTime = 0
                    aud.Voices(v - 1).Tracks(t - 1).PatternList(p - 1).StartOffset = 0
                    aud.Voices(v - 1).Tracks(t - 1).PatternList(p - 1).DoLoop = True
                Next
            Next
        Next


        Sequencer.Start_Audition()
    End Sub

    Private Sub btnStopAudition_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnStopAudition.PreviewMouseDown
        Sequencer.Stop_Audition()
        ' or Sequencer.StopPlaying (Audition) ?
    End Sub

    Private Sub btnPlayNote_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnPlayNote.PreviewMouseDown

        'Sequencer.Audition.Voices(0).MidiChannel = 9
        'Sequencer.Play_Note(45, 100, NoteDuration.Sixteenth)       ' Audition Voice 0, Duration = 1 QuarterNote
        Sequencer.Play_Note(45, 100, NoteDuration.Half)       ' Audition Voice 0, Duration = 1 QuarterNote
    End Sub

    Private Sub btnPlayPattern_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnPlayPattern.PreviewMouseDown
        Sequencer.Audition.Voices(0).MidiChannel = 9
        'Sequencer.Play_Pattern(Pattern1)
        Sequencer.Play_Pattern(Pattern1, CBool(cbPatternDoLoop.IsChecked))
    End Sub

    Private Sub btnPlayPattern2_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnPlayPattern2.PreviewMouseDown
        Sequencer.Play_Pattern(Pattern2, CBool(cbPatternDoLoop.IsChecked))
    End Sub

    Public Sub MidiOutShortMsg(port As Byte, status As Byte, data1 As Byte, data2 As Byte)
        Select Case port
            Case 0
                If hMidiOut0 <> 0 Then
                    MIO.OutShortMsg(hMidiOut0, status, data1, data2)
                End If
            Case 1

            Case 2

        End Select

    End Sub

    Public Sub MidiOutLongMsg(port As Byte, SysxData As Byte())
        Select Case port
            Case 0
                If hMidiOut0 <> 0 Then
                    'MIO.OutShortMsg(hMidiOut0, status, data1, data2)
                    MIO.OutLongMsg(hMidiOut0, SysxData)
                End If
            Case 1

            Case 2

        End Select
    End Sub

    Private Sub btnMoresGM_Click(sender As Object, e As RoutedEventArgs) Handles btnMoresGM.Click
        Dim Multi_mode_sysx As Byte() = {&HF0, &H43, &H10, &H7F, &H0, &HA, &H0, &H1, &H3, &HF7}
        Dim GM_ON_sysx As Byte() = {&HF0, &H7E, &H7F, &H9, &H1, &HF7}

        MIO.OutLongMsg(hMidiOut0, Multi_mode_sysx)

        Dim t = Task.Run(Async Function()
                             Await Task.Delay(100)
                             Return 42
                         End Function)
        t.Wait()

        MIO.OutLongMsg(hMidiOut0, GM_ON_sysx)
    End Sub

    Private Sub btnAllNotesOff_Click(sender As Object, e As RoutedEventArgs) Handles btnAllNotesOff.Click
        Dim stat As Byte

        For i = 0 To &HF
            stat = CByte(i Or &HB0)
            MidiOutShortMsg(0, stat, &H7B, 0)           ' All Notes Off (B0, 7B, 0)            
        Next

    End Sub

    Private Sub btnDebug_Click(sender As Object, e As RoutedEventArgs) Handles btnDebug.Click

        If (DebugWindow1 Is Nothing) OrElse (DebugWindow1.IsLoaded = False) Then
            DebugWindow1 = New DebugWindow(Me)
            DebugWindow1.Owner = Me                 ' An owner window can never cover an owned window.
            DebugWindow1.Show()
        Else
            DebugWindow1.WindowState = WindowState.Normal
            DebugWindow1.Activate()
        End If


    End Sub

    Private Sub btnSavePattern_Click(sender As Object, e As RoutedEventArgs) Handles btnSavePattern.Click
        SequencerBase.SavePattern(Pattern1)
    End Sub

    Private Sub btnSavePatternAsXML_Click(sender As Object, e As RoutedEventArgs) Handles btnSavePatternAsXML.Click
        SequencerBase.SavePattern_as_XML(Pattern1)
    End Sub

    Private Sub btnLoadPattern_Click(sender As Object, e As RoutedEventArgs) Handles btnLoadPattern.Click
        SequencerBase.LoadPattern(Pattern1)
    End Sub

    Private Sub btnLoadPatternXML_Click(sender As Object, e As RoutedEventArgs) Handles btnLoadPatternXML.Click
        Dim patx As New SequencerBase.PatternX
        If SequencerBase.LoadPatternFromXML(patx) = True Then
            Sequencer.Stop_Audition()
            Pattern1 = patx.ToPattern
            BpmSlider.Value = patx.BPM

            'nudVc0MidiChannel.Value = 9     ' reset to drums
            nudVc0Transpose.Value = 0       ' reset Transpose
            Sequencer.Play_Pattern(Pattern1, CBool(cbPatternDoLoop.IsChecked))
        End If


    End Sub

    Private Sub nudVc0MidiChannel_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudVc0MidiChannel.ValueChanged
        Sequencer.Audition.Voices(0).MidiChannel = CByte(nudVc0MidiChannel.Value)
        ShowAuVC0_VoiceName()
    End Sub

    Private Sub nudVc0Transpose_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudVc0Transpose.ValueChanged
        Sequencer.Audition.Voices(0).NoteTranspose = CShort(nudVc0Transpose.Value)
    End Sub

    Private Sub nudVc0VoiceNum_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudVc0VoiceNum.ValueChanged
        Sequencer.Audition.Voices(0).VoiceNumberGM = CByte(nudVc0VoiceNum.Value)

        Dim status As Byte
        Dim channel As Byte = CByte(nudVc0MidiChannel.Value)
        Dim voice As Byte = CByte(nudVc0VoiceNum.Value)
        status = CByte(&HC0 Or channel)
        MidiOutShortMsg(0, status, voice, 0)

        ShowAuVC0_VoiceName()
    End Sub

    Private Sub ShowAuVC0_VoiceName()
        If nudVc0VoiceNum IsNot Nothing Then
            If nudVc0MidiChannel.Value <> 9 Then
                lblAuTestVoiceName.Content = Get_GM_VoiceName(CByte(nudVc0VoiceNum.Value))
            Else
                lblAuTestVoiceName.Content = Get_GM_DrumVoiceName(CByte(nudVc0VoiceNum.Value))
            End If
        End If
    End Sub

    Private Sub btnSetVc0_Click(sender As Object, e As RoutedEventArgs) Handles btnSetVc0.Click
        Sequencer.Audition.Voices(0).MidiChannel = CByte(nudVc0MidiChannel.Value)
        Sequencer.Audition.Voices(0).VoiceNumber = CByte(nudVc0VoiceNum.Value)
        Sequencer.Audition.Voices(0).NoteTranspose = CShort(nudVc0Transpose.Value)
        Dim status As Byte
        Dim channel As Byte = CByte(nudVc0MidiChannel.Value)
        Dim voice As Byte = CByte(nudVc0VoiceNum.Value)

        status = CByte(&HC0 Or channel)

        MidiOutShortMsg(0, status, voice, 0)
    End Sub

    Private Sub Msg_Comp_Loaded()

    End Sub


#Region "Menu Items"
    Private Sub Mi_MidiPorts_Click(sender As Object, e As RoutedEventArgs) Handles Mi_MidiPorts.Click
        Dim dlg As New DlgMidiPorts(Me)
        dlg.Owner = Me
        dlg.ShowDialog()

    End Sub

    Private Sub Mi_File_New_Composition_Click(sender As Object, e As RoutedEventArgs) Handles Mi_File_New_Composition.Click
        If SequencerBase.CurrentCompositionFileName <> "" Then
            SaveCompositionIfChanged()
        End If

        CurrentCompositionFileName = CompositionsDirectory & DefaultNameForNewComposition
        Sequencer.Composition = New Composition

        ShowCurrentCompsitionName()
        'SequencerUI.SequencerPanel1.CompositionWasLoaded()      ' initialize first
        'CompositionPanel.CompositionWasLoaded(Nothing)      ' initialize first
        CompositionPanel.Composition = Nothing
        Set_LoopRestartState()

    End Sub

    Private Sub Mi_File_Load_Composition_Click(sender As Object, e As RoutedEventArgs) Handles Mi_File_Load_Composition.Click
        If SequencerBase.CurrentCompositionFileName <> "" Then
            SaveCompositionIfChanged()
        End If

        If SequencerBase.Dialog_LoadCompositionFromXML(Sequencer.Composition) = True Then
            SidePanel1.CloseOpenWindow()
            'OutputMessage("Composition was Loaded")            
            'SequencerUI.SequencerPanel1.CompositionWasLoaded()          ' set the values in UI
            'CompositionPanel.CompositionWasLoaded(Sequencer.Composition)          ' set the values in UI
            CompositionPanel.Composition = Sequencer.Composition
            Sequencer.BPM = Sequencer.Composition.Tempo
            Set_LoopRestartState()
            ShowCurrentCompsitionName()
        End If
    End Sub

    Private Sub Mi_File_Save_Composition_Click(sender As Object, e As RoutedEventArgs) Handles Mi_File_Save_Composition.Click
        If SequencerBase.Dialog_SaveComposition_as_XML(Sequencer.Composition) = True Then
            'MessageBox.Show("XML was saved", "Composition save as XML", MessageBoxButton.OK)
            'OutputMessage("XML was saved")
            ShowCurrentCompsitionName()
        End If
    End Sub

    Private Sub Mi_File_Import_MidiFile_Click(sender As Object, e As RoutedEventArgs) Handles Mi_File_Import_MidiFile.Click
        If Sequencer.IsRunning Then Command_Stop()
        If Sequencer.AuditionIsRunning = True Then Sequencer.Stop_Audition()
        Dim comp As New Composition
        Dim dlg As New SequencerUITools.MidiImport1(comp)
        dlg.Owner = Me
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner
        If dlg.ShowDialog() = True Then

            If SequencerBase.CurrentCompositionFileName <> "" Then
                SaveCompositionIfChanged()
            End If

            Sequencer.Composition = comp
            CompositionPanel.Composition = comp

            Command_Restart()       ' back to poition 0

            Set_LoopRestartState()
            ShowCurrentCompsitionName()

            CompositionPanel.CollapseAllVoicePanels()

            CurrentCompositionFileName = ""

        End If

    End Sub

    Private Sub Mi_About_Click(sender As Object, e As RoutedEventArgs) Handles Mi_About.Click
        Dim win As New AboutWin
        win.Owner = Me
        win.ShowDialog()
    End Sub

    Private Sub MiSoundReset_Click(sender As Object, e As RoutedEventArgs) Handles MiSoundReset.Click
        ' Emergency in case of hanging notes, inappropriate controller settings,..

        ' All Notes Off, All Sound Off, Reset All Controllers --> Port 0
        Dim stat As Byte

        '-- all notes off 123 (7B)
        For i = 0 To &HF
            stat = CByte(i Or &HB0)
            MidiOutShortMsg(0, stat, &H7B, 0)           ' All Notes Off (Bx, 7B, 0)            
        Next

        '-- all sound off 120 (78h)
        For i = 0 To &HF
            stat = CByte(i Or &HB0)
            MidiOutShortMsg(0, stat, &H78, 0)           ' All Notes Off (Bx, 78, 0)            
        Next

        '-- reset all controllers 121 (79h)
        For i = 0 To &HF
            stat = CByte(i Or &HB0)
            MidiOutShortMsg(0, stat, &H79, 0)           ' All Notes Off (Bx, 79, 0)            
        Next
    End Sub


#End Region

    Private Sub btnKeyboard2_Click(sender As Object, e As RoutedEventArgs) Handles btnKeyboard2.Click
        Mkbd.show("Transpose")
    End Sub

    Private Sub btnTestTimedEvent_Click(sender As Object, e As RoutedEventArgs) Handles btnTestTimedEvent.Click

        If Sequencer.AuditionIsRunning = True Then

            Dim time As UInteger = CUInt(Sequencer.AuditionTime)
            'time = Sequencer.TimeRoundUp(time, 4)
            'time = Sequencer.GetTimeOfNextMeasure(time)
            time = Sequencer.GetTimeOfNextBeat(time)
            Dim tmev As New TimedEvent_TransposeDelta(time, 0, 2)
            Sequencer.AddTimedEvent(tmev)

        End If

    End Sub




#Region "Audition Pattern Store"
    Private Sub btnLoadToPatternStore_Click(sender As Object, e As RoutedEventArgs) Handles btnLoadToPatternStore.Click
        Dim patx As New SequencerBase.PatternX
        Dim pat As SequencerBase.Pattern
        If SequencerBase.LoadPatternFromXML(patx) = True Then
            pat = patx.ToPattern
            Sequencer.Audition.PatternStore.Add(pat)
            lbxPatternStore.Items.Add(pat.Label)
            'ListPatternStoreContent()
        End If
    End Sub

    Private Sub ListPatternStoreContent()
        lbxPatternStore.Items.Clear()

        For Each pattern In Sequencer.Audition.PatternStore

            lbxPatternStore.Items.Add(pattern.Label)

        Next

    End Sub

    Private Sub lbxPatternStore_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles lbxPatternStore.MouseDoubleClick
        Dim ndx As Integer = lbxPatternStore.SelectedIndex

        If ndx <> -1 Then
            If Sequencer.AuditionIsRunning = True Then Sequencer.Stop_Audition()
            Sequencer.Play_Pattern(Sequencer.Audition.PatternStore(ndx), True)

        End If
    End Sub

    Private Sub btnRemoveFromPatternStore_Click(sender As Object, e As RoutedEventArgs) Handles btnRemoveFromPatternStore.Click
        Dim ndx As Integer = lbxPatternStore.SelectedIndex

        If ndx <> -1 Then
            If Sequencer.AuditionIsRunning = True Then Sequencer.Stop_Audition()
            Sequencer.Audition.PatternStore.RemoveAt(ndx)
            lbxPatternStore.Items.RemoveAt(ndx)
        End If
    End Sub

    Private Sub btnAuditionPatternStoreToVc1_Click(sender As Object, e As RoutedEventArgs) Handles btnAuditionPatternStoreToVc1.Click
        Dim ndx As Integer = lbxPatternStore.SelectedIndex

        If ndx <> -1 Then
            'If Sequencer.AuditionIsRunning = True Then Sequencer.Stop_Audition()
            Dim pat As SequencerBase.Pattern = Sequencer.Audition.PatternStore(ndx)
            pat.StartOffset = 0
            pat.DoLoop = True
            Sequencer.Audition.Voices(1).Tracks(0).PatternList.Clear()
            Sequencer.Audition.Voices(1).InsertPattern(0, Sequencer.Audition.PatternStore(ndx))
        End If
    End Sub

#End Region


#Region "Directplay tab"
    Private Sub lbDpPatternStore_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles lbDpPatternStore.MouseDoubleClick

    End Sub

    Private Sub btnLoadToDpPatternStore_Click(sender As Object, e As RoutedEventArgs) Handles btnLoadToDpPatternStore.Click
        Dim patx As New SequencerBase.PatternX
        Dim pat As SequencerBase.Pattern
        If SequencerBase.LoadPatternFromXML(patx) = True Then
            pat = patx.ToPattern
            Sequencer.DPlay.PatternStore.Add(pat)
            'lbDpPatternStore.Items.Add(pat.Label)           ' xxx Label only                        
        End If
    End Sub

    Private Sub btnRemoveFromDpPatternStore_Click(sender As Object, e As RoutedEventArgs) Handles btnRemoveFromDpPatternStore.Click

    End Sub

    Private Sub lbDpPatternStore_MouseMove(sender As Object, e As MouseEventArgs) Handles lbDpPatternStore.MouseMove
        If e.LeftButton = MouseButtonState.Pressed Then
            Dim listbox = TryCast(sender, ListBox)
            If listbox IsNot Nothing Then
                If listbox.SelectedItem IsNot Nothing Then
                    'Dim dataFormat As String = "Pattern"
                    'dataObject.SetData(DataFormat, listbox.SelectedItem)            ' format as string
                    Dim dataObject As New DataObject
                    dataObject.SetData(GetType(Pattern), listbox.SelectedItem)     ' format as type
                    DragDrop.DoDragDrop(listbox, dataObject, DragDropEffects.Copy)
                End If
            End If
        End If

    End Sub

    Private Sub tblkV0S0_DragOver(sender As Object, e As DragEventArgs) Handles tblkV0S0.DragOver
        If e.Data.GetDataPresent(GetType(Pattern)) Then
            e.Effects = DragDropEffects.Copy
        Else
            e.Effects = DragDropEffects.None
        End If
    End Sub

    Private Sub tblkV0S0_Drop(sender As Object, e As DragEventArgs) Handles tblkV0S0.Drop
        If e.Data.GetDataPresent(GetType(Pattern)) Then
            Dim pattern As Pattern = CType(e.Data.GetData(GetType(Pattern)), Pattern)
            If pattern IsNot Nothing Then
                Sequencer.DPlay.PlayPattern(0, 0, pattern, 960)
            End If
        End If
    End Sub

    Private Sub tblkV1S0_DragOver(sender As Object, e As DragEventArgs) Handles tblkV1S0.DragOver
        If e.Data.GetDataPresent(GetType(Pattern)) Then
            e.Effects = DragDropEffects.Copy
        Else
            e.Effects = DragDropEffects.None
        End If
    End Sub

    Private Sub tblkV1S0_Drop(sender As Object, e As DragEventArgs) Handles tblkV1S0.Drop
        If e.Data.GetDataPresent(GetType(Pattern)) Then
            Dim pattern As Pattern = CType(e.Data.GetData(GetType(Pattern)), Pattern)
            If pattern IsNot Nothing Then
                Sequencer.DPlay.PlayPattern(1, 0, pattern, 960)
            End If
        End If
    End Sub

    Private Sub btnDirectplayVc0_Click(sender As Object, e As RoutedEventArgs) Handles btnDirectplayVc0.Click
        Dim vcpop As New VoicePopup(Sequencer.DPlay.Voices(0))
        vcpop.Owner = Me
        vcpop.ShowDialog()
    End Sub

#End Region

#Region "Debug tab"
    Private Sub ShowCurrentCompsitionName()
        Dim title As String = WindowTitle_Base
        Me.Title = WindowTitle_Base & "  -  " & IO.Path.GetFileNameWithoutExtension(SequencerBase.CurrentCompositionFileName)
    End Sub

    Private Sub btnShowPointers_Click(sender As Object, e As RoutedEventArgs) Handles btnShowPointers.Click
        ShowPointers()
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        tbDebugOut.Clear()
    End Sub

    Private Sub btnResetMidiOutShortMsgCounter_Click(sender As Object, e As RoutedEventArgs) Handles btnResetMidiOutShortMsgCounter.Click
        MidiOutShortMsg_Counter = 0
    End Sub

    Private Sub tgbtnLoopMode_Checked(sender As Object, e As RoutedEventArgs) Handles tgbtnLoopMode.Checked
        Sequencer.Composition.LoopMode = True
        'SequencerUI.SequencerPanel1.LoopCanvasVisibility = Visibility.Visible
        CompositionPanel.LoopCanvasVisibility = Visibility.Visible
    End Sub

    Private Sub tgbtnLoopMode_Unchecked(sender As Object, e As RoutedEventArgs) Handles tgbtnLoopMode.Unchecked
        Sequencer.Composition.LoopMode = False
        'SequencerUI.SequencerPanel1.LoopCanvasVisibility = Visibility.Hidden
        CompositionPanel.LoopCanvasVisibility = Visibility.Hidden
    End Sub

    Private Sub tgbtnRestartAtEnd_Checked(sender As Object, e As RoutedEventArgs) Handles tgbtnRestartAtEnd.Checked
        Sequencer.Composition.RestartAtEnd = True
    End Sub

    Private Sub tgbtnRestartAtEnd_Unchecked(sender As Object, e As RoutedEventArgs) Handles tgbtnRestartAtEnd.Unchecked
        Sequencer.Composition.RestartAtEnd = False
    End Sub

    Private Sub TabControl1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles TabControl1.SelectionChanged
        If TabControl1.SelectedItem Is tiSequencer Then
            CompositionPanel.RedrawAllTracks()
        End If
    End Sub












#End Region


End Class
