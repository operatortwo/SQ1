Imports System.Text
Imports SequencerBase

Public Class DebugWindow

    Private MainWin As MainWindow = Nothing

    Private strb As New StringBuilder

    Public Sub New(MainWindow As MainWindow)
        InitializeComponent()
        MainWin = MainWindow
    End Sub
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        SetupAudition()
        SetupComposition()
        ShowAuditionTime()
        ShowSequencerTime()
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        MainWin.Activate()
    End Sub

    Private Sub SetupAudition()
        'tvAudition.Items.Clear()
        Dim aud As Composition
        aud = MainWin.Sequencer.Audition

        tvAudition.Tag = aud
        tvVoices.Tag = aud.Voices

        tvVoices.Header = "Voices "

        Dim voiceItem As TreeViewItem
        Dim tracks As TreeViewItem
        Dim trackItem As TreeViewItem
        Dim patternItem As TreeViewItem

        For v = 1 To aud.Voices.Count

            voiceItem = New TreeViewItem
            voiceItem.Header = "Voice " & (v - 1)
            voiceItem.IsExpanded = True
            voiceItem.Tag = aud.Voices(v - 1)
            tvVoices.Items.Add(voiceItem)

            tracks = New TreeViewItem
            tracks.Header = "Tracks"
            tracks.IsExpanded = True
            tracks.Tag = aud.Voices(v - 1).Tracks
            voiceItem.Items.Add(tracks)

            For t = 1 To aud.Voices(v - 1).Tracks.Count

                trackItem = New TreeViewItem
                trackItem.Header = "Track " & (t - 1)
                trackItem.IsExpanded = True
                trackItem.Tag = aud.Voices(v - 1).Tracks(t - 1)
                tracks.Items.Add(trackItem)

                For p = 1 To aud.Voices(v - 1).Tracks(t - 1).PatternList.Count

                    patternItem = New TreeViewItem
                    patternItem.Header = "Pattern " & (p - 1)
                    patternItem.IsExpanded = True
                    patternItem.Tag = aud.Voices(v - 1).Tracks(t - 1).PatternList(p - 1)
                    trackItem.Items.Add(patternItem)

                Next



            Next

        Next



    End Sub

    Private Sub SetupComposition()
        'tvAudition.Items.Clear()
        Dim comp As Composition
        comp = MainWin.Sequencer.Composition

        tvComposition.Tag = comp
        tvcVoices.Tag = comp.Voices

        tvcVoices.Header = "Voices "

        Dim voiceItem As TreeViewItem
        Dim tracks As TreeViewItem
        Dim trackItem As TreeViewItem
        Dim patternItem As TreeViewItem

        For v = 1 To comp.Voices.Count

            voiceItem = New TreeViewItem
            voiceItem.Header = "Voice " & (v - 1)
            voiceItem.IsExpanded = True
            voiceItem.Tag = comp.Voices(v - 1)
            tvcVoices.Items.Add(voiceItem)

            tracks = New TreeViewItem
            tracks.Header = "Tracks"
            tracks.IsExpanded = True
            tracks.Tag = comp.Voices(v - 1).Tracks
            voiceItem.Items.Add(tracks)

            For t = 1 To comp.Voices(v - 1).Tracks.Count

                trackItem = New TreeViewItem
                trackItem.Header = "Track " & (t - 1)
                trackItem.IsExpanded = True
                trackItem.Tag = comp.Voices(v - 1).Tracks(t - 1)
                tracks.Items.Add(trackItem)

                For p = 1 To comp.Voices(v - 1).Tracks(t - 1).PatternList.Count

                    patternItem = New TreeViewItem
                    patternItem.Header = "Pattern " & (p - 1)
                    patternItem.IsExpanded = True
                    patternItem.Tag = comp.Voices(v - 1).Tracks(t - 1).PatternList(p - 1)
                    trackItem.Items.Add(patternItem)

                Next

            Next

        Next



    End Sub

    Public Sub ScreenRefresh()
        Try
            ShowAuditionTime()
            ShowSequencerTime()

            UpdateTbResult()
        Catch

        End Try
    End Sub

    Private Sub ShowAuditionTime()
        tbAudition.Text = CStr(Math.Round(MainWin.Sequencer.AuditionTime, 0))
    End Sub
    Private Sub ShowSequencerTime()
        tbSequencerTime.Text = CStr(Math.Round(MainWin.Sequencer.SequencerTime, 0))
    End Sub


    Private TreeViewSelection As TreeViewItem


    Private Sub TreeView1_SelectedItemChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Object)) Handles TreeView1.SelectedItemChanged
        Dim tv As TreeViewItem
        tv = TryCast(e.NewValue, TreeViewItem)
        If tv IsNot Nothing Then
            TreeViewSelection = tv
            UpdateTbResult()
        End If
    End Sub

    Private Sub UpdateTbResult()

        If TreeViewSelection IsNot Nothing Then
            Dim tag As Object = TreeViewSelection.Tag
            If tag IsNot Nothing Then

                Dim tp As Type
                tp = tag.GetType

                Select Case tp
                    Case GetType(Composition)
                        tbResult.Text = tp.Name
                    Case GetType(List(Of Voice))
                        Dim lst As List(Of Voice) = CType(tag, List(Of Voice))
                        tbResult.Text = "List of Voice, count: " & lst.Count
                    Case GetType(Voice)
                        ShowVoiceParams(tag)
                    Case GetType(List(Of Track))
                        Dim lst As List(Of Track) = CType(tag, List(Of Track))
                        tbResult.Text = "List of Track, count: " & lst.Count
                    Case GetType(Track)
                        ShowTrackParams(tag)
                    Case GetType(List(Of Pattern))
                        Dim lst As List(Of Pattern) = CType(tag, List(Of Pattern))
                        tbResult.Text = "List of Pattern, count: " & lst.Count
                    Case GetType(Pattern)
                        ShowPatternParams(tag)
                    Case Else
                        tbResult.Text = tp.Name

                End Select

            Else
                tbResult.Text = "Nothing"

            End If
        End If

    End Sub

    Private Sub ShowVoiceParams(tag As Object)
        tbResult.Clear()
        Dim vc As Voice
        vc = TryCast(tag, Voice)
        If vc IsNot Nothing Then

            strb.Clear()
            strb.AppendLine("Title = " & vc.Title)
            strb.AppendLine("PortNumber = " & vc.PortNumber)
            strb.AppendLine("MidiChannel = " & vc.MidiChannel)
            strb.AppendLine("NoteTranspose = " & vc.NoteTranspose)
            strb.AppendLine("BankSelectMSB = " & vc.BankSelectMSB)
            strb.AppendLine("BankSelectLSB = " & vc.BankSelectLSB)
            strb.AppendLine("VoiceNumber = " & vc.VoiceNumber)
            strb.AppendLine("VoiceNumberGM = " & vc.VoiceNumberGM)
            strb.AppendLine("NoteOffList-Count = " & vc.NoteOffList.Count)
            strb.AppendLine()
            strb.AppendLine("DebugEventOutList.Count = " & MainWin.Sequencer.DebugEventOutList.Count)

            tbResult.AppendText(strb.ToString)
        End If
    End Sub


    Private Sub ShowTrackParams(tag As Object)

        tbResult.Clear()
        Dim trk As Track
        trk = TryCast(tag, Track)
        If trk IsNot Nothing Then
            strb.Clear()

            strb.AppendLine("Title = " & trk.Title)
            strb.AppendLine("PatternListPtr = " & trk.PatternListPtr)
            strb.AppendLine("PatternList.Count = " & trk.PatternList.Count)
        End If

        tbResult.AppendText(strb.ToString)
    End Sub

    Private Sub ShowPatternParams(tag As Object)

        tbResult.Clear()
        Dim pat As Pattern
        pat = TryCast(tag, Pattern)
        If pat IsNot Nothing Then
            strb.Clear()

            strb.AppendLine("Label = " & pat.Label)
            strb.AppendLine("---------------")
            strb.AppendLine("StartTime =" & pat.StartTime)
            strb.AppendLine("StartOffset =" & pat.StartOffset)
            strb.AppendLine("Length = " & pat.Length)
            strb.AppendLine("Duration = " & pat.Duration)
            strb.AppendLine("DoLoop = " & pat.DoLoop)
            strb.AppendLine("Ended = " & pat.Ended)
            strb.AppendLine("EventListPtr = " & pat.EventListPtr)
            strb.AppendLine("EventList.Count = " & pat.EventList.Count)

            If pat.EventList.Count > 0 Then
                strb.AppendLine()
                strb.AppendLine("--- Event List ---")
            End If

            Dim tev As TrackEvent
            For i = 1 To pat.EventList.Count
                tev = pat.EventList(i - 1)


                If i - 1 = pat.EventListPtr Then
                    strb.AppendLine(TrackEventToString(tev, True))
                Else
                    strb.AppendLine(TrackEventToString(tev, False))
                End If

            Next
        End If

        tbResult.AppendText(strb.ToString)
    End Sub


    Private Function TrackEventToString(tev As TrackEvent, mark As Boolean) As String
        Dim str As String

        str = CStr(tev.Time) & "  "
        str = str & Hex(tev.Status) & "  "
        str = str & Hex(tev.Data1) & "  "
        str = str & Hex(tev.Data2) & "  "
        str = str & tev.Duration & "  "

        If mark = True Then
            str = str & "  <--"
        End If

        Return str
    End Function


    Private Sub btnRefresh_Click(sender As Object, e As RoutedEventArgs) Handles btnRefresh.Click
        'tbAudition.Text = CStr(Math.Round(MainWin.Sequencer.AuditionTime, 0))
        ScreenRefresh()
    End Sub

    Private Sub btnMidiOutList_Click(sender As Object, e As RoutedEventArgs) Handles btnMidiOutList.Click

        If MainWin.Sequencer.AuditionIsRunning Then
            MainWin.Sequencer.Stop_Audition()
        End If

        Dim dlg As New DebugMidiOutList(MainWin)
        dlg.Owner = Me
        dlg.ShowDialog()
    End Sub

    Private Sub btnClearMidiOutList_Click(sender As Object, e As RoutedEventArgs) Handles btnClearMidiOutList.Click
        MainWin.Sequencer.DebugEventOutList.Clear()
        ScreenRefresh()
    End Sub

    Private Sub btnUpdateTreeView_Click(sender As Object, e As RoutedEventArgs) Handles btnUpdateTreeView.Click
        tvcVoices.Items.Clear()
        tvVoices.Items.Clear()
        SetupAudition()
        SetupComposition()
    End Sub
End Class
