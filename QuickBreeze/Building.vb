Imports System.IO
Imports Ionic.Zip
Imports System.Threading
Imports System.Security.Cryptography
Imports System.Text
Public Class Building
    Public directory As String = CurDir()
    Public resources As String = directory & "\Resources"
    Public drive As String = Path.GetPathRoot(My.Application.Info.DirectoryPath)
    Public WorkingDirectory As String = drive & "QuickBreeze\"
    Public bundles As String = directory & "\Firmware Bundles\"
    Public SHAHash As String
    Public IPSW As String

    Public iBSSName As String
    Public iBSSKey As String
    Public iBSSIV As String
    Public RootFSName As String
    Public RootFSKey As String
    Public RestoreRamdiskName As String
    Public RestoreRamdiskKey As String
    Public RestoreRamdiskIV As String
    Public Sub Decide()
        If SHAHash = "b9a34984d93ca183021bfab7bb4e29fc21c55696" Then
            iBSSKey = "6d26c20f472d9ed5ab6219e632e35b4c582c1c402104aa39d75471171c88d473"
            iBSSIV = "5a01a1e31d2ae895690cd279dbd5e3c0"
            RootFSName = "038-0017-002.dmg"
            RootFSKey = "982437b30d334c744c94b9a73ab70e0fc6ed94c181b2a8b0fde6ee03f2546cc9b2c5b01c"
            RestoreRamdiskName = "038-0032-002.dmg"
            RestoreRamdiskKey = "06849aead2e9a6ca8a82c3929bad5c2368942e3681a3d5751720d2aacf0694c0"
            RestoreRamdiskIV = "9b20ae16bebf4cf1b9101374c3ab0095"
            Call iPodTouch4G()
            Exit Sub
        ElseIf SHAHash = "366b28e9c95936bd4b11a84d54fefaf079fd6411" Then 'iPhone 4 (4.2.1)
            iBSSKey = "32398d3d1328ed3f0e1949446a1357585ae1973b3c8434b83df49ac55cf45d06"
            iBSSIV = "45bbf0fa98573425fa21dc6e529eba6"
            RootFSName = "038-0019-002.dmg"
            RootFSKey = "b2ee5018ef7d02e45ef67449d9e2ed5f876efae949de64a9a93dbcf7ff9ed84e041e9167"
            RestoreRamdiskName = "038-0032-002.dmg"
            RestoreRamdiskKey = "06849aead2e9a6ca8a82c3929bad5c2368942e3681a3d5751720d2aacf0694c0"
            RestoreRamdiskIV = "9b20ae16bebf4cf1b9101374c3ab0095"
            Call iPhone4()
            Exit Sub
        Else
            End
        End If
    End Sub
    Sub Cmd(ByVal file As String, ByVal arg As String)
        Dim procNlite As New Process
        Dim winstyle As Integer = 1
        procNlite.StartInfo.FileName = file
        procNlite.StartInfo.Arguments = " " & arg
        procNlite.StartInfo.WindowStyle = winstyle
        Application.DoEvents()
        procNlite.Start()
        Do Until procNlite.HasExited
            Application.DoEvents()
            For i = 0 To 5000000
                Application.DoEvents()
            Next
        Loop
        procNlite.WaitForExit()
    End Sub
    Public Sub getSHA1()
        BrowseIPSW.ShowDialog()
        If BrowseIPSW.FileName = "" Then
            MsgBox("Select IPSW!")
            Exit Sub
        Else
            Changer("Processing IPSW")
            IPSW = BrowseIPSW.FileName
            Dim SHA1Hasher As SHA1 = SHA1.Create()
            Dim data As Byte() = SHA1Hasher.ComputeHash(Encoding.Default.GetBytes(IPSW))
            Dim sBuilder As New StringBuilder()
            Dim i As Integer
            For i = 0 To data.Length - 1
                sBuilder.Append(data(i).ToString("x2"))
            Next i
            SHAHash = sBuilder.ToString.ToLower
            Call Decide()
            Exit Sub
        End If
    End Sub
    Public Sub ExtractIPSW()
        Changer("Extracting IPSW")
        Dim ZipToUnpack As String = IPSW
        Dim Zip As ZipFile = ZipFile.Read(ZipToUnpack)
        Zip.ExtractAll(WorkingDirectory, True)
    End Sub
    Private Sub iPhone4()
        Call ExtractIPSW()
        Changer("Extracting Resources")
        System.IO.File.Copy(resources & "\zlib1.dll", WorkingDirectory & "zlib1.dll", True)
        System.IO.File.Copy(resources & "\xpwntool.exe", WorkingDirectory & "xpwntool.exe", True)
        System.IO.File.Copy(resources & "\libpng12.dll", WorkingDirectory & "libpng12.dll", True)
        System.IO.File.Copy(resources & "\libeay32.dll", WorkingDirectory & "libeay32.dll", True)
        System.IO.File.Copy(resources & "\hfsplus.exe", WorkingDirectory & "hfsplus.exe", True)
        System.IO.File.Copy(resources & "\bspatch.exe", WorkingDirectory & "bspatch.exe", True)
        System.IO.File.Copy(resources & "\Cydia.tar", WorkingDirectory & "Cydia.tar", True)
        System.IO.File.Copy(resources & "\dmg.exe", WorkingDirectory & "dmg.exe", True)

        Changer("Decrypting Restore Ramdisk")
        ChDir(WorkingDirectory)
        Cmd("xpwntool.exe", RestoreRamdiskName & " D" & RestoreRamdiskName & " -k " & RestoreRamdiskKey & " -iv " & RestoreRamdiskIV)
        Changer("Decrypting iBSS")
        System.IO.File.Copy(WorkingDirectory & "Firmware\dfu\iBSS.n90ap.RELEASE.dfu", WorkingDirectory & "iBSS.n90ap.RELEASE.dfu", True)
        Cmd("xpwntool.exe", "iBSS.n90ap.RELEASE.dfu DiBSS.n90ap.RELEASE.dfu -k " & iBSSKey & " -iv " & iBSSIV)

        Changer("Decrypting RootFS")
        Cmd("dmg.exe", "extract " & RootFSName & " D" & RootFSName & " -k " & RootFSKey) 'RootFS

        Changer("Extracting Firmware Bundle")
        System.IO.File.Copy(bundles & "\ip4\iBSS.n90ap.RELEASE.patch", WorkingDirectory & "iBSS.n90ap.RELEASE.patch", True)
        System.IO.File.Copy(bundles & "\ip4\asr.patch", WorkingDirectory & "asr.patch", True)
        System.IO.File.Copy(bundles & "\ip4\fstab.patch", WorkingDirectory & "fstab.patch", True)

        Changer("Patching iBSS")
        Cmd("bspatch.exe", "DiBSS.n90ap.RELEASE.dfu iBSS.n90ap.RELEASE.pwned.dfu iBSS.n90ap.RELEASE.patch")
        System.IO.File.Delete(WorkingDirectory & "iBSS.n90ap.RELEASE.patch")

        Changer("Encrypting iBSS")
        Cmd("xpwntool.exe", "iBSS.n90ap.RELEASE.pwned.dfu  iBSS.n90ap.RELEASEd.dfu -t iBSS.n90ap.RELEASE.dfu")
        System.IO.File.Delete(WorkingDirectory & "iBSS.n90ap.RELEASE.dfu")
        My.Computer.FileSystem.RenameFile(WorkingDirectory & "iBSS.n90ap.RELEASEd.dfu", "iBSS.n90ap.RELEASE.dfu")
        System.IO.File.Delete(WorkingDirectory & "DiBSS.n90ap.RELEASE.dfu")
        System.IO.File.Delete(WorkingDirectory & "iBSS.n90ap.RELEASE.pwned.dfu")
        System.IO.File.Copy(WorkingDirectory & "iBSS.n90ap.RELEASE.dfu", WorkingDirectory & "\firmware\dfu\iBSS.n90ap.RELEASE.dfu", True)
        System.IO.File.Delete(WorkingDirectory & "iBSS.n90ap.RELEASE.dfu")

        Changer("Extracting asr")
        Cmd("hfsplus.exe", "D" & RootFSName & " extract /usr/sbin/asr asr")

        Changer("Extracting fstab")
        Cmd(WorkingDirectory & "hfsplus.exe", "D" & RootFSName & " extract /private/etc/fstab fstab")

        Changer("Uploading pwned asr")
        Cmd(WorkingDirectory & "bspatch.exe", "asr asr.pwned asr.patch")
        System.IO.File.Delete(WorkingDirectory & "asr")
        My.Computer.FileSystem.RenameFile(WorkingDirectory & "asr.pwned", "asr")
        Cmd(WorkingDirectory & "hfsplus.exe", "D" & RootFSName & " add asr /usr/sbin/")
        System.IO.File.Delete(WorkingDirectory & "asr")
        System.IO.File.Delete(WorkingDirectory & "asr.patch")

        Changer("Uploading pwned fstab")
        Cmd(WorkingDirectory & "bspatch.exe", "fstab fstab.pwned fstab.patch")
        System.IO.File.Delete(WorkingDirectory & "fstab")
        My.Computer.FileSystem.RenameFile(WorkingDirectory & "fstab.pwned", "fstab")
        Cmd(WorkingDirectory & "hfsplus.exe", "D" & RootFSName & " add fstab /private/etc/fstab")
        System.IO.File.Delete(WorkingDirectory & "fstab")
        System.IO.File.Delete(WorkingDirectory & "fstab.patch")

        Changer("Adding Cydia")
        Cmd(WorkingDirectory & "hfsplus.exe", "D" & RootFSName & " untar Cydia.tar")

        Changer("Rebuilding RootFS")
        Cmd(WorkingDirectory & "dmg.exe", "build " & "D" & RootFSName & " DR" & RootFSName)
        System.IO.File.Delete(WorkingDirectory & "D" & RootFSName)
        System.IO.File.Delete(WorkingDirectory & RootFSName)
        My.Computer.FileSystem.RenameFile(WorkingDirectory & "DR" & RootFSName, RootFSName)

        Changer("Encrypting Restore Ramdisk")
        Cmd(WorkingDirectory & "xpwntool.exe", "D" & RestoreRamdiskName & " T" & RestoreRamdiskName & " -t " & RestoreRamdiskName)
        System.IO.File.Delete(WorkingDirectory & RestoreRamdiskName)
        System.IO.File.Delete(WorkingDirectory & "D" & RestoreRamdiskName)
        My.Computer.FileSystem.RenameFile(WorkingDirectory & "T" & RestoreRamdiskName, RestoreRamdiskName)
        Changer("Cleaning Up")
        System.IO.File.Delete(WorkingDirectory & "zlib1.dll")
        System.IO.File.Delete(WorkingDirectory & "xpwntool.exe")
        System.IO.File.Delete(WorkingDirectory & "libpng32.dll")
        System.IO.File.Delete(WorkingDirectory & "libpng12.dll")
        System.IO.File.Delete(WorkingDirectory & "libeay32.dll")
        System.IO.File.Delete(WorkingDirectory & "hfsplus.exe")
        System.IO.File.Delete(WorkingDirectory & "bspatch.exe")
        System.IO.File.Delete(WorkingDirectory & "Cydia.tar")
        System.IO.File.Delete(WorkingDirectory & "dmg.exe")
        Changer("Creating IPSW")
        Dim this As New ZipFile
        this.AddDirectory(WorkingDirectory)
        Dim desktop As String = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        this.Save(desktop & "\iPhone3,1_4.2.1_8C148_Custom.ipsw")
        Changer("Done!")
    End Sub
    Private Sub Changer(ByVal Message As String)
        If Message = "Processing IPSW" And Message = "Extracting IPSW" And Message = "Creating IPSW" Then
            Label1.Text = Message
            Label1.Left = (Width / 2) - (Label1.Width / 2)
            Exit Sub
        Else
            Label1.Text = Message
            Label1.Left = (Width / 2) - (Label1.Width / 2)
            ProgressBar.PerformStep()
            Exit Sub
        End If
    End Sub
    Private Sub iPodTouch4G()
        Call ExtractIPSW()
        Changer("Extracting Resources")
        System.IO.File.Copy(resources & "\zlib1.dll", WorkingDirectory & "zlib1.dll", True)
        System.IO.File.Copy(resources & "\xpwntool.exe", WorkingDirectory & "xpwntool.exe", True)
        System.IO.File.Copy(resources & "\libpng12.dll", WorkingDirectory & "libpng12.dll", True)
        System.IO.File.Copy(resources & "\libeay32.dll", WorkingDirectory & "libeay32.dll", True)
        System.IO.File.Copy(resources & "\hfsplus.exe", WorkingDirectory & "hfsplus.exe", True)
        System.IO.File.Copy(resources & "\bspatch.exe", WorkingDirectory & "bspatch.exe", True)
        System.IO.File.Copy(resources & "\Cydia.tar", WorkingDirectory & "Cydia.tar", True)
        System.IO.File.Copy(resources & "\dmg.exe", WorkingDirectory & "dmg.exe", True)

        Changer("Decrypting Restore Ramdisk")
        ChDir(WorkingDirectory)
        Cmd("xpwntool.exe", RestoreRamdiskName & " D" & RestoreRamdiskName & " -k " & RestoreRamdiskKey & " -iv " & RestoreRamdiskIV)
        Changer("Decrypting iBSS")
        System.IO.File.Copy(WorkingDirectory & "Firmware\dfu\iBSS.n81ap.RELEASE.dfu", WorkingDirectory & "iBSS.n81ap.RELEASE.dfu", True)
        Cmd("xpwntool.exe", "iBSS.n81ap.RELEASE.dfu DiBSS.n81ap.RELEASE.dfu -k " & iBSSKey & " -iv " & iBSSIV)

        Changer("Decrypting RootFS")
        Cmd("dmg.exe", "extract " & RootFSName & " D" & RootFSName & " -k " & RootFSKey) 'RootFS

        Changer("Extracting Firmware Bundle")
        System.IO.File.Copy(bundles & "\ipt4\iBSS.n81ap.RELEASE.patch", WorkingDirectory & "iBSS.n81ap.RELEASE.patch", True)
        System.IO.File.Copy(bundles & "\ipt4\asr.patch", WorkingDirectory & "asr.patch", True)
        System.IO.File.Copy(bundles & "\ipt4\fstab.patch", WorkingDirectory & "fstab.patch", True)

        Changer("Patching iBSS")
        Cmd("bspatch.exe", "DiBSS.n81ap.RELEASE.dfu iBSS.n81ap.RELEASE.pwned.dfu iBSS.n81ap.RELEASE.patch")
        System.IO.File.Delete(WorkingDirectory & "iBSS.n81ap.RELEASE.patch")

        Changer("Encrypting iBSS")
        Cmd("xpwntool.exe", "iBSS.n81ap.RELEASE.pwned.dfu  iBSS.n81ap.RELEASEd.dfu -t iBSS.n81ap.RELEASE.dfu")
        System.IO.File.Delete(WorkingDirectory & "iBSS.n81ap.RELEASE.dfu")
        My.Computer.FileSystem.RenameFile(WorkingDirectory & "iBSS.n81ap.RELEASEd.dfu", "iBSS.n81ap.RELEASE.dfu")
        System.IO.File.Delete(WorkingDirectory & "DiBSS.n81ap.RELEASE.dfu")
        System.IO.File.Delete(WorkingDirectory & "iBSS.n81ap.RELEASE.pwned.dfu")
        System.IO.File.Copy(WorkingDirectory & "iBSS.n81ap.RELEASE.dfu", WorkingDirectory & "\firmware\dfu\iBSS.n81ap.RELEASE.dfu", True)
        System.IO.File.Delete(WorkingDirectory & "iBSS.n81ap.RELEASE.dfu")

        Changer("Extracting asr")
        Cmd("hfsplus.exe", "D" & RootFSName & " extract /usr/sbin/asr asr")

        Changer("Extracting fstab")
        Cmd(WorkingDirectory & "hfsplus.exe", "D" & RootFSName & " extract /private/etc/fstab fstab")

        Changer("Uploading pwned asr")
        Cmd(WorkingDirectory & "bspatch.exe", "asr asr.pwned asr.patch")
        System.IO.File.Delete(WorkingDirectory & "asr")
        My.Computer.FileSystem.RenameFile(WorkingDirectory & "asr.pwned", "asr")
        Cmd(WorkingDirectory & "hfsplus.exe", "D" & RootFSName & " add asr /usr/sbin/")
        System.IO.File.Delete(WorkingDirectory & "asr")
        System.IO.File.Delete(WorkingDirectory & "asr.patch")

        Changer("Uploading pwned fstab")
        Cmd(WorkingDirectory & "bspatch.exe", "fstab fstab.pwned fstab.patch")
        System.IO.File.Delete(WorkingDirectory & "fstab")
        My.Computer.FileSystem.RenameFile(WorkingDirectory & "fstab.pwned", "fstab")
        Cmd(WorkingDirectory & "hfsplus.exe", "D" & RootFSName & " add fstab /private/etc/fstab")
        System.IO.File.Delete(WorkingDirectory & "fstab")
        System.IO.File.Delete(WorkingDirectory & "fstab.patch")

        Changer("Adding Cydia")
        Cmd(WorkingDirectory & "hfsplus.exe", "D" & RootFSName & " untar Cydia.tar")

        Changer("Rebuilding RootFS")
        Cmd(WorkingDirectory & "dmg.exe", "build " & "D" & RootFSName & " DR" & RootFSName)
        System.IO.File.Delete(WorkingDirectory & "D" & RootFSName)
        System.IO.File.Delete(WorkingDirectory & RootFSName)
        My.Computer.FileSystem.RenameFile(WorkingDirectory & "DR" & RootFSName, RootFSName)

        Changer("Encrypting Restore Ramdisk")
        Cmd(WorkingDirectory & "xpwntool.exe", "D" & RestoreRamdiskName & " T" & RestoreRamdiskName & " -t " & RestoreRamdiskName)
        System.IO.File.Delete(WorkingDirectory & RestoreRamdiskName)
        System.IO.File.Delete(WorkingDirectory & "D" & RestoreRamdiskName)
        My.Computer.FileSystem.RenameFile(WorkingDirectory & "T" & RestoreRamdiskName, RestoreRamdiskName)
        Changer("Cleaning Up")
        System.IO.File.Delete(WorkingDirectory & "zlib1.dll")
        System.IO.File.Delete(WorkingDirectory & "xpwntool.exe")
        System.IO.File.Delete(WorkingDirectory & "libpng32.dll")
        System.IO.File.Delete(WorkingDirectory & "libpng12.dll")
        System.IO.File.Delete(WorkingDirectory & "libeay32.dll")
        System.IO.File.Delete(WorkingDirectory & "hfsplus.exe")
        System.IO.File.Delete(WorkingDirectory & "bspatch.exe")
        System.IO.File.Delete(WorkingDirectory & "Cydia.tar")
        System.IO.File.Delete(WorkingDirectory & "dmg.exe")
        Changer("Creating IPSW")
        Dim this As New ZipFile
        this.AddDirectory(WorkingDirectory)
        Dim desktop As String = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        this.Save(desktop & "\iPod4,1_4.2.1_8C148_Custom.ipsw")
        Changer("Done!")
    End Sub
    Private Sub btnBegin_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnBegin.Click
        btnBegin.Enabled = False
        Call getSHA1()
        Exit Sub
    End Sub
End Class
