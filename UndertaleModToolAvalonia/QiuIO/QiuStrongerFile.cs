using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using UTMTdrid;

namespace UndertaleModToolAvalonia.QiuIO;

public class QiuStrongerFile:IFile
{
    public FileInfo? file;

    public QiuStrongerFile(FileInfo file)
    {
        this.file = file;
    }
    
    public QiuStrongerFile()
    {
    }

    public Task<StorageItemProperties> GetBasicPropertiesAsync()
    {
        if(file==null) return Task.FromResult<StorageItemProperties>(null);
        return Task.FromResult(new StorageItemProperties(null,
            File.GetCreationTime(file.FullName),
            File.GetLastWriteTime(file.FullName)));
    }

    public Task<string?> SaveBookmarkAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IStorageFolder?> GetParentAsync()
    {
        // var f = new QiuStrongerDirectory();
        // // f.isFile = false;
        // // f.directory = file.Directory;
        // return Task.FromResult<IStorageFolder?>(f);
        throw new NotImplementedException();
    }

    public Task DeleteAsync()
    {
        File.Delete(file.FullName);
        return Task.CompletedTask;
    }

    public Task<IStorageItem?> MoveAsync(IStorageFolder destination)
    {
        throw new NotImplementedException();
    }

    public string? TryGetLocalPath()
    {
        return file?.FullName;
    }

    public Task<Stream> OpenReadAsync()
    {
        return Task.FromResult<Stream>(File.OpenRead(file.FullName));
    }

    public Task<Stream> OpenWriteAsync()
    {
        return Task.FromResult<Stream>(File.OpenWrite(file.FullName));
    }

    // public void
    //     \u0028This\u0020interface\u0020or\u0020abstract\u0020class\u0020is\u0020\u002Dnot\u002D\u0020implementable\u0020by\u0020user\u0020code\u0020\u0021\u0029()
    // {
    // }

    public string Name
    {
        get
        {
            return file.Name;
        }
    }

    public Uri Path
    {
        get
        {
            throw new NotImplementedException();
            //return new Uri(file.FullName);
        }
    }

    public bool CanBookmark { get; }

    public void Dispose()
    {
        //throw new NotImplementedException();
    }
}