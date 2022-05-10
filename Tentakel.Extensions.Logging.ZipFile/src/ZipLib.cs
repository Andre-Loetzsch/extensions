using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Tentakel.Extensions.Logging.ZipFile;

public class ZipLib
{

    private int mCompressionLevel = 6;

    public int CompressionLevel
    {
        get { return mCompressionLevel; }
        set { mCompressionLevel = value; }
    }

    /// <summary>
    /// Diese Funktion komprimiert alle Dateien in einem Ordner
    /// </summary>
    /// <param name="InputDir">Der Ordner der komprimiert werden soll</param>
    /// <param name="FileName">Gibt den Namen an nach dem die ZIP Datei benannt werden soll</param>
    /// <param name="OutputDir">Gibt das Ziel für die ZIP Datei an. Wenn kein Ziel übergeben wurde wird die Datei im Parent Ordner erstellt</param>
    public void CompressDirectory(string InputDir, string FileName, string OutputDir)
    {
        List<string> Files = new List<string>();
        string RelativePath = null;
        GetAllFiles(InputDir, ref Files);

        if (string.IsNullOrEmpty(OutputDir)) OutputDir = Path.GetDirectoryName(InputDir);
        if (Directory.Exists(OutputDir) == false) Directory.CreateDirectory(OutputDir);

        FileStream ZFS = new FileStream(OutputDir + "\\" + FileName, FileMode.Create);
        ICSharpCode.SharpZipLib.Zip.ZipOutputStream ZOut = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(ZFS);

        ZOut.SetLevel(6);

        ICSharpCode.SharpZipLib.Zip.ZipEntry ZipEntry = default(ICSharpCode.SharpZipLib.Zip.ZipEntry);

        byte[] Buffer = new byte[4097];
        int ByteLen = 0;
        FileStream FS = null;

        int ParentDirLen = InputDir.Length + 1;
        for (int i = 0; i <= Files.Count - 1; i++)
        {
            //Relativen Pfad für die Zip Datei erstellen
            RelativePath = Files[i].Substring(ParentDirLen);

            //ZipEntry erstellen
            ZipEntry = new ICSharpCode.SharpZipLib.Zip.ZipEntry(RelativePath);
            ZipEntry.DateTime = System.DateTime.Now;

            //Eintrag hinzufügen
            ZOut.PutNextEntry(ZipEntry);

            //Datei in den Stream schreiben
            FS = new FileStream(Files[i], FileMode.Open, FileAccess.Read, FileShare.Read);
            do
            {
                ByteLen = FS.Read(Buffer, 0, Buffer.Length);
                ZOut.Write(Buffer, 0, ByteLen);
            }
            while (!(ByteLen <= 0));
            FS.Close();
        }

        ZOut.Finish();
        ZOut.Close();
        ZFS.Close();
    }


    /// <summary>
    /// Diese Funktion komprimiert alle angegebenen Dateien die aus einem Ordner stammen und nicht aus unterschiedlichen Ordnern.
    /// Das hat den zweck falls man einen Ordner komprimieren will jedoch nicht mit allen Dateien sondern nur mit bestimmten Dateien.
    /// Dadurch bleibt die Ordnerstruktur erhalten wenn das Archiv erstellt wurde. Im Gegenstatz zur Funktion "CompressFiles" die keine Ordnerstrukuren erstellt
    /// </summary>
    /// <param name="InputFiles">Die Dateien die komprimiert werden sollen</param>
    /// <param name="FileName">Gibt den Namen an nach dem die ZIP Datei benannt werden soll</param>
    /// <param name="OutputDir">Gibt das Ziel für die ZIP Datei an. Wenn kein Ziel übergeben wurde wird die Datei im Parent Ordner erstellt</param>
    public void CompressDirectory(List<string> InputFiles, string FileName, string OutputDir)
    {
        string RelativePath = null;

        if (Directory.Exists(OutputDir) == false) Directory.CreateDirectory(OutputDir);

        FileStream ZFS = new FileStream(OutputDir + "\\" + FileName, FileMode.Create);
        ICSharpCode.SharpZipLib.Zip.ZipOutputStream ZOut = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(ZFS);

        ZOut.SetLevel(6);

        ICSharpCode.SharpZipLib.Zip.ZipEntry ZipEntry = default(ICSharpCode.SharpZipLib.Zip.ZipEntry);

        byte[] Buffer = new byte[4097];
        int ByteLen = 0;
        FileStream FS = null;

        int ParentDirLen = Path.GetDirectoryName(InputFiles[0]).Length;
        for (int i = 0; i <= InputFiles.Count - 1; i++)
        {
            //Relativen Pfad für die Zip Datei erstellen
            RelativePath = InputFiles[i].Substring(ParentDirLen);

            //ZipEntry erstellen
            ZipEntry = new ICSharpCode.SharpZipLib.Zip.ZipEntry(RelativePath);
            ZipEntry.DateTime = System.DateTime.Now;

            //Eintrag hinzufügen
            ZOut.PutNextEntry(ZipEntry);

            //Datei in den Stream schreiben
            FS = new FileStream(InputFiles[i], FileMode.Open, FileAccess.Read, FileShare.Read);
            do
            {
                ByteLen = FS.Read(Buffer, 0, Buffer.Length);
                ZOut.Write(Buffer, 0, ByteLen);
            }
            while (!(ByteLen <= 0));
            FS.Close();
        }

        ZOut.Finish();
        ZOut.Close();
        ZFS.Close();
    }

    /// <summary>
    /// Diese Funktion komprimiert Dateien zu einem ZIP-Archiv.
    /// </summary>
    /// <param name="InputFiles">Die Liste mit Dateien die komprimiert werden soll.</param>
    /// <param name="FileName">Der Dateiname der ZIP-Datei (ohne Pfad).</param>
    /// <param name="OutputDir">Das Ausgabeverzeichnis wo die ZIP Datei gespeichert werden soll.</param>
    /// <remarks></remarks>
    public void CompressFiles(List<string> InputFiles, string FileName, string OutputDir)
    {
        FileStream ZFS = new FileStream(OutputDir + "\\" + FileName, FileMode.Create);
        ICSharpCode.SharpZipLib.Zip.ZipOutputStream ZOut = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(ZFS);

        ZOut.SetLevel(6);

        ICSharpCode.SharpZipLib.Zip.ZipEntry ZipEntry = default(ICSharpCode.SharpZipLib.Zip.ZipEntry);

        byte[] Buffer = new byte[4097];
        int ByteLen = 0;
        FileStream FS = null;


        for (int i = 0; i <= InputFiles.Count - 1; i++)
        {
            //ZipEntry erstellen
            ZipEntry = new ICSharpCode.SharpZipLib.Zip.ZipEntry(Path.GetFileName(InputFiles[i]));
            ZipEntry.DateTime = System.DateTime.Now;

            //Eintrag hinzufügen
            ZOut.PutNextEntry(ZipEntry);

            //Datei in den Stream schreiben
            FS = new FileStream(InputFiles[i], FileMode.Open, FileAccess.Read, FileShare.Read);
            do
            {
                ByteLen = FS.Read(Buffer, 0, Buffer.Length);
                ZOut.Write(Buffer, 0, ByteLen);
            }
            while (!(ByteLen <= 0));
            FS.Close();
        }

        ZOut.Finish();
        ZOut.Close();
        ZFS.Close();
    }

    /// <summary>
    /// Diese Funktion dekomprimiert eine ZIP-Datei.
    /// </summary>
    /// <param name="FileName">Die Datei die dekomprimiert werden soll.</param>
    /// <param name="OutputDir">Das Verzeichnis in dem die Dateien dekomprimiert werden sollen.</param>
    public void DecompressFile(string FileName, string OutputDir)
    {
        FileStream ZFS = new FileStream(FileName, FileMode.Open);
        ICSharpCode.SharpZipLib.Zip.ZipInputStream ZIN = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(ZFS);

        ICSharpCode.SharpZipLib.Zip.ZipEntry ZipEntry = default(ICSharpCode.SharpZipLib.Zip.ZipEntry);

        byte[] Buffer = new byte[4097];
        int ByteLen = 0;
        FileStream FS = null;

        string InZipDirName = null;
        string InZipFileName = null;
        string TargetFileName = null;

        do
        {
            ZipEntry = ZIN.GetNextEntry();
            if (ZipEntry == null) break;


            InZipDirName = Path.GetDirectoryName(ZipEntry.Name) + "\\";
            InZipFileName = Path.GetFileName(ZipEntry.Name);

            if (Directory.Exists(OutputDir + "\\" + InZipDirName) == false) Directory.CreateDirectory(OutputDir + "\\" + InZipDirName);

            if (InZipDirName == "\\") InZipDirName = "";
            TargetFileName = OutputDir + "\\" + InZipDirName + InZipFileName;

            FS = new FileStream(TargetFileName, FileMode.Create);
            do
            {
                ByteLen = ZIN.Read(Buffer, 0, Buffer.Length);
                FS.Write(Buffer, 0, ByteLen);
            }
            while (!(ByteLen <= 0));
            FS.Close();
        }
        while (true);

        ZIN.Close();
        ZFS.Close();
    }

    private void GetAllFiles(string Root, ref List<string> FileArray)
    {
        try
        {
            string[] Files = System.IO.Directory.GetFiles(Root);
            string[] Folders = System.IO.Directory.GetDirectories(Root);

            for (int i = 0; i <= Files.Length - 1; i++)
            {
                FileArray.Add(Files[i].ToString());
            }

            for (int i = 0; i <= Folders.Length - 1; i++)
            {
                GetAllFiles(Folders[i], ref FileArray);
            }
        }
        catch (Exception Ex)
        {
        }
    }
}