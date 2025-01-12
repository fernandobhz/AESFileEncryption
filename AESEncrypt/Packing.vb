Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.IO.Compression

Public Module Packing

    Function GenerateRSA_AESKey(ByVal RSAPublicKey As String, ByRef outAESKey As Byte()) As Byte()

        Dim AES As New AesCryptoServiceProvider
        outAESKey = AES.Key

        Dim RSA As New RSACryptoServiceProvider
        RSA.FromXmlString(RSAPublicKey)

        Dim RSA_AESKey As Byte() = RSA.Encrypt(outAESKey, False)

        Return RSA_AESKey

    End Function

    Sub ParseRSA_AESKey(RSA_AESKey As Byte(), ByVal RSAPrivateKey As String, ByRef outAESKey As Byte())

        Dim RSA As New RSACryptoServiceProvider
        RSA.FromXmlString(RSAPrivateKey)

        outAESKey = RSA.Decrypt(RSA_AESKey, False)

    End Sub



    Function PackString(inStr As String, ByVal AESKey As Byte()) As Byte()

        Dim inBuff As Byte() = System.Text.Encoding.UTF8.GetBytes(inStr)
        Return Pack(inBuff, AESKey)

    End Function

    Function Pack(inBuff As Byte(), ByVal AESKey As Byte()) As Byte()

        Dim inMS As New MemoryStream(inBuff)
        Dim outMS As New MemoryStream

        Pack(inMS, outMS, AESKey, Nothing)

        Return outMS.ToArray

    End Function

    Sub Pack(ByRef inStream As Stream, ByRef outStream As Stream, ByVal AESKey As Byte(), ByRef Progress As Nullable(Of Integer))

        Dim AES As New AesCryptoServiceProvider
        AES.Key = AESKey

        outStream.Write(AES.IV, 0, 16)

        Const BuffSize As Integer = 1024 * 64
        Dim Buff(BuffSize - 1) As Byte

        Using cryptoStream As New CryptoStream(outStream, AES.CreateEncryptor, CryptoStreamMode.Write)
            Using deflateStream As New DeflateStream(cryptoStream, CompressionMode.Compress)

                Dim bytesRead As Integer

                Do
                    bytesRead = inStream.Read(Buff, 0, BuffSize)
                    deflateStream.Write(Buff, 0, bytesRead)

                    If Not Progress Is Nothing Then
                        Progress = inStream.Position / inStream.Length * 100
                        Application.DoEvents()
                    End If

                Loop Until bytesRead = 0

            End Using
        End Using
    End Sub



    Function UnpackString(inBuff As Byte(), ByVal AESKey As Byte()) As String

        Dim B As Byte() = Unpack(inBuff, AESKey)
        Return System.Text.Encoding.UTF8.GetString(B)

    End Function

    Function Unpack(inBuff As Byte(), ByVal AESKey As Byte()) As Byte()

        Dim inMS As New MemoryStream(inBuff)
        Dim outMS As New MemoryStream

        Unpack(inMS, outMS, AESKey, Nothing)

        Return outMS.ToArray

    End Function

    Sub Unpack(ByRef inStream As Stream, ByRef outStream As Stream, AESKey As Byte(), ByRef Progress As Nullable(Of Integer))

        Dim Iv(15) As Byte
        inStream.Read(Iv, 0, 16)

        Dim AES As New AesCryptoServiceProvider
        AES.Key = AESKey
        AES.IV = Iv

        Const BuffSize As Integer = 1024 * 64
        Dim Buff(BuffSize - 1) As Byte

        Using cryptoStream As New CryptoStream(inStream, AES.CreateDecryptor, CryptoStreamMode.Read)
            Using inflateStream As New DeflateStream(cryptoStream, CompressionMode.Decompress)

                Dim bytesRead As Integer

                Do
                    bytesRead = inflateStream.Read(Buff, 0, BuffSize)
                    outStream.Write(Buff, 0, bytesRead)

                    If Not Progress Is Nothing Then
                        Progress = inStream.Position / inStream.Length * 100
                        Application.DoEvents()
                    End If

                Loop Until bytesRead = 0

            End Using
        End Using
    End Sub

End Module
