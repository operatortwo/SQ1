Imports System.ComponentModel
Imports SequencerBase

Public Class TrackCanvas

    Private SeqPanel As SequencerPanel
    Private Trkel As TrackElement
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        Dim obj As Object = FindLogicalParent(Me, GetType(SequencerPanel))
        SeqPanel = TryCast(obj, SequencerPanel)
        obj = FindLogicalParent(Me, GetType(TrackElement))
        Trkel = TryCast(obj, TrackElement)
    End Sub


    ' Length in SequencerTicks, assuming Ticks per Quater Note = 960
    ' as example: at 120 BPM there is the need for 30 bars per Minute

    Private Const DefaultLength As Double = 4 * 960                     ' 4 beats (= 1 bar = 1 measure)
    Private Const MinimumLength As Double = 1 * 960                     ' 1 beats 
    Private Const MaximumLength As Double = 100 * 30 * 4 * 960          ' 100 Minutes at 120 BPM

    Private Const DefaultScaleX As Double = 1
    Private Const MinimumScaleX As Double = 0.1
    Private Const MaximumScaleX As Double = 10

    Private LastPositionOnTrack As UInteger                             ' in Sequencer ticks, from MouseMove
    Private RightClickPositionOnTrack As UInteger                       ' in Sequencer Ticks, from ContextMenuOpening

    ' 3840 = 240 px (device-independent units (1/96th inch per unit)
    ' f.e. 3840 / 16 = 240, set to "Width" property of canvas
    'Private Const TicksToPixelFactor = 0.0625     ' ( 1 / 16 )
    'Private Const TicksToPixelFactor = 0.04       ' ( 1 / 25 )
    'Public Const TicksToPixelFactor = 0.03125       ' ( 1 / 32 )
    'Public Const PixelToTicksFactor = 32            ' ( 1 / TicksToPixelFactor )


    Public Property NoteRangeStart As Byte = 0          ' first note
    Public Property NoteRangeEnd As Byte = 127          ' last note
    Public Property ShowOnlyUsedNotes As Boolean        ' when TrackView is VoiceNotes or DrumVoiceNotes
    Public Property ListOfUsedNotes As New List(Of Byte)
    ' NoteRange and ShowOnlyUsedNotes has only affect to the TrackView, the CompositionTrack itself
    ' still have full NoteRange

    Private Const PixelPerNote = 40                 '  40 Pixel per Note (Key)    (Y)
    ' Scale Y = 0.1 - 2.0  (4 - 80 pixel)



    ''' <summary>
    ''' How TrackCanvas looks like
    ''' </summary>
    Public Enum TrackViewType
        Pattern
        VoiceNotes
        DrumVoiceNotes
    End Enum

    Private _TrackView As TrackViewType = TrackViewType.Pattern
    Public Property TrackView As TrackViewType
        Get
            Return _TrackView
        End Get
        Set(value As TrackViewType)
            If value = _TrackView Then Exit Property
            _TrackView = value
            If _TrackView = TrackViewType.Pattern Then
                Trkel.NoteIndexColumn.Visibility = Visibility.Collapsed
                'TrackCanvas.DrawTrack   ' refresh                
            ElseIf TrackView = TrackViewType.VoiceNotes Then
                Trkel.NoteIndexColumn.Visibility = Visibility.Visible
                ' set NoteRange and ListOfUsedNotes
                'TrackCanvas.DrawTrack   ' refresh
            ElseIf TrackView = TrackViewType.DrumVoiceNotes Then
                Trkel.NoteIndexColumn.Visibility = Visibility.Visible
                ' set NoteRange and ListOfUsedNotes
                'TrackCanvas.DrawTrack   ' refresh
            End If
        End Set
    End Property

    'noteIndexColumn

#Region "Size"
    Private Const LengthDescription As String = "Length in Sequencer Ticks, depending on Sequencer, at this moment 960 Ticks per Quarter Note.
The Minimum value is 4 * 960, the maximum value is (100 * 30 * 4 * 960) which is approximately 100 Minutes at 120 BPM"

    Public Shared ReadOnly LengthProperty As DependencyProperty = DependencyProperty.Register("Length", GetType(Double), GetType(TrackCanvas), New FrameworkPropertyMetadata(DefaultScaleX, New PropertyChangedCallback(AddressOf OnLengthChanged), New CoerceValueCallback(AddressOf CoerceLength)))
    ' appears in code
    ''' <summary>
    ''' Length in Sequencer Ticks, depending on Sequencer. At this Moment 960 Ticks per Quarter Note.
    ''' </summary>        
    <Description(LengthDescription), Category("TrackCanvas")>       ' appears in VS property
    Public Property Length() As Double
        Get
            Return CDbl(GetValue(LengthProperty))
        End Get
        Set(ByVal value As Double)
            SetValue(LengthProperty, value)
        End Set
    End Property

    Private Overloads Shared Function CoerceLength(ByVal d As DependencyObject, ByVal value As Object) As Object
        Dim newValue As Double = CDbl(value)

        If newValue < MinimumLength Then
            Return MinimumLength
        ElseIf newValue > MaximumLength Then
            Return MaximumLength
        End If

        Return newValue
    End Function

    Private Shared Sub OnLengthChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As TrackCanvas = CType(d, TrackCanvas)
        Dim value As Double
        value = control.Length * TicksToPixelFactor * control.ScaleX

        control.Canvas1.Width = value
        control.DrawTrack()

        'd.SetCurrentValue(WidthProperty, value)
        'SetCurrentValue changes the effective value Of the Property, but existing triggers, Data bindings,
        'And styles will continue to work.
    End Sub


    Public Shared ReadOnly ScaleXProperty As DependencyProperty = DependencyProperty.Register("ScaleX", GetType(Double), GetType(TrackCanvas), New FrameworkPropertyMetadata(DefaultScaleX, New PropertyChangedCallback(AddressOf OnScaleXChanged), New CoerceValueCallback(AddressOf CoerceScaleX)))
    ' appears in code
    ''' <summary>
    ''' Scales the canvas in X direction.
    ''' </summary>        
    <Description("Scales the canvas in X direction."), Category("TrackCanvas")>       ' appears in VS property
    Public Property ScaleX() As Double
        Get
            Return CDbl(GetValue(ScaleXProperty))
        End Get
        Set(ByVal value As Double)
            SetValue(ScaleXProperty, value)
        End Set
    End Property

    Private Overloads Shared Function CoerceScaleX(ByVal d As DependencyObject, ByVal value As Object) As Object
        Dim newValue As Double = CDbl(value)

        If newValue < MinimumScaleX Then
            Return MinimumScaleX
        ElseIf newValue > MaximumScaleX Then
            Return MaximumScaleX
        End If

        Return newValue
    End Function

    Private Shared Sub OnScaleXChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As TrackCanvas = CType(d, TrackCanvas)
        Dim value As Double
        value = control.Length * TicksToPixelFactor * control.ScaleX

        control.Canvas1.Width = value
        'control.Width = value                          ' removes DataBinding,..
        'd.SetCurrentValue(WidthProperty, value)
        'SetCurrentValue changes the effective value Of the Property, but existing triggers, Data bindings,
        'And styles will continue to work.
        control.DrawTrack()
    End Sub

#End Region


#Region "Control"

    Public Shared ReadOnly CompositionProperty As DependencyProperty = DependencyProperty.Register("Composition", GetType(Composition), GetType(TrackCanvas), New FrameworkPropertyMetadata(New Composition, New PropertyChangedCallback(AddressOf OnCompositionChanged)))
    ' appears in code
    ''' <summary>
    ''' Sequencer's Composition object
    ''' </summary>        
    <Description("Sequencer's Composition object"), Category("TrackCanvas")>       ' appears in VS property
    Public Property Composition() As Composition
        Get
            Return CType(GetValue(CompositionProperty), Composition)
        End Get
        Set(ByVal value As Composition)
            SetValue(CompositionProperty, value)
        End Set
    End Property

    Private Shared Sub OnCompositionChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As TrackCanvas = CType(d, TrackCanvas)
        control.DrawTrack()
    End Sub

    Public Shared ReadOnly VoiceNumberProperty As DependencyProperty = DependencyProperty.Register("VoiceNumber", GetType(Integer), GetType(TrackCanvas), New FrameworkPropertyMetadata(0, New PropertyChangedCallback(AddressOf OnVoiceNumberChanged)))
    ' appears in code
    ''' <summary>
    ''' Index of associated Voice in Composition
    ''' </summary>        
    <Description("Index of associated Voice in Composition"), Category("TrackCanvas")>       ' appears in VS property
    Public Property VoiceNumber() As Integer
        Get
            Return CInt(GetValue(VoiceNumberProperty))
        End Get
        Set(ByVal value As Integer)
            SetValue(VoiceNumberProperty, value)
        End Set
    End Property

    Private Shared Sub OnVoiceNumberChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As TrackCanvas = CType(d, TrackCanvas)
        control.DrawTrack()
    End Sub

    Public Shared ReadOnly TrackNumberProperty As DependencyProperty = DependencyProperty.Register("TrackNumber", GetType(Integer), GetType(TrackCanvas), New FrameworkPropertyMetadata(0, New PropertyChangedCallback(AddressOf OnTrackNumberChanged)))
    ' appears in code
    ''' <summary>
    ''' Index of associated Voice in Composition
    ''' </summary>        
    <Description("Index of associated Voice in Composition"), Category("TrackCanvas")>       ' appears in VS property
    Public Property TrackNumber() As Integer
        Get
            Return CInt(GetValue(TrackNumberProperty))
        End Get
        Set(ByVal value As Integer)
            SetValue(TrackNumberProperty, value)
        End Set
    End Property

    Private Shared Sub OnTrackNumberChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As TrackCanvas = CType(d, TrackCanvas)
        control.DrawTrack()
    End Sub

    Public Sub DrawTrack()
        Canvas1.Children.Clear()

        Dim value As Double
        value = Length * TicksToPixelFactor * ScaleX
        SetValue(WidthProperty, value)

        If VoiceNumber >= Composition.Voices.Count Then
            DrawText("Invalid Voice Number: " & VoiceNumber, 10, 2, Brushes.Red)
            Exit Sub
        End If

        If TrackNumber >= Composition.Voices(VoiceNumber).Tracks.Count Then
            DrawText("Invalid Track Number: " & TrackNumber, 10, 2, Brushes.Red)
            Exit Sub
        End If

        Dim patlist As List(Of Pattern) = Composition.Voices(VoiceNumber).Tracks(TrackNumber).PatternList

        If patlist Is Nothing Then
            DrawText("Pattern List is Nothing", 10, 2, Brushes.Red)
            Exit Sub
        End If

        If patlist.Count = 0 Then
            DrawText("Pattern List is empty", 10, 2, Brushes.Red)
            Exit Sub
        End If

        'DrawMeasure()
        'DrawText("Track", 20, 120, 20, FontWeights.Light)

        AnalyzeTrack()

        For Each pattern In patlist
            DrawPattern(pattern)
        Next

    End Sub

    Public Sub AnalyzeTrack()
        NoteRangeStart = 0
        NoteRangeEnd = 127
        'ShowOnlyUsedNotes
        ListOfUsedNotes.Clear()

        Dim status As Byte
        Dim note As Byte

        Dim patlist As List(Of Pattern) = Composition.Voices(VoiceNumber).Tracks(TrackNumber).PatternList
        For Each pattern In patlist
            For Each tev In pattern.EventList

                If tev.Type = EventType.MidiEvent Then
                    status = CByte(tev.Status And &HF0)
                    If (status = &H90) And (tev.Data2 > 0) Then
                        note = tev.Data1
                        If Not ListOfUsedNotes.Contains(note) Then
                            ListOfUsedNotes.Add(note)
                        End If
                    End If
                End If
            Next
            ListOfUsedNotes.Sort()
            NoteRangeStart = ListOfUsedNotes.FirstOrDefault
            NoteRangeEnd = ListOfUsedNotes.LastOrDefault
        Next

    End Sub

#End Region

#Region "Events"


    'Public Event LocationChanged2(ByVal value As Double)

    Private Sub Canvas1_MouseMove(sender As Object, e As MouseEventArgs)
        'Dim ev As New RoutedPropertyChangedEventArgs(Of Double)(CDbl(ev.OldValue), CDbl(ev.NewValue), LocationChangedEvent)
        'Dim ev As New RoutedPropertyChangedEventArgs(Of Double)(CDbl(ev.OldValue), CDbl(ev.NewValue), LocationChangedEvent)
        Dim pt As Point
        pt = e.GetPosition(Canvas1)

        'RaiseEvent LocationChanged2(pt.X)

        'Me.OnLocationChanged(ev)
    End Sub

    Private Sub UserControl_MouseMove(sender As Object, e As MouseEventArgs)
        Dim pt As Point
        pt = e.GetPosition(Canvas1)

        Dim pos As UInteger
        'pos = CUInt((pt.X / TicksToPixelFactor / ScaleX))
        pos = PixelToTicks(pt.X)
        LastPositionOnTrack = pos
        'SequencerPanel1.TracksFooter.lblPositionOnTrack.Content = TimeTo_MBT(pos)
        If SeqPanel IsNot Nothing Then
            SeqPanel.TracksFooter.lblPositionOnTrack.Content = TimeTo_MBT(pos)
        End If
    End Sub

    Private Sub UserControl_MouseLeave(sender As Object, e As MouseEventArgs)
        'SequencerPanel1.TracksFooter.lblPositionOnTrack.Content = ""        
        If SeqPanel IsNot Nothing Then
            SeqPanel.TracksFooter.lblPositionOnTrack.Content = ""
        End If
    End Sub

    Private Sub UserControl_DragOver(sender As Object, e As DragEventArgs)
        If e.Data.GetDataPresent(GetType(Pattern)) Then
            Dim pt As Point = e.GetPosition(CType(sender, UIElement))
            Dim time As UInteger            ' sequencer Ticks        
            time = PixelToTicks(pt.X)
            If IsOnPattern(time) = False Then
                e.Effects = DragDropEffects.Copy
            Else
                e.Effects = DragDropEffects.None
            End If
        Else
            e.Effects = DragDropEffects.None
        End If
    End Sub

    Private Sub UserControl_Drop(sender As Object, e As DragEventArgs)
        If e.Data.GetDataPresent(GetType(Pattern)) Then
            Dim pattern As Pattern = CType(e.Data.GetData(GetType(Pattern)), Pattern)
            If pattern IsNot Nothing Then
                'Destination2.Add(pattern)
                Dim pt As Point = e.GetPosition(CType(sender, UIElement))
                Dim pos As UInteger         ' sequencer Ticks        
                pos = PixelToTicks(pt.X)
                InsertPattern(pos, pattern)
            End If
        End If
    End Sub

#End Region

#Region "Context Menu"

    Private Sub UserControl_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs)

        RightClickPositionOnTrack = PixelToTicks(e.CursorLeft)

        If Clipboard.GetDataObject.GetDataPresent(GetType(Pattern)) = True Then

            If IsOnPattern(RightClickPositionOnTrack) = False Then
                Mi_PastePattern.IsEnabled = True
            Else
                Mi_PastePattern.IsEnabled = False
            End If
        Else
            Mi_PastePattern.IsEnabled = False
        End If


    End Sub

    Private Sub Mi_PastePattern_Click(sender As Object, e As RoutedEventArgs) Handles Mi_PastePattern.Click
        Dim pattern As Pattern = CType(Clipboard.GetDataObject.GetData(GetType(Pattern)), Pattern)
        InsertPattern(RightClickPositionOnTrack, pattern)
    End Sub

    Private Sub Mi_DeleteTrack_Click(sender As Object, e As RoutedEventArgs) Handles Mi_DeleteTrack.Click
        DeleteTrack()
    End Sub

    Private Sub DeleteTrack()
        If MessageBox.Show("Delete this Track and dispose all of it's Data ?", "Delete Track",
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                        MessageBoxResult.Cancel) = MessageBoxResult.Yes Then

            Dim voiceNumber As Integer = Me.VoiceNumber
            Dim trackNumber As Integer = Me.TrackNumber
            'Dim comp As SequencerBase.Composition = Sequencer.Composition
            Dim comp As Composition = Composition

            If comp IsNot Nothing Then
                If voiceNumber < comp.Voices.Count Then
                    If trackNumber < comp.Voices.Count Then
                        comp.Voices(voiceNumber).Tracks.RemoveAt(trackNumber)           ' remove Track

                        Dim tobj As Object = FindLogicalParent(Me, GetType(TrackElement))
                        Dim trkel As TrackElement = TryCast(tobj, TrackElement)

                        '--- remove Track in UI ---
                        If trkel IsNot Nothing Then
                            Dim stk As StackPanel = TryCast(trkel.Parent, StackPanel)
                            If stk IsNot Nothing Then
                                If stk.Children.Contains(trkel) Then
                                    stk.Children.Remove(trkel)
                                    stk.UpdateLayout()
                                End If
                            End If
                        End If
                        '---

                    End If
                End If
            End If
        End If

    End Sub





#End Region





End Class
