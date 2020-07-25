Imports System.Security.Cryptography
Imports System.IO.Compression
Imports System.IO

Module Main

    Sub Main()
        Try
            InstallContexMenu.InstallAsOpen(".aes")

            Dim Args As String() = Environment.GetCommandLineArgs

            If Args.Count < 2 Then
                MsgBox("Usage AESDecrypt filename [AESKey]")
                Exit Sub
            End If

            Dim inputFile As String = Args(1)

            If inputFile.Length > 4 Then
                If inputFile.Substring(1, 2) <> ":\" Then
                    inputFile = String.Format("{0}\{1}", Environment.CurrentDirectory, inputFile)
                End If
            Else
                inputFile = String.Format("{0}\{1}", Environment.CurrentDirectory, inputFile)
            End If

            If Not My.Computer.FileSystem.FileExists(inputFile) Then
                MsgBox("File doesn't exists ", MsgBoxStyle.Critical)
                Exit Sub
            End If





            Dim Password As String
            Dim PassOk As Boolean


            If Args.Count = 3 Then
                Password = Args(2)
            Else

                Do
                    Password = InputBox("Password to encrypt file, max 32 chars", "AES Key")

                    If Password.Length > 32 Then
                        MsgBox("Password can't be more than 32 chars")
                        PassOk = False
                    Else
                        PassOk = True
                    End If
                Loop Until PassOk
            End If


            Dim Key As Byte() = System.Text.Encoding.ASCII.GetBytes(Password)

            If Key.Length < 32 Then
                ReDim Preserve Key(31)
            End If





            Dim P As New Progress
            P.Show()

            Dim destFile As String = inputFile.Substring(0, inputFile.LastIndexOf(".aes"))

            Using inStream As New FileStream(inputFile, FileMode.Open)
                Using outStream As New FileStream(destFile, FileMode.Create)
                    Unpack(inStream, outStream, Key, P.ProgressBar1.Value)
                End Using
            End Using

        Catch ex As Exception
            MsgBox(String.Format("Error: {1}{0}Stack Trace:{0} {2}", vbCrLf, ex.Message, ex.StackTrace))
        End Try
    End Sub

End Module

