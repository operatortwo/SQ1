Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Windows

Public Module File_IO

    Friend BaseDirectory As String = AppDomain.CurrentDomain.BaseDirectory
    Public ReadOnly CompositionsDirectory As String = BaseDirectory & "Compositions\"
    Friend DefaultPatternDirectory As String = BaseDirectory & "Pattern\"
    Public LastPatternDirectory As String = ""

    Friend DataDirectory As String = BaseDirectory & "Data\"

    Public CurrentCompositionFileName As String = ""

    Public Sub LoadPattern(ByRef DestinationPattern As Pattern)
        Dim ofd As New Microsoft.Win32.OpenFileDialog

        If IO.Directory.Exists(DefaultPatternDirectory) Then
            ofd.InitialDirectory = DefaultPatternDirectory
        Else
            ofd.InitialDirectory = BaseDirectory
        End If

        ofd.Filter = "Pattern files|*.pat"
        If ofd.ShowDialog() = False Then Exit Sub

        Dim pattern As Pattern

        Try
            Dim bf As New BinaryFormatter()
            Dim fs As FileStream

            fs = New FileStream(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.Read)
            pattern = CType(bf.Deserialize(fs), Pattern)
            fs.Close()

            'Me.Title = "Sequencer - " & Path.GetFileNameWithoutExtension(ofd.FileName)
            DestinationPattern = pattern
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Load Pattern")
        End Try

    End Sub

    Public Function LoadPatternFromXML(ByRef DestinationPattern As PatternX) As Boolean
        Dim ofd As New Microsoft.Win32.OpenFileDialog

        If IO.Directory.Exists(LastPatternDirectory) Then
            ofd.InitialDirectory = LastPatternDirectory
        ElseIf Directory.Exists(DefaultPatternDirectory) Then
            ofd.InitialDirectory = DefaultPatternDirectory
        Else
            ofd.InitialDirectory = BaseDirectory
        End If

        ofd.Filter = "XML files|*.xml"
        If ofd.ShowDialog() = False Then Return False

        LastPatternDirectory = Path.GetDirectoryName(ofd.FileName)

        Try
            Dim myObject As New PatternX
            ' Construct an instance of the XmlSerializer with the type
            ' of object that is being deserialized.
            Dim mySerializer As Xml.Serialization.XmlSerializer = New Xml.Serialization.XmlSerializer(GetType(PatternX))
            ' To read the file, create a FileStream.
            Dim myFileStream As FileStream = New FileStream(ofd.FileName, FileMode.Open)
            ' Call the Deserialize method and cast to the object type.
            myObject = CType(mySerializer.Deserialize(myFileStream), PatternX)

            DestinationPattern = myObject
            '--- additional
            If DestinationPattern.TPQ <> Sequencer.TPQ Then
                AdjustTimesToTPQ(DestinationPattern, Sequencer.TPQ)
            End If

            If DestinationPattern.Name = "" Then
                DestinationPattern.Name = Path.GetFileNameWithoutExtension(ofd.SafeFileName)
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Load Pattern XML")
            Return False
        End Try

        Return True
    End Function

    Private Sub AdjustTimesToTPQ(Pattern As PatternX, DestinationTPQ As Integer)
        Dim scale As Double = DestinationTPQ / Pattern.TPQ
        Dim tev As TrackEvent

        For i = 1 To Pattern.EventList.Count
            tev = Pattern.EventList(i - 1)
            tev.Time = CUInt(tev.Time * scale)
            tev.Duration = CUInt(tev.Duration * scale)
        Next

        Pattern.Length = CUInt(Pattern.Length * scale)
        Pattern.Duration = CUInt(Pattern.Duration * scale)
        Pattern.TPQ = DestinationTPQ

    End Sub


    Public Sub LoadPatternFromXML_2(ByRef DestinationPattern As Pattern, ByRef bpm As Integer)
        Dim ofd As New Microsoft.Win32.OpenFileDialog

        'ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory & "Data"
        ofd.Filter = "XML files|*.xml"
        If ofd.ShowDialog() = False Then Exit Sub

        Dim pattern As New Pattern

        Try
            Dim xmltree As XElement = XElement.Load(ofd.FileName)
            'xmltree.Element("Pattern")
            'Pattern Length

            '<Root><Child></Child></Root>
            'Dim str As String = xmltree.<Pattern>.<Length>.Value
            Dim bpmstr As String = xmltree.<BPM>.Value
            Dim len As String = xmltree.<Length>.Value
            Dim duration As String = xmltree.<Duration>.Value
            'Dim len As String = xmltree.<EventList>.Value
            'Dim len As String = xmltree.<Length>.Value
            'Dim len As String = xmltree.<Length>.Value

            bpm = CInt(bpmstr)
            pattern.Length = CUInt(len)
            pattern.Duration = CUInt(duration)

            For Each t In xmltree.<EventList>.<TrackEvent>
                Dim tev As New TrackEvent
                tev.Time = CUInt(t.<Time>.Value)
                tev.Status = CByte(t.<Status>.Value)
                tev.Data1 = CByte(t.<Data1>.Value)
                tev.Data2 = CByte(t.<Data2>.Value)
                tev.Duration = CUInt(t.<Duration>.Value)
                'tev.Type = CType(t.<Type>.Value, EventType)
                tev.Type = EventType.MidiEvent
                pattern.EventList.Add(tev)
            Next

            Dim c As Integer = pattern.EventList.Count

            DestinationPattern = pattern

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Load Pattern XML")
        End Try

    End Sub


    Public Sub SavePattern(pat As Pattern)
        Dim sfd As New Microsoft.Win32.SaveFileDialog

        '--- Create folder if not exists
        If Not IO.Directory.Exists(DataDirectory) Then
            IO.Directory.CreateDirectory(DataDirectory)
        End If
        '---

        sfd.InitialDirectory = DataDirectory
        sfd.Filter = "Pattern files|*.pat"
        sfd.DefaultExt = ".pat"

        If sfd.ShowDialog() = False Then Exit Sub

        'deep copy the pattern

        Dim pat2 As Pattern

        pat2 = pat.Copy
        pat2.StartTime = 0
        pat2.StartOffset = 0
        pat2.Duration = pat2.Length
        pat2.DoLoop = False
        pat2.Ended = False
        pat2.EventListPtr = 0

        Dim fs As FileStream
        Try
            Dim bf As New BinaryFormatter()
            fs = New IO.FileStream(sfd.FileName, IO.FileMode.Create)          'create or truncate / write
            bf.Serialize(fs, pat2)
            MessageBox.Show("Pattern was saved", "Pattern save", MessageBoxButton.OK)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Pattern save", MessageBoxButton.OK)
        Finally
#Disable Warning BC42104 ' Variable is used before it has been assigned a value
            If fs IsNot Nothing Then fs.Close()
#Enable Warning BC42104 ' Variable is used before it has been assigned a value
        End Try
    End Sub

    Public Sub SavePattern_as_XML(pat As Pattern)
        Dim sfd As New Microsoft.Win32.SaveFileDialog

        '--- Create folder if not exists
        If Not IO.Directory.Exists(DataDirectory) Then
            IO.Directory.CreateDirectory(DataDirectory)
        End If
        '---

        sfd.InitialDirectory = DataDirectory
        sfd.Filter = "XML files|*.xml"
        sfd.DefaultExt = ".xml"

        If sfd.ShowDialog() = False Then Exit Sub


        'deep copy the pattern

        Dim pat2 As Pattern

        pat2 = pat.Copy
        pat2.StartTime = 0
        pat2.StartOffset = 0
        pat2.Duration = pat2.Length
        pat2.DoLoop = False
        pat2.Ended = False
        pat2.EventListPtr = 0

        Dim fs As FileStream
        Try
            fs = New FileStream(sfd.FileName, FileMode.Create)      'create or truncate / write        
            Dim seria As New Xml.Serialization.XmlSerializer(pat.GetType)
            seria.Serialize(fs, pat2)
            MessageBox.Show("XML was saved", "Pattern save as XML", MessageBoxButton.OK)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Pattern save as XML", MessageBoxButton.OK)
        Finally
#Disable Warning BC42104 ' Variable is used before it has been assigned a value
            If fs IsNot Nothing Then fs.Close()
#Enable Warning BC42104 ' Variable is used before it has been assigned a value
        End Try

    End Sub

    Public Function Dialog_SaveComposition_as_XML(comp As Composition) As Boolean
        Dim sfd As New Microsoft.Win32.SaveFileDialog

        '--- Create folder if not exists
        If Not IO.Directory.Exists(CompositionsDirectory) Then
            IO.Directory.CreateDirectory(CompositionsDirectory)
        End If
        '---

        sfd.InitialDirectory = CompositionsDirectory
        sfd.Filter = "XML files|*.xml"
        sfd.DefaultExt = ".xml"

        If sfd.ShowDialog() = False Then Return False

        Return SaveComposition_as_XML(sfd.FileName, comp)

    End Function

    Public Function SaveComposition_as_XML(FileName As String, comp As Composition) As Boolean
        Reset_CompositionPointers(comp)
        Dim fs As FileStream
        Try
            fs = New FileStream(FileName, IO.FileMode.Create)      'create or truncate / write        
            Dim seria As New Xml.Serialization.XmlSerializer(comp.GetType)
            seria.Serialize(fs, comp)

            CurrentCompositionFileName = FileName
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Composition save as XML", MessageBoxButton.OK)
            Return False
        Finally
#Disable Warning BC42104 ' Variable is used before it has been assigned a value
            If fs IsNot Nothing Then fs.Close()
#Enable Warning BC42104 ' Variable is used before it has been assigned a value
        End Try

        Return True
    End Function

    Public Function Dialog_LoadCompositionFromXML(ByRef DestinationComposition As Composition) As Boolean
        Dim ofd As New Microsoft.Win32.OpenFileDialog

        If IO.Directory.Exists(CompositionsDirectory) Then
            ofd.InitialDirectory = CompositionsDirectory
        Else
            ofd.InitialDirectory = BaseDirectory
        End If

        ofd.Filter = "XML files|*.xml"
        If ofd.ShowDialog() = False Then Return False

        Return LoadCompositionFromXML(ofd.FileName, DestinationComposition)
    End Function

    Public Function LoadCompositionFromXML(FileName As String, ByRef DestinationComposition As Composition) As Boolean
        Dim myFileStream As FileStream
        Try
            Dim myObject As Composition
            ' Construct an instance of the XmlSerializer with the type
            ' of object that is being deserialized.
            Dim mySerializer As New Xml.Serialization.XmlSerializer(GetType(Composition))
            ' To read the file, create a FileStream.
            myFileStream = New FileStream(FileName, FileMode.Open)
            ' Call the Deserialize method and cast to the object type.
            myObject = CType(mySerializer.Deserialize(myFileStream), Composition)

            myObject.Voices.RemoveAt(0)                 ' remove the default Voice from New
            Reset_CompositionPointers(myObject)
            DestinationComposition = myObject

            CurrentCompositionFileName = FileName
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Load Composition XML")
            Return False
        Finally
#Disable Warning BC42104 ' Variable is used before it has been assigned a value
            If myFileStream IsNot Nothing Then myFileStream.Close()
#Enable Warning BC42104 ' Variable is used before it has been assigned a value
        End Try

        Return True
    End Function

    Private Sub Reset_CompositionPointers(comp As Composition)
        If comp Is Nothing Then Exit Sub

        For Each voice In comp.Voices
            For Each track In voice.Tracks
                track.PatternListPtr = 0
                For Each pattern In track.PatternList
                    pattern.EventListPtr = 0
                    pattern.StartOffset = 0
                Next
            Next
        Next
    End Sub

End Module
