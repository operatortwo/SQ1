<Serializable>
Public Class Pattern

    Public Property Name As String = ""             ' need Property for WPF DataBinding
    Public StartTime As UInteger                    ' in Ticks 
    Public StartOffset As UInteger                  ' for repeated play
    Public Length As UInteger                       ' in ticks (1 beat = 960), for the pattern itself
    Public Duration As UInteger                     ' while    
    'Public DoLoop As Boolean                        ' try to avoid this, then only 1 Proc for PlayTrack is needed 
    '                                               ' (for Composition + Audition) instead use 100'000 * TPQ as Duration
    Public Ended As Boolean                         ' EventPtr is at end of EventList (needed ?)
    Public EventListPtr As Integer                  ' ptr to EventList
    Public EventList As New List(Of TrackEvent)

    ''' <summary>
    ''' Make a deep copy of this Pattern
    ''' </summary>
    ''' <returns></returns>
    Public Function Copy() As Pattern

        Dim pat2 As New Pattern

        pat2.Name = String.Copy(Name)
        pat2.StartTime = Me.StartTime               ' Me.StartTime
        pat2.StartOffset = StartOffset
        pat2.Length = Length
        pat2.Duration = Duration
        'pat2.DoLoop = DoLoop
        pat2.Ended = Ended
        pat2.EventListPtr = EventListPtr

        Dim evList As New List(Of TrackEvent)
        Dim tev As TrackEvent

        For i = 1 To EventList.Count
            tev = EventList(i - 1).Copy
            evList.Add(tev)
        Next

        pat2.EventList = evList

        Return pat2
    End Function

    ''' <summary>
    ''' Make a deep copy of Pattern and return it as PatternX
    ''' </summary>
    ''' <returns></returns>
    Public Function ToPatternX() As PatternX
        Dim pat2 As New PatternX

        'pat2.Name = Name
        'pat2.Category = Category
        'pat2.SubCategory = SubCategory
        'pat2.VoiceType = VoiceType
        'pat2.Source = Source
        'pat2.BPM = BPM

        pat2.StartTime = Me.StartTime               ' Me.StartTime
        pat2.StartOffset = StartOffset
        pat2.Length = Length
        pat2.Duration = Duration
        'pat2.DoLoop = DoLoop
        pat2.Ended = Ended
        pat2.EventListPtr = EventListPtr

        Dim evList As New List(Of TrackEvent)
        Dim tev As TrackEvent

        For i = 1 To EventList.Count
            tev = EventList(i - 1).Copy
            evList.Add(tev)
        Next

        pat2.EventList = evList

        Return pat2
    End Function


End Class

<Serializable>
Public Class TrackEvent
    ''' <summary>
    ''' [Ticks] for all Events
    ''' </summary>
    ''' <returns></returns>
    Public Property Time As UInteger
    Public Property Type As EventType
    Public Property Status As Byte

    ''' <summary>
    ''' If MidiEvent: Data-Byte 1 / If MetaEvent: MetaEventType
    ''' </summary>
    ''' <returns></returns>
    Public Property Data1 As Byte

    ''' <summary>
    ''' for MidiEvents
    ''' </summary>
    ''' <returns></returns>
    Public Property Data2 As Byte

    ''' <summary>
    ''' Data Array for MetaEvents and SysxEvents
    ''' </summary>
    ''' <returns></returns>
    Public Property DataX As Byte()

    ''' <summary>
    ''' [Ticks] for Note-On Events. Calculated from Time until Note-off. 0 if no Note-Off.
    ''' </summary>
    ''' <returns></returns>
    Public Property Duration As UInteger


    Public Function Copy() As TrackEvent
        Dim tev2 As New TrackEvent

        tev2.Data1 = Data1
        tev2.Data2 = Data2

        If DataX IsNot Nothing Then
            If DataX.Count > 1 Then
                Dim dx2 As Byte() = New Byte(DataX.Count - 1) {}        ' byte(upperBound)
                For i = 1 To DataX.Count
                    dx2(i - 1) = DataX(i - 1)
                Next
                tev2.DataX = dx2
            End If
        End If

        tev2.Duration = Duration
        tev2.Status = Status
        tev2.Time = Time
        tev2.Type = Type

        Return tev2
    End Function

End Class

''' <summary>
''' Pattern with additional Description: Name / Category / Group / Source / BPM for exchange and Library
''' </summary>
<Serializable>
Public Class PatternX
    Inherits Pattern

    'Public Name As String = ""                     ' Pattern name
    Public Category As String = ""                 ' 
    Public Category2 As String = ""
    Public VoiceType As String = ""
    Public Source As String = ""                   ' f.e. the Device Name, MIDI, App Name,...
    Public BPM As Integer = 120                    ' BeatsPerMinute (Tempo)
    Public TPQ As Integer = Sequencer.TPQ          ' Declared for better interchage between applications
    '                                              '              with different internal TPQ's

    ''' <summary>
    ''' Make a deep copy of PatternX
    ''' </summary>
    ''' <returns></returns>
    Public Overloads Function Copy() As PatternX
        Dim pat2 As New PatternX

        pat2.Name = Name
        pat2.Category = Category
        pat2.Category2 = Category2
        pat2.VoiceType = VoiceType
        pat2.Source = Source
        pat2.BPM = BPM

        pat2.StartTime = Me.StartTime               ' Me.StartTime
        pat2.StartOffset = StartOffset
        pat2.Length = Length
        pat2.Duration = Duration
        'pat2.DoLoop = DoLoop
        pat2.Ended = Ended
        pat2.EventListPtr = EventListPtr

        Dim evList As New List(Of TrackEvent)
        Dim tev As TrackEvent

        For i = 1 To EventList.Count
            tev = EventList(i - 1).Copy
            evList.Add(tev)
        Next

        pat2.EventList = evList

        Return pat2
    End Function

    ''' <summary>
    ''' Make a deep copy of the Pattern part of PatternX.
    ''' Use PatternX.Name as initial value for Pattern.Label
    ''' </summary>
    ''' <returns></returns>
    Public Function ToPattern() As Pattern
        Dim pat2 As New Pattern

        'pat2.Name = Name
        'pat2.Category = Category
        'pat2.SubCategory = SubCategory
        'pat2.VoiceType = VoiceType
        'pat2.Source = Source
        'pat2.BPM = BPM
        '-> lose Name, Category, SubCategory, VoiceType, Source, BPM of PatternX

        pat2.Name = Name                           ' set initial text of Label

        pat2.StartTime = Me.StartTime               ' Me.StartTime
        pat2.StartOffset = StartOffset
        pat2.Length = Length
        pat2.Duration = Duration
        'pat2.DoLoop = DoLoop
        pat2.Ended = Ended
        pat2.EventListPtr = EventListPtr

        Dim evList As New List(Of TrackEvent)
        Dim tev As TrackEvent

        For i = 1 To EventList.Count
            tev = EventList(i - 1).Copy
            evList.Add(tev)
        Next

        pat2.EventList = evList

        Return pat2
    End Function


End Class


Public Enum EventType
    Unkown = 0
    MidiEvent = 1           ' channel message
    F0SysxEvent = 240       ' &HF0 / 240
    F7SysxEvent = 247       ' &HF7 / 247    normal sysx
    MetaEvent = 255         ' &HFF / 255    escape sysx
End Enum

Public Enum MidiEventType
    NoteOffEvent = &H80
    NoteOnEvent = &H90
    PolyKeyPressure = &HA0
    ControlChange = &HB0
    ProgramChange = &HC0
    ChannelPressure = &HD0
    PitchBend = &HE0
End Enum

Public Enum MetaEventType
    SequenceNumber = 0
    TextEvent = 1
    CopyrightNotice = 2
    SequenceOrTrackName = 3
    InstrumentName = 4
    Lyric = 5
    Marker = 6
    CuePoint = 7
    MIDIChannelPrefix = &H20
    EndOfTrack = &H2F
    SetTempo = &H51
    SMPTEOffset = &H54
    TimeSignature = &H58
    KeySignature = &H59
    SequencerSpecific = &H7F
End Enum