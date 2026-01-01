using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace UndertaleModToolAvalonia.QiuIO;

public class CompatStorageFile:IFile
{
    IStorageFile _file;

    public CompatStorageFile(IStorageFile file)
    {
        _file = file;
    }
    public string Name => _file.Name;
    public Uri Path => _file.Path;
    public Task<StorageItemProperties> GetBasicPropertiesAsync()
    {
        return _file.GetBasicPropertiesAsync();
    }

    public bool CanBookmark => _file.CanBookmark;

    public Task<string?> SaveBookmarkAsync()
    {
        return _file.SaveBookmarkAsync();
    }

    public Task<IStorageFolder?> GetParentAsync()
    {
        return _file.GetParentAsync();
    }

    public Task DeleteAsync()
    {
        return _file.DeleteAsync();
    }

    public Task<IStorageItem?> MoveAsync(IStorageFolder destination)
    {
        return _file.MoveAsync(destination);
    }

    public string? TryGetLocalPath()
    {
        return _file.TryGetLocalPath();
    }

    public Task<Stream> OpenReadAsync()
    {
        return _file.OpenReadAsync();
    }

    public Task<Stream> OpenWriteAsync()
    {
        return _file.OpenWriteAsync();
    }
}