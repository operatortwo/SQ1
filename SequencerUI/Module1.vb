Imports System.Resources

Public Module Module1                                       ' Public for access from MainWindow

    Public WithEvents Sequencer As SequencerBase.Sequencer  ' Sequencer Instance, definied in MainWindow, assigned in MainWindow

    Public Event ScreenRefreshChildren()        ' to be subscribed from Controls outside SequencerPanel

    Public Property CompositionPanel1 As SequencerPanel

    Public SkipChangedEvent As Boolean

    Public Const TicksToPixelFactor = 0.03125       ' ( 1 / 32 )
    Public Const PixelToTicksFactor = 32            ' ( 1 / TicksToPixelFactor )

    ''' <summary>
    ''' To be called from the Refresh-Timer. Raises the ScreenRefreshChildren Event.
    ''' </summary>
    Public Sub ScreenRefreshMain()
        RaiseEvent ScreenRefreshChildren()
    End Sub

    Public Function FindLogicalParent(base As FrameworkElement, targetType As Type) As Object

        Dim current As DependencyObject = base.Parent

        While current IsNot Nothing
            If current.[GetType]() = targetType Then
                Return current
            End If
            current = LogicalTreeHelper.GetParent(current)
        End While
        Return Nothing
    End Function

    ''' <summary>
    ''' Specialized function to round a Position X on Sequencer Canvas to the nearest beat, including Scale X
    ''' </summary>
    ''' <param name="MousePosition">Position x on the sequencer canvas</param>
    ''' <returns>Rounded Position in Pixel</returns>
    Public Function RoundToBeat(MousePosition As Double, ScaleX As Double) As Double
        'Dim TickPosition As Double = MousePosition * PixelToTicksFactor / SequencerPanel1.TracksHeader.ScaleX
        Dim TickPosition As Double = MousePosition * PixelToTicksFactor / ScaleX
        Dim RoundedTickPosition As Double = RoundToStep(TickPosition, SequencerBase.Sequencer.TPQ)
        'Dim RoundedPosition As Double = RoundedTickPosition * TicksToPixelFactor * SequencerPanel1.TracksHeader.ScaleX
        Dim RoundedPosition As Double = RoundedTickPosition * TicksToPixelFactor * ScaleX

        Return RoundedPosition
    End Function

    ''' <summary>
    ''' General function to round a value to the nearest step
    ''' </summary>
    ''' <param name="value">Value to round</param>
    ''' <param name="stepvalue">value of one step</param>
    ''' <returns>Rounded value</returns>
    Public Function RoundToStep(value As Double, stepvalue As Integer) As Double
        If stepvalue = 0 Then stepvalue = 1
        Dim steps As Double = Fix(value / stepvalue)

        Dim smod As Double = Math.Abs(value Mod stepvalue)  ' remainder
        If smod >= stepvalue / 2 Then       ' round up if necessary
            steps += 1
        End If

        Return steps * stepvalue
    End Function

    ''' <summary>
    ''' Make this Setting for PatternLibrary accessible from outside SequenerUI
    ''' </summary>
    ''' <param name="state"></param>
    Public Sub SetSequencerUISetting_CheckPresetPatternUpdate(state As Boolean)
        If state <> My.Settings.CheckPresetPatternUpdate Then
            My.Settings.CheckPresetPatternUpdate = state
            My.Settings.Save()
        End If
    End Sub
    ''' <summary>
    ''' Alllow upgrading SequencerUI Settings from Main Application
    ''' </summary>
    Public Sub SequencerUISettings_Upgrade()
        My.Settings.Upgrade()
        My.Settings.Save()
    End Sub

End Module
