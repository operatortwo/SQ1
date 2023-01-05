Imports SequencerBase
Imports SequencerBase.Sequencer

Partial Public Class MainWindow

    Private Pattern1 As New Pattern
    Private Pattern2 As New Pattern


    Private Sub CreateAuditionTestData()

        Dim pos As Long = 0                         ' CLng(Sequencer.SequencerPosition)
        Dim q As Long = 240 - (pos Mod 240)         ' sync to beat
        'Pattern1.StartTime = pos + q

        Pattern1.Label = "Pattern 1"
        Pattern1.EventList = New List(Of TrackEvent)
        Pattern1.Length = 1 * 960
        Pattern1.Duration = 8 * 960
        'Pattern1.DoLoop = True
        Pattern1.StartTime = 0
        Dim note As Byte = 31                       ' 36
        Dim duration = 60                           ' 60 not too short, else missing notes with GS Wavetable Synth
        Dim evt As EventType = EventType.MidiEvent
        Pattern1.EventList.Add(New TrackEvent With {.Time = 0, .Type = evt, .Status = &H99, .Data1 = note, .Data2 = 100, .Duration = CUInt(duration)})
        Pattern1.EventList.Add(New TrackEvent With {.Time = 120, .Type = evt, .Status = &H99, .Data1 = note, .Data2 = 100, .Duration = CUInt(duration)})
        Pattern1.EventList.Add(New TrackEvent With {.Time = 2 * 120, .Type = evt, .Status = &H99, .Data1 = note, .Data2 = 100, .Duration = CUInt(duration)})
        Pattern1.EventList.Add(New TrackEvent With {.Time = 3 * 120, .Type = evt, .Status = &H99, .Data1 = note, .Data2 = 100, .Duration = CUInt(duration)})

        '0 240 480 720

        '----

        Pattern2.Label = "Pattern 2"
        Pattern2.EventList = New List(Of TrackEvent)
        Pattern2.Length = 2 * 960
        Pattern2.Duration = 8 * 960
        'Pattern1.DoLoop = True        

        Pattern2.StartTime = 0
        Dim note2 As Byte = 41                       ' 36
        Dim duration2 = 120                           ' 60 not too short, else missing notes with GS Wavetable Synth
        Dim evt2 As EventType = EventType.MidiEvent
        Pattern2.EventList.Add(New TrackEvent With {.Time = 0, .Type = evt2, .Status = &H99, .Data1 = note2, .Data2 = 100, .Duration = CUInt(duration2)})
        Pattern2.EventList.Add(New TrackEvent With {.Time = 1 * 240, .Type = evt2, .Status = &H99, .Data1 = note2, .Data2 = 100, .Duration = CUInt(duration2)})
        Pattern2.EventList.Add(New TrackEvent With {.Time = 2 * 240, .Type = evt2, .Status = &H99, .Data1 = note2, .Data2 = 100, .Duration = CUInt(duration2)})
        Pattern2.EventList.Add(New TrackEvent With {.Time = 3 * 240, .Type = evt2, .Status = &H99, .Data1 = note2, .Data2 = 100, .Duration = CUInt(duration2)})




    End Sub


End Class
