Partial Public Class MainWindow

    Public WithEvents MIO As New Midi_IO.Midi_IO

    Public hMidiIn As UInteger              ' for Midi-In

    Public hMidiOut0 As UInteger            ' for Midi-Out    Port 0, routing in voice
    Public hMidiOut1 As UInteger
    Public hMidiOut2 As UInteger
    Public hMidiOut3 As UInteger

    Public MidiOutPort0_preferred As String = ""
    Public MidiOutPort1_preferred As String = ""
    Public MidiOutPort2_preferred As String = ""
    Public MidiOutPort3_preferred As String = ""

    Public MidiOutPort0_selected As String = ""
    Public MidiOutPort1_selected As String = ""
    Public MidiOutPort2_selected As String = ""
    Public MidiOutPort3_selected As String = ""

    Public AlternativeMidiOutPort As String = "first available"

    ''' <summary>
    ''' Open Midi Output Ports
    ''' </summary>
    Private Sub OpenMidiOutPorts()

        If MIO.MidiOutPorts.Count = 0 Then Exit Sub

        MidiOutPort0_selected = SelectMidiOutPort(MidiOutPort0_preferred)
        MidiOutPort1_selected = SelectMidiOutPort(MidiOutPort1_preferred)
        MidiOutPort2_selected = SelectMidiOutPort(MidiOutPort2_preferred)
        MidiOutPort3_selected = SelectMidiOutPort(MidiOutPort3_preferred)

        If MidiOutPort0_selected <> "" Then
            MIO.OpenMidiOutPort(MidiOutPort0_selected, hMidiOut0, 0)
        End If

        If MidiOutPort1_selected <> "" Then
            MIO.OpenMidiOutPort(MidiOutPort1_selected, hMidiOut1, 0)
        End If

        If MidiOutPort2_selected <> "" Then
            MIO.OpenMidiOutPort(MidiOutPort2_selected, hMidiOut2, 0)
        End If

        If MidiOutPort3_selected <> "" Then
            MIO.OpenMidiOutPort(MidiOutPort3_selected, hMidiOut3, 0)
        End If

        SetMidiOutPortNameList()

    End Sub

    Private Function SelectMidiOutPort(preferredPort As String) As String
        Dim selectedPort As String = ""

        If preferredPort = "" Then
            Return ""
        End If

        If MIO.MidiOutPorts.Exists(Function(x) x.portName = preferredPort) Then
            selectedPort = preferredPort
        ElseIf AlternativeMidiOutPort = "first available" Then
            selectedPort = MIO.MidiOutPorts(0).portName
        ElseIf AlternativeMidiOutPort <> "" Then
            If MIO.MidiOutPorts.Exists(Function(x) x.portName = AlternativeMidiOutPort) Then
                selectedPort = AlternativeMidiOutPort
            End If
        Else
            selectedPort = ""
        End If

        Return selectedPort
    End Function


    Friend Sub UpdateMidiOutPorts()

        If hMidiOut0 <> 0 Then
            MIO.CloseMidiOutPort(hMidiOut0)
            hMidiOut0 = 0
        End If

        If hMidiOut1 <> 0 Then
            MIO.CloseMidiOutPort(hMidiOut1)
            hMidiOut0 = 1
        End If

        If hMidiOut2 <> 0 Then
            MIO.CloseMidiOutPort(hMidiOut2)
            hMidiOut2 = 0
        End If

        If hMidiOut3 <> 0 Then
            MIO.CloseMidiOutPort(hMidiOut3)
            hMidiOut3 = 0
        End If

        If MidiOutPort0_selected <> "" Then
            MIO.OpenMidiOutPort(MidiOutPort0_selected, hMidiOut0, 0)
        End If

        If MidiOutPort1_selected <> "" Then
            MIO.OpenMidiOutPort(MidiOutPort1_selected, hMidiOut1, 0)
        End If

        If MidiOutPort2_selected <> "" Then
            MIO.OpenMidiOutPort(MidiOutPort2_selected, hMidiOut2, 0)
        End If

        If MidiOutPort3_selected <> "" Then
            MIO.OpenMidiOutPort(MidiOutPort3_selected, hMidiOut3, 0)
        End If

    End Sub

    Friend Sub SetMidiOutPortNameList()

        SequencerBase.MidiOutPortNameList(0).Name = MidiOutPort0_selected
        SequencerBase.MidiOutPortNameList(1).Name = MidiOutPort1_selected
        SequencerBase.MidiOutPortNameList(2).Name = MidiOutPort2_selected
        SequencerBase.MidiOutPortNameList(3).Name = MidiOutPort3_selected

        If MidiOutPort0_preferred = MidiOutPort0_selected Then
            SequencerBase.MidiOutPortNameList(0).IsPreferred = True
        Else
            SequencerBase.MidiOutPortNameList(0).IsPreferred = False
        End If

        If MidiOutPort1_preferred = MidiOutPort1_selected Then
            SequencerBase.MidiOutPortNameList(1).IsPreferred = True
        Else
            SequencerBase.MidiOutPortNameList(1).IsPreferred = False
        End If

        If MidiOutPort2_preferred = MidiOutPort2_selected Then
            SequencerBase.MidiOutPortNameList(2).IsPreferred = True
        Else
            SequencerBase.MidiOutPortNameList(2).IsPreferred = False
        End If

        If MidiOutPort0_preferred = MidiOutPort3_selected Then
            SequencerBase.MidiOutPortNameList(3).IsPreferred = True
        Else
            SequencerBase.MidiOutPortNameList(3).IsPreferred = False
        End If

    End Sub


    Private Sub MidiInData(hmi As UInteger, dwInstance As UInteger, status As Byte, data1 As Byte, data2 As Byte, dwTimestamp As UInteger) Handles MIO.MidiInData

    End Sub

    Private Sub MidiInLongdata(hmi As UInteger, dwInstance As UInteger, buffer As Byte(), dwTimestamp As UInteger) Handles MIO.MidiInLongdata

    End Sub


End Class
