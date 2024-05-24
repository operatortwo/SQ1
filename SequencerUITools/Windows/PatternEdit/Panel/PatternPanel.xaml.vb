Imports System.ComponentModel
Imports System.Security.Cryptography.X509Certificates
Imports DailyUserControls
Imports SequencerBase

Public Class PatternPanel
    Inherits UserControl

    Public Sub New()

        ' this call is required for the Designer
        InitializeComponent()

    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        InitializePatternAdorner()
        Dim en As String() = [Enum].GetNames(GetType(KeyTextOptions))
        cmbKeyText.ItemsSource = en
        NotePanel.PatternPanel = Me
        UpdatePanel()
        selBtnKeyMode.Value = KeyMode.UsedRange
    End Sub

    Private Sub UpdatePanel()
        InitializePatternInfo(Pattern)
        CreateNoteList()
        SetVscrollValues()
        DrawKeys()
        DrawMeasure()
        DrawPattern()
    End Sub

#Region "Properties"

#Region "Appearance Properties"



#End Region

#Region "Sequencer Properties"

    Private Shared NewPattern As New Pattern With {.Name = "New Pattern", .Length = DefaultTPQ, .Duration = DefaultTPQ}

    Public Shared ReadOnly PatternProperty As DependencyProperty = DependencyProperty.Register("Pattern", GetType(Pattern), GetType(PatternPanel), New FrameworkPropertyMetadata(NewPattern.Copy, New PropertyChangedCallback(AddressOf OnPatternChanged), Nothing))
    <Description("The Pattern to edit."), Category("PatternPanel")>
    Public Property Pattern() As Pattern
        Get
            Return GetValue(PatternProperty)
        End Get
        Set(ByVal value As Pattern)
            SetValue(PatternProperty, value)
        End Set
    End Property

    Private Shared Sub OnPatternChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As PatternPanel = CType(d, PatternPanel)
        control.UpdatePanel()
    End Sub


    Public Shared ReadOnly VoiceProperty As DependencyProperty = DependencyProperty.Register("Voice", GetType(Voice), GetType(PatternPanel), New FrameworkPropertyMetadata)
    <Description("Voice for manually Midi-Out, f.e. Playing Notes on the KeyPanel."), Category("PatternPanel")>
    Public Property Voice As Voice
        Get
            Return GetValue(VoiceProperty)
        End Get
        Set(value As Voice)
            SetValue(VoiceProperty, value)
        End Set
    End Property


#End Region

#Region "MIDI Properties"

    Private Const DefaultTPQ As UInteger = 960
    Private Const MinTPQ = 96
    Private Const MaxTPQ = 1920


    Public Shared ReadOnly TicksPerQuarterNoteProperty As DependencyProperty = DependencyProperty.Register("TicksPerQuarterNote", GetType(UInteger), GetType(PatternPanel), New FrameworkPropertyMetadata(DefaultTPQ, New PropertyChangedCallback(AddressOf OnTickPerQuarterNoteChanged), New CoerceValueCallback(AddressOf CoerceTicksPerQuarterNote)))
    <Description("960 for SQ1. Can be customized for third-party applications. Range: 96 to 1920"), Category("PatternPanel")>
    Public Property TicksPerQuarterNote() As UInteger
        Get
            Return CInt(GetValue(TicksPerQuarterNoteProperty))
        End Get
        Set(value As UInteger)
            SetValue(TicksPerQuarterNoteProperty, value)
        End Set
    End Property

    Private Overloads Shared Function CoerceTicksPerQuarterNote(ByVal d As DependencyObject, ByVal value As Object) As Object
        Dim newValue As UInteger = value

        If newValue > MaxTPQ Then
            Return MaxTPQ
        ElseIf newValue < MinTPQ Then
            Return MinTPQ
        End If

        Return newValue
    End Function

    Private Shared Sub OnTickPerQuarterNoteChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As PatternPanel = CType(d, PatternPanel)
        control.SetupPanel()
    End Sub

#End Region

#End Region

#Region "Public Procedures"


    Public Sub ScreenRefresh(PlayerTime As UInteger)
        If PlayPositionAdorner1 IsNot Nothing Then
            PlayPositionAdorner1.ScaleX = sldScaleX.Value

            Dim time As UInteger

            time = PlayerTime

            'time = (PlayerTime - Pattern.StartTime) Mod Pattern.Length

            PlayPositionAdorner1.PatternTime = time
            MeasureAdornerLayer.Update()
        End If
    End Sub
    ''' <summary>
    ''' Redraw the Panel
    ''' </summary>
    Public Sub UpdatePatternPanel()
        UpdatePanel()
    End Sub


#End Region

#Region "Control"

    Private Sub MeasurePanel_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles MeasurePanel.SizeChanged
        DrawMeasure
    End Sub

    Private Sub NotePanel_MouseMove(sender As Object, e As MouseEventArgs) Handles NotePanel.MouseMove
        Dim pt As Point
        pt = e.GetPosition(NotePanel)

        Dim pos As UInteger
        pos = PixelToTicks(pt.X)
        lblMouseTimePosition.Content = TimeTo_MBT(pos)
        Dim tev As TrackEvent = PosXYtoTrackEvent(Pattern, pt.X, pt.Y)
        If tev IsNot Nothing Then
            Dim sb As New Text.StringBuilder
            sb.Append(Hex(tev.Status) & "h ")
            sb.Append("n:" & tev.Data1 & " ")
            sb.Append("v:" & tev.Data2 & " ")
            sb.Append("dur:" & tev.Duration & " ")
            sb.Append("from:" & tev.Time & " ")
            sb.Append("to:" & tev.Time + tev.Duration & " ")
            lblEventInfo.Content = sb.ToString
        Else
            lblEventInfo.Content = "no hit"
        End If
    End Sub

    Private Sub NotePanel_MouseLeave(sender As Object, e As MouseEventArgs) Handles NotePanel.MouseLeave
        lblMouseTimePosition.Content = ""
        lblEventInfo.Content = ""
    End Sub

    Private Sub sldScaleX_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldScaleX.ValueChanged
        If Me.IsLoaded = True Then
            SetHscrollValues()
            DrawPattern()
        End If
    End Sub

    Private Sub sldScaleY_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldScaleY.ValueChanged
        If IsLoaded = True Then
            Dim vscvrel As Double = MasterVScroll.Value / e.OldValue

            SetVscrollValues()
            DrawKeys()

            ' scaleY has changed, try to keep the same "First note row on screen"
            MasterVScroll.Value = vscvrel * e.NewValue
            KeyPanelScroll_ScrollV()
            NotePanelScroll_ScrollV()
        End If
    End Sub

#Region "H Scroll"
    Private Sub MasterHScroll_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles MasterHScroll.SizeChanged
        SetHscrollValues()
    End Sub

    Private Sub MasterHScroll_Scroll(sender As Object, e As Primitives.ScrollEventArgs) Handles MasterHScroll.Scroll
        MeasurePanelScroll_ScrollH()
        NotePanelScroll_ScrollH()
    End Sub

    Private Sub SetHscrollValues()

        If Pattern IsNot Nothing Then
                Dim CanvasLength As Double
                Dim Maximum As Double
                Dim ActualWidth As Double

                CanvasLength = Pattern.Length * TicksToPixelFactor * sldScaleX.Value
                ActualWidth = MasterHScroll.ActualWidth
                Maximum = CanvasLength - ActualWidth
                If Maximum < 0 Then Maximum = 0

                MasterHScroll.Maximum = CanvasLength - ActualWidth
                MasterHScroll.ViewportSize = ActualWidth

                MasterHScroll.SmallChange = Math.Round(0.02 * Maximum, 0)
                MasterHScroll.LargeChange = Math.Round(0.1 * Maximum, 0)

            End If

    End Sub

    Private Sub MeasurePanelScroll_ScrollH()
        MeasurePanelScroll.ScrollToHorizontalOffset(MasterHScroll.Value)
    End Sub

    Private Sub NotePanelScroll_ScrollH()
        NotePanelScroll.ScrollToHorizontalOffset(MasterHScroll.Value)
    End Sub

#End Region

#Region "V Scroll"
    Private Sub MasterVScroll_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles MasterVScroll.SizeChanged
        SetVscrollValues()
    End Sub

    Private Sub MasterVScroll_Scroll(sender As Object, e As Primitives.ScrollEventArgs) Handles MasterVScroll.Scroll
        KeyPanelScroll_ScrollV()
        NotePanelScroll_ScrollV()
    End Sub

    Private Sub SetVscrollValues()

        Dim CanvasHeight As Double
        Dim Maximum As Double
        Dim ActualHeight As Double

        CanvasHeight = NumberOfNoteRows * PixelPerNoteRow * sldScaleY.Value
        ActualHeight = MasterVScroll.ActualHeight
        Maximum = CanvasHeight - ActualHeight
        If Maximum < 0 Then Maximum = 0

        MasterVScroll.Maximum = CanvasHeight - ActualHeight
        MasterVScroll.ViewportSize = ActualHeight

        MasterHScroll.SmallChange = Math.Round(0.02 * Maximum, 0)
        MasterHScroll.LargeChange = Math.Round(0.1 * Maximum, 0)

        NotePanel.Height = CanvasHeight
    End Sub

    Private Sub KeyPanelScroll_ScrollV()
        KeyPanelScroll.ScrollToVerticalOffset(MasterVScroll.Value)
    End Sub

    Private Sub NotePanelScroll_ScrollV()
        NotePanelScroll.ScrollToVerticalOffset(MasterVScroll.Value)
    End Sub


#End Region


    Private Sub KeyPanel_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles KeyPanel.MouseLeftButtonDown
        Dim pt As Point
        pt = e.GetPosition(KeyPanel)
        Dim notenum As Integer = PosYtoNoteNumber(pt.Y)
        If notenum <> -1 Then
            KeyNoteNumber_playing = notenum
            Voice.Manually_OutShort(0, &H90, notenum, 100)                      ' Note on
        End If
    End Sub

    Private Sub KeyPanel_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles KeyPanel.MouseLeftButtonUp
        Voice.Manually_OutShort(0, &H90, KeyNoteNumber_playing, 0)              ' Note off
        KeyNoteNumber_playing = 128                                             ' invalidate notenumber
    End Sub
    Private Sub KeyPanel_MouseMove(sender As Object, e As MouseEventArgs) Handles KeyPanel.MouseMove
        Dim pt As Point
        pt = e.GetPosition(KeyPanel)

        Dim rownum As Integer = Fix(pt.Y / PixelPerNoteRow / sldScaleY.Value)
        Dim notenum As Integer
        notenum = PosYtoNoteNumber(pt.Y)

        lblMouseKeyPos1.Content = "Y: " & pt.Y
        lblMouseKeyPosition.Content = " r: " & rownum & " n: " & notenum

        '---
        If e.LeftButton = MouseButtonState.Pressed Then
            If notenum <> -1 Then
                If notenum <> KeyNoteNumber_playing Then
                    Voice.Manually_OutShort(0, &H90, KeyNoteNumber_playing, 0)          ' Note off                
                    Voice.Manually_OutShort(0, &H90, notenum, 100)                      ' Note on
                    KeyNoteNumber_playing = notenum
                End If
            End If
        End If
    End Sub

    Private Sub KeyPanel_MouseLeave(sender As Object, e As MouseEventArgs) Handles KeyPanel.MouseLeave
        lblMouseKeyPos1.Content = ""
        lblMouseKeyPosition.Content = ""
        '---
        If KeyNoteNumber_playing < 128 Then
            Voice.Manually_OutShort(0, &H90, KeyNoteNumber_playing, 0)          ' Note off                
            KeyNoteNumber_playing = 128                             ' invalidate notenumber
        End If
    End Sub

    Private Sub selBtnKeyMode_Click(sender As Object, e As RoutedEventArgs) Handles selBtnKeyMode.Click
        Dim dlg As New KeyModeDlg(Me)
        dlg.Owner = SequencerUI.FindLogicalParent(Me, GetType(PatternEditWin))
        dlg.ShowDialog()
    End Sub

    Private Sub selBtnKeyMode_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Integer)) Handles selBtnKeyMode.ValueChanged

        If [Enum].IsDefined(GetType(KeyMode), e.NewValue) Then
            tblkKeyMode.Text = GetKeyModeName(e.NewValue)
            PanelKeyMode = e.NewValue
        End If

    End Sub

    Private Sub cmbKeyText_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbKeyText.SelectionChanged
        If IsLoaded = True Then
            KeyTextOption = cmbKeyText.SelectedIndex
            SetVscrollValues()
            DrawKeys()
        End If
    End Sub

    Private Sub KeyPanel_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs) Handles KeyPanel.PreviewMouseWheel
        ' e.Delta is negative if the mouse wheel is rotated in a downward direction (toward the user). 
        If e.Delta < 0 Then
            MasterVScroll.Value += PixelPerNoteRow
        Else
            MasterVScroll.Value -= PixelPerNoteRow
        End If
        KeyPanelScroll_ScrollV()
        NotePanelScroll_ScrollV()
        e.Handled = True
    End Sub

    Private Sub NotePanel_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs) Handles NotePanel.PreviewMouseWheel
        ' e.Delta is negative if the mouse wheel is rotated in a downward direction (toward the user). 
        If e.Delta < 0 Then
            MasterVScroll.Value += PixelPerNoteRow
        Else
            MasterVScroll.Value -= PixelPerNoteRow
        End If
        KeyPanelScroll_ScrollV()
        NotePanelScroll_ScrollV()
        e.Handled = True
    End Sub





#End Region









End Class
