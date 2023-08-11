Public Module Module3

    Public Event ScreenRefreshUITools()        ' to be subscribed from Tool windows
    Public Sub ScreenRefresh_UITools_Main()
        RaiseEvent ScreenRefreshUITools()
    End Sub
End Module
