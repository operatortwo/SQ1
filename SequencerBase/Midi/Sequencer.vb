Public Class Sequencer

    Public Event TimerTick()
    Public Event SequencerRunningChanged(IsRunning As Boolean)
    Public Event Play_Sequence_EndReached()

    Friend Const DebugEventOutList_MaxEvents = 1000                      ' limit the List's size
    Public DebugEventOutList As New List(Of PortTrackEvent)(1000)

    'Public WithEvents Composition As New Composition
    'Public WithEvents Audition As New Composition
    Public Property Composition As New Composition
    'Public Composition As New Composition
    Public Property Audition As New Composition

    Public ReadOnly Property PlayAuditionErrors As Integer  ' Number of Catches in the Play_Audition Sub
    Public ReadOnly Property PlaySequenceErrors As Integer  ' Number of Catches in the Play_Sequence Sub
    Public ReadOnly Property TimedEventErrors As Integer    ' Number of Catches in the Check_TimedEvents Sub

    Public Property AuditionTime As Double         ' Timer Ticks, for Audition
    Public ReadOnly Property SequencerTime As Double        ' Sequencer Ticks, Sequencer position
    Public ReadOnly Property IsRunning As Boolean           ' Is Sequencer Running ?
    Public ReadOnly Property AuditionIsRunning As Boolean

    Public Const TPQ = 960                                  ' Ticks per Quarter Note 
    Private Const TPQdiv60 = TPQ \ 60                       ' auxiliary

    Private _BPM As Single = 120
    ''' <summary>
    ''' Tempo (BeatsPerMinute) Minimum: 10, Maximum: 300. Values above and below will be corrected    
    ''' </summary>
    ''' <returns></returns>
    Public Property BPM As Single                      ' tempo (Beats per Minute)
        Get
            Return _BPM
        End Get
        Set(value As Single)
            If value < 10 Then
                _BPM = 10
            ElseIf value > 300 Then
                _BPM = 300
            Else
                _BPM = value
            End If
        End Set
    End Property

    Private ReadOnly Stopwatch As New Stopwatch         ' to accurately measure elapsed time
    Private LastStopwatchTick As Long                   ' last Stopwatch.elapsedTicks    


    Declare Auto Function timeBeginPeriod Lib "winmm.dll" (uPeriod As UInteger) As UInteger
    Declare Auto Function timeEndPeriod Lib "winmm.dll" (uPeriod As UInteger) As UInteger

    Declare Auto Function timeSetEvent Lib "winmm.dll" (uDelay As UInteger, uResolution As UInteger, lpTimeProc As TimerProc, dwUser As IntPtr, fuEvent As UInteger) As UInteger
    Declare Auto Function timeKillEvent Lib "winmm.dll" (uTimerID As UInteger) As UInteger

    Private TimerID As UInteger

    Private Const TIME_PERIODIC = 1

    Delegate Sub TimerProc(uID As UInteger, uMsg As UInteger, dwUser As UInteger, dw1 As UInteger, dw2 As UInteger)
    Private ReadOnly fptrTimeProc As New TimerProc(AddressOf TickCallback)

    Private _TimerInterval As UInteger = 3
    ''' <summary>
    ''' Between 1 and 10 Milliseconds. Default = 3
    ''' </summary>
    ''' <returns></returns>
    Public Property TimerInterval As UInteger
        Get
            Return _TimerInterval
        End Get
        Set(value As UInteger)
            If value > 10 Then
                value = 10
            ElseIf value < 1 Then
                value = 1
            End If
            _TimerInterval = value
        End Set
    End Property

    Private _TimerResolution As UInteger = 3
    ''' <summary>
    ''' Between 0 and 10 , 0 = most accurate. Default = 3
    ''' </summary>
    ''' <returns></returns>
    Public Property TimerResolution As UInteger
        Get
            Return _TimerResolution
        End Get
        Set(value As UInteger)
            If value > 10 Then
                value = 10
            End If
            _TimerResolution = value
        End Set
    End Property


    Public Sub New()
        SequencerInstance = Me
    End Sub

    Public Sub Start_Timer()
        ' start the main timer
        If TimerID <> 0 Then Exit Sub
        Stopwatch.Start()
        timeBeginPeriod(TimerResolution)
        TimerID = timeSetEvent(TimerInterval, TimerResolution, fptrTimeProc, IntPtr.Zero, TIME_PERIODIC)
    End Sub

    Public Sub Stop_Timer()
        ' stop the main timer
        If TimerID <> 0 Then
            Stopwatch.Stop()
            timeKillEvent(TimerID)
            timeEndPeriod(TimerResolution)
            TimerID = 0
        End If
    End Sub

    ''' <summary>
    ''' Start or resume from current position
    ''' </summary>
    Public Sub Start_Sequencer()
        If IsRunning = True Then Exit Sub                   ' if  running -> exit
        _IsRunning = True

        '_AuditionTime = SequencerTime
        Stop_Audition()                                     ' when Sequencer starts, Audition has to stop

        RaiseEvent SequencerRunningChanged(IsRunning)       ' for User Interface

    End Sub

    Public Sub Stop_Sequencer()
        If IsRunning = False Then Exit Sub                  ' if not running -> exit

        'AllNotesOff()

        For v = 1 To Composition.Voices.Count
            Composition.Voices(v - 1).AllRunningNotesOff(CUInt(SequencerTime))
        Next

        _IsRunning = False

        RaiseEvent SequencerRunningChanged(IsRunning)       ' f.e. for signalizing
    End Sub

    Public Sub Set_SequencerTime(newTime As Double)
        If IsRunning = True Then
            If newTime < SequencerTime Then
                For v = 1 To Composition.Voices.Count
                    Composition.Voices(v - 1).AllRunningNotesOff(CUInt(SequencerTime))
                Next
            End If
        End If

        _SequencerTime = newTime
        Set_CompositionPointers(newTime)
    End Sub

    Private Sub Set_CompositionPointers(newTime As Double)
        'Reset_CompositionPointers()                            ' all to 0 (save, but set more ptrs than needed
        '                                                       ' (need time if many Voices + tracks + pattern)
        '                                                       ' this is now made in Track.Play for the next Pattern

        For Each voice In Composition.Voices
            For Each track In voice.Tracks
                If track.PatternList.Count > 0 Then

                    track.PatternListPtr = 0                    ' to first
                    '--- find pattern
                    Dim within As Boolean = False               ' init FALSE for repeated use

                    For Each pattern In track.PatternList
                        If newTime < pattern.StartTime Then
                            Exit For                            ' before this pattern
                        End If
                        If newTime < pattern.StartTime + pattern.Duration Then
                            within = True
                            Exit For                            ' within this pattern
                        End If
                        track.PatternListPtr += 1
                    Next

                    If track.PatternListPtr < track.PatternList.Count Then
                        track.PatternList(track.PatternListPtr).StartOffset = 0     ' reset to start value
                        track.PatternList(track.PatternListPtr).EventListPtr = 0    ' reset to first event
                    End If

                    '--- find event
                    If within = True Then

                        If track.PatternList(track.PatternListPtr).EventList.Count > 0 Then
                            Dim pat As Pattern = track.PatternList(track.PatternListPtr)
                            ' set StartOffset
                            Dim newTimeOffset As UInteger = CUInt(newTime - pat.StartTime)
                            Dim rep As UInteger = newTimeOffset \ pat.Length
                            pat.StartOffset = rep * pat.Length
                            ' find event
                            Dim evOffset As UInteger = newTimeOffset - pat.StartOffset

                            For Each tev In pat.EventList
                                If tev.Time >= evOffset Then
                                    Exit For
                                End If
                                pat.EventListPtr += 1
                            Next


                        End If
                    End If
                End If
            Next
        Next

    End Sub

    Public Sub Reset_AllCompositionPointers()
        If IsRunning = True Then Exit Sub                   ' if  running -> exit
        If Composition Is Nothing Then Exit Sub

        For Each voice In Composition.Voices
            For Each track In voice.Tracks
                track.PatternListPtr = 0
                For Each pattern In track.PatternList
                    pattern.EventListPtr = 0
                    pattern.StartOffset = 0
                Next
            Next
        Next

    End Sub

    Public Sub Reset_AllAuditionPointers()
        If AuditionIsRunning = True Then Exit Sub                   ' if  running -> exit
        If Audition Is Nothing Then Exit Sub

        For Each voice In Audition.Voices
            For Each track In voice.Tracks
                track.PatternListPtr = 0
                For Each pattern In track.PatternList
                    pattern.EventListPtr = 0
                    pattern.StartOffset = 0
                Next
            Next
        Next

    End Sub

    Public Sub Reset_AllCompositionVuVelocityValues()
        If IsRunning = True Then Exit Sub                   ' if  running -> exit
        If Composition Is Nothing Then Exit Sub

        For Each voice In Composition.Voices
            voice.VU_Velocity = 0
        Next

    End Sub

    Public Sub Start_Audition()
        If AuditionIsRunning = True Then Exit Sub
        'Audition.ResetAllEventListPtrs()
        Reset_AllAuditionPointers()
        _AuditionTime = 0
        _AuditionIsRunning = True
    End Sub

    Public Sub Stop_Audition()
        If AuditionIsRunning = False Then Exit Sub

        For i = 1 To Audition.Voices.Count
            Audition.Voices(i - 1).AllRunningNotesOff(CUInt(AuditionTime))
        Next

        _AuditionIsRunning = False
    End Sub

    Public Sub Play_Note(NoteNumber As Byte, Velocity As Byte, Duration As UInteger)
        ' assume VoiceNumber = 0

        If AuditionIsRunning = False Then
            Start_Audition()
        End If

        Audition.Voices(0).PlaySingleNote(CUInt(AuditionTime), CUInt(AuditionTime), 0, NoteNumber, Velocity, Duration)

    End Sub

    Public Sub Play_Pattern(vc As Voice, pt As Pattern, DoLoop As Boolean)
        'Dim vc0 As Voice = Audition.Voices(0)

        'vc0.MidiChannel = vc.MidiChannel

        'xxxxx deep copy Voice --> Audition.Voices(0) = vc.copy
        'Audition.Voices(0) = vc
        Audition.Voices(0).Tracks(0).PatternListPtr = 0

        Play_Pattern(pt, DoLoop)
    End Sub

    Public Sub Play_Pattern(pt As Pattern, DoLoop As Boolean)
        ' assume VoiceNumber = 0

        If AuditionIsRunning = False Then
            Start_Audition()
        End If

        Audition.Voices(0).Tracks(0).PatternList.Clear()
        Audition.Voices(0).Tracks(0).PatternListPtr = 0

        pt.StartTime = CUInt(AuditionTime)
        pt.StartOffset = 0
        pt.EventListPtr = 0
        pt.Ended = False
        'pt.DoLoop = DoLoop
        If DoLoop = True Then
            pt.Duration = 100000 * TPQ      ' not endless but very long (100'000 beats) to avoid handling of pt.DoLoop
        End If


        Audition.Voices(0).InsertPattern(CUInt(AuditionTime), pt)

    End Sub

    Private Sub TickCallback(uID As UInteger, uMsg As UInteger, dwUser As UInteger, dw1 As UInteger, dw2 As UInteger)

        Dim currentTick As Long = Stopwatch.ElapsedTicks
        Dim DeltaTicks As Long                                      'stopwatch ticks
        Dim DeltaSongTicks As Double                                ' player ticks

        DeltaTicks = currentTick - LastStopwatchTick
        LastStopwatchTick = currentTick

        'Dim DeltaMilliSeconds As Double = DeltaTicks / Stopwatch.Frequency * 1000

        ' Ticks = time(ms) * BPM * TPQ / 60'000
        ' Ticks = time(sec) * BPM * TPQ / 60                        ' 960 / 60 = 16     (TPQdiv60)
        ' Ticks = time(sec) * BPM * 16
        DeltaSongTicks = DeltaTicks / Stopwatch.Frequency * BPM * TPQdiv60

        RaiseEvent TimerTick()          ' needed ?

        If AuditionIsRunning = True Then
            _AuditionTime += DeltaSongTicks
            Try
                Play_Audition(CLng(AuditionTime))
            Catch
                _PlayAuditionErrors += 1
            End Try
        End If

        If IsRunning = True Then
            _SequencerTime += DeltaSongTicks
            Try
                Play_Sequence()
            Catch
                _PlaySequenceErrors += 1
            End Try
        End If

        If AuditionIsRunning = True Or IsRunning = True Then
            If TimedEvents.Count > 0 Then
                Try
                    Check_TimedEvents()
                Catch
                    _TimedEventErrors += 1
                End Try
            End If
        End If


    End Sub

    Private Sub Play_Audition(AuditionTime As Long)
        If Audition Is Nothing Then Exit Sub

        For v = 1 To Audition.Voices.Count
            For t = 1 To Audition.Voices(v - 1).Tracks.Count

                'Audition.Voices(v - 1).Tracks(t - 1).AuditionPlayPattern(AuditionTime, Audition.Voices(v - 1))

                Audition.Voices(v - 1).Tracks(t - 1).PlayTrack(AuditionTime, Audition.Voices(v - 1), Audition.Voices(v - 1).Tracks(t - 1))
                'Composition.Voices(v - 1).Tracks(t - 1).PlayTrack(CLng(SequencerTime), Composition.Voices(v - 1), Composition.Voices(v - 1).Tracks(t - 1))

            Next
        Next

        '--- turn notes at end of NoteDuration off

        For v = 1 To Audition.Voices.Count
            Audition.Voices(v - 1).Do_TimedNoteOff(AuditionTime)
        Next


    End Sub

    Private Sub Play_Sequence()

        '--- turn notes at end of NoteDuration off
        ' turn note off is set to the beginning for some specisl Midi-import where notes are immediately
        ' retriggered (time, duration: 0,120  120,120  240,120   360,120,...)
        ' can affect the sound on slow sound devices (GS wt synth)

        For v = 1 To Composition.Voices.Count
            Composition.Voices(v - 1).Do_TimedNoteOff(SequencerTime)
        Next

        ' or For each Voice in Composition.Voices

        For v = 1 To Composition.Voices.Count
            For t = 1 To Composition.Voices(v - 1).Tracks.Count
                Composition.Voices(v - 1).Tracks(t - 1).PlayTrack(CLng(SequencerTime), Composition.Voices(v - 1), Composition.Voices(v - 1).Tracks(t - 1))
            Next
        Next

        '--- if LoopMode, check for loopEnd

        If Composition.LoopMode = True Then
            If Composition.LoopEnd > Composition.LoopStart Then
                If SequencerTime >= Composition.LoopEnd Then
                    AllNotesOff()
                    Set_SequencerTime(Composition.LoopStart)    ' restart Loop
                    Exit Sub                                    ' exit here, no check for end of sequence needed
                End If
            End If
        End If

        '--- check for end of sequence ---

        If SequencerTime >= Composition.Length Then

            If Composition.RestartAtEnd = True Then
                AllNotesOff()
                Set_SequencerTime(0)                            ' restart sequence
            Else
                Stop_Sequencer()
                RaiseEvent Play_Sequence_EndReached()
            End If

        End If

    End Sub

    Private Sub AllNotesOff()
        For v = 1 To Composition.Voices.Count
            Composition.Voices(v - 1).AllRunningNotesOff(CUInt(SequencerTime))
        Next
    End Sub

    ''' <summary>
    ''' Send ProgramChange and ChannelVolume of Composition Voices to output
    ''' </summary>
    Public Sub Initialize_Midi()
        If Composition Is Nothing Then Exit Sub

        For Each voice In Composition.Voices
            PlayEvent(voice, &HC0, voice.VoiceNumberGM, 0)
            PlayEvent(voice, &HB0, 7, voice.Volume)             ' Channel volume coarse
            PlayEvent(voice, &HB0, 10, voice.Pan)               ' Panorama MSB
        Next

    End Sub

    Private Sub PlayEvent(voice As Voice, status As Byte, data1 As Byte, data2 As Byte)
        'vc.PlaySingleNote(CUInt(Sequencer.SequencerTime), vc.MidiChannel, &H40, 100, 480)
        Dim tev As New SequencerBase.TrackEvent
        tev.Type = SequencerBase.EventType.MidiEvent
        tev.Time = CUInt(SequencerTime)
        tev.Status = CByte(status Or voice.MidiChannel)
        tev.Data1 = data1
        tev.Data2 = data2
        voice.PlayEvent(CUInt(SequencerTime), CUInt(SequencerTime), tev)
    End Sub

End Class

