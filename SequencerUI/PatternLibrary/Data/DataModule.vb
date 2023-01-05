Imports System.Data
Imports System.IO
Imports System.IO.Compression

Module DataModule

    Public Property DSI As DataSetIndex               ' Instance of the DataSet in module for access in all parts



#Region "DataSet Index"

    Friend Function LoadIndex() As Boolean

        Dim fs As FileStream
        Dim Archive As ZipArchive

        Try
            fs = New FileStream(PatternLibFullname, FileMode.Open, FileAccess.ReadWrite)
            Archive = New ZipArchive(fs, ZipArchiveMode.Update)

            Dim entry As ZipArchiveEntry = Archive.GetEntry(LibIndexName)
            DSI = New DataSetIndex
            DSI.ReadXml(entry.Open, XmlReadMode.IgnoreSchema)
            DSI.AcceptChanges()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error loading Index of Pattern-Library")
            Return False
        Finally

#Disable Warning BC42104 ' Variable is used before it has been assigned a value
            If Archive IsNot Nothing Then Archive.Dispose()
            If fs IsNot Nothing Then fs.Close()
#Enable Warning BC42104 ' Variable is used before it has been assigned a value

        End Try

        Return True
    End Function

    Friend Function SaveIndex() As Boolean
        If DSI Is Nothing Then Return False

        Dim fs As FileStream
        Dim Archive As ZipArchive

        Try
            fs = New FileStream(PatternLibFullname, FileMode.Open, FileAccess.ReadWrite)
            Archive = New ZipArchive(fs, ZipArchiveMode.Update)

            ' update Index

            Dim IndexEntry As ZipArchiveEntry = Archive.GetEntry(LibIndexName)
            Dim indexstream As Stream
            indexstream = IndexEntry.Open

            indexstream.Seek(0, SeekOrigin.Begin)
            DSI.WriteXml(indexstream, XmlWriteMode.WriteSchema)
            indexstream.Close()

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error saving Index of Pattern-Library")
            Return False
        Finally

#Disable Warning BC42104 ' Variable is used before it has been assigned a value
            If Archive IsNot Nothing Then Archive.Dispose()
            If fs IsNot Nothing Then fs.Close()
#Enable Warning BC42104 ' Variable is used before it has been assigned a value

        End Try

        Return True



    End Function

    Friend Sub CreateNewDataSetIndex()
        DSI = New DataSetIndex

        '--- add Default entries

        ' cat cat2 sources voiceTypes

        Dim row As DataSetIndex.DT_SourcesRow

        'row = DSI.DT_Sources.NewDT_SourcesRow
        'row.Name = Convert.DBNull                  ' create a new row without content
        'DSI.DT_Sources.AddDT_SourcesRow(row)

        row = DSI.DT_Sources.NewDT_SourcesRow
        row.Name = "Midi-File"
        DSI.DT_Sources.AddDT_SourcesRow(row)

        row = DSI.DT_Sources.NewDT_SourcesRow
        row.Name = "Recorded"
        DSI.DT_Sources.AddDT_SourcesRow(row)


        'Dim mrow As DataSetIndex.DT_MainRow = DSI.DT_Main.NewDT_MainRow
        'mrow.Pattern_Name = "empty"
        'DSI.DT_Main.AddDT_MainRow(mrow)

        'DSI.AcceptChanges()

    End Sub


    Friend Function InsertOrOverwriteIndexRow(name As String, NewRow As DataSetIndex.DT_MainRow, overwriteExisting As Boolean) As Boolean
        If DSI Is Nothing Then Return False
        If NewRow Is Nothing Then Return False

        If overwriteExisting = False Then
            DSI.DT_Main.AddDT_MainRow(NewRow)
            Return True
        End If

        Try
            For Each row In DSI.DT_Main
                If row.Pattern_Name = name Then
                    If NewRow.IsID_CategoryNull = False Then row.ID_Category = NewRow.ID_Category
                    If NewRow.IsID_Category2Null = False Then row.ID_Category2 = NewRow.ID_Category
                    If NewRow.IsID_VoiceTypeNull = False Then row.ID_VoiceType = NewRow.ID_VoiceType
                    row.IsDrumPattern = NewRow.IsDrumPattern
                    row.IsPresetPattern = NewRow.IsPresetPattern
                    If NewRow.IsID_SourceNull = False Then row.ID_Source = NewRow.ID_Source
                    If NewRow.IsLengthNull = False Then row.Length = NewRow.Length
                    If NewRow.IsBPMNull = False Then row.BPM = NewRow.BPM
                    If NewRow.IsDateTimeNull = False Then row.DateTime = NewRow.DateTime
                    If NewRow.IsRemarksNull = False Then row.Remarks = NewRow.Remarks
                    ' do not update Position, keep the old value
                    'row.Position = NewRow.Position
                    Return True
                End If
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message & vbCrLf & "Failed to update Index")
            Return False
        End Try

        'not found, create new
        DSI.DT_Main.AddDT_MainRow(NewRow)
        Return True

    End Function


#Region "Variables Table"

    Friend Function Get_DSI_VarValue(name As String) As Integer
        'Dim row As DSP.T90_VariablenRow
        Dim row As DataSetIndex.DT_VariablesRow

        row = DSI.DT_Variables.FindByName(name)
        If row Is Nothing Then
            Return 0                                        ' = value of not-set integer
        Else
            Return row.Value
        End If

    End Function

    Friend Function Get_DSI_VarText(name As String) As String
        'Dim row As DSP.T90_VariablenRow
        Dim row As DataSetIndex.DT_VariablesRow

        row = DSI.DT_Variables.FindByName(name)
        If row Is Nothing Then
            Return ""                                        ' = content of non-set string
        Else
            Return row.Text
        End If

    End Function

    Friend Function Set_DSI_VarValue(name As String, value As Integer) As Boolean
        'Create line if not present
        'in this table PrimaryKey is a string

        If name Is Nothing Then Return False                ' Disallow Nothing string
        If name = "" Then Return False                      ' Don't allow blank names

        Dim row As DataSetIndex.DT_VariablesRow             ' the line to be inserted/changed
        row = DSI.DT_Variables.FindByName(name)             ' get if available

        If row Is Nothing Then                              ' if not available
            row = DSI.DT_Variables.NewDT_VariablesRow       ' create row
            row.Name = name                                 ' Set variable name
            DSI.DT_Variables.AddDT_VariablesRow(row)        ' add row to DataTable
        End If

        If row.Value <> value Then                           ' only if changed
            row.Value = value                                ' set value
        End If

        Return True
    End Function

    Friend Function Set_DSI_VarText(name As String, text As String) As Boolean
        'Create line if not present
        'in this table PrimaryKey is a string

        If name Is Nothing Then Return False                ' Disallow Nothing string
        If name = "" Then Return False                      ' Don't allow blank names

        Dim row As DataSetIndex.DT_VariablesRow             ' the line to be inserted/changed
        row = DSI.DT_Variables.FindByName(name)             ' get if available

        If row Is Nothing Then                              ' if not available
            row = DSI.DT_Variables.NewDT_VariablesRow       ' create row
            row.Name = name                                 ' Set variable name
            DSI.DT_Variables.AddDT_VariablesRow(row)        ' add row to DataTable
        End If

        If row.Text <> text Then                            ' only if changed
            row.Text = text                                 ' set text
        End If

        Return True
    End Function


#End Region
#End Region


End Module
