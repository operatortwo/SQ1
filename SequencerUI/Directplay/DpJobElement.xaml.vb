Imports SequencerBase

Public Class DpJobElement
    Inherits UserControl
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    ' Control Background="#00000000" to detect MouseDown on the entire Area of the control (not only on content)

    Private FocusBrush As New SolidColorBrush(Color.FromArgb(&HFF, &H0, &H0, &HFF))
    Private FocusBackgroundBrush As New SolidColorBrush(Color.FromArgb(&H7F, &HB8, &HD3, &HFF))
    Friend IsRunningStroke As New SolidColorBrush(Color.FromArgb(&HFF, 180, 0, 0))

    Friend ParentSlot As DpSlot
    Friend Job As Directplay.Job

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        Dim pobj As Object = FindLogicalParent(Me, GetType(DpSlot))
        ParentSlot = TryCast(pobj, DpSlot)
    End Sub

    Private Sub UserControl_GotFocus(sender As Object, e As RoutedEventArgs)
        FocusRectangle.Stroke = FocusBrush
        MainGrid.Background = FocusBackgroundBrush
        If ParentSlot IsNot Nothing Then
            ParentSlot.SelectedJobElement = Me
        End If
    End Sub

    Private Sub UserControl_LostFocus(sender As Object, e As RoutedEventArgs)
        FocusRectangle.Stroke = Nothing
        MainGrid.Background = Nothing
        If Parent IsNot Nothing Then
            ParentSlot.SelectedJobElement = Nothing
        End If
    End Sub

    Private Sub UserControl_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        Me.Focus()
    End Sub

    Private Sub nudDuration_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If Job IsNot Nothing Then
            Job.Duration = CUInt(nudDuration.Value * Directplay.TicksPerBeat)
        End If
    End Sub
End Class
