Imports SequencerBase

Public Class PatRect_MoveAndSizeWin

    Private Const TPQ As Integer = Sequencer.TPQ

    Private PatternRectangle As PatternRectangle
    Private Pattern As Pattern
    Private PatternList As List(Of Pattern)

    Private TrackCanvas As TrackCanvas
    Private VoiceNumber As Integer
    Private TrackNumber As Integer
    Private Comp As Composition

    Private LeftLimit As UInteger       ' End Position of Previous Pattern or 0 if there is no Pattern
    Private RightLimit As UInteger      ' Start position of Next Pattern or Composition End if there is no Pattern

    Private PositionMin As UInteger     ' End Position of Previous Pattern or 0 if there is no Pattern
    Private PositionMax As UInteger     ' Start position of Next Pattern or Composition End if there is no Pattern
    Private PositionCurrent As UInteger

    Private Const DurationMin As UInteger = Sequencer.TPQ       ' 1 Beat
    Private DurationMax As UInteger     ' Start position to next Pattern Start or Composition End if there is no Pattern
    Private DurationCurrent As UInteger

    Private OriginalDurationMax As UInteger     ' for reset

    Public Sub New(PatternRect As PatternRectangle)

        ' required by the designer
        InitializeComponent()

        PatternRectangle = PatternRect
        Pattern = PatternRect.Pattern

        Comp = PatternRect.ParentCanvas.Composition

    End Sub
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        lblDialogDescription.Content = "Edit Pattern '" & Pattern.Name & "'"

        Dim tobj As Object = FindLogicalParent(PatternRectangle, GetType(TrackCanvas))
        Dim tc As TrackCanvas = TryCast(tobj, TrackCanvas)

        If tc IsNot Nothing Then
            TrackCanvas = tc
            VoiceNumber = tc.VoiceNumber
            TrackNumber = tc.TrackNumber
            'Comp = Sequencer.Composition
            'now in NEW
        End If


        '--- check if base objects are present and valid

        If Comp Is Nothing Or VoiceNumber >= Comp.Voices.Count Or TrackNumber >= Comp.Voices(VoiceNumber).Tracks.Count Then
            MessageBox.Show("Can not continue with this input. Have to close Dialog", "Error")
            Me.Close()
        End If

        PatternList = Comp.Voices(tc.VoiceNumber).Tracks(tc.TrackNumber).PatternList

        '---

        PositionCurrent = Pattern.StartTime
        DurationCurrent = Pattern.Duration

        lblPositionCurrent.Content = PositionCurrent / TPQ
        lblDurationCurrent.Content = DurationCurrent / TPQ

        lblLengthCurrent.Content = Pattern.Length / TPQ

        '--- get left Limit
        'previous pattern.start + duration

        If PatternList IsNot Nothing Then
            Dim ndx As Integer              ' index of current pattern
            ndx = PatternList.FindIndex(Function(x) x.StartTime = Pattern.StartTime)
            If ndx >= 1 Then
                LeftLimit = PatternList(ndx - 1).StartTime + PatternList(ndx - 1).Duration
            Else
                LeftLimit = 0
            End If
        Else
            LeftLimit = 0
        End If

        PositionMin = LeftLimit

        '---get right Limit
        'next pattern start - this pattern.duration

        If PatternList IsNot Nothing Then
            Dim ndx As Integer              ' index of current pattern
            ndx = PatternList.FindIndex(Function(x) x.StartTime = Pattern.StartTime)
            If ndx = -1 Then
                RightLimit = Comp.Length
            ElseIf (ndx + 1) < PatternList.Count Then
                RightLimit = PatternList(ndx + 1).StartTime - Pattern.Duration
            Else
                RightLimit = Comp.Length
            End If
        Else
            RightLimit = Comp.Length - Pattern.Duration
        End If

        PositionMax = RightLimit

        '---get max duration

        'Pattern.StartTime to nextPattern.startTime

        If PatternList IsNot Nothing Then
            Dim ndx As Integer              ' index of current pattern
            ndx = PatternList.FindIndex(Function(x) x.StartTime = Pattern.StartTime)
            If ndx = -1 Then
                DurationMax = Comp.Length - Pattern.StartTime
            ElseIf (ndx + 1) < PatternList.Count Then
                DurationMax = PatternList(ndx + 1).StartTime - Pattern.StartTime
            Else
                DurationMax = Comp.Length - Pattern.StartTime
            End If
        Else
            DurationMax = Comp.Length - Pattern.StartTime
        End If

        OriginalDurationMax = DurationMax           ' for reset

        ShowPositionMinMax()
        ShowDurationMinMax()
    End Sub

    Private Sub ShowPositionMinMax()
        lblPositionMin.Content = PositionMin / TPQ
        lblPositionMax.Content = PositionMax / TPQ
    End Sub

    Private Sub ShowDurationMinMax()
        lblDurationMin.Content = DurationMin / TPQ            ' 1 Beat
        lblDurationMax.Content = DurationMax / TPQ
    End Sub


    Private Sub btnResetValues_Click(sender As Object, e As RoutedEventArgs) Handles btnResetValues.Click
        PositionMin = LeftLimit
        PositionMax = RightLimit

        DurationMax = OriginalDurationMax

        ShowPositionMinMax()
        ShowDurationMinMax()

        PositionCurrent = Pattern.StartTime
        lblPositionCurrent.Content = PositionCurrent / TPQ

        DurationCurrent = Pattern.Duration
        lblDurationCurrent.Content = DurationCurrent / TPQ
    End Sub

    Private Sub repBtnPositionDec_Click(sender As Object, e As RoutedEventArgs) Handles repBtnPositionDec.Click

        If PositionCurrent > PositionMin Then
            PositionCurrent = CUInt(PositionCurrent - Sequencer.TPQ)
            lblPositionCurrent.Content = PositionCurrent / TPQ
            DurationMax = CUInt(DurationMax + TPQ)
            lblDurationMax.Content = DurationMax / TPQ
        End If

    End Sub

    Private Sub repBtnPositionInc_Click(sender As Object, e As RoutedEventArgs) Handles repBtnPositionInc.Click

        If PositionCurrent < PositionMax Then
            PositionCurrent = CUInt(PositionCurrent + Sequencer.TPQ)
            lblPositionCurrent.Content = PositionCurrent / TPQ
            DurationMax = CUInt(DurationMax - TPQ)
            lblDurationMax.Content = DurationMax / TPQ
        End If

    End Sub

    Private Sub repBtnDurationDec_Click(sender As Object, e As RoutedEventArgs) Handles repBtnDurationDec.Click

        If DurationCurrent > DurationMin Then
            DurationCurrent = CUInt(DurationCurrent - Sequencer.TPQ)
            lblDurationCurrent.Content = DurationCurrent / TPQ
            PositionMax = CUInt(PositionMax + TPQ)
            lblPositionMax.Content = PositionMax / TPQ
        End If

    End Sub

    Private Sub repBtnDurationInc_Click(sender As Object, e As RoutedEventArgs) Handles repBtnDurationInc.Click

        If DurationCurrent < DurationMax Then
            DurationCurrent = CUInt(DurationCurrent + Sequencer.TPQ)
            lblDurationCurrent.Content = DurationCurrent / TPQ
            PositionMax = CUInt(PositionMax - TPQ)
            lblPositionMax.Content = PositionMax / TPQ
        End If

    End Sub

    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        '--- save changes
        Pattern.StartTime = PositionCurrent
        Pattern.Duration = DurationCurrent
        TrackCanvas.DrawTrack()                 ' refresh
        Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        ' IsCancel is set at btnCancel
    End Sub

End Class
