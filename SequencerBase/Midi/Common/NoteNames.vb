Public Module NoteNames
    ' NoteNumber To NoteName                                60 --> "C4"
    ' NoteNameList -- SortedList(Of Integer, String)        24, "C1"
    ' GetGM_VoiceName(VoiceNum As Byte) As String           0 --> "Acoustic Grand Piano"
    ' GM_VoiceNames -- SortedList(Of Integer, String)       {0, "Acoustic Grand Piano"},...
    ' Get_GM_DrumVoiceName(KeyNum As Byte) As String        35 --> "Acoustic Bass Drum"
    ' GM_DrumVoiceNames -- SortedList(Of Integer, String)   {35, "Acoustic Bass Drum"},...

    ' assumed (not verified) a Standard 88Keys Keyboard goes from  A0 - C8  (NoteNumber 21 - 108)

    '   GetNoteNameList(FirstNote as Byte, LastNote as Byte, Add_NoteNumber as Boolean, Reverse as Boolean)as Dictionary(Of Integer, String)
    '   GetNoteNameList_88Keys(Add_NoteNumber as Boolean, Reverse as Boolean)as Dictionary(Of Integer, String)
    '   Get_GM_DrumVoiceNameList(FirstNote as Byte, LastNote as Byte, Add_NoteNumber as Boolean, Reverse as Boolean)as Dictionary(Of Integer, String)
    '   Get_GM_DrumVoiceNameList_Map(Add_NoteNumber as Boolean, Reverse as Boolean)as Dictionary(Of Integer, String)
    '

#Region "NoteNumber to NoteName"
    ''' <summary>
    ''' Midi Note-number to Midi Note Name (f.e. 60 to 'C 4')
    ''' </summary>
    ''' <param name="NoteNumber"></param>
    ''' <returns></returns>
    Public Function NoteNumber_to_NoteName(NoteNumber As Byte) As String

        If NoteNumber > 127 Then
            Return ""                   ' return empty String if noteNr is invalid
        End If

        Return KeyNames(NoteNumber Mod 12).Name & (NoteNumber \ 12) - 1

        ' A4 = 440Hz = NoteNr: 69 (Def: Middle C = C4 = NoteNr: 60)
    End Function

    Private Class KeyName
        Public Property Offset As Integer
        Public Property Name As String          ' key name

    End Class

    Private ReadOnly KeyNames As New List(Of KeyName) From
        {
        New KeyName With {.Offset = 0, .Name = "C"},
        New KeyName With {.Offset = 1, .Name = "C#"},
        New KeyName With {.Offset = 2, .Name = "D"},
        New KeyName With {.Offset = 3, .Name = "D#"},
        New KeyName With {.Offset = 4, .Name = "E"},
        New KeyName With {.Offset = 5, .Name = "F"},
        New KeyName With {.Offset = 6, .Name = "F#"},
        New KeyName With {.Offset = 7, .Name = "G"},
        New KeyName With {.Offset = 8, .Name = "G#"},
        New KeyName With {.Offset = 9, .Name = "A"},
        New KeyName With {.Offset = 10, .Name = "A#"},
        New KeyName With {.Offset = 11, .Name = "B"}
        }

#End Region

#Region "NoteNameList"
    ''' <summary>
    ''' NoteNameList for using as ItemsSource
    ''' </summary>
    Public ReadOnly NoteNameList As New SortedList(Of Integer, String) From
        {
 {0, "C-1"},        '-1
 {1, "C#-1"},
 {2, "D-1"},
 {3, "D#-1"},
 {4, "E-1"},
 {5, "F-1"},
 {6, "F#-1"},
 {7, "G-1"},
 {8, "G#-1"},
 {9, "A-1"},
 {10, "A#-1"},
 {11, "B-1"},
 {12, "C0"},        '0
 {13, "C#0"},
 {14, "D0"},
 {15, "D#0"},
 {16, "E0"},
 {17, "F0"},
 {18, "F#0"},
 {19, "G0"},
 {20, "G#0"},
 {21, "A0"},
 {22, "A#0"},
 {23, "B0"},
 {24, "C1"},        '1
 {25, "C#1"},
 {26, "D1"},
 {27, "D#1"},
 {28, "E1"},
 {29, "F1"},
 {30, "F#1"},
 {31, "G1"},
 {32, "G#1"},
 {33, "A1"},
 {34, "A#1"},
 {35, "B1"},
 {36, "C2"},        '2
 {37, "C#2"},
 {38, "D2"},
 {39, "D#2"},
 {40, "E2"},
 {41, "F2"},
 {42, "F#2"},
 {43, "G2"},
 {44, "G#2"},
 {45, "A2"},
 {46, "A#2"},
 {47, "B2"},
 {48, "C3"},        '3
 {49, "C#3"},
 {50, "D3"},
 {51, "D#3"},
 {52, "E3"},
 {53, "F3"},
 {54, "F#3"},
 {55, "G3"},
 {56, "G#3"},
 {57, "A3"},
 {58, "A#3"},
 {59, "B3"},
 {60, "C4"},        '4      C4
 {61, "C#4"},
 {62, "D4"},
 {63, "D#4"},
 {64, "E4"},
 {65, "F4"},
 {66, "F#4"},
 {67, "G4"},
 {68, "G#4"},
 {69, "A4"},
 {70, "A#4"},
 {71, "B4"},
 {72, "C5"},        '5
 {73, "C#5"},
 {74, "D5"},
 {75, "D#5"},
 {76, "E5"},
 {77, "F5"},
 {78, "F#5"},
 {79, "G5"},
 {80, "G#5"},
 {81, "A5"},
 {82, "A#5"},
 {83, "B5"},
 {84, "C6"},        '6
 {85, "C#6"},
 {86, "D6"},
 {87, "D#6"},
 {88, "E6"},
 {89, "F6"},
 {90, "F#6"},
 {91, "G6"},
 {92, "G#6"},
 {93, "A6"},
 {94, "A#6"},
 {95, "B6"},
 {96, "C7"},        '7
 {97, "C#7"},
 {98, "D7"},
 {99, "D#7"},
 {100, "E7"},
 {101, "F7"},
 {102, "F#7"},
 {103, "G7"},
 {104, "G#7"},
 {105, "A7"},
 {106, "A#7"},
 {107, "B7"},
 {108, "C8"},       '8
 {109, "C#8"},
 {110, "D8"},
 {111, "D#8"},
 {112, "E8"},
 {113, "F8"},
 {114, "F#8"},
 {115, "G8"},
 {116, "G#8"},
 {117, "A8"},
 {118, "A#8"},
 {119, "B8"},
 {120, "C9"},       '9
 {121, "C#9"},
 {122, "D9"},
 {123, "D#9"},
 {124, "E9"},
 {125, "F9"},
 {126, "F#9"},
 {127, "G9"}
 }

#End Region

#Region "Get_GM_VoiceName"
    ''' <summary>
    ''' Returns the GM VoiceName from GM VoiceNumber
    ''' </summary>
    ''' <param name="VoiceNum"></param>
    ''' <returns></returns>
    Public Function Get_GM_VoiceName(VoiceNum As Byte) As String
        Dim str As String = ""

        If GM_VoiceNames.TryGetValue(VoiceNum, str) = True Then
            Return str
        Else
            Return VoiceNum & " - unknown Voice"
        End If

    End Function
#End Region

#Region "GM VoiceNames List"
    ''' <summary>
    ''' GemeralMidi VoiceNames sorted by VoiceNumber
    ''' </summary>
    Public ReadOnly GM_VoiceNames As New SortedList(Of Integer, String) From
          {
{0, "Acoustic Grand Piano"},
{1, "Bright Acoustic Piano"},
{2, "Electric Grand Piano"},
{3, "Honky-tonk Piano"},
{4, "Electric Piano 1"},
{5, "Electric Piano 2"},
{6, "Harpsichord"},
{7, "Clavi"},
{8, "Celesta"},
{9, "Glockenspiel"},
{10, "Music Box"},
{11, "Vibraphone"},
{12, "Marimba"},
{13, "Xylophone"},
{14, "Tubular Bells"},
{15, "Dulcimer"},
{16, "Drawbar Organ"},
{17, "Percussive Organ"},
{18, "Rock Organ"},
{19, "Church Organ"},
{20, "Reed Organ"},
{21, "Accordion"},
{22, "Harmonica"},
{23, "Tango Accordion"},
{24, "Acoustic Guitar (nylon)"},
{25, "Acoustic Guitar (steel)"},
{26, "Electric Guitar (jazz)"},
{27, "Electric Guitar (clean)"},
{28, "Electric Guitar (muted"},
{29, "Overdriven Guitar"},
{30, "Distortion Guitar"},
{31, "Guitar harmonics"},
{32, "Acoustic Bass"},
{33, "Electric Bass (finger)"},
{34, "Electric Bass (pick)"},
{35, "Fretless Bass"},
{36, "Slap Bass 1"},
{37, "Slap Bass 2"},
{38, "Synth Bass 1"},
{39, "Synth Bass 2"},
{40, "Violin"},
{41, "Viola"},
{42, "Cello"},
{43, "Contrabass"},
{44, "Tremolo Strings"},
{45, "Pizzicato Strings"},
{46, "Orchestral Harp"},
{47, "Timpani"},
{48, "String Ensemble 1"},
{49, "String Ensemble 2"},
{50, "SynthStrings 1"},
{51, "SynthStrings 2"},
{52, "Choir Aahs"},
{53, "Voice Oohs"},
{54, "Synth Voice"},
{55, "Orchestra Hit"},
{56, "Trumpet"},
{57, "Trombone"},
{58, "Tuba"},
{59, "Muted Trumpet"},
{60, "French Horn"},
{61, "Brass Section"},
{62, "SynthBrass 1"},
{63, "SynthBrass 2"},
{64, "Soprano Sax"},
{65, "Alto Sax"},
{66, "Tenor Sax"},
{67, "Baritone Sax"},
{68, "Oboe"},
{69, "English Horn"},
{70, "Bassoon"},
{71, "Clarinet"},
{72, "Piccolo"},
{73, "Flute"},
{74, "Recorder"},
{75, "Pan Flute"},
{76, "Blown Bottle"},
{77, "Shakuhachi"},
{78, "Whistle"},
{79, "Ocarina"},
{80, "Lead 1 (square)"},
{81, "Lead 2 (sawtooth)"},
{82, "Lead 3 (calliope)"},
{83, "Lead 4 (chiff)"},
{84, "Lead 5 (charang)"},
{85, "Lead 6 (voice)"},
{86, "Lead 7 (fifths)"},
{87, "Lead 8 (bass + lead)"},
{88, "Pad 1 (New age)"},
{89, "Pad 2 (warm)"},
{90, "Pad 3 (polysynth)"},
{91, "Pad 4 (choir)"},
{92, "Pad 5 (bowed)"},
{93, "Pad 6 (metallic)"},
{94, "Pad 7 (halo)"},
{95, "Pad 8 (sweep)"},
{96, "FX 1 (rain)"},
{97, "FX 2 (soundtrack)"},
{98, "FX 3 (crystal)"},
{99, "FX 4 (atmosphere)"},
{100, "FX 5 (brightness)"},
{101, "FX 6 (goblins)"},
{102, "FX 7 (echoes)"},
{103, "FX 8 (sci-fi)"},
{104, "Sitar"},
{105, "Banjo"},
{106, "Shamisen"},
{107, "Koto"},
{108, "Kalimba"},
{109, "Bag pipe"},
{110, "Fiddle"},
{111, "Shanai"},
{112, "Tinkle Bell"},
{113, "Agogo"},
{114, "Steel Drums"},
{115, "Woodblock"},
{116, "Taiko Drum"},
{117, "Melodic Tom"},
{118, "Synth Drum"},
{119, "Reverse Cymbal"},
{120, "Guitar Fret Noise"},
{121, "Breath Noise"},
{122, "Seashore"},
{123, "Bird Tweet"},
{124, "Telephone Ring"},
{125, "Helicopter"},
{126, "Applause"},
{127, "Gunshot"}
}
#End Region

#Region "Get GM DrumVoiceName"
    ''' <summary>
    ''' Returns the GM DrumVoiceName from GM DrumKeyNumber
    ''' </summary>
    ''' <param name="KeyNum"></param>
    ''' <returns></returns>
    Public Function Get_GM_DrumVoiceName(KeyNum As Byte) As String
        Dim str As String = ""

        If GM_DrumVoiceNames.TryGetValue(KeyNum, str) = True Then
            Return str
        Else
            Return KeyNum & " - not listed"
        End If

    End Function
#End Region

#Region "GM DrumVoiceNames List"
    ''' <summary>
    ''' GemeralMidi DrumVoiceNames sorted by DrumVoiceNumber
    ''' </summary>
    Public ReadOnly GM_DrumVoiceNames As New SortedList(Of Integer, String) From
          {
{35, "Acoustic Bass Drum"},
{36, "Bass Drum 1"},
{37, "Side Stick"},
{38, "Acoustic Snare"},
{39, "Hand Clap"},
{40, "Electric Snare"},
{41, "Low Floor Tom"},
{42, "Closed Hi Hat"},
{43, "High Floor Tom"},
{44, "Pedal Hi-Hat"},
{45, "Low Tom"},
{46, "Open Hi-Hat"},
{47, "Low-Mid Tom"},
{48, "Hi Mid Tom"},
{49, "Crash Cymbal 1"},
{50, "High Tom"},
{51, "Ride Cymbal 1"},
{52, "Chinese Cymbal"},
{53, "Ride Bell"},
{54, "Tambourine"},
{55, "Splash Cymbal"},
{56, "Cowbell"},
{57, "Crash Cymbal 2"},
{58, "Vibraslap"},
{59, "Ride Cymbal 2"},
{60, "Hi Bongo"},
{61, "Low Bongo"},
{62, "Mute Hi Conga"},
{63, "Open Hi Conga"},
{64, "Low Conga"},
{65, "High Timbale"},
{66, "Low Timbale"},
{67, "High Agogo"},
{68, "Low Agogo"},
{69, "Cabasa"},
{70, "Maracas"},
{71, "Short Whistle"},
{72, "Long Whistle"},
{73, "Short Guiro"},
{74, "Long Guiro"},
{75, "Claves"},
{76, "Hi Wood Block"},
{77, "Low Wood Block"},
{78, "Mute Cuica"},
{79, "Open Cuica"},
{80, "Mute Triangle"},
{81, "Open Triangle"}
}
#End Region

    ''' <summary>
    ''' Returns a list of NoteNumber - NoteName pairs.
    ''' </summary>
    ''' <param name="FirstNote">note number of first note</param>
    ''' <param name="LastNote">note number of last note</param>
    ''' <param name="Reverse">True: returns a List in descending order (first item is LastNote)</param>
    ''' <param name="Add_NoteNumber">Option for NoteName. f.e.: False: "C1", True: "24 - C1"</param>
    ''' <returns></returns>
    Public Function GetNoteNameList(FirstNote As Byte, LastNote As Byte, Reverse As Boolean, Add_NoteNumber As Boolean) As Dictionary(Of Integer, String)
        Dim List As New Dictionary(Of Integer, String)

        If FirstNote > 127 Then FirstNote = 127
        If LastNote > 127 Then LastNote = 127
        If LastNote < FirstNote Then Return List        ' invalid range, return empty List

        For i = FirstNote To LastNote
            If Add_NoteNumber = False Then
                List.Add(i, NoteNumber_to_NoteName(i))
            Else
                List.Add(i, i & " - " & NoteNumber_to_NoteName(i))
            End If
        Next

        If Reverse = True Then
            Dim ListRev As New Dictionary(Of Integer, String)
            For Each item In List.Reverse
                ListRev.Add(item.Key, item.Value)
            Next
            Return ListRev
        End If

        Return List
    End Function

    ''' <summary>
    ''' Returns a list of NoteNumber - NoteName pairs. Note Range is fixed from 21 - 108 (A0 - C8). 
    ''' This should represent the range of a 88 keys keyboard. (assumed, but not verified)    
    ''' </summary>
    ''' <param name="Reverse">True: returns a List in descending order (first item is LastNote)</param>
    ''' <param name="Add_NoteNumber">Option for NoteName. f.e.: False: "C1", True: "24 - C1"</param>
    ''' <returns></returns>
    Public Function GetNoteNameList_88Keys(Reverse As Boolean, Add_NoteNumber As Boolean) As Dictionary(Of Integer, String)
        Return GetNoteNameList(21, 108, Reverse, Add_NoteNumber)
    End Function


    ''' <summary>
    ''' Returns a list of NoteNumber - NoteName pairs. NoteNames are the Names of the Drum-Instrument 
    ''' which is mapped to a certain NoteNumber. Not all Notes are mapped to an instrument, then the name is set to
    ''' 'not listed'.
    ''' </summary>
    ''' <param name="FirstNote">note number of first note</param>
    ''' <param name="LastNote">note number of last note</param>
    ''' <param name="Reverse">True: returns a List in descending order (first item is LastNote)</param>
    ''' <param name="Add_NoteNumber">Option for NoteName. f.e.: False: "Electric Snare", True: "40 - Electric Snare"</param>
    ''' <returns></returns>
    Public Function Get_GM_DrumVoiceNameList(FirstNote As Byte, LastNote As Byte, Reverse As Boolean, Add_NoteNumber As Boolean) As Dictionary(Of Integer, String)
        Dim List As New Dictionary(Of Integer, String)

        If FirstNote > 127 Then FirstNote = 127
        If LastNote > 127 Then LastNote = 127
        If LastNote < FirstNote Then Return List        ' invalid range, return empty List

        For i = FirstNote To LastNote
            If Add_NoteNumber = False Then
                List.Add(i, Get_GM_DrumVoiceName(i))
            Else
                List.Add(i, i & " - " & Get_GM_DrumVoiceName(i))
            End If
        Next

        If Reverse = True Then
            Dim ListRev As New Dictionary(Of Integer, String)
            For Each item In List.Reverse
                ListRev.Add(item.Key, item.Value)
            Next
            Return ListRev
        End If

        Return List
    End Function

    ''' <summary>
    ''' Returns a list of NoteNumber - NoteName pairs. NoteNames are the Names of the Drum-Instrument 
    ''' which is mapped to a certain NoteNumber. Only mapped notes are listed (35 - 81).
    ''' </summary>
    ''' <param name="Reverse">True: returns a List in descending order (first item is LastNote)</param>
    ''' <param name="Add_NoteNumber">Option for NoteName. f.e.: False: "Electric Snare", True: "40 - Electric Snare"</param>
    ''' <returns></returns>
    Public Function Get_GM_DrumVoiceNameList_Map(Reverse As Boolean, Add_NoteNumber As Boolean) As Dictionary(Of Integer, String)
        Dim List As New Dictionary(Of Integer, String)

        For Each item In GM_DrumVoiceNames
            If Add_NoteNumber = False Then
                List.Add(item.Key, item.Value)
            Else
                List.Add(item.Key, item.Key & " - " & item.Value)
            End If
        Next

        If Reverse = True Then
            Dim ListRev As New Dictionary(Of Integer, String)
            For Each item In List.Reverse
                ListRev.Add(item.Key, item.Value)
            Next
            Return ListRev
        End If

        Return List
    End Function

End Module
