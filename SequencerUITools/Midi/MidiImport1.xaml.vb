Imports System.Collections.ObjectModel
Imports System.Runtime.InteropServices
Imports System.Windows.Controls.Primitives
Imports Microsoft.Win32
Imports Midi_File
Imports SequencerBase

Public Class MidiImport1

    Private comp As Composition

    Public Sub New(comp_ret As Composition)

        ' required by the designer
        InitializeComponent()

        comp = comp_ret

    End Sub


    Private Mid As New Midi_File.CMidiFile

    Private MidiFullname As String = ""

    Public Class TrackRow
        Public Property TrackNumber As Integer
        Public Property IsSelected As Boolean = True
        Public Property TrackName As String = ""
        Public Property NumberOfEvents As Integer           ' NumberOf ALL Events
        Public Property NoteOnEvents As Integer             ' NumberOf (MidiEvent + NoteOn)
        Public Property NoteOffEvents As Integer            ' NumberOf (MidiEvent + NoteOff)
        Public Property PoyphonicAftertouchEvents As Integer    ' NumberOf (MidiEvent + An)
        Public Property ControlChangeEvents As Integer      ' NumberOf (MidiEvent + Bn)
        Public Property ProgramChangeEvents As Integer      ' NumberOf (MidiEvent + Cn)
        Public Property MonophonicAftertouchEvents As Integer    ' NumberOf (MidiEvent + Dn) ChannelAftertouch
        Public Property PitchBendingEvents As Integer    ' NumberOf (MidiEvent + En)
        Public Property MetaEvents As Integer
        Public Property F0SysxEvents As Integer
        Public Property F7SysxEvents As Integer
        Public Property UnknownEvents As Integer
        Public Property NumberOfUsedChannels As Byte
        Public Property ChannelNumber As Byte               ' channel number if IsSingleChannel



    End Class

    ' ev CollectionViewSource in XAML

    Public Property TrackTable As New ObservableCollection(Of TrackRow)
    'Public Property TrackList As New List(Of TrackRow)

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Escape Then
            Close()
        End If
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)

    End Sub

    Private Sub btnLoad_Click(sender As Object, e As RoutedEventArgs) Handles btnLoad.Click
        Dim ofd As New Microsoft.Win32.OpenFileDialog

        ofd.Title = "Midi-File load"
        ofd.Filter = "Midi files|*.mid"

        If ofd.ShowDialog() = False Then Exit Sub
        MidiFullname = ofd.FileName

        tbFullname.Text = MidiFullname
        tbFilename.Text = IO.Path.GetFileName(MidiFullname)

        If Mid.ReadMidiFile(MidiFullname) = True Then

            ShowLoadState(True)                                 ' was loaded without any error
            btnImport.IsEnabled = True                          ' Import is now possible

            Dim str As String = ""

            str &= "SMF Format: " & Mid.SmfFormat
            str &= "   NumOfTracks: " & Mid.NumberOfTracks
            str &= "   TPQ: " & Mid.TPQ
            str &= "   LastTick: " & Mid.LastTick
            str &= "   Beats: " & Math.Round(Mid.LastTick / Mid.TPQ, 0)

            tbProperties.Text = str

        Else
            ShowLoadState(False)
            btnImport.IsEnabled = False
            tbProperties.Text = "Error: " & Mid.ErrorText
        End If

        FillTrackTable(Mid)

        '--- when there are multichannel tracks, enable button to split multichannel tracks to singelchannel tracks

        btnSplit.IsEnabled = False

        For Each track In TrackTable
            If track.NumberOfUsedChannels > 1 Then
                btnSplit.IsEnabled = True
                Exit For
            End If
        Next

        '---

    End Sub

    Private Sub FillTrackTable(mid As CMidiFile)

        TrackTable.Clear()

        For i = 1 To mid.TrackList.Count
            Dim row As New TrackRow

            row.TrackNumber = i
            row.TrackName = mid.GetTrackName(i - 1)
            row.NumberOfEvents = mid.TrackList(i - 1).EventList.Count

            For Each trev In mid.TrackList(i - 1).EventList
                If IsNoteOnEvent(trev) Then
                    row.NoteOnEvents += 1
                ElseIf IsNoteOffEvent(trev) Then
                    row.NoteOffEvents += 1
                ElseIf IsControlChangeEvent(trev) Then              ' Bn
                    row.ControlChangeEvents += 1
                ElseIf IsProgramChangeEvent(trev) Then              ' Cn
                    row.ProgramChangeEvents += 1
                ElseIf IsPitchBendingEvent(trev) Then               ' En
                    row.PitchBendingEvents += 1
                ElseIf IsMetaEvent(trev) Then
                    row.MetaEvents += 1
                ElseIf IsPolyphonicAftertouchEvent(trev) Then       ' An
                    row.PoyphonicAftertouchEvents += 1
                ElseIf IsMonophonicAftertouchEvent(trev) Then       ' Dn
                    row.MonophonicAftertouchEvents += 1
                ElseIf IsF0SysxEvent(trev) Then
                    row.F0SysxEvents += 1
                ElseIf IsF7SysxEvent(trev) Then
                    row.F7SysxEvents += 1
                ElseIf IsUnknownEvent(trev) Then
                    row.UnknownEvents += 1
                End If
            Next

            Dim channel As Byte
            Dim cnt As Byte = GetNumberOfUsedChannels(mid.TrackList(i - 1).EventList, channel)
            row.NumberOfUsedChannels = cnt
            If cnt = 1 Then
                row.ChannelNumber = channel
            End If

            TrackTable.Add(row)
        Next

    End Sub

    Private Sub btnSplit_Click(sender As Object, e As RoutedEventArgs) Handles btnSplit.Click

        If SplitMultichannelTracks(Mid) = True Then
            FillTrackTable(Mid)
            btnSplit.IsEnabled = False
        Else
            MessageBox.Show("Split to Singlechannel tracks failed", "Split")
        End If

    End Sub

    Private Function SplitMultichannelTracks(ByRef mid As CMidiFile) As Boolean
        Dim tl2 As New List(Of CMidiFile.TrackChunk)
        Try
            For i = 1 To TrackTable.Count
                If TrackTable(i - 1).NumberOfUsedChannels <= 1 Then
                    tl2.Add(mid.TrackList(TrackTable(i - 1).TrackNumber - 1))            ' copy unchanged
                Else
                    '--- split 
                    Dim src As CMidiFile.TrackChunk
                    src = mid.TrackList(TrackTable(i - 1).TrackNumber - 1)
                    Dim dst As New List(Of CMidiFile.TrackChunk)
                    For di = 1 To 16                                                ' all 16 midi channels
                        dst.Add(New CMidiFile.TrackChunk)
                    Next

                    Dim chn As Byte
                    For Each srcev In src.EventList
                        chn = GetChannelNumberOfEvent(srcev)
                        dst(chn).EventList.Add(srcev)
                    Next

                    '- remove unused TracksChunks
                    For t = 15 To 0 Step -1
                        If dst(t).EventList.Count = 0 Then
                            dst.RemoveAt(t)
                        End If
                    Next

                    '- add each track to tl2
                    For Each tc In dst
                        tl2.Add(tc)
                    Next


                End If
            Next

        Catch ex As Exception
            Return False                ' mid is unchanged
        End Try

        mid.TrackList = tl2
        Return True
    End Function

    Private Sub btnImport_Click(sender As Object, e As RoutedEventArgs) Handles btnImport.Click

        If IsAtLeast1TrackSelected() = False Then
            MessageBox.Show("At least 1 Track need to ber selected", "Import")
            Exit Sub
        End If


        comp.TicksPerQuarterNote = Sequencer.TPQ
        comp.Name = IO.Path.GetFileNameWithoutExtension(Mid.MidiName)
        comp.Comments = "Imported from Midi.File"

        '--- convert Event-Times
        Try
            For Each track In Mid.TrackList
                For Each ev In track.EventList
                    ev.Time = ev.Time * Sequencer.TPQ / Mid.TPQ
                Next
            Next
        Catch ex As Exception
            MessageBox.Show("Error converting Event-Time. Import cancelled.", "Import from MidiFile")
            Exit Sub
        End Try

        '--- convert Event-Durations
        Try
            For Each track In Mid.TrackList
                For Each ev In track.EventList
                    ev.Duration = ev.Duration * Sequencer.TPQ / Mid.TPQ
                Next
            Next
        Catch ex As Exception
            MessageBox.Show("Error converting Event-Duration. Import cancelled.", "Import from MidiFile")
            Exit Sub
        End Try

        '--- remove Note off's
        Dim mtrev As CMidiFile.TrackEvent

        For Each track In Mid.TrackList
            For i = track.EventList.Count To 1 Step -1
                mtrev = track.EventList(i - 1)
                If IsNoteOffEvent(mtrev) Then
                    track.EventList.RemoveAt(i - 1)
                End If
            Next
        Next

        '--- remove EndOfTrack's

        For Each track In Mid.TrackList
            If track.EventList.Count = 0 Then Continue For
            Dim ndx As Integer = track.EventList.Count - 1          ' last
            Dim trev As CMidiFile.TrackEvent
            trev = track.EventList(ndx)
            If IsEndOfTrackEvent(trev) Then
                track.EventList.RemoveAt(ndx)
            End If
        Next

        '--- get Track Length's

        Dim time As UInteger
        Dim lasttime As UInteger

        For Each track In Mid.TrackList
            If track.EventList.Count > 0 Then
                time = track.EventList.Last().Time
                If time > lasttime Then lasttime = time
            End If
        Next

        Dim remainder As UInteger = lasttime Mod Sequencer.TPQ
        If remainder > 0 Then
            lasttime += (Sequencer.TPQ - remainder)        ' round up to next beat
        End If

        comp.Length = lasttime

        'try to find tempo -> composition tempo

        '--- 

        comp.Voices.Clear()

        For i = 1 To TrackTable.Count
            If TrackTable(i - 1).IsSelected = False Then Continue For

            Dim vc As New SequencerBase.Voice
            Dim trk As New SequencerBase.Track

            vc.Tracks.Add(trk)

            If TrackTable(i - 1).NumberOfUsedChannels = 1 Then
                vc.MidiChannel = TrackTable(i - 1).ChannelNumber
            Else
                vc.IsMultiChannel = True
            End If

            vc.Title = TrackTable(i - 1).TrackName
            'trk.Title = TrackList(i - 1).TrackName
            trk.Title = i - 1

            'create pattern

            Dim pat As New Pattern

            pat.Label = TrackTable(i - 1).TrackName
            pat.StartTime = 0
            pat.Length = comp.Length
            pat.Duration = comp.Length


            Dim src As List(Of CMidiFile.TrackEvent) = Mid.TrackList(i - 1).EventList

            For Each srcev In src
                Dim dstev As New TrackEvent
                dstev.Time = srcev.Time
                dstev.Type = srcev.Type
                dstev.Status = srcev.Status
                dstev.Data1 = srcev.Data1
                dstev.Data2 = srcev.Data2
                dstev.DataX = srcev.DataX
                dstev.Duration = srcev.Duration
                pat.EventList.Add(dstev)
            Next

            trk.PatternList.Add(pat)

            comp.Voices.Add(vc)

        Next


        DialogResult = True
        Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Close()
    End Sub

    Private Sub ShowLoadState(isOk As Boolean)
        If isOk = True Then
            imgLoadOk.Visibility = Visibility.Visible
            imgLoadFail.Visibility = Visibility.Hidden
        Else
            imgLoadFail.Visibility = Visibility.Visible
            imgLoadOk.Visibility = Visibility.Hidden
        End If

    End Sub


    Private Function IsNoteOnEvent(trev As Midi_File.CMidiFile.TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.MidiEvent Then
            Dim stat As Byte = trev.Status And &HF0
            If stat = &H90 Then
                If trev.Data2 > 0 Then Return True
            End If
        End If
        Return False
    End Function
    Private Function IsNoteOffEvent(trev As Midi_File.CMidiFile.TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.MidiEvent Then
            Dim stat As Byte = trev.Status And &HF0
            If (stat = &H90 And trev.Data2 = 0) OrElse (stat = &H80) Then Return True
        End If
        Return False
    End Function
    Private Function IsPolyphonicAftertouchEvent(trev As Midi_File.CMidiFile.TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.MidiEvent Then
            Dim stat As Byte = trev.Status And &HF0
            If stat = &HA0 Then Return True
        End If
        Return False
    End Function
    Private Function IsControlChangeEvent(trev As Midi_File.CMidiFile.TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.MidiEvent Then
            Dim stat As Byte = trev.Status And &HF0
            If stat = &HB0 Then Return True
        End If
        Return False
    End Function
    Private Function IsProgramChangeEvent(trev As Midi_File.CMidiFile.TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.MidiEvent Then
            Dim stat As Byte = trev.Status And &HF0
            If stat = &HC0 Then Return True
        End If
        Return False
    End Function
    Private Function IsMonophonicAftertouchEvent(trev As Midi_File.CMidiFile.TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.MidiEvent Then
            Dim stat As Byte = trev.Status And &HF0
            If stat = &HD0 Then Return True
        End If
        Return False
    End Function
    Private Function IsPitchBendingEvent(trev As Midi_File.CMidiFile.TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.MidiEvent Then
            Dim stat As Byte = trev.Status And &HF0
            If stat = &HE0 Then Return True
        End If
        Return False
    End Function
    Private Function IsF0SysxEvent(trev As Midi_File.CMidiFile.TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.F0SysxEvent Then Return True
        Return False
    End Function
    Private Function IsF7SysxEvent(trev As Midi_File.CMidiFile.TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.F7SysxEvent Then Return True
        Return False
    End Function
    Private Function IsMetaEvent(trev As Midi_File.CMidiFile.TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.MetaEvent Then Return True
        Return False
    End Function
    Private Function IsUnknownEvent(trev As Midi_File.CMidiFile.TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.Unkown Then Return True
        Return False
    End Function
    Private Function IsEndOfTrackEvent(trev As Midi_File.CMidiFile.TrackEvent) As Boolean
        If trev.Type = Midi_File.EventType.MetaEvent Then
            If trev.Data1 = Midi_File.MetaEventType.EndOfTrack Then Return True
        End If
        Return False
    End Function

    Private Function IsAtLeast1TrackSelected() As Boolean
        For Each row In TrackTable
            If row.IsSelected = True Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Eventlist"></param>
    ''' <param name="chn">returns first channel number in List</param>
    ''' <returns></returns>
    Private Function GetNumberOfUsedChannels(Eventlist As List(Of CMidiFile.TrackEvent), ByRef chn As Byte) As Byte
        Dim stat As Byte
        Dim channel As Byte

        Dim chlist As New List(Of Byte)(16)

        For Each trev In Eventlist
            stat = trev.Status And &HF0
            If stat >= &H80 And stat < &HF0 Then        ' exclude F0, F7
                channel = trev.Status And &HF
                If Not chlist.Contains(channel) Then
                    chlist.Add(channel)
                End If
            End If
        Next

        If chlist.Count > 0 Then
            chn = chlist(0)             ' first item in list
        End If

        Return chlist.Count
    End Function

    Private Function GetChannelNumberOfEvent(trev As CMidiFile.TrackEvent) As Byte
        Dim stat As Byte
        Dim channel As Byte

        stat = trev.Status And &HF0
        If stat >= &H80 And stat < &HF0 Then        ' exclude F0, F7    (choose channel 0)
            channel = trev.Status And &HF
        End If

        Return channel
    End Function


End Class
