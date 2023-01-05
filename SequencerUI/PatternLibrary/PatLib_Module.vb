Imports SequencerBase
Imports SequencerUI.My
Imports System.Collections.ObjectModel
Imports System.Data
Imports System.IO
Imports System.IO.Compression

Friend Module PatLib_Module
    Friend BaseDirectory As String = AppDomain.CurrentDomain.BaseDirectory
    Friend PatternDirectory As String = BaseDirectory & "Pattern\"

    Friend PresetPatternLibFilename As String = "PresetPattern.lib"
    Friend PresetPatternLibFullname As String = PatternDirectory & PresetPatternLibFilename

    Friend PresetPatternInfoFilename As String = "PresetPatternInfo.xml"
    Friend PresetPatternInfoFullname As String = PatternDirectory & PresetPatternInfoFilename

    Friend PatternLibFilename As String = "PatternLibrary.lib"
    Friend PatternLibFullname As String = ""

    Friend LibIndexName As String = "Index.xml"

    Friend OwnerWindow As Window

    Friend Function GetPatternFromArchive(name As String) As Pattern
        If name Is Nothing Then Return Nothing
        If name = "" Then Return Nothing

        Dim fs As FileStream
        Dim Archive As ZipArchive
        Dim PatternName As String = "Pattern\" & name & ".xml"
        Dim Pattern As Pattern

        Dim myDeSerializer As New Xml.Serialization.XmlSerializer(GetType(Pattern))

        Try
            fs = New FileStream(PatternLibFullname, FileMode.Open, FileAccess.Read)
            Archive = New ZipArchive(fs, ZipArchiveMode.Read)

            Dim PatEntry As ZipArchiveEntry = Archive.GetEntry(PatternName)
            If PatEntry Is Nothing Then Exit Try
            Dim patstream As Stream
            patstream = PatEntry.Open
            Pattern = CType(myDeSerializer.Deserialize(patstream), Pattern)

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error loading Pattern from Library")
            Return Nothing

#Disable Warning BC42104 ' Variable is used before it has been assigned a value
        Finally
            If Archive IsNot Nothing Then Archive.Dispose()
            If fs IsNot Nothing Then fs.Close()
        End Try

        Return Pattern
#Enable Warning BC42104 ' Variable is used before it has been assigned a value
    End Function

    Friend Function GetPatternsFromArchive(names As ArrayList) As List(Of Pattern)
        Dim PatList As New List(Of Pattern)
        If names Is Nothing Then Return PatList
        If names.Count = 0 Then Return PatList

        Dim fs As FileStream
        Dim Archive As ZipArchive
        Dim PatternName As String
        Dim PatEntry As ZipArchiveEntry
        Dim PatStream As Stream

        Dim Pattern As Pattern

        Dim myDeSerializer As New Xml.Serialization.XmlSerializer(GetType(Pattern))

        Try
            fs = New FileStream(PatternLibFullname, FileMode.Open, FileAccess.Read)
            Archive = New ZipArchive(fs, ZipArchiveMode.Read)

            For Each name In names
                PatternName = "Pattern\" & CStr(name) & ".xml"
                PatEntry = Archive.GetEntry(PatternName)
                If PatEntry Is Nothing Then Continue For
                PatStream = PatEntry.Open
                Try
                    Pattern = CType(myDeSerializer.Deserialize(PatStream), Pattern)
                    PatList.Add(Pattern)
                Catch
                End Try
            Next

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error loading Patterns from Library")
            Return PatList

        Finally

#Disable Warning BC42104 ' Variable is used before it has been assigned a value
            If Archive IsNot Nothing Then Archive.Dispose()
            If fs IsNot Nothing Then fs.Close()
#Enable Warning BC42104 ' Variable is used before it has been assigned a value

        End Try

        Return PatList
    End Function

    ''' <summary>
    ''' Try to write the given Pattern to the Pattern-Library.
    ''' </summary>
    ''' <param name="name">Name of the Pattern</param>
    ''' <param name="pattern">The Pattern</param>
    ''' <param name="overwriteExisting">FALSE: If an entry with the same name existst, name is extended to a unique name. 
    ''' TRUE: If an entry with the same name exists, it will be replaced by the new Pattern. 
    ''' This is intended for a 'read/modify/write' sequence.</param>
    ''' <returns>Name o</returns>
    Friend Function SavePatternToArchive(name As String, pattern As Pattern, overwriteExisting As Boolean) As String
        If name Is Nothing Then Return Nothing
        If name = "" Then Return Nothing
        If pattern Is Nothing Then Return Nothing

        Dim fs As FileStream
        Dim Archive As ZipArchive
        Dim strm As Stream
        Dim InsertNameBase As String
        Dim InsertNameFull As String
        Dim InsertEntry As ZipArchiveEntry
        Dim ReturnName As String                        ' name without path and '.xml' extension

        Dim mySerializer As New Xml.Serialization.XmlSerializer(GetType(Pattern))
        Try
            fs = New FileStream(PatternLibFullname, FileMode.Open, FileAccess.ReadWrite)
            Archive = New ZipArchive(fs, ZipArchiveMode.Update)

            InsertNameBase = name & ".xml"
            InsertNameFull = "Pattern\" & InsertNameBase

            If overwriteExisting = True Then
                Dim oldentry As ZipArchiveEntry = Archive.GetEntry(InsertNameFull)
                If oldentry IsNot Nothing Then
                    oldentry.Delete()
                End If
            End If

            InsertNameFull = CreateUniqueEntryName("Pattern\", InsertNameBase, Archive.Entries)
            InsertEntry = Archive.CreateEntry(InsertNameFull)

            pattern.Name = InsertNameBase.Replace(".xml", "")     ' make sure the name is not empty and contains _000 enum
            ReturnName = pattern.Name

            strm = InsertEntry.Open
            mySerializer.Serialize(strm, pattern)
            strm.Close()

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error inserting preset Pattern")
            Return Nothing
        Finally

#Disable Warning BC42104 ' Variable is used before it has been assigned a value
            If Archive IsNot Nothing Then Archive.Dispose()
            If fs IsNot Nothing Then fs.Close()
#Enable Warning BC42104 ' Variable is used before it has been assigned a value

        End Try

        Return ReturnName
    End Function

    ''' <summary>
    ''' Try to create a new empty Pattern-Library with Index file.
    ''' </summary>
    ''' <param name="fullname"></param>
    ''' <returns></returns>
    Friend Function CreateNewPatternLibrary(fullname As String) As Boolean
        Dim fs As FileStream
        Dim Archive As ZipArchive

        Try
            fs = New FileStream(fullname, FileMode.Create)
            Archive = New ZipArchive(fs, ZipArchiveMode.Update)

            CreateNewDataSetIndex()

            Set_DSI_VarValue("LibVersionMajor", 1)
            Set_DSI_VarValue("LibVersionMinor", 0)

            Dim IndexEntry As ZipArchiveEntry = Archive.CreateEntry(LibIndexName)
            DSI.WriteXml(IndexEntry.Open, XmlWriteMode.WriteSchema)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error creating Pattern Library")
            Return False
        Finally

#Disable Warning BC42104 ' Variable is used before it has been assigned a value
            If Archive IsNot Nothing Then Archive.Dispose()
            If fs IsNot Nothing Then fs.Close()
#Enable Warning BC42104 ' Variable is used before it has been assigned a value

        End Try

        Return True
    End Function

    ''' <summary>
    ''' Imports Preset Patterns from a Preset Library.
    ''' </summary>
    ''' <param name="LibraryFullname"></param>
    ''' <param name="PresetsFullname"></param>
    ''' <returns></returns>
    Friend Function ImportPresetPatterns(LibraryFullname As String, PresetsFullname As String) As Boolean
        If LibraryFullname Is Nothing Then Return False
        If LibraryFullname = "" Then Return False
        If IO.File.Exists(LibraryFullname) = False Then Return False
        If PresetsFullname Is Nothing Then Return False
        If PresetsFullname = "" Then Return False
        If IO.File.Exists(PresetsFullname) = False Then Return False

        Dim fsPre As FileStream
        Dim fs As FileStream
        Dim ArchivePre As ZipArchive
        Dim Archive As ZipArchive

        Try
            fsPre = New FileStream(PresetsFullname, FileMode.Open, FileAccess.Read)
            ArchivePre = New ZipArchive(fsPre, ZipArchiveMode.Read)
            fs = New FileStream(LibraryFullname, FileMode.Open, FileAccess.ReadWrite)
            Archive = New ZipArchive(fs, ZipArchiveMode.Update)

            Dim IndexEntry As ZipArchiveEntry = Archive.GetEntry(LibIndexName)
            Dim indexstream As Stream
            indexstream = IndexEntry.Open
            DSI.Clear()
            DSI.ReadXml(indexstream, XmlReadMode.IgnoreSchema)

            Dim InsertNameBase As String
            Dim InsertNameFull As String
            Dim InsertEntry As ZipArchiveEntry
            Dim mrow As DataSetIndex.DT_MainRow
            Dim strm As Stream
            Dim patxstream As Stream
            Dim myDeSerializerX As New Xml.Serialization.XmlSerializer(GetType(PatternX))
            Dim mySerializer As New Xml.Serialization.XmlSerializer(GetType(Pattern))
            Dim patx As New PatternX
            Dim pat As New Pattern

            For Each prepx As ZipArchiveEntry In ArchivePre.Entries
                InsertNameBase = prepx.Name
                InsertNameFull = "Pattern\" & prepx.Name
                InsertNameFull = CreateUniqueEntryName("Pattern\", InsertNameBase, Archive.Entries)
                InsertEntry = Archive.CreateEntry(InsertNameFull)

                patxstream = prepx.Open
                patx = CType(myDeSerializerX.Deserialize(patxstream), PatternX)
                pat = patx.ToPattern
                pat.Name = InsertNameBase.Replace(".xml", "")     ' make sure the name is not empty and contains _000 enum

                strm = InsertEntry.Open
                mySerializer.Serialize(strm, pat)
                strm.Close()

                mrow = DSI.DT_Main.NewDT_MainRow
                mrow.IsPresetPattern = True
                mrow.Pattern_Name = InsertNameBase.Replace(".xml", "")      ' name without .xml
                mrow.DateTime = Now
                mrow.Position = CUInt(mrow.ID)
                If prepx.FullName.Contains("Drum") Then
                    mrow.IsDrumPattern = True
                End If

                mrow.BPM = CUShort(patx.BPM)
                mrow.Length = CUInt(patx.Length \ patx.TPQ)

                Dim insID As Integer                        ' Lookup ID to insert

                '-- Category                
                insID = FindOrInsertCategory(patx.Category)
                If insID <> 0 Then
                    mrow.ID_Category = insID
                End If
                '-- Category2                
                insID = FindOrInsertCategory(patx.Category2)
                If insID <> 0 Then
                    mrow.ID_Category2 = insID
                End If
                '-- VoiceType
                'patx.VoiceType
                insID = FindOrInsertVoiceType(patx.VoiceType)
                If insID <> 0 Then
                    mrow.ID_VoiceType = insID
                End If
                '-- Source
                insID = FindOrInsertSource(patx.Source)
                If insID <> 0 Then
                    mrow.ID_Source = insID
                End If

                DSI.DT_Main.AddDT_MainRow(mrow)
            Next

            '---
            Dim version As Integer

            Try
                Dim Information As XElement = XElement.Load(PresetPatternInfoFullname)      ' PresetPatternInfo.xml
                version = CInt(Information.<PresetVersion>.Value)
            Catch
            End Try

            Set_DSI_VarValue("PresetVersion", version)

            ' update Index
            indexstream.Seek(0, SeekOrigin.Begin)
            DSI.WriteXml(indexstream, XmlWriteMode.WriteSchema)
            indexstream.Close()

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error inserting preset Pattern")
            Return False
        Finally

#Disable Warning BC42104 ' Variable is used before it has been assigned a value
            If ArchivePre IsNot Nothing Then ArchivePre.Dispose()
            If fsPre IsNot Nothing Then fsPre.Close()
            If Archive IsNot Nothing Then Archive.Dispose()
            If fs IsNot Nothing Then fs.Close()
#Enable Warning BC42104 ' Variable is used before it has been assigned a value

        End Try

        Return True
    End Function

    ''' <summary>
    ''' Test if the given Name is alredy used. Enumerate with '_000' until a unique name is found.
    ''' </summary>
    ''' <param name="RelativePath">Relative path like 'MyPath\ or empty string'</param>
    ''' <param name="EntryName">Like 'readme.txt'. The name part may be modified by the function.</param>
    ''' <param name="ArchiveEntries">EntryCollection of the destination ZipArchive </param>
    ''' <returns></returns>
    Friend Function CreateUniqueEntryName(RelativePath As String, ByRef EntryName As String, ArchiveEntries As ReadOnlyCollection(Of ZipArchiveEntry)) As String
        If EntryName Is Nothing Then Return Nothing
        Dim NewFullName As String = RelativePath & EntryName
        If NewFullName Is Nothing Then Return ""
        Dim NewEntryName As String

        Dim entry As ZipArchiveEntry

        entry = ArchiveEntries.FirstOrDefault(Function(x) x.FullName = NewFullName)
        ' the normal situation, the given name is unique
        If entry Is Nothing Then Return NewFullName

        ' an entry with this name already exists, create a unique one

        Dim basename As String = Path.GetFileNameWithoutExtension(EntryName)
        Dim ext As String = Path.GetExtension(EntryName)

        ' Check if there is already a special numeric ending in the name ("_nnn")
        If basename.Length > 4 Then
            If basename(basename.Length - 4) = "_" Then
                If IsNumeric(Right(basename, 3)) = True Then
                    basename = basename.Remove(basename.Length - 4)         ' strip the numeric ending
                Else
                    NewEntryName = basename & "_001" & ext
                    NewFullName = RelativePath & NewEntryName
                    ' check if _001 already exists in the archive
                    entry = ArchiveEntries.FirstOrDefault(Function(x) x.FullName = NewFullName)
                    If entry Is Nothing Then
                        EntryName = NewEntryName                            ' modify input name
                        Return NewFullName
                    End If
                End If
            End If
        End If

        Try
            Dim likepattern As String = RelativePath & basename & "_###" & ext

            Dim query = From ae In ArchiveEntries
                        Where ae.FullName Like likepattern
                        Order By ae.Name
                        Select ae.Name

            Dim nlist As New List(Of UShort)
            Dim num As UShort
            Dim numstr As String

            For i = 1 To query.Count
                numstr = Right(Path.GetFileNameWithoutExtension(query(i - 1)), 3)   ' get the '###' filtered above
                If UShort.TryParse(numstr, num) = True Then
                    nlist.Add(num)
                End If
            Next

            Dim lastnum As UShort
            If nlist.Count > 0 Then
                lastnum = nlist.Last(Function(x) x > 0)
            End If
            lastnum = CUShort(lastnum + 1)                                                       ' set to next available number

            If lastnum > 999 Then Throw New Exception("Entry num > 999")
            NewEntryName = basename & "_" & lastnum.ToString("D3") & ext
            NewFullName = RelativePath & NewEntryName
        Catch ex As Exception
            ' something went wrong, not serious, we can go on
            ' this will create an entry with a duplicate name

            Console.WriteLine("xxx Exception  " & NewFullName)
            Return RelativePath & EntryName
        End Try

        EntryName = NewEntryName                        ' this is modified
        Return NewFullName
    End Function


    ''' <summary>
    ''' Test if the given Name is alredy used in DT_Main. Enumerate with '_000' until a unique name is found.
    ''' </summary>
    ''' <param name="InputName">The name may be modified by the function.</param>
    ''' <returns>FALSE if InputName is unchanged.</returns>
    Friend Function CreateUniqueIndexName(ByRef InputName As String) As Boolean
        If InputName Is Nothing Then
            Return False
        End If

        If DSI.DT_Main Is Nothing Then Return False

        Dim Name As String = InputName
        Dim row As DataSetIndex.DT_MainRow

        row = DSI.DT_Main.FirstOrDefault(Function(x) x.Pattern_Name = Name)
        If row Is Nothing Then Return False

        ' an entry with this name already exists, create a unique one

        ' Check if there is already a special numeric ending in the name ("_nnn")

        Dim basename As String = InputName
        Dim NewName As String = ""

        If basename.Length > 4 Then
            If basename(basename.Length - 4) = "_" Then
                If IsNumeric(Right(basename, 3)) = True Then
                    basename = basename.Remove(basename.Length - 4)         ' strip the numeric ending
                Else
                    NewName = basename & "_001"
                    ' check if _001 already exists in the archive
                    row = DSI.DT_Main.FirstOrDefault(Function(x) x.Pattern_Name = NewName)
                    If row Is Nothing Then
                        InputName = NewName                            ' modify input name
                        Return True
                    End If
                End If
            End If
        End If

        '--- find number

        Try
            Dim likepattern As String = basename & "_###"

            Dim query = From ae In DSI.DT_Main
                        Where ae.Pattern_Name Like likepattern
                        Order By ae.Pattern_Name
                        Select ae.Pattern_Name

            Dim nlist As New List(Of UShort)
            Dim num As UShort
            Dim numstr As String

            For i = 1 To query.Count
                numstr = Right(Path.GetFileNameWithoutExtension(query(i - 1)), 3)   ' get the '###' filtered above
                If UShort.TryParse(numstr, num) = True Then
                    nlist.Add(num)
                End If
            Next

            Dim lastnum As UShort
            If nlist.Count > 0 Then
                lastnum = nlist.Last(Function(x) x > 0)
            End If
            lastnum = CUShort(lastnum + 1)                                                       ' set to next available number

            If lastnum > 999 Then Throw New Exception("Entry num > 999")
            NewName = basename & "_" & lastnum.ToString("D3")
        Catch ex As Exception
            ' something went wrong, not serious, we can go on
            ' this will create an entry with a duplicate name

            Console.WriteLine("xxx Exception  " & NewName)
            Return False
        End Try

        InputName = NewName                        ' this is modified
        Return True
    End Function


#Region "FindOrInsert"

    ''' <summary>
    ''' In DT_Categories: check if the given name is already listed. If True, return the ID of the row, else
    '''  insert a new row and return the new ID. 
    ''' </summary>
    ''' <param name="InputName"></param>
    ''' <returns>Returns 0 if input name is nothing or empty.</returns>
    Friend Function FindOrInsertCategory(InputName As String) As Integer
        If InputName Is Nothing Then Return 0
        If InputName = "" Then Return 0

        For Each row In DSI.DT_Categories
            If row.Name = InputName Then
                Return row.ID
            End If
        Next

        Dim newrow As DataSetIndex.DT_CategoriesRow = DSI.DT_Categories.NewDT_CategoriesRow
        newrow.Name = InputName
        newrow.Position = CUShort(DSI.DT_Categories.Rows.Count + 1)
        DSI.DT_Categories.AddDT_CategoriesRow(newrow)
        Return newrow.ID
    End Function

    ''' <summary>
    ''' In DT_Categories2: check if the given name is already listed. If True, return the ID of the row, else
    '''  insert a new row and return the new ID. 
    ''' </summary>
    ''' <param name="InputName"></param>
    ''' <returns>Returns 0 if input name is nothing or empty.</returns>
    Friend Function FindOrInsertCategory2(InputName As String) As Integer
        If InputName Is Nothing Then Return 0
        If InputName = "" Then Return 0

        For Each row In DSI.DT_Categories2
            If row.Name = InputName Then
                Return row.ID
            End If
        Next

        Dim newrow As DataSetIndex.DT_Categories2Row = DSI.DT_Categories2.NewDT_Categories2Row
        newrow.Name = InputName
        newrow.Position = CUShort(DSI.DT_Categories.Rows.Count + 1)
        DSI.DT_Categories2.AddDT_Categories2Row(newrow)
        Return newrow.ID
    End Function

    ''' <summary>
    ''' In DT_VoiceType: check if the given name is already listed. If True, return the ID of the row, else
    '''  insert a new row and return the new ID. 
    ''' </summary>
    ''' <param name="InputName"></param>
    ''' <returns>Returns 0 if input name is nothing or empty.</returns>
    Friend Function FindOrInsertVoiceType(InputName As String) As Integer
        If InputName Is Nothing Then Return 0
        If InputName = "" Then Return 0

        For Each row In DSI.DT_VoiceTypes
            If row.Name = InputName Then
                Return row.ID
            End If
        Next

        Dim newrow As DataSetIndex.DT_VoiceTypesRow = DSI.DT_VoiceTypes.NewDT_VoiceTypesRow
        newrow.Name = InputName
        newrow.Position = CUShort(DSI.DT_VoiceTypes.Rows.Count + 1)
        DSI.DT_VoiceTypes.AddDT_VoiceTypesRow(newrow)
        Return newrow.ID
    End Function

    ''' <summary>
    ''' In DT_Sources: check if the given name is already listed. If True, return the ID of the row, else
    '''  insert a new row and return the new ID. 
    ''' </summary>
    ''' <param name="InputName"></param>
    ''' <returns>Returns 0 if input name is nothing or empty.</returns>
    Friend Function FindOrInsertSource(InputName As String) As Integer
        If InputName Is Nothing Then Return 0
        If InputName = "" Then Return 0

        For Each row In DSI.DT_Sources
            If row.Name = InputName Then
                Return row.ID
            End If
        Next

        Dim newrow As DataSetIndex.DT_SourcesRow = DSI.DT_Sources.NewDT_SourcesRow
        newrow.Name = InputName
        newrow.Position = CUShort(DSI.DT_Sources.Rows.Count + 1)
        DSI.DT_Sources.AddDT_SourcesRow(newrow)
        Return newrow.ID
    End Function

#End Region


#Region "Dialogs"
    ''' <summary>
    ''' Check if there is a Setting for PatternLibFullname. If none exists, ask user for path.
    ''' </summary>
    ''' <param name="PatLibFullnameSetting">The current setting value</param>
    ''' <returns>TRUE if setting was saved successfully. FALSE if user cancelled.</returns>
    Friend Function CheckIfPatLibFullnameSettingExists(PatLibFullnameSetting As String) As Boolean
        If PatLibFullnameSetting = "" Then
            Dim dlg As New DlgSetPathToLibrary
            dlg.Owner = OwnerWindow
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner
            dlg.ShowDialog()
            If dlg.DialogResult = False Then Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Assume there is a setting for PatternLibFullname. Check if the file exists, else ask user to change path
    '''  or create a new PatternLibrary.
    ''' </summary>
    ''' <param name="PatLibFullnameSetting">The current setting value</param>
    ''' <returns>TRUE if PatternLibrary was found or created. FALSE if user cancelled or an error occured while creating.</returns>
    Friend Function CheckIfPatLibFullnameExitst(PatLibFullnameSetting As String) As Boolean
        If IO.File.Exists(PatLibFullnameSetting) Then
            Return True
        Else
            Dim dlg As New DlgFindOrCreateLibrary
            dlg.Owner = OwnerWindow
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner
            dlg.ShowDialog()
            If dlg.DialogResult = True Then
                Return True
            Else
                Return False
            End If
        End If
    End Function

#End Region
End Module
