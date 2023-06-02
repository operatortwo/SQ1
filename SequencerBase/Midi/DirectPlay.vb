Imports System.Collections.ObjectModel
Imports SequencerBase.Directplay

Public Class Directplay

    Public Property PatternStore As New ObservableCollection(Of Pattern)    ' need 'Property' for WPF Databinding

    Public Property Voices As New List(Of DirectplayVoice)

    Private Const NumberOfVoices = 4
    Private Const NumbeOfSlotsPerVoice = 4

    Public Const DefaultVoiceIndex = 0
    Public Const DefaultDrumVoiceIndex = 1

    Public Const MaximumNumberOfQueueItems = 8

    Private Const Beat = Sequencer.TPQ

    Private vc1 As DirectplayVoice

    Public Sub New()
        For v = 1 To NumberOfVoices
            Dim vc As New DirectplayVoice
            For q = 1 To NumbeOfSlotsPerVoice
                'vc.Queues.Add(New Queue(Of Job))
                vc.Slots.Add(New Slot)
            Next
            Voices.Add(vc)
        Next

        Voices(0).Title = "Default Voice"
        Voices(0).Volume = 127

        Voices(1).Title = "Default Drum Voice"
        Voices(1).Volume = 127
        Voices(1).MidiChannel = 9

        Dim queue = GetQueue(Voices(0), 10)

    End Sub

    ''' <summary>
    ''' the main player proc called from TimerTick
    ''' </summary>
    ''' <param name="currentTime">DirectplayTime</param>
    Friend Sub Play(currentTime As UInteger)

        For Each voice In Voices

            voice.Do_TimedNoteOff(currentTime)

            For Each slot In voice.Slots

                If slot.Queue.Count > 0 Then
                    Dim job As Job = slot.Queue.Peek
                    If job.JobType = JobType.Pattern Then
                        PlayPatternJob(currentTime, voice, slot.Queue, CType(job, PatternJob))
                    End If

                End If

            Next
        Next

    End Sub

    Private Sub PlayPatternJob(currentTime As UInteger, voice As Voice, queue As Queue(Of Job), job As PatternJob)

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
                        queue.Dequeue()
                        If queue.Count > 0 Then
                            Dim newjob As Job = queue.Peek
                            newjob.StartTime = currentTime + GetAlignOffset(currentTime, newjob.StartAlign)
                        End If
                        Exit While
                    End If

                End If

            End While

        End If

    End Sub


    ''' <summary>
    ''' Enqueue Pattern for playing
    ''' </summary>
    ''' <param name="VoiceNumber"></param>
    ''' <param name="SlotNumber"></param>
    ''' <param name="Pattern">Pattern where desired duration is set</param>
    ''' <param name="StartAlign">forward alignment in ticks, f.e. 960 = start at next beat in DirectplayTime</param>
    ''' <returns>True if successful, False if Voice or Queue number invalid, pattern is nothing or queue is full</returns>
    Public Function PlayPattern(VoiceNumber As Integer, SlotNumber As Integer, Pattern As Pattern, StartAlign As UInteger) As Boolean
        If VoiceNumber >= Voices.Count Then Return False
        If SlotNumber >= Voices(VoiceNumber).Slots.Count Then Return False
        If Voices(VoiceNumber).Slots(SlotNumber).Queue.Count >= MaximumNumberOfQueueItems Then Return False
        If Pattern Is Nothing Then Return False

        Dim job As New PatternJob
        job.StartTime = 0
        job.StartOffset = 0
        job.Length = Pattern.Length
        job.Duration = Pattern.Duration
        job.StartAlign = StartAlign
        job.EventListPtr = 0
        job.EventList = Pattern.EventList

        '--- if this is the only job, prepare to start
        If Voices(VoiceNumber).Slots(SlotNumber).Queue.Count = 0 Then
            Dim time As UInteger
            time = CUInt(SequencerInstance.DirectplayTime)
            job.StartTime = time + GetAlignOffset(time, StartAlign)
        End If

        '--- in any case: enqueue the job
        Voices(VoiceNumber).Slots(SlotNumber).Queue.Enqueue(job)

        Return True
    End Function

    Public Function AddJob(Voice As DirectplayVoice, QueueNumber As Integer, job As Job) As Boolean
        Dim queue = GetQueue(Voice, QueueNumber)
        If queue Is Nothing Then Return False
        Voice.Slots(QueueNumber).Queue.Enqueue(job)
        Return True
    End Function

    Private Function GetQueue(Voice As DirectplayVoice, QueueNumber As Integer) As Queue(Of Job)
        If Voice Is Nothing Then Return Nothing
        If QueueNumber >= Voice.Slots.Count Then Return Nothing
        Return Voice.Slots(QueueNumber).Queue
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

#Region "Classes"

    Public Class DirectplayVoice
        Inherits Voice
        'Public Queues As New List(Of Queue(Of Job))
        Public Slots As New List(Of Slot)
    End Class

    Public Class Slot
        Public Queue As New Queue(Of Job)
        Public Hold As Boolean
        Public Ring As Boolean
    End Class

    Public MustInherit Class Job
        Friend JobType As JobType
        Friend Label As String = ""
        Friend StartAlign As UInteger                           ' f.e. 1 Beat
        Friend StartTime As UInteger                            ' will be calculateted before start playing
        Friend StartOffset As UInteger
        Friend Length As UInteger
        Friend Duration As UInteger
    End Class

    Private Class PatternJob
        Inherits Job
        Public EventListPtr As Integer                              ' ptr to EventList
        Public EventList As New List(Of TrackEvent)

        Public Sub New()
            JobType = JobType.Pattern
        End Sub
    End Class

    Friend Enum JobType
        Pattern
        Value
        Empty
    End Enum

#End Region


End Class
