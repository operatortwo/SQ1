Imports Midi_Keyboard

Public Class RemoteControl

    Private MainWin As MainWindow = Nothing

    Public RemoteControlWindow As RemoteControl

    Private LastWindowPosition As New Point

    Public Sub New(MainWindow As MainWindow)
        InitializeComponent()
        MainWin = MainWindow

        '--- follow MainWindow Location ---
        AddHandler MainWin.LocationChanged, AddressOf ParentWindow_LocationChanged
        '--- remove LocationChanged Handler when closing, to avoid multiple Handlers (when opening again)
        AddHandler Me.Closing, AddressOf RemoteControlWindow_closing

        LastWindowPosition.X = MainWin.Left()
        LastWindowPosition.Y = MainWin.Top

        Dim scale As Double = GetScreenScale(MainWin)
        Dim pt As Point
        pt = MainWin.PointToScreen(New Point)

        Me.Left = pt.X + 40 / scale
        Me.Top = pt.Y + 60 / scale


    End Sub

    Public Sub OpenOrActivate_RemoteControlWindow()

        If IsWindowOpen(RemoteControlWindow) Then
            RemoteControlWindow.Activate()
        Else
            RemoteControlWindow = New RemoteControl(MainWin)
            RemoteControlWindow.Owner = Me
            '--- follow MainWindow Location ---
            AddHandler Me.LocationChanged, AddressOf ParentWindow_LocationChanged
            '--- remove LocationChanged Handler when closing, to avoid multiple Handlers (when opening again)
            AddHandler RemoteControlWindow.Closing, AddressOf RemoteControlWindow_closing

            LastWindowPosition.X = Me.Left()
            LastWindowPosition.Y = Me.Top

            Dim scale As Double = GetScreenScale(Me)
            Dim pt As Point
            pt = Me.PointToScreen(New Point)

            RemoteControlWindow.Left = pt.X + 40 / scale
            RemoteControlWindow.Top = pt.Y + 60 / scale

            RemoteControlWindow.Show()
        End If

    End Sub

    Private Sub ParentWindow_LocationChanged(sender As Object, e As EventArgs)
        Dim ParentWindow = MainWin
        Dim newpos As New Point With {.X = ParentWindow.Left(), .Y = ParentWindow.Top}
        Dim pdiff As New Point

        pdiff.X = LastWindowPosition.X - ParentWindow.Left()
        pdiff.Y = LastWindowPosition.Y - ParentWindow.Top

        If IsWindowOpen(Me) Then
            Me.Left -= pdiff.X
            Me.Top -= pdiff.Y

            LastWindowPosition.X = ParentWindow.Left()
            LastWindowPosition.Y = ParentWindow.Top
        End If

    End Sub

    Private Sub RemoteControlWindow_closing(sender As Object, e As EventArgs)
        RemoveHandler MainWin.LocationChanged, AddressOf ParentWindow_LocationChanged
    End Sub

    Private Function GetScreenScale(visual As Visual) As Double
        ' assuming ScaleX = ScaleY

        Dim scale As Double
        Try
            Dim pt1 As Point
            Dim pt2 As Point

            pt1 = visual.PointToScreen(New Point(0, 0))
            pt2 = visual.PointToScreen(New Point(100, 100))
            scale = (pt2.X - pt1.X) / 100
        Catch
            Return 1.0                  ' in case of invalid visual
        End Try

        If scale <= 0 Then
            scale = 1                   ' reset to default in case of invalid value
        End If

        Return scale
    End Function

    Private Function IsWindowOpen(win As Window) As Boolean
        If IsNothing(win) OrElse win.IsLoaded = False Then
            Return False
        Else
            Return True
        End If
    End Function

    Private Sub btnRestart_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnRestart.PreviewMouseDown
        MainWin.Command_Restart()
    End Sub

    Private Sub btnStop_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnStop.PreviewMouseDown
        MainWin.Command_Stop()
    End Sub

    Private Sub btnPlay_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles btnPlay.PreviewMouseDown
        MainWin.Command_Play()
    End Sub
End Class
