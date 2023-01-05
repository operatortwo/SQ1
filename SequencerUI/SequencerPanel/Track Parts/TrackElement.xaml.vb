Public Class TrackElement

#Region "Grid Splitter"
    Public Property CompositionTrack As SequencerBase.Track

    Private Const GridSplitterHeight = 6

    Private Sub GridSplitter1_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles GridSplitter1.PreviewMouseLeftButtonDown
        Dim el As UIElement = CType(sender, UIElement)
        el.CaptureMouse()
    End Sub

    Private Sub GridSplitter1_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles GridSplitter1.PreviewMouseLeftButtonUp
        Dim el As UIElement = CType(sender, UIElement)
        If el.IsMouseCaptured Then
            el.ReleaseMouseCapture()
        End If
    End Sub

    Private Sub GridSplitter1_PreviewMouseMove(sender As Object, e As MouseEventArgs) Handles GridSplitter1.PreviewMouseMove
        Dim el As UIElement = CType(sender, UIElement)
        If el.IsMouseCaptured Then

            If e.LeftButton = MouseButtonState.Pressed Then

                Dim pt As Point
                pt = e.GetPosition(Me)

                If pt.Y < GridSplitterHeight Then pt.Y = GridSplitterHeight

                If pt.Y > MaxHeight Then pt.Y = MaxHeight       ' defined in (TrackElement) UserControl property
                Me.Height = pt.Y

            End If

        End If
    End Sub

    Private Sub tbTrackTitle_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbTrackTitle.TextChanged
        If CompositionTrack IsNot Nothing Then
            If SkipChangedEvent = False Then
                Dim srcTextBox As TextBox
                srcTextBox = CType(e.Source, TextBox)
                CompositionTrack.Title = srcTextBox.Text
            End If
        End If
    End Sub

#End Region


#Region "TrackView"

    Private _AllowUserToChangeTrackViewType As Boolean
    Public Property AllowUserToChangeTrackViewType As Boolean
        Get
            Return AllowUserToChangeTrackViewType
        End Get
        Set(value As Boolean)
            _AllowUserToChangeTrackViewType = value
            If value = True Then
                Me.btnTrackViewType.Visibility = Visibility.Visible
                Me.btnTrackScaleYPlus.Visibility = Visibility.Visible
                Me.btnTrackScaleYMinus.Visibility = Visibility.Visible
                Me.lblZoomY.Visibility = Visibility.Visible
            Else
                Me.btnTrackViewType.Visibility = Visibility.Collapsed
                Me.btnTrackScaleYPlus.Visibility = Visibility.Collapsed
                Me.btnTrackScaleYMinus.Visibility = Visibility.Collapsed
                Me.lblZoomY.Visibility = Visibility.Collapsed
            End If
        End Set
    End Property











#End Region


    Private Sub btnTrackViewType_Click(sender As Object, e As RoutedEventArgs) Handles btnTrackViewType.Click
        ' open dialog to set TrackView Options.
        ' - set TrackViewType
        ' - if TrackViewType.VoiceNotes 
        '                                   RangeMode Or UsedNotesMode
        '                                   RangeStart, RangeEnd
        ' - if TrackViewType.DrumVoiceNotes         
        Dim win As New TrackViewSettings(Me)
        win.Owner = Application.Current.MainWindow
        If win.ShowDialog() = True Then

        End If

    End Sub

#Region "Scale Y"

    Private Const ScaleY_Minimum = 0.1
    Private Const ScaleY_Maximum = 2.0
    Private Const ScaleY_Step = 0.1
    Private _ScaleY As Single = 1
    Public Property ScaleY As Single
        Get
            Return _ScaleY
        End Get
        Set(value As Single)
            If value = _ScaleY Then Exit Property
            If value > ScaleY_Maximum Then value = ScaleY_Maximum
            If value < ScaleY_Minimum Then value = ScaleY_Minimum
            _ScaleY = value
            lblZoomY.Content = Math.Round(_ScaleY, 1)
        End Set
    End Property
    Private Sub btnTrackScaleYPlus_Click(sender As Object, e As RoutedEventArgs) Handles btnTrackScaleYPlus.Click
        If ScaleY < ScaleY_Maximum Then
            ScaleY = CSng(ScaleY + ScaleY_Step)
        End If
    End Sub

    Private Sub btnTrackScaleYMinus_Click(sender As Object, e As RoutedEventArgs) Handles btnTrackScaleYMinus.Click
        If ScaleY > ScaleY_Minimum Then
            ScaleY = CSng(ScaleY - ScaleY_Step)
        End If
    End Sub







#End Region

End Class
