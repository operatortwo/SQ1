Imports System.Globalization
Imports System.Windows.Data

Public Class HexToString_Converter
    Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        'Return Hex(value)
        Return Hex(value)
        'Dim ret As Byte = 0
        'Return If(Byte.TryParse(CType(value, String), ret), ret, 0)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Dim number As Integer = System.Convert.ToInt32(CStr(value), 16)
        Return number
    End Function
End Class