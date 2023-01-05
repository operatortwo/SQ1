Imports SequencerBase.Sequencer

<Serializable>
Public Class Composition

    '--- Header ---

    Public Property Name As String = ""
    Public Property Comments As String = ""
    Public TimeSignature_BeatsPerBar As Byte = 4            ' beats per measure
    Public TimeSignature_NoteValuePerBeat As Byte = 4       ' note value that represents one beat
    Public Tempo As Short = 120                     ' Beats Per Minute
    Public TicksPerQuarterNote As Short = TPQ
    Public Length As UInteger = 4 * 4 * TPQ         ' in Sequencer Ticks, needed for Track canvas
    Public RestartAtEnd As Boolean
    Public LoopMode As Boolean
    Public LoopStart As UInteger
    Public LoopEnd As UInteger
    Public LayoutVersionMajor As Byte = 1
    Public LayoutVersionMinor As Byte = 0

    '--- Pattern Store ---
    Public Property PatternStore As New List(Of Pattern)
    ' collection of patterns for further work. can be empty

    ' Port List
    ' Port 0 (Desired Port) 
    ' Port 1
    ' Port 2
    ' Port 3

    '--- Arrangement  ---
    '
    '       A B D B D C D E
    '
    '   Section     Label    Start   Length
    '------------------------------------------
    '       A       Intro 
    '       B       Main A
    '       C       Main B
    '       D       Refrain
    '       E       Ending
    '

    '--- Voices ---
    Public Property Voices As New List(Of Voice)

    Public Sub New()
        Voices.Add(New Voice)
        Voices(0).Tracks.Add(New Track)
        'Voices(0).Tracks(0).PatternList.Add(New Pattern)

    End Sub

    Public Sub ResetAllEventListPtrs()
        If Me Is Nothing Then Exit Sub
        For v = 1 To Voices.Count
            For t = 1 To Voices(v - 1).Tracks.Count
                For p = 1 To Voices(v - 1).Tracks(t - 1).PatternList.Count
                    Voices(v - 1).Tracks(t - 1).PatternList(p - 1).EventListPtr = 0
                Next
            Next
        Next
    End Sub

End Class
