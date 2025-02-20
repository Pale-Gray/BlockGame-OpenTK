using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace Blockgame_OpenTK.Util;

public class Zip : IDisposable
{

    private ZipArchive _archive;
    private List<ZipArchiveEntry> _entries = new();
    private bool _disposed;

    public static Zip Open(string file)
    {

        Zip zip = new Zip();

        zip._archive = ZipFile.OpenRead(file);
        zip._entries = zip._archive.Entries.ToList();

        return zip;

    }

    public List<ZipArchiveEntry> GetFilesInDirectory(string path) => _entries.Where(entry => entry.FullName.StartsWith(path) && entry.FullName.Last() != '/').ToList();
    public ZipArchiveEntry GetFile(string filename) => _entries.Where(entry => entry.FullName == filename).First();

    public void Dispose()
    {
        
        Dispose(true);
        GC.SuppressFinalize(this);

    }

    protected virtual void Dispose(bool disposing)
    {

        if (_disposed) return;

        if (disposing)
        {

            if (_archive != null) _archive.Dispose();
            _entries.Clear();

        }

        _disposed = true;

    }

}