using System.Text;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace UTMTdrid;

public class MAUIBridge
{
    public static async Task<FileResult?> PickFile(PickOptions options)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = await result.OpenReadAsync();
                    var image = ImageSource.FromStream(() => stream);
                }
            }

            return result;
        }
        catch (Exception ignored)
        {
            // The user canceled or something went wrong
        }

        return null;
    }
    public static async Task<string?> SaveFile(string? recommandName,CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<string?>();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var stream = new MemoryStream();
#pragma warning disable CA1416
            var result = await FileSaver.Default.SaveAsync(recommandName??"c.bin", stream, cancellationToken);
#pragma warning restore CA1416
            if (result.IsSuccessful){
                new FileInfo(result.FilePath).Delete();
                tcs.SetResult(result.FilePath);
            }
            else
            {
                tcs.SetResult(null);
            }
        });
        return await tcs.Task;
    }
    public static async Task<string?> PickFolder(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<string?>();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
#pragma warning disable CA1416
            var result = await FolderPicker.Default.PickAsync(cancellationToken);
#pragma warning restore CA1416
            if (result.IsSuccessful){
                tcs.SetResult(result.Folder.Path);
            }
            else
            {
                tcs.SetResult(null);
            }
        });
        return await tcs.Task;
    }
    public delegate Task<bool>  AskDialogImpt(string title, string message);
    public static AskDialogImpt? AskDialog { set; get; }
    
    public delegate Task<string?>  InputDialogImpt(string title, string message);
    public static InputDialogImpt? InputDialog { set; get; }
    
    public delegate Task<bool>  HasRequiredStoragePermissionImpt();
    public static HasRequiredStoragePermissionImpt? HasRequiredStoragePermission { set; get; }
}