Imports System.Collections.ObjectModel
Imports SequencerBase.Directplay

Public Class Directplay

    Public Property PatternStore As New ObservableCollection(Of Pattern)    ' need 'Property' for WPF Databinding

    Public Property Voices As New List(Of DirectplayVoice)

    Public Const NumberOfVoices = 6                            ' 2 default + 4 user voices
    Public Const NumberOfSlotsPerVoice = 4

    Public Const MaximumNumberOfJoblistItems = 8

    Public Const TicksPerBeat = Sequencer.TPQ

    Public SystemVoice As DirectplayVoice                       ' reserved voice for direct access
    Public SystemDrumVoice As DirectplayVoice                   ' reserved voice for direct access
    'UserVoice = Voices( 2,3,4,5)

    Public Sub New()
        For v = 1 To NumberOfVoices
            Dim vc As New DirectplayVoice
            For q = 1 To NumberOfSlotsPerVoice
                'vc.Queues.Add(New Queue(Of Job))
                vc.Slots.Add(New Slot)
            Next
            Voices.Add(vc)
        Next

        Voices(0).Title = "System Voice"
        Voices(0).Volume = 127
        SystemVoice = Voices(0)                                 ' fix and keep as reserved voice

        Voices(1).Title = "System Drum Voice"
        Voices(1).Volume = 127
        Voices(1).MidiChannel = 9
        SystemDrumVoice = Voices(1)                             ' fix and keep as reserved voice

        Voices(2).MidiChannel = 9                               ' user slot 1 in MainWindow

        Voices(2).Slots(0).RingPlay = True
        Voices(3).Slots(0).RingPlay = True

        'Dim queue = GetQueue(Voices(0), 10)         ' xxx

    End Sub


#Region "Classes"

    Public Class DirectplayVoice
        Inherits Voice
        Public Slots As New List(Of Slot)
    End Class

    Public Class Slot
        Public Joblist As New List(Of Job)
        Public IsRunning As Boolean = True              ' if TRUE: is running
        Public HoldCurrent As Boolean                   ' if TRUE: restart the job when job duration is over
        Public RingPlay As Boolean                        ' if TRUE: enque the current job when job duration is over
        Public Group As Integer                         ' selected Group number for receiving group commands
        Public Refresh_UI As Boolean                    ' something was changed by code, signal that DpSlot should refresh
    End Class

    Public MustInherit Class Job
        Public JobType As JobType
        Public Label As String = ""
        Public StartAlign As UInteger                           ' f.e. 1 Beat
        Public StartTime As UInteger                            ' will be calculateted before start playing
        Public StartOffset As UInteger
        Public Length As UInteger
        Public Duration As UInteger
    End Class

    Public Class PatternJob
        Inherits Job
        Public EventListPtr As Integer                              ' ptr to EventList
        Public EventList As New List(Of TrackEvent)

        Public Sub New()
            JobType = JobType.Pattern
        End Sub

    End Class

    Public Enum JobType
        Pattern
        Wait
    End Enum

#End Region


    ''' <summary>
    ''' the main player proc called from TimerTick
    ''' </summary>
    ''' <param name="currentTime">DirectplayTime</param>
    Friend Sub Play(currentTime As UInteger)
        For Each voice In Voices
            voice.Do_TimedNoteOff(currentTime)          ' turn notes at end of their duration off
            For Each slot In voice.Slots
                If slot.Joblist.Count > 0 Then
                    Dim job As Job = slot.Joblist(slot.Joblist.Count - 1)
                    PlayJob(currentTime, voice, slot, job)
                End If
            Next
        Next
    End Sub

    Private Sub PlayJob(currentTime As UInteger, voice As Voice, slot As Slot, job As Job)

        If job.JobType = JobType.Pattern Then
            PlayPatternJob(currentTime, voice, slot, CType(job, PatternJob))
        End If

        ' check if the joblistlist needs to be changed (if the duration of the job is over)

        Dim timeOfNextJob As UInteger

        'If job.StartOffset >= job.Duration Or (currentTime >= job.StartTime + job.Duration) Then
        If currentTime >= (job.StartTime + job.Duration) Then

            timeOfNextJob = job.StartTime + job.Duration

            If slot.RingPlay = False Then
                slot.Joblist.Remove(job)
            Else
                Dim rjob As Job
                rjob = job
                slot.Joblist.Remove(job)
                rjob.StartOffset = 0

                If job.JobType = JobType.Pattern Then
                    CType(job, PatternJob).EventListPtr = 0
                End If

                slot.Joblist.Insert(0, rjob)
            End If

            If slot.Joblist.Count > 0 Then
                Dim newjob As Job = slot.Joblist(slot.Joblist.Count - 1)
                'newjob.StartTime = currentTime + GetAlignOffset(currentTime, newjob.StartAlign)
                newjob.StartTime = timeOfNextJob
            End If
            slot.Refresh_UI = True

        End If



    End Sub


    Private Sub PlayPatternJob(currentTime As UInteger, voice As Voice, slot As Slot, job As PatternJob)

        If (job.EventListPtr) >= job.EventList.Count Then Exit Sub

        Dim tev As TrackEvent
        tev = job.EventList(job.EventListPtr)

        If job.StartOffset < job.Duration Then

            While job.StartTime + job.StartOffset + tev.Time <= currentTime

                ' send also MetaEvents to play f.e. SetTempo
                voice.PlayEvent(CUInt(currentTime), job.StartTime + job.StartOffset + tev.Time, tev)

                job.EventListPtr += 1                                       ' try to go to next event

                If Not job.EventListPtr >= job.EventList.Count Then
                    tev = job.EventList(job.EventListPtr)                   ' if not above last event                    
                Else
                    ' restart pattern

                    job.EventListPtr = 0                    ' to start   
                    job.StartOffset += job.Length
                    tev = job.EventList(job.EventListPtr)

                    'If job.StartOffset >= job.Duration Then

                    '    If slot.RingPlay = False Then
                    '        slot.Joblist.Remove(job)
                    '    Else
                    '        Dim rjob As Job
                    '        rjob = job
                    '        slot.Joblist.Remove(job)
                    '        rjob.StartOffset = 0
                    '        slot.Joblist.Insert(0, rjob)
                    '    End If

                    '    If slot.Joblist.Count > 0 Then
                    '        Dim newjob As Job = slot.Joblist(slot.Joblist.Count - 1)
                    '        newjob.StartTime = currentTime + GetAlignOffset(currentTime, newjob.StartAlign)
                    '    End If
                    '    slot.Refresh_UI = True
                    '    Exit While
                    'End If

                End If

            End While

        End If

    End Sub


    Friend Sub Play_old(currentTime As UInteger)

        For Each voice In Voices

            voice.Do_TimedNoteOff(currentTime)

            For Each slot In voice.Slots

                If slot.Joblist.Count > 0 Then
                    Dim job As Job = slot.Joblist(slot.Joblist.Count - 1)

                    PlayJob(currentTime, voice, slot, job)
                End If

            Next
        Next

    End Sub
    Private Sub PlayPatternJob_old(currentTime As UInteger, voice As Voice, slot As Slot, job As PatternJob)

        If (job.EventListPtr) >= job.EventList.Count Then Exit Sub

        Dim tev As TrackEvent
        tev = job.EventList(job.EventListPtr)

        If job.StartOffset < job.Duration Then

            While job.StartTime + job.StartOffset + tev.Time <= currentTime

                ' send also MetaEvents to play f.e. SetTempo
                voice.PlayEvent(CUInt(currentTime), job.StartTime + job.StartOffset + tev.Time, tev)

                job.EventListPtr += 1                                       ' try to go to next event

                If Not job.EventListPtr >= job.EventList.Count Then
                    tev = job.EventList(job.EventListPtr)                   ' if not above last event                    
                Else
                    ' restart pattern

                    job.EventListPtr = 0                    ' to start   
                    job.StartOffset += job.Length
                    tev = job.EventList(job.EventListPtr)

                    If job.StartOffset >= job.Duration Then

                        If slot.RingPlay = False Then
                            slot.Joblist.Remove(job)
                        Else
                            Dim rjob As Job
                            rjob = job
                            slot.Joblist.Remove(job)
                            rjob.StartOffset = 0
                            slot.Joblist.Insert(0, rjob)
                        End If

                        If slot.Joblist.Count > 0 Then
                            Dim newjob As Job = slot.Joblist(slot.Joblist.Count - 1)
                            newjob.StartTime = currentTime + GetAlignOffset(currentTime, newjob.StartAlign)
                        End If
                        slot.Refresh_UI = True
                        Exit While
                    End If

                End If

            End While

        End If

    End Sub


    ''' <summary>
    ''' Insert Pattern for playing
    ''' </summary>
    ''' <param name="VoiceNumber"></param>
    ''' <param name="SlotNumber"></param>
    ''' <param name="Pattern">Pattern where desired duration is set</param>
    ''' <param name="StartAlign">forward alignment in ticks, f.e. 960 = start at next beat in DirectplayTime</param>
    ''' <returns>True if successful, False if Voice or Queue number invalid, pattern is nothing or queue is full</returns>
    Public Function InsertPattern(VoiceNumber As Integer, SlotNumber As Integer, Pattern As Pattern, StartAlign As UInteger) As Boolean
        If VoiceNumber >= Voices.Count Then Return False
        If SlotNumber >= Voices(VoiceNumber).Slots.Count Then Return False
        If Voices(VoiceNumber).Slots(SlotNumber).Joblist.Count >= MaximumNumberOfJoblistItems Then Return False
        If Pattern Is Nothing Then Return False

        Dim job As New PatternJob
        job.StartTime = 0
        job.StartOffset = 0
        job.Length = Pattern.Length
        job.Duration = Pattern.Duration
        job.StartAlign = StartAlign
        job.EventListPtr = 0
        job.EventList = Pattern.EventList
        job.Label = Pattern.Label

        '--- if this is the only job, prepare to start
        If Voices(VoiceNumber).Slots(SlotNumber).Joblist.Count = 0 Then
            Dim time As UInteger
            time = CUInt(SequencerInstance.DirectplayTime)
            job.StartTime = time + GetAlignOffset(time, StartAlign)
        End If

        '--- in any case: enqueue the job
        'Voices(VoiceNumber).Slots(SlotNumber).Joblist.Enqueue(job)
        Voices(VoiceNumber).Slots(SlotNumber).Joblist.Insert(0, job)

        Return True
    End Function

    Public Function AddJob(Voice As DirectplayVoice, QueueNumber As Integer, job As Job) As Boolean
        Dim queue = GetQueue(Voice, QueueNumber)
        If queue Is Nothing Then Return False
        'Voice.Slots(QueueNumber).Joblist.Enqueue(job)
        Voice.Slots(QueueNumber).Joblist.Insert(0, job)
        Return True
    End Function

    Private Function GetQueue(Voice As DirectplayVoice, QueueNumber As Integer) As List(Of Job)
        If Voice Is Nothing Then Return Nothing
        If QueueNumber >= Voice.Slots.Count Then Return Nothing
        Return Voice.Slots(QueueNumber).Joblist
    End Function

    ''' <summary>
    ''' Get Offset needed to align time to the align grid.
    ''' </summary>
    ''' <param name="Time">current time</param>
    ''' <param name="Align">forward alignment in ticks, f.e. 960 = start at next beat.</param>
    ''' <returns></returns>
    Public Function GetAlignOffset(Time As UInteger, Align As UInteger) As UInteger
        If Align = 0 Then Return 0

        Dim Newtime As UInteger

        Dim ElapsedTicks As UInteger                        ' in this unit
        Dim RemainingTicks As UInteger                      ' in this unit     

        ElapsedTicks = Time Mod Align
        RemainingTicks = Align - ElapsedTicks
        Newtime = Time + RemainingTicks                     ' round up to end of this unit

        Return RemainingTicks
    End Function


End Class
