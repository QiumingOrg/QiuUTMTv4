using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace UndertaleModToolAvalonia.QiuIO;

public interface IFile
{
    public static IFile? IStorageFileToIFile(IStorageFile? file)
    {
        if(file == null) return null;
        return new CompatStorageFile(file);
    }
    
    public static List<IFile> IStorageFileAToIFileA(IEnumerable<IStorageFile>? file)
    {
        List<IFile> e=new List<IFile>();
        if(file == null) return e;
        foreach (IStorageFile file1 in file)
        {
            e.Add(new CompatStorageFile(file1));
        }

        return e;
    }
    string Name { get; }

    /// <summary>Gets the file-system path of the item.</summary>
    /// <remarks>
    /// Android backend might return file path with "content:" scheme.
    /// Browser and iOS backends might return relative uris.
    /// </remarks>
    Uri Path { get; }

    /// <summary>Gets the basic properties of the current item.</summary>
    Task<StorageItemProperties> GetBasicPropertiesAsync();

    /// <summary>
    /// Returns true is item can be bookmarked and reused later.
    /// </summary>
    bool CanBookmark { get; }

    /// <summary>Saves items to a bookmark.</summary>
    /// <returns>
    /// Returns identifier of a bookmark. Can be null if OS denied request.
    /// </returns>
    Task<string?> SaveBookmarkAsync();

    /// <summary>Gets the parent folder of the current storage item.</summary>
    Task<IStorageFolder?> GetParentAsync();

    /// <summary>Deletes the current storage item and it's contents</summary>
    /// <returns></returns>
    Task DeleteAsync();

    /// <summary>
    /// Moves the current storage item and it's contents to a <see cref="T:Avalonia.Platform.Storage.IStorageFolder" />
    /// </summary>
    /// <param name="destination">The <see cref="T:Avalonia.Platform.Storage.IStorageFolder" /> to move the item into</param>
    /// <returns></returns>
    Task<IStorageItem?> MoveAsync(IStorageFolder destination);

    string? TryGetLocalPath();
    
    Task<Stream> OpenReadAsync();

    /// <summary>Opens stream for writing to the file.</summary>
    /// <exception cref="T:System.UnauthorizedAccessException" />
    Task<Stream> OpenWriteAsync();
}