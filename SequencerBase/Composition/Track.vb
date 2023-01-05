Public Class Track

    Public Property Title As String = ""

    Public PatternListPtr As Integer
    Public Property PatternList As New List(Of Pattern)

    ''' <summary>
    ''' Make a deep copy of this Track
    ''' </summary>
    ''' <returns></returns>
    Public Function Copy() As Track

        Dim trk2 As New Track

        trk2.Title = String.Copy(Title)
        trk2.PatternListPtr = PatternListPtr

        Dim patList As New List(Of Pattern)
        Dim pat As Pattern

        For i = 1 To PatternList.Count
            pat = PatternList(i - 1).Copy
            patList.Add(pat)
        Next

        trk2.PatternList = patList

        Return trk2
    End Function

    Public Sub AuditionPlayPattern(CurrentTime As Long, Voice As Voice)

        If (PatternListPtr + 1) > PatternList.Count Then Exit Sub

        Dim pat As Pattern = PatternList(PatternListPtr)

        If pat Is Nothing Then Exit Sub
        'pat.StartOffset = 0

        If (pat.EventListPtr + 1) > pat.EventList.Count Then Exit Sub

        Dim tev As TrackEvent
        tev = pat.EventList(pat.EventListPtr)

        If pat.StartOffset < pat.Duration Then

            While pat.StartTime + pat.StartOffset + tev.Time <= CurrentTime

                'If tev.Type = EventType.MidiEvent Then             ' send also MetaEvents to play f.e. SetTempo
                'Voice.PlayNote(CUInt(CurrentTime), tev.Data1, tev.Data2, tev.Duration)
                Voice.PlayEvent(CUInt(CurrentTime), pat.StartTime + pat.StartOffset + tev.Time, tev)
                'End If

                pat.EventListPtr += 1                                       ' try to go to next event

                If Not pat.EventListPtr + 1 > pat.EventList.Count Then
                    tev = pat.EventList(pat.EventListPtr)                   ' if not above last event
                Else
                    ' restart pattern

                    pat.EventListPtr = 0                    ' to start   
                    pat.StartOffset += pat.Length
                    tev = pat.EventList(pat.EventListPtr)


                    If pat.StartOffset >= pat.Duration Then

                        If pat.DoLoop = False Then
                            pat.Ended = True
                            If PatternListPtr + 1 < PatternList.Count Then
                                PatternListPtr += 1             ' to next pattern
                            End If
                            Exit While
                        Else
                            pat.StartTime += pat.Duration
                            pat.StartOffset = 0
                            Exit While
                        End If

                    End If

                End If

            End While

        End If

    End Sub


    Public Sub PlayTrack(CurrentTime As Long, Voice As Voice, Track As Track)

        'xxx  the play loop could be improved
        ' check only once at the beginning
        ' repeated play loop will not need ptr check
        '--
        ' the reason for this check is: SequencerTime can be set from UserInteraction to a certain position.
        ' The pointers are then set according to the current position, this can be after the last Note, before
        ' pattern repeat occurs. When the Sequencer continues playing fron this position, zhere need to be a 
        ' change to next pattern or to pattern repeat (startOffset)
        ' currently its a bit inconsistent that the player Proc sets the pointer always to the next Event that
        ' has to be played (player sets pointers ahead, setTime sets pointers to current time)

        Dim pat As Pattern
        Dim tev As TrackEvent

        Do
            '--- some checks and corrections at the beginning

            If PatternListPtr >= PatternList.Count Then Exit Do     ' exit if end of track
            pat = PatternList(PatternListPtr)
            If pat Is Nothing Then                                  ' try to progress after empty pattern 
                PatternListPtr += 1                                 ' to next pattern
                If PatternListPtr < PatternList.Count Then
                    PatternList(PatternListPtr).EventListPtr = 0    ' reset next
                    PatternList(PatternListPtr).StartOffset = 0     ' reset next
                End If
                Continue Do
            End If

            If pat.EventListPtr >= pat.EventList.Count Then         ' at the end of the eventlist
                'xxx decide if restart or next pattern
                ' from beat 33

                If CurrentTime - pat.StartTime < pat.Duration Then
                    ' restart Pattern
                    pat.EventListPtr = 0
                    pat.StartOffset += pat.Length
                Else
                    '--- next pattern
                    PatternListPtr += 1                                 ' to next pattern
                    If PatternListPtr < PatternList.Count Then
                        PatternList(PatternListPtr).EventListPtr = 0    ' reset next
                        PatternList(PatternListPtr).StartOffset = 0     ' reset next
                    End If
                End If

                Continue Do
            End If

            '--- play event

            tev = pat.EventList(pat.EventListPtr)

            If pat.StartTime + pat.StartOffset + tev.Time <= CurrentTime Then
                'If tev.Type = EventType.MidiEvent Then         ' send also MetaEvents to play f.e. SetTempo
                Voice.PlayEvent(CUInt(CurrentTime), pat.StartTime + pat.StartOffset + tev.Time, tev)
                'End If
            Else
                    Exit Do
            End If

            If CurrentTime - pat.StartTime >= pat.Duration Then
                ' Duration of this Pattern reached
                ' -> next Pattern
                'pat.EventListPtr = 0        ' reset current
                'pat.StartOffset = 0         ' reset current
                PatternListPtr += 1                                 ' to next pattern
                If PatternListPtr < PatternList.Count Then
                    PatternList(PatternListPtr).EventListPtr = 0    ' reset next
                    PatternList(PatternListPtr).StartOffset = 0     ' reset next
                End If
            ElseIf (pat.EventListPtr + 1) >= pat.EventList.Count Then
                ' end of Pattern reached
                ' restart Pattern
                pat.EventListPtr = 0
                pat.StartOffset += pat.Length
            Else
                pat.EventListPtr += 1                               ' to next event
            End If
        Loop


    End Sub


    'Public Sub CompositionPlayPattern(CurrentTime As Long, Voice As Voice)

    '    If (PatternListPtr + 1) > PatternList.Count Then Exit Sub

    '    Dim pat As Pattern = PatternList(PatternListPtr)

    '    If pat Is Nothing Then Exit Sub
    '    'pat.StartOffset = 0

    '    If (pat.EventListPtr + 1) > pat.EventList.Count Then Exit Sub

    '    Dim tev As TrackEvent
    '    tev = pat.EventList(pat.EventListPtr)

    '    If pat.StartOffset < pat.Duration Then

    '        While pat.StartTime + pat.StartOffset + tev.Time <= CurrentTime

    '            If tev.Type = EventType.MidiEvent Then
    '                'Voice.PlayNote(CUInt(CurrentTime), tev.Data1, tev.Data2, tev.Duration)
    '                Voice.PlayEvent(CUInt(CurrentTime), tev)
    '            End If

    '            pat.EventListPtr += 1                                       ' try to go to next event

    '            If Not pat.EventListPtr + 1 > pat.EventList.Count Then
    '                tev = pat.EventList(pat.EventListPtr)                   ' if not above last event
    '            Else
    '                ' restart pattern

    '                pat.EventListPtr = 0                    ' to start   
    '                pat.StartOffset += pat.Length
    '                tev = pat.EventList(pat.EventListPtr)


    '                If pat.StartOffset >= pat.Duration Then

    '                    If pat.DoLoop = False Then
    '                        pat.Ended = True
    '                        If PatternListPtr + 1 < PatternList.Count Then
    '                            PatternListPtr += 1             ' to next pattern
    '                        End If
    '                        Exit While
    '                    Else
    '                        pat.StartTime += pat.Duration
    '                        pat.StartOffset = 0
    '                        Exit While
    '                    End If

    '                End If

    '            End If

    '        End While

    '    End If

    'End Sub

End Class
