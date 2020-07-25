Imports Microsoft.Win32
Imports System.Windows.Forms

Friend Class InstallContexMenu

    Friend Shared Sub InstallAsContext()
        Dim MyExePath As String = InstallApp()

        If Not String.IsNullOrEmpty(MyExePath) Then
            InstallVerb(FileExtension:="*", ProgramPath:=MyExePath, Verb:=My.Application.Info.ProductName)
            MsgBox(String.Format("{0} installed, use windows context menu to use it", My.Application.Info.ProductName))
            End
        End If
    End Sub

    Friend Shared Sub InstallAsOpen(FileExtension As String)
        Dim MyExePath As String = InstallApp()

        If Not String.IsNullOrEmpty(MyExePath) Then
            InstallVerb(FileExtension:=FileExtension, ProgramPath:=MyExePath, Verb:="open")
            MsgBox(String.Format("{0} installed, double click on file extension {1} to use it", My.Application.Info.ProductName, FileExtension))
            End
        End If
    End Sub

    Private Shared Function InstallApp() As String
        If Process.GetCurrentProcess.ProcessName.Contains("vshost") Then Return Nothing

        Dim AppFolder As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        Dim MyAppFolder As String = IO.Path.Combine(AppFolder, My.Application.Info.ProductName)

        If Not My.Computer.FileSystem.DirectoryExists(MyAppFolder) Then
            My.Computer.FileSystem.CreateDirectory(MyAppFolder)
        End If

        Dim MyExePath As String = IO.Path.Combine(MyAppFolder, IO.Path.GetFileName(Application.ExecutablePath))

        If Application.ExecutablePath = MyExePath Then Return Nothing

        If My.Computer.FileSystem.FileExists(MyExePath) Then
            My.Computer.FileSystem.DeleteFile(MyExePath)
        End If

        My.Computer.FileSystem.CopyFile(Application.ExecutablePath, MyExePath)

        Return MyExePath
    End Function

    Friend Shared Sub InstallVerb(FileExtension As String, ProgramPath As String, Optional Verb As String = "open")
        Dim RClasses = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes", True)


        If RClasses.GetSubKeyNames.FirstOrDefault(Function(x) x = FileExtension) Is Nothing Then
            RClasses.CreateSubKey(FileExtension)
        End If

        Dim RExtension = RClasses.OpenSubKey(FileExtension, True)


        If RExtension.GetSubKeyNames.FirstOrDefault(Function(x) x = "shell") Is Nothing Then
            RExtension.CreateSubKey("shell")
        End If

        Dim RShell = RExtension.OpenSubKey("shell", True)


        If RShell.GetSubKeyNames.FirstOrDefault(Function(x) x = Verb) IsNot Nothing Then
            RShell.DeleteSubKeyTree(Verb)
        End If

        Dim RKey = RShell.CreateSubKey(Verb)
        Dim CmdKey = RKey.CreateSubKey("command")

        CmdKey.SetValue(String.Empty, String.Format("{0} ""%1""", ProgramPath))
    End Sub

End Class
