Partial Public Class Sequencer

    Public TimedEvents As New List(Of TimedEvent)

    Private Const MaximumTimedEventsCount = 16              ' do not store more than x TimedEvents

#Region "TimedEvent Class definitions"
    Public MustInherit Class TimedEvent                     ' base class for different Types of TimedEvents
        Public Time As UInteger                             ' most derived classes will need Time

        Public Overridable Sub DoItNow()                    ' when time has come, this Sub will be executed
            ' individual code for each TimedEvent Type
        End Sub

    End Class

    'TransposeAbsolute               ' to a certain value
    'TransposeDelta                  ' difference to current
    'AuditionStop
    'CompositionStop
    'AuditionMoveTo
    'CompositionMoveTo

    'TimedTransposeDelta

    Public Class TimedEvent_TransposeDelta
        Inherits TimedEvent
        'Public Time As UInteger                    ' defined in base class
        Public VoiceNumber As Integer
        Public TransposeDelta As Short

        Public Sub New(DesiredTime As UInteger, Voice_Number As Integer, Transpose_Delta As Short)
            Time = DesiredTime
            VoiceNumber = Voice_Number
            TransposeDelta = Transpose_Delta
        End Sub

        Public Overrides Sub DoItNow()
            Dim sqi = SequencerInstance
            Dim vc As Integer = VoiceNumber                             ' voice number
            Dim tsp_new As Short = TransposeDelta                       ' transpose delta value

            If vc >= sqi.Audition.Voices.Count Then Exit Sub
            Dim tsp_old As Short = sqi.Audition.Voices(vc).NoteTranspose
            tsp_new += tsp_old
            If tsp_new < 0 Then tsp_new = 0
            If tsp_new > 127 Then tsp_new = 127

            sqi.Audition.Voices(vc).NoteTranspose = tsp_new
        End Sub

    End Class

#End Region

#Region "TimedEvent Processing"
    Public Function AddTimedEvent(tmev As TimedEvent) As Boolean
        If TimedEvents.Count >= MaximumTimedEventsCount Then Return False       ' if List if full
        If tmev.Time <= AuditionTime Then Return False                          ' invalid Time

        ' if the only one item
        If TimedEvents.Count = 0 Then
            TimedEvents.Add(tmev)
            Return True
        End If

        'check for duplicate (same time and same type) replace with new event

        ' search where the new item must be inserted
        Dim ndx As Integer
        ndx = TimedEvents.FindLastIndex(Function(x) x.Time <= tmev.Time)
        If ndx = -1 Then
            Return False
        Else
            TimedEvents.Insert(ndx + 1, tmev)
        End If

        Return True
    End Function

    Private Sub Check_TimedEvents()
        If TimedEvents.Count > 0 Then
            Dim ndx As Integer

            Do While TimedEvents(0).Time <= AuditionTime

                Dim tmev As TimedEvent = TimedEvents(ndx)

                '--- do processing

                Try
                    Select Case tmev.GetType
                        Case GetType(TimedEvent_TransposeDelta)
                            'DoTimedEvent_TransposeDelta(CType(tmev, TimedEvent_TransposeDelta))
                            CType(tmev, TimedEvent_TransposeDelta).DoItNow()
                    End Select



                Catch
                End Try

                '--- remove event
                TimedEvents.RemoveAt(0)
                If TimedEvents.Count = 0 Then Exit Do

            Loop

        End If
    End Sub

    Private Sub DoTimedEvent_TransposeDelta(tmev As TimedEvent_TransposeDelta)
        Dim vc As Integer = tmev.VoiceNumber                             ' voice number
        Dim tsp_new As Integer = tmev.TransposeDelta                        ' transpose delta value

        If vc >= Audition.Voices.Count Then Exit Sub
        Dim tsp_old As Integer = Audition.Voices(vc).NoteTranspose
        tsp_new += tsp_old
        If tsp_new < 0 Then tsp_new = 0
        If tsp_new > 127 Then tsp_new = 127

        Audition.Voices(vc).NoteTranspose = CShort(tsp_new)
    End Sub

#End Region

#Region "Time calculations"
    ' from Wikipedia:
    'In musical notation, a bar (Or measure) Is a segment of time corresponding to a specific number of beats 
    'in which each beat is represented by a particular note value...
    '... the number of beats in each bar is specified at the beginning of the score by the time signature.
    'In simple time, (such As 3/4), the top figure indicates the number of beats per bar, while the bottom number
    'indicates the note value of the beat (the beat has a quarter note value in the 3/4 example).




    ''' <summary>
    ''' Assuming 4/4 signature. Gets the time when the current measure (bar) ends and the next begins.
    ''' </summary>
    ''' <param name="Time"></param>
    ''' <returns></returns>
    Public Function GetTimeOfNextMeasure(Time As UInteger) As UInteger
        Dim Newtime As UInteger
        Dim tpq As Integer = Sequencer.TPQ
        Dim TicksPerUnit As Integer = 4 * tpq               ' here a unit is a measure (bar) = 4 quaterNotes Length
        Dim ElapsedTicks As UInteger                        ' in this unit
        Dim RemainingTicks As UInteger                      ' in this unit     

        ElapsedTicks = CUInt(Time Mod TicksPerUnit)
        RemainingTicks = CUInt(TicksPerUnit - ElapsedTicks)
        Newtime = Time + RemainingTicks                     ' round up to end of this unit

        Return Newtime
    End Function

    Public Function GetTimeOfNextBeat(Time As UInteger) As UInteger
        Dim Newtime As UInteger
        Dim tpq As Integer = Sequencer.TPQ
        Dim TicksPerUnit As Integer = tpq                   ' here a unit is a beat = 1 quaterNote Length
        Dim ElapsedTicks As UInteger                        ' in this unit
        Dim RemainingTicks As UInteger                      ' in this unit     

        ElapsedTicks = CUInt(Time Mod TicksPerUnit)
        RemainingTicks = CUInt(TicksPerUnit - ElapsedTicks)
        Newtime = Time + RemainingTicks                     ' round up to end of this unit

        Return Newtime
    End Function



#End Region

End Class
