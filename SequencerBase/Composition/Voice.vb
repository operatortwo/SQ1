Public Class Voice

    Public Property Title As String = ""
    Public Property PortNumber As Byte              ' Port Mapping (Name is in PortList of Composition)
    Public Property IsMultiChannel As Boolean       ' TRUE: allow MidiEvents to multiple Midi-Channels, ignore MidiChannel
    Public Property MidiChannel As Byte             ' when IsMultiChannel = FALSE: send all MidiEvents to this Midi-Channel
    Public Property Solo As Boolean
    Public Property Mute As Boolean
    Public Property Volume As Byte = 100
    Public Property Pan As Byte = 64                ' 64 = Center
    Public Property VoiceNumberGM As Byte
    Public Property BankSelectMSB As Byte           ' when non GM
    Public Property BankSelectLSB As Byte           ' when non-GM
    Public Property VoiceNumber As Byte             ' when non-GM, Bank Select Msb,Lsb + Progarm Change
    Public Property NoteTranspose As Short
    Public Property Tracks As New List(Of Track)        ' for Composition and Audition

    '--- Signal the UI that these properties have been updated.
    '--- The ScreenRefresh-tick proc will update the controls and reset the Booleans

    Public Refresh_VU_Velocity As Boolean
    Public VU_Velocity As Byte                          ' for VolumeUnit-Meter (Velocity is treated as Volume)

    ''' <summary>
    ''' Make a deep copy of this Voice
    ''' </summary>
    ''' <returns></returns>
    Public Function Copy() As Voice
        Dim vc2 As New Voice

        vc2.Title = String.Copy(Title)
        vc2.PortNumber = PortNumber
        vc2.IsMultiChannel = IsMultiChannel
        vc2.MidiChannel = MidiChannel
        vc2.Solo = Solo
        vc2.Mute = Mute
        vc2.Volume = Volume
        vc2.Pan = Pan
        vc2.VoiceNumberGM = VoiceNumberGM
        vc2.BankSelectMSB = BankSelectMSB
        vc2.BankSelectLSB = BankSelectLSB
        vc2.VoiceNumber = VoiceNumber
        vc2.NoteTranspose = NoteTranspose

        Dim TrackList As New List(Of Track)
        Dim trk As Track

        For i = 1 To Tracks.Count
            trk = Tracks(i - 1).Copy
            TrackList.Add(trk)
        Next

        vc2.Tracks = TrackList
        'Public Property Tracks As New List(Of Track)

        Return vc2
    End Function

    ''' <summary>
    ''' Play a Track event. CurrentTime and PlannedTime can be slightly different. The distinction is needed
    '''  for a little few of files from MidiImport where NoteOff is immediately followed by a new NoteOn
    '''  with the same NoteNumber. There it's important to keep the sequence NoteOn,NoteOff,NoteOn,NoteOff.
    ''' </summary>
    ''' <param name="currentTime">Sequencer Time</param>
    ''' <param name="plannedTime">StartTime + StartOffset + TevTime</param>
    ''' <param name="tev"></param>
    Public Sub PlayEvent(CurrentTime As UInteger, PlannedTime As UInteger, tev As TrackEvent)

        'xxxx  send track event for output, decide if it is note or other short, or if it is long msg

        If tev.Type = EventType.MidiEvent Then
            Dim status As Byte = CByte(tev.Status And &HF0)
            Dim channel As Byte
            Dim StatChan As Byte

            If IsMultiChannel = False Then
                StatChan = status Or MidiChannel            ' use MidiChannel of this voice
                channel = MidiChannel
            Else
                StatChan = tev.Status                       ' unchanged Status
                channel = CByte(tev.Status And &HF)         ' unchanged Channel
            End If

            If (status = &H90) And (tev.Data2 > 0) Then
                Do_TimedNoteOff(CurrentTime)                ' xxx fix for MetalOutOfSequenceOnOff - why ? - further analysis necessary
                PlayNote(CurrentTime, PlannedTime, channel, tev.Data1, tev.Data2, tev.Duration)       ' Note On + Duration
            Else
                OutShort(PortNumber, CurrentTime, StatChan, tev.Data1, tev.Data2)       ' can be &h80, &hA0, &hB0, &hC0, &hD0, &hE0
            End If

        ElseIf tev.Type = EventType.MetaEvent Then
            If tev.Data1 = MetaEventType.SetTempo Then
                Dim micros As Integer
                micros = tev.DataX(0) * 65536 + tev.DataX(1) * 256 + tev.DataX(2)
                SequencerInstance.BPM = CSng(Math.Round(60 * 1000 * 1000 / micros, 2))   ' 2 Decimal places
            End If

        End If

        '--- Update values for sequencer panel (UI) ---

        PropertyUpdates(tev)

    End Sub

    ''' <summary>
    ''' Play a Single Note
    ''' </summary>
    ''' <param name="CurrentTime"></param>
    ''' <param name="channel">only matters if IsMultiChannel set to TRUE, else MidiChannel is used</param>
    ''' <param name="NoteNumber"></param>
    ''' <param name="Velocity"></param>
    ''' <param name="Duration"></param>
    Public Sub PlaySingleNote(CurrentTime As UInteger, PlannedTime As UInteger, channel As Byte, NoteNumber As Byte, Velocity As Byte, Duration As UInteger)
        ' Public Sub for external call
        If IsMultiChannel = False Then
            PlayNote(CurrentTime, PlannedTime, MidiChannel, NoteNumber, Velocity, Duration)      ' force to pre-selected channel
        Else
            PlayNote(CurrentTime, PlannedTime, channel, NoteNumber, Velocity, Duration)          ' original channel from caller
        End If
    End Sub

    Private Sub PlayNote(CurrentTime As UInteger, PlannedTime As UInteger, channel As Byte, NoteNumber As Byte, Velocity As Byte, Duration As UInteger)
        ' Private Sub for class-internal use
        'Dim status As Byte = &H90                   ' NoteOn
        'status = CByte(status Or (MidiChannel And &HF))

        If Mute = False Then

            Dim status As Byte = CByte(&H90 Or (channel And &HF))

            Dim note As Short = NoteNumber + NoteTranspose
            If note < 0 Then note = 0
            If note > 127 Then note = 127

            OutShort(PortNumber, CurrentTime, status, CByte(note), Velocity)

            'Insert_NoteOff(CurrentTime + Duration, status, CByte(note), Velocity)
            Insert_NoteOff(PlannedTime + Duration, status, CByte(note), Velocity)


        End If
        '--- for VU-Meter---

        VU_Velocity = Velocity
        Refresh_VU_Velocity = True

    End Sub

    Public Sub InsertPattern(startTime As UInteger, pt As Pattern)
        Me.Tracks(0).PatternList.Add(pt)

    End Sub

    Public Sub Do_TimedNoteOff(CurrentTime As Double)
        ' check if time for NoteOff

        While NoteOffList.Count > 0
            Dim tev As TrackEvent
            tev = NoteOffList.Item(0)

            If tev.Time <= CurrentTime Then
                'OutShortMsg(tev.Status, tev.Status And &HF, tev.Data1, 0)
                OutShort(PortNumber, CUInt(CurrentTime), tev.Status, tev.Data1, 0)
                NoteOffList.RemoveAt(0)

            Else
                Exit While
            End If

        End While

    End Sub


    Private Const NoteOffListCapacity = 64                                  ' 64
    Public NoteOffList As New List(Of TrackEvent)(NoteOffListCapacity)

    Private Sub Insert_NoteOff(OffTime As UInteger, status As Byte, data1 As Byte, data2 As Byte)
        Dim stat As Byte = CByte(status And &HF0)
        If stat = &H90 Then                                 ' only for NoteOn
            If data2 > 0 Then                               ' not NoteOff (NoteOn + Velocity 0)

                Dim tev As New TrackEvent With {.Time = OffTime, .Status = status, .Data1 = data1, .Data2 = data2}

                ' if the only one item
                If NoteOffList.Count = 0 Then
                    NoteOffList.Add(tev)
                    Exit Sub
                End If

                ' check if list if full
                If NoteOffList.Count = NoteOffListCapacity Then
                    Dim tevX As TrackEvent
                    tevX = NoteOffList.Last
                    'OutShortMsg(tevX.Status, tevX.Status And &HF, tevX.Data1, 0)            ' play Note Off

                    Module1.OutShort(PortNumber, OffTime, tevX.Status, tevX.Data1, 0)                            ' play Note Off

                    NoteOffList.RemoveAt(NoteOffListCapacity - 1)                           ' remove last item
                    Debug.WriteLine("Full -> Removed")
                End If

                ' check if the new item belongs to the end of the list
                If OffTime > NoteOffList.Item(NoteOffList.Count - 1).Time Then
                    NoteOffList.Add(tev)
                    Exit Sub
                End If

                ' check if the new item belongs to the beginning of the list
                If OffTime < NoteOffList.Item(0).Time Then
                    NoteOffList.Insert(0, tev)
                    Exit Sub
                End If

                ' now search where the new item must be inserted
                Dim ndx As Integer
                ndx = NoteOffList.FindLastIndex(Function(x) x.Time <= tev.Time)
                If ndx = -1 Then
                    Exit Sub
                Else
                    NoteOffList.Insert(ndx + 1, tev)
                End If

            End If
        End If
    End Sub

    Public Sub AllRunningNotesOff(CurrentTime As UInteger)
        Dim tev As TrackEvent
        For i = 1 To NoteOffList.Count
            tev = NoteOffList(i - 1)
            'If tev.Type = EventType.MidiEvent Then                 ' EventType is Unkown
            Module1.OutShort(PortNumber, CurrentTime, tev.Status, tev.Data1, 0)              ' play Note Off           
            'End If
        Next

        NoteOffList.Clear()                                         ' clears List 

    End Sub

    '--- for UI ---

    ''' <summary>
    ''' The Midi-Part was processed, now check for updates of the Voice Properties
    ''' </summary>
    ''' <param name="tev"></param>
    Private Sub PropertyUpdates(tev As TrackEvent)

        If tev.Type = EventType.MidiEvent Then
            Dim status As Byte = CByte(tev.Status And &HF0)

            If status = MidiEventType.ProgramChange Then
                VoiceNumberGM = tev.Data1
            ElseIf status = MidiEventType.ControlChange Then
                If tev.Data1 = 7 Then
                    Volume = tev.Data2                              ' Channel volume coarse (MSB)                    
                ElseIf tev.Data1 = 10 Then
                    Pan = tev.Data2                                 ' Panorama MSB
                End If

            End If

        End If

    End Sub

End Class


