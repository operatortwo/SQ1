Imports System.IO
Imports System.Reflection
Imports System.Xml.Serialization
Imports System.Linq

Public Module Module1
    Public Event MidiOutShortMsg(port As Byte, status As Byte, data1 As Byte, data2 As Byte)

    Public SequencerInstance As Sequencer

    Public MidiOutShortMsg_Counter As Integer

    Friend Sub OutShort(port As Byte, CurrentTime As UInteger, status As Byte, data1 As Byte, data2 As Byte)
        RaiseEvent MidiOutShortMsg(port, status, data1, data2)

        MidiOutShortMsg_Counter += 1

        ' currentTime is needed for the debug list
        Dim vtev As New SequencerBase.PortTrackEvent
        vtev.PortNumber = port
        vtev.Type = EventType.MidiEvent
        vtev.Time = CurrentTime
        vtev.Status = status
        vtev.Data1 = data1
        vtev.Data2 = data2
        InsertDebugMidiOutEvent(vtev)
    End Sub


    ''' <summary>
    ''' Directly send Midi Out Short Msg. For debug use, step through List of already sent Events.
    ''' </summary>
    ''' <param name="port"></param>
    ''' <param name="CurrentTime"></param>
    ''' <param name="status"></param>
    ''' <param name="data1"></param>
    ''' <param name="data2"></param>
    Public Sub DebugMidiOutShort(port As Byte, CurrentTime As UInteger, status As Byte, data1 As Byte, data2 As Byte)
        RaiseEvent MidiOutShortMsg(port, status, data1, data2)
    End Sub


    Public Enum NoteDuration As UInteger
        Full = 4 * 960
        Half = 2 * 960
        Quarter = 960
        Eighth = 480
        Sixteenth = 240
        ThirtySecond = 120
        DefaultDrum = 60
    End Enum

    ''' <summary>
    ''' This option let the user select if Measure and Beat in 'Measure:Beat:Tick' conversion is 0-based or 1-based
    ''' Programmers may like counting where fist index is 0 (MBT starts at 0:0:0), while musicians are used to 
    ''' have first position at MBT 1:1:0 (counting 4 beats as 'one, two, three, four') Default is TRUE / 0-based /
    ''' programmers choice
    ''' </summary>
    ''' <returns></returns>
    Public Property MBT_0_based As Boolean = True

    ''' <summary>
    ''' Sequencer Ticks to Measure:Beat:Ticks considering MBT_0_based Boolean
    ''' </summary>
    ''' <param name="time"></param>
    ''' <returns></returns>
    Public Function TimeTo_MBT(time As Long) As String

        Dim meas As Long                            ' measure (assume: 4/4)
        Dim beat As Integer                         ' beat inside measure
        Dim ticks As Integer

        meas = time \ (4 * Sequencer.TPQ)                      '  \ = returns an integer result        
        'beat = CInt((time - meas) \ 960 Mod 4)
        beat = CInt((time \ 960) Mod 4)
        ticks = CInt(time Mod 960)

        If MBT_0_based = True Then
            Return CStr(meas) & " : " & CStr(beat) & " : " & CStr(ticks)                ' base 0
        Else
            Return CStr(meas + 1) & " : " & CStr(beat + 1) & " : " & CStr(ticks)        ' base 1
        End If

    End Function


    Private Sub InsertDebugMidiOutEvent(vtev As PortTrackEvent)
        If SequencerInstance.DebugEventOutList.Count >= Sequencer.DebugEventOutList_MaxEvents Then
            SequencerInstance.DebugEventOutList.Clear()
        End If

        SequencerInstance.DebugEventOutList.Add(vtev)
    End Sub

    ''' <summary>
    ''' Used for debug. Adds PortNumber (of current voice)
    ''' </summary>
    Public Class PortTrackEvent
        Inherits TrackEvent
        Public Property PortNumber As Byte
    End Class

    Public MidiOutPortNameList As New List(Of MidiOutPortName) From
  {
  {New MidiOutPortName With {.Name = "Port 0"}},
  {New MidiOutPortName With {.Name = "Port 1"}},
  {New MidiOutPortName With {.Name = "Port 2"}},
  {New MidiOutPortName With {.Name = "Port 3"}}
  }


#Region "Compare two complex objects"

    Public Function IsEqual(obj1 As Object, obj2 As Object) As Boolean

        If obj1 Is Nothing Then Return False
        If obj2 Is Nothing Then Return False

        If obj1.GetType() <> obj2.GetType Then Return False

        Dim ms1 As New MemoryStream
        Dim seria1 As New Xml.Serialization.XmlSerializer(obj1.GetType)
        seria1.Serialize(ms1, obj1)

        Dim ms2 As New MemoryStream
        Dim seria2 As New Xml.Serialization.XmlSerializer(obj2.GetType)
        seria2.Serialize(ms2, obj2)

        Dim buf1 As Byte() = ms1.GetBuffer()
        Dim buf2 As Byte() = ms2.GetBuffer()

        'Dim buf1 As Byte() = ms1.ToArray()
        'Dim buf2 As Byte() = ms2.ToArray()

        Dim ret As Boolean
        ret = buf1.SequenceEqual(buf2)

        ms1.Close()
        ms2.Close()

        Return ret

        '---
        ' xxx Function is working, but it maybe can be improved. (lower memory usage with byte to byte comparing)


        'ms1.Position = 0
        'ms1.ReadByte()
        ' while ms1.Length

        'using Xml.Serialization needs no <Serializable>
        '--- binary Serialization needs <Seializable> on each class
        ' Dim bf As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        ' bf.Serialize(ms, o)

    End Function








    ' Stream.GetBuffer

    'Imports System.IO
    'Imports System.Runtime.Serialization.Formatters.Binary
    Public Class CPersistent
        'Abspeichern eines Objekts In eine Datei als statische Methode:
        Public Shared Sub saveObject(o As Object, datei As String)
            Dim fs As New FileStream(datei, FileMode.Create, FileAccess.Write, FileShare.None)
            Dim bf As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            bf.Serialize(fs, o)
            fs.Close()

        End Sub
        'Ebenso das Laden des Objekts aus einer Datei:
        Public Shared Function loadObject(datei As String) As Object
            Dim o As Object
            Dim fs As New FileStream(datei, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim bf As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            o = bf.Deserialize(fs)
            fs.Close()
            Return o
        End Function
    End Class


#End Region

End Module

Public Class MidiOutPortName
    Public Name As String = ""
    Public IsPreferred As Boolean
End Class
