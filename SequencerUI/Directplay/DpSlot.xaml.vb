Imports DailyUserControls
Imports SequencerBase
Imports SequencerBase.Directplay
Imports System.ComponentModel

Public Class DpSlot
    Inherits UserControl

    Public Sub New()

        ' this call is required for the Designer
        InitializeComponent()
        Console.WriteLine("Initialized")
    End Sub

    Private Property Voice As DirectplayVoice
    Private Property Slot As Slot

    Friend SelectedJobElement As DpJobElement

    Private RefreshHandlerAdded As Boolean              ' prevent the handler is added more than once

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        If Sequencer IsNot Nothing Then                             ' avoid exception in designer
            If Sequencer.DPlay IsNot Nothing Then                   ' avoid exception in designer
                Voice = Sequencer.DPlay.Voices(VoiceNumber)
                Slot = Sequencer.DPlay.Voices(VoiceNumber).Slots(SlotNumber)
                tgBtnRing.IsChecked = Slot.RingPlay
                ' make sure the handler if added only once. At this moment, _Loaded is called 2 times at start,
                ' followed by _Unloaded _Loaded pairs when switching TabItems.
                ' Multiple handlers would make multiple calls to the the handler-procedure
                If RefreshHandlerAdded = False Then
                    AddHandler SequencerUI.Module1.ScreenRefreshChildren, AddressOf ScreenRefresh
                    RefreshHandlerAdded = True
                End If
            End If
        End If
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        RemoveHandler SequencerUI.Module1.ScreenRefreshChildren, AddressOf ScreenRefresh
        RefreshHandlerAdded = False
    End Sub

    Public Sub ScreenRefresh()

        If Slot IsNot Nothing Then
            If Slot.Refresh_UI = True Then

                '--- so far, these elements can not be changed by code, so no comparison/update is needed
                'IsRunning As Boolean                     ' if TRUE: is running
                'HoldCurrent As Boolean                   ' if TRUE: restart the job when job duration is over
                'RingPlay As Boolean                      ' if TRUE: enque the current job when job duration is over
                'Group As Integer                         ' selected Group number for receiving group commands

                'Joblist As New List(Of Job)

                Slot.Refresh_UI = False     ' at start: give the player the chance to set it again while refreshing

                Update_JobElementStack()

            End If
        End If

    End Sub

    Private Sub Update_JobElementStack()
        If Slot IsNot Nothing Then
            JobElementStack.Children.Clear()

            ' try to avoid exception in some cases (List changed)
            Dim job As Job

            For i = Slot.Joblist.Count To 1 Step -1
                job = Slot.Joblist(i - 1)
                Dim jel As New DpJobElement
                jel.Job = job
                jel.JobLabelText.Text = job.Label
                jel.JobLengthText.Text = CStr(job.Length / TicksPerBeat)
                jel.nudDuration.SetValueSilent(job.Duration / TicksPerBeat)
#Disable Warning BC42025 ' Access of shared member, constant member, enum member or nested type through an instance
                JobElementStack.SetDock(jel, Dock.Bottom)
#Enable Warning BC42025 ' Access of shared member, constant member, enum member or nested type through an instance
                If i = Slot.Joblist.Count Then
                    'jel.IsRunningRectangle.Stroke = jel.IsRunningStroke
                    jel.FocusRectangle.Stroke = jel.IsRunningStroke
                    jel.IsEnabled = False
                End If
                JobElementStack.Children.Add(jel)
            Next

        End If
    End Sub


#Region "Voice and Slot"
    Public Shared ReadOnly VoiceNumberProperty As DependencyProperty = DependencyProperty.Register("VoiceNumber", GetType(Integer), GetType(DpSlot), New FrameworkPropertyMetadata(2, New PropertyChangedCallback(AddressOf OnVoiceNumberChanged), New CoerceValueCallback(AddressOf CoerceVoiceNumber)))
    <Description("Number of associated voice. Voices 0 and 1 are reserved for the system"), Category("Dp Slot")>
    Public Property VoiceNumber() As Integer
        Get
            Return CInt(GetValue(VoiceNumberProperty))
        End Get
        Set(ByVal value As Integer)
            SetValue(VoiceNumberProperty, value)
        End Set
    End Property

    Private Overloads Shared Function CoerceVoiceNumber(ByVal d As DependencyObject, ByVal value As Object) As Object
        Dim newValue As Integer = CInt(value)

        If newValue < 2 Then
            Return 2
        ElseIf newValue >= NumberOfVoices Then
            Return NumberOfVoices - 1
        End If

        Return newValue
    End Function

    Private Shared Sub OnVoiceNumberChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        'Dim control As DpSlot = CType(d, DpSlot)
    End Sub

    Public Shared ReadOnly SlotNumberProperty As DependencyProperty = DependencyProperty.Register("SlotNumber", GetType(Integer), GetType(DpSlot), New FrameworkPropertyMetadata(0, New PropertyChangedCallback(AddressOf OnSlotNumberChanged), New CoerceValueCallback(AddressOf CoerceSlotNumber)))
    <Description("Slot number of associated voice"), Category("Dp Slot")>
    Public Property SlotNumber() As Integer
        Get
            Return CInt(GetValue(SlotNumberProperty))
        End Get
        Set(ByVal value As Integer)
            SetValue(SlotNumberProperty, value)
        End Set
    End Property

    Private Overloads Shared Function CoerceSlotNumber(ByVal d As DependencyObject, ByVal value As Object) As Object
        Dim newValue As Integer = CInt(value)

        If newValue < 0 Then
            Return 0
        ElseIf newValue >= NumberOfSlotsPerVoice Then
            Return NumberOfSlotsPerVoice - 1
        End If

        Return newValue
    End Function

    Private Shared Sub OnSlotNumberChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        'Dim control As DpSlot = CType(d, DpSlot)
    End Sub

#End Region



    Private Sub JobElementStack_MouseMove(sender As Object, e As MouseEventArgs) Handles JobElementStack.MouseMove
        If e.LeftButton = MouseButtonState.Pressed Then
            If SelectedJobElement IsNot Nothing Then
                Dim dataObject As New DataObject
                dataObject.SetData(GetType(DpJobElement), SelectedJobElement)     ' format as type
                'DoDragDrop is a blocking function, returns when the operation ended

                DragDrop.DoDragDrop(SelectedJobElement, dataObject, DragDropEffects.Copy)

            End If
        End If
    End Sub

    Private Sub JobElementStack_Drop(sender As Object, e As DragEventArgs) Handles JobElementStack.Drop
        If e.Data.GetDataPresent(GetType(Pattern)) Then
            Dim pat As Pattern = TryCast(e.Data.GetData(GetType(Pattern)), Pattern)
            If pat IsNot Nothing Then

                '---> insert to slot.joblist
                Sequencer.DPlay.InsertPattern(Me.VoiceNumber, Me.SlotNumber, pat, 960)
                Slot.Refresh_UI = True

            End If
        End If
    End Sub


    Private Sub RemoveTarget_Drop(sender As Object, e As DragEventArgs) Handles RemoveTarget.Drop
        If e.Data.GetDataPresent(GetType(DpJobElement)) Then
            Dim jbel As DpJobElement = TryCast(e.Data.GetData(GetType(DpJobElement)), DpJobElement)
            If jbel IsNot Nothing Then
                If jbel.Job IsNot Nothing Then
                    Slot.Joblist.Remove(jbel.Job)
                End If
                JobElementStack.Children.Remove(jbel)
                'remove also job from Slot
                'Slot.Joblist.Remove(job)
            End If
        End If
    End Sub

    Private Sub tgBtnRun_Checked(sender As Object, e As RoutedEventArgs) Handles tgBtnRun.Checked
        If Slot IsNot Nothing Then
            Slot.IsRunning = True
        End If
    End Sub

    Private Sub tgBtnRun_Unchecked(sender As Object, e As RoutedEventArgs) Handles tgBtnRun.Unchecked
        If Slot IsNot Nothing Then
            Slot.IsRunning = False
        End If
    End Sub

    Private Sub tgBtnRing_Checked(sender As Object, e As RoutedEventArgs) Handles tgBtnRing.Checked
        If Slot IsNot Nothing Then
            Slot.RingPlay = True
        End If
    End Sub

    Private Sub tgBtnRing_Unchecked(sender As Object, e As RoutedEventArgs) Handles tgBtnRing.Unchecked
        If Slot IsNot Nothing Then
            Slot.RingPlay = False
        End If
    End Sub
End Class
