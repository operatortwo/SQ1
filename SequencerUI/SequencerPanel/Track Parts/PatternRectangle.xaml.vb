Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports SequencerBase
Public Class PatternRectangle

    Friend ParentCanvas As TrackCanvas

    ''' <summary>
    ''' Need TrackCanvas to retrieve ScaleX
    ''' </summary>
    ''' <param name="TrkCanvas"></param>
    Public Sub New(TrkCanvas As TrackCanvas)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ParentCanvas = TrkCanvas
    End Sub

    Private Shared ReadOnly DefaultFillBrush As Brush = New SolidColorBrush(Color.FromArgb(&H7F, &H6B, &HB5, &H43))
    '#7F6BB543


    Public Shared ReadOnly TextProperty As DependencyProperty = DependencyProperty.Register("Text", GetType(String), GetType(PatternRectangle), New PropertyMetadata())
    ' appears in code
    ''' <summary>
    ''' The Text on the rectangle, normally the name of the pattern.
    ''' </summary>
    <Description("The Text on the rectangle, normally the name of the pattern"), Category("Pattern Rectangle")>   ' appears in VS property
    Public Property Text As String
        Get
            Return CType(GetValue(TextProperty), String)
        End Get
        Set(ByVal value As String)
            SetValue(TextProperty, value)
        End Set
    End Property


    Public Shared ReadOnly PatternProperty As DependencyProperty = DependencyProperty.Register("Pattern", GetType(Pattern), GetType(PatternRectangle), New PropertyMetadata())
    ''' <summary>
    ''' Pattern Data
    ''' </summary>
    <Description("Pattern Data"), Category("Pattern Rectangle")>
    Public Property Pattern As Pattern
        Get
            Return CType(GetValue(PatternProperty), Pattern)
        End Get
        Set(value As Pattern)
            SetValue(PatternProperty, value)
        End Set
    End Property



#Region "OnRender"

    Protected Overrides Sub OnRender(ByVal dc As DrawingContext)
        ' additional drawings 

        Dim rect As New Rect(New Size(Width, Height))

        'dc.DrawRectangle(Background, Nothing, rect)

        Dim pen As New Pen(Brushes.Green, 2.0)


        '--- Separation-Line for repeated Pattern
        If Pattern IsNot Nothing Then
            If Pattern.Duration > Pattern.Length Then
                Dim pt1 As New Point(0, Height - 10)                      ' 0,1
                Dim pt2 As New Point(0, Height)
                'Dim pxlength As Integer = CInt(Pattern.Length / PixelToTicksFactor * SequencerPanel1.TracksHeader.ScaleX)
                Dim pxlength As Integer = CInt(Pattern.Length / PixelToTicksFactor * ParentCanvas.ScaleX)

                Dim div As Integer = CInt(Pattern.Duration \ Pattern.Length)
                Dim dmod As Integer = CInt(Pattern.Duration Mod Pattern.Length)

                If dmod = 0 Then div -= 1

                If div > 10 Then
                    ' avoid excessive drawing when 'looped' pattern (Length >= 96'000'000) or very long repeated pattern
                    div = 10
                End If

                For i = 1 To div
                        pt1.X += pxlength
                        pt2.X += pxlength
                        dc.DrawLine(pen, pt1, pt2)
                    Next



                End If
            End If

    End Sub

#End Region


#Region "Context Menu"
    Private Sub UserControl_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs)
        If Sequencer.IsRunning Then
            e.Handled = True                ' suppress ContextMenu while playing
            Beep()
        End If
    End Sub

    Private Sub Mi_Info_Click(sender As Object, e As RoutedEventArgs) Handles Mi_Info.Click
        Dim pt As Pattern = Me.Pattern.Copy
        Dim win As New Pattern_Info(pt)
        win.Owner = Application.Current.MainWindow
        win.WindowStartupLocation = WindowStartupLocation.CenterOwner
        win.ShowDialog()
    End Sub

    Private Sub Mi_Play_Click(sender As Object, e As RoutedEventArgs) Handles Mi_Play.Click
        Dim pt As Pattern = Me.Pattern.Copy
        pt.DoLoop = False

        If pt.Duration > Sequencer.TPQ * 4 Then
            pt.Duration = Sequencer.TPQ * 4
        End If

        Dim tobj As Object = FindLogicalParent(Me, GetType(TrackCanvas))
        Dim tc As TrackCanvas = TryCast(tobj, TrackCanvas)

        If tc IsNot Nothing Then

            Dim voiceNumber As Integer = tc.VoiceNumber
            'Dim comp As Composition = Sequencer.Composition
            Dim comp As Composition = ParentCanvas.Composition

            If comp IsNot Nothing Then
                If voiceNumber < comp.Voices.Count Then
                    Dim vc As Voice
                    vc = tc.Composition.Voices(tc.VoiceNumber)
                    Sequencer.Play_Pattern(vc, pt, False)
                End If
            End If
        End If

    End Sub

    Private Sub Mi_Copy_Click(sender As Object, e As RoutedEventArgs) Handles Mi_Copy.Click
        Dim pat As New DataObject
        pat.SetData(GetType(SequencerBase.Pattern), Me.Pattern.Copy)
        Clipboard.SetDataObject(pat)
    End Sub

    Private Sub Mi_MoveAndSize_Click(sender As Object, e As RoutedEventArgs) Handles Mi_MoveAndSize.Click
        Dim win As New PatRect_MoveAndSizeWin(Me)
        win.Owner = Application.Current.MainWindow
        win.WindowStartupLocation = WindowStartupLocation.CenterOwner
        win.ShowDialog()



    End Sub

    Private Sub Mi_Edit_Click(sender As Object, e As RoutedEventArgs) Handles Mi_Edit.Click

    End Sub

    Private Sub Mi_Delete_Click(sender As Object, e As RoutedEventArgs) Handles Mi_Delete.Click

        If MessageBox.Show("Delete this Pattern and dispose all of it's Data ?", "Delete Pattern",
                       MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                       MessageBoxResult.Cancel) = MessageBoxResult.Yes Then

            Dim tobj As Object = FindLogicalParent(Me, GetType(TrackCanvas))
            Dim tc As TrackCanvas = TryCast(tobj, TrackCanvas)

            If tc IsNot Nothing Then

                Dim voiceNumber As Integer = tc.VoiceNumber
                Dim trackNumber As Integer = tc.TrackNumber
                'Dim comp As Composition = Sequencer.Composition
                Dim comp As Composition = ParentCanvas.Composition

                If comp IsNot Nothing Then
                    If voiceNumber < comp.Voices.Count Then
                        If trackNumber < comp.Voices(voiceNumber).Tracks.Count Then
                            '-- remove pattern from PatternList
                            tc.Composition.Voices(tc.VoiceNumber).Tracks(tc.TrackNumber).PatternList.Remove(Me.Pattern)

                            '-- remove PatternRectangle from TrackCanvas
                            If tc.Canvas1.Children.Contains(Me) Then
                                tc.Canvas1.Children.Remove(Me)
                                tc.Canvas1.UpdateLayout()
                            End If

                        End If
                    End If
                End If
            End If

        End If

    End Sub



#End Region

End Class
