Imports System.ComponentModel
Imports SequencerBase
Imports SequencerUI.TrackPanelStackHeader
Imports UserControls1

Public Class SequencerPanel

    'Public Property Composition As New SequencerBase.Composition

    Private Property _Composition As New Composition
    ''' <summary>
    ''' Changing this Property will redraw All Voices and Tracks.
    ''' No redraw when new and existing Object are equal.  
    ''' </summary>
    ''' <returns></returns>
    Public Property Composition As SequencerBase.Composition
        Get
            Return _Composition
        End Get
        Set(value As SequencerBase.Composition)
            If value Is Nothing Then
                _Composition = New Composition
                RedrawAllVoicesAndTracks()
                Exit Property
            Else
                If value.Equals(_Composition) Then      ' no changes when Input is equal to existing
                    Exit Property
                Else
                    _Composition = value
                    RedrawAllVoicesAndTracks()
                End If
            End If
        End Set
    End Property


    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()               ' important !
        ' Add any initialization after the InitializeComponent() call.

        ' setting a reference in a Module is only useful if there is only 1 Instance of this Class in the application
        CompositionPanel1 = Me                ' try to avoid this

        '--- Set SeqPanel in this fixed Element of SequencerPanel, it contains ScaleX
        Me.TracksFooter.SeqPanel = Me

        '--- SeqPanel in other Elements have to retrieve it with 'FindLogicalParent'
        ' These are Elements with variable count, Elements that can be added and removed in Runtime
        '--- VoicePanel
        '--- TrackPanel
        '--- TrackCanvas
        ' TracksHeader is created at this point, but not PlayPositionAdorner1 and LoopCursorAdorner1
        ' SeqPanel for all three are created at UserControl_Loaded of TrackPanelStackHeader


    End Sub

    Public Sub ScreenRefresh()

        TracksHeader.MeasureAdornerLayer.Update()

        If Sequencer.IsRunning Then
            TracksFooter.ScrollIntoView(Sequencer.SequencerTime)
        End If

        Dim vc As Voice

        For Each el In VoicePanelStack.Children
                Dim vp As VoicePanel = TryCast(el, VoicePanel)
                If vp IsNot Nothing Then
                vc = vp.CompositionVoice
                If vc IsNot Nothing Then

                    If vc.VoiceNumberGM <> vp.VoiceElements.nudGmVoice.Value Then
                        vp.VoiceElements.nudGmVoice.SetValueSilent(vc.VoiceNumberGM)
                        vp.VoiceElements.tbGmVoiceName.Text = Get_GM_VoiceName(vc.VoiceNumberGM)
                    End If

                    If vc.Volume <> vp.VoiceElements.ssldVolume.Value Then
                        vp.VoiceElements.ssldVolume.SetValueSilent(vc.Volume)
                    End If

                    If vc.Pan <> vp.VoiceElements.ssldPan.Value Then
                        vp.VoiceElements.ssldPan.SetValueSilent(vc.Pan)
                    End If


                    If vc.Refresh_VU_Velocity = True Then
                        'vp.VoiceElements.VU_Meter.Value = vc.VU_Velocity
                        vp.VU_Meter.Value = vc.VU_Velocity
                        vc.Refresh_VU_Velocity = False
                    Else
                        'decrease value at each refresh                        
                        If vp.VU_Meter.Value < 20 Then
                            vp.VU_Meter.Value = 0
                        Else
                            vp.VU_Meter.Value = vp.VU_Meter.Value / 5 * 4
                        End If

                    End If
                End If
            End If
            Next




    End Sub

    Public Sub ScrollIntoView(time As Double)
        TracksFooter.ScrollIntoView(time)
    End Sub

    Public Sub UpdateScaleX()
        TracksFooter.UpdateScaleX()
    End Sub


#Region "Common"

    Public Property ShowSequencerPosition As Boolean        ' measure Adorner

    Public Property ScrollPositionIntoView As Boolean       ' AutoScroll when moving

    Public Property LoopCanvasVisibility As Visibility
        Get
            Return TracksHeader.LoopCanvas.Visibility
        End Get
        Set(value As Visibility)
            TracksHeader.LoopCanvas.Visibility = value
        End Set
    End Property


    ''' <summary>
    ''' Called when the 'Composition' Property of this Panel was changed. 
    ''' Normally not needed to call this Sub directly.
    ''' </summary>
    Public Sub RedrawAllVoicesAndTracks()
        'If Comp1 IsNot Nothing Then Composition = Comp1

        VoicePanelStack.Children.Clear()
        TrackPanelStack.Children.Clear()

        btnCollapseExpandAll.IsChecked = False          ' reset state to default
        btnMuteUnMuteAll.IsChecked = False              ' reset state to default

        SkipChangedEvent = True     ' suppress Midi-Messages from VoicePanels while creating

        Dim Comp As Composition = Composition
        Dim vc As SequencerBase.Voice

        For i = 1 To Comp.Voices.Count
            Dim tpnl As New TrackPanel
            TrackPanelStack.Children.Add(tpnl)
            vc = Comp.Voices(i - 1)
            AddVoicePanel(vc, tpnl, i - 1)
            AddTrackElement(vc, tpnl, i - 1)
        Next

        SkipChangedEvent = False

        TracksFooter.sldScaleX.Value = 0.5
        UpdateScaleX()
        'TrackPanelStack.UpdateLayout()

        If Sequencer IsNot Nothing Then
            Sequencer.Initialize_Midi()         ' now send initial Midi-Messages (ProgramChange, Volume, Pan, ..)
        End If
    End Sub

    ''' <summary>
    ''' Redraw all Tracks. Update the Drawing when the composition was not changed by SequencerUI itself.
    ''' (f.e. when a Pattern was inserted by Code)
    ''' </summary>
    Public Sub RedrawAllTracks()
        For Each panel As TrackPanel In TrackPanelStack.Children
            For Each element As TrackElement In panel.TrackElementStack.Children
                element.TrackCanvas.DrawTrack()
            Next
        Next
    End Sub


    Public Sub SetLoopCanvasVisibility(visibility As Visibility)
        'SequencerPanel1.TracksHeader.LoopCanvas.Visibility = visibility
        Me.TracksHeader.LoopCanvas.Visibility = visibility
    End Sub

    Private Sub AddVoicePanel(voice As SequencerBase.Voice, sibling As TrackPanel, voiceNumber As Integer)
        Dim vpnl As VoicePanel
        Dim vele As VoiceElements
        vpnl = New VoicePanel

        vpnl.CompositionVoice = voice
        vpnl.VoiceElements.CompositionVoice = voice
        vpnl.Sibling = sibling
        vpnl.VoiceNumber = voiceNumber

        sibling.sibling = vpnl
        sibling.CompositionVoice = voice

        vpnl.tbVoiceTitle.Text = voice.Title

        vele = vpnl.VoiceElements

        vele.nudPortNumber.Value = voice.PortNumber
        vele.nudMidiChannel.Value = voice.MidiChannel

        vele.nudGmVoice.Value = voice.VoiceNumberGM
        vele.nudVcMSB.Value = voice.BankSelectMSB
        vele.nudVcLSB.Value = voice.BankSelectLSB
        vele.nudVcNum.Value = voice.VoiceNumber

        vele.ssldVolume.Value = voice.Volume
        vele.ssldPan.Value = voice.Pan

        VoicePanelStack.Children.Add(vpnl)

    End Sub


    Private Sub AddTrackElement(voice As SequencerBase.Voice, trackPanel As TrackPanel, voiceNumber As Integer)

        For i = 1 To voice.Tracks.Count

            Dim trkel As New TrackElement

            trackPanel.TrackElementStack.Children.Add(trkel)
            trkel.CompositionTrack = voice.Tracks(i - 1)
            trkel.tbTrackTitle.Text = voice.Tracks(i - 1).Title

            If Me.AllowUserToChangeTrackViewType = True Then
                trkel.AllowUserToChangeTrackViewType = True
            Else
                trkel.AllowUserToChangeTrackViewType = False
            End If

            'trkel.TrackCanvas.Composition = Sequencer.Composition
            'trkel.TrackCanvas.Length = Sequencer.Composition.Length
            trkel.TrackCanvas.Composition = Composition
            trkel.TrackCanvas.Length = Composition.Length

            trkel.TrackCanvas.VoiceNumber = voiceNumber
            trkel.TrackCanvas.TrackNumber = i - 1

            'trkel.TrackCanvas.ScaleX=
        Next

    End Sub

    Private Sub btnAddVoice_Click(sender As Object, e As RoutedEventArgs) Handles btnAddVoice.Click

        Dim vc As New SequencerBase.Voice

        Dim tpnl As New TrackPanel
        TrackPanelStack.Children.Add(tpnl)

        'Sequencer.Composition.Voices.Add(vc)
        'AddVoicePanel(vc, tpnl, Sequencer.Composition.Voices.Count - 1)
        Composition.Voices.Add(vc)
        AddVoicePanel(vc, tpnl, Composition.Voices.Count - 1)

    End Sub

    Private Sub btnCollapseExpandAll_Checked(sender As Object, e As RoutedEventArgs) Handles btnCollapseExpandAll.Checked
        For Each el In VoicePanelStack.Children
            Dim vp As VoicePanel = TryCast(el, VoicePanel)
            If vp IsNot Nothing Then
                vp.Expander.IsExpanded = False
            End If
        Next
    End Sub

    Private Sub btnCollapseExpandAll_Unchecked(sender As Object, e As RoutedEventArgs) Handles btnCollapseExpandAll.Unchecked
        For Each el In VoicePanelStack.Children
            Dim vp As VoicePanel = TryCast(el, VoicePanel)
            If vp IsNot Nothing Then
                vp.Expander.IsExpanded = True
            End If
        Next
    End Sub

    Private Sub btnMuteUnMuteAll_Checked(sender As Object, e As RoutedEventArgs) Handles btnMuteUnMuteAll.Checked
        For Each el In VoicePanelStack.Children
            Dim vp As VoicePanel = TryCast(el, VoicePanel)
            If vp IsNot Nothing Then
                vp.tgbtnMute.IsChecked = True
            End If
        Next
    End Sub

    Private Sub btnMuteUnMuteAll_Unchecked(sender As Object, e As RoutedEventArgs) Handles btnMuteUnMuteAll.Unchecked
        For Each el In VoicePanelStack.Children
            Dim vp As VoicePanel = TryCast(el, VoicePanel)
            If vp IsNot Nothing Then
                vp.tgbtnMute.IsChecked = False
            End If
        Next
    End Sub

    Private Sub VoicePanelStack_MouseWheel(sender As Object, e As MouseWheelEventArgs) Handles VoicePanelStack.MouseWheel
        e.Handled = True                    ' avoid ScrollViewer scroll
    End Sub

    Private Sub VoicePanelStackScroll_ScrollChanged(sender As Object, e As ScrollChangedEventArgs) Handles VoicePanelStackScroll.ScrollChanged
        'was a bug. sometimes when changing volume, pan,.. e.VerticalOffset was set to 0) cahnge was 0, but extentWidth was > 0
        'TrackPanelStackScroll.ScrollToVerticalOffset(e.VerticalOffset)
        TrackPanelStackScroll.ScrollToVerticalOffset(VoicePanelStackScroll.VerticalOffset)  ' fix 1
    End Sub

    Private Sub TrackPanelStackScroll_ScrollChanged(sender As Object, e As ScrollChangedEventArgs) Handles TrackPanelStackScroll.ScrollChanged
        'VoicePanelStackScroll.ScrollToVerticalOffset(e.VerticalOffset)
    End Sub

    Private Sub TrackPanelStack_MouseWheel(sender As Object, e As MouseWheelEventArgs) Handles TrackPanelStack.MouseWheel
        e.Handled = True                    ' avoid ScrollViewer scroll
    End Sub

    Private Sub TracksHeader_RowHeaderSizeChanged(sender As Object, e As SizeChangedEventArgs) Handles TracksHeader.RowHeaderSizeChanged
        TracksFooter.LeftPart.Width = e.NewSize.Width

        'For Each panel As TrackPanel In SequencerPanel1.TrackPanelStack.Children
        For Each panel As TrackPanel In Me.TrackPanelStack.Children
            For Each element As TrackElement In panel.TrackElementStack.Children
                element.LeftPart.Width = e.NewSize.Width
            Next
        Next

    End Sub

#End Region

#Region "Dependency Properties"

    Public Shared ReadOnly AllowUserToChangeTrackViewTypeProperty As DependencyProperty = DependencyProperty.Register("AllowUserToChangeTrackViewType", GetType(Boolean), GetType(SequencerPanel), New FrameworkPropertyMetadata(True, New PropertyChangedCallback(AddressOf OnAllowUserToChangeTrackViewTypeChanged)))
    <Description("Enable or disable TrackView Option in TrackPanels"), Category("Sequencer Panel")>   ' appears in VS property
    Public Property AllowUserToChangeTrackViewType() As Boolean
        Get
            Return CType(GetValue(AllowUserToChangeTrackViewTypeProperty), Boolean)
        End Get
        Set
            SetValue(AllowUserToChangeTrackViewTypeProperty, Value)
        End Set
    End Property

    Private Shared Sub OnAllowUserToChangeTrackViewTypeChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As SequencerPanel = CType(d, SequencerPanel)
        Dim bol As Boolean = CType(d.GetValue(AllowUserToChangeTrackViewTypeProperty), Boolean)

        For Each panel As TrackPanel In control.TrackPanelStack.Children
            For Each element As TrackElement In panel.TrackElementStack.Children
                If bol = True Then
                    element.AllowUserToChangeTrackViewType = True
                Else
                    element.AllowUserToChangeTrackViewType = False
                End If
            Next
        Next

    End Sub





#End Region



End Class
