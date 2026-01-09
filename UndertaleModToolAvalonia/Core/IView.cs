using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using Microsoft.Maui.Storage;
using Prism.Commands;
using UndertaleModToolAvalonia.QiuIO;
using UTMTdrid;

namespace UndertaleModToolAvalonia;

public interface IView
{
    public UserControl View => (UserControl)this;

    public async Task<List<IFile>?> OpenFileDialog(FilePickerOpenOptions options)
    {
        if (OperatingSystem.IsAndroid())
        {
            QiuFuncMain.clearCallbacks();
            var options1 = PickOptions.Default;
            options1.PickerTitle = options.Title;
            var fileResult = await FilePicker.Default.PickAsync(options1);
            if (fileResult == null) return null;
            var t = fileResult.FullPath;
            return new List<IFile> { new QiuStrongerFile(new FileInfo(t)) };
        }

        TopLevel topLevel = TopLevel.GetTopLevel(View)!;
        return IFile.IStorageFileAToIFileA(await topLevel.StorageProvider.OpenFilePickerAsync(options));
    }

    public async Task<IFile?> SaveFileDialog(FilePickerSaveOptions options)
    {
        if (OperatingSystem.IsAndroid())
        {
            QiuFuncMain.clearCallbacks();
            var t = await MAUIBridge.SaveFile(
                (options.SuggestedFileName ?? "file") + (options.DefaultExtension ?? ".bin"), CancellationToken.None);
            if (t == null) return null;
            return new QiuStrongerFile(new FileInfo(t));
        }

        TopLevel topLevel = TopLevel.GetTopLevel(View)!;
        return IFile.IStorageFileToIFile(await topLevel.StorageProvider.SaveFilePickerAsync(options));
    }

    public async Task<IReadOnlyList<IStorageFolder>> OpenFolderDialog(FolderPickerOpenOptions options)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(View)!;
        return await topLevel.StorageProvider.OpenFolderPickerAsync(options);
    }

    public async Task<bool> LaunchUriAsync(Uri uri)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(View)!;
        return await topLevel.Launcher.LaunchUriAsync(uri);
    }

    public async Task<MessageWindow.Result> MessageDialog(string message, string? title = null, bool ok = true,
        bool yes = false, bool no = false, bool cancel = false)
    {
        if (!OperatingSystem.IsWindows())
        {
            return await CommonDialog(message, title, ok, yes, no, cancel);
        }

        Window window = View.FindLogicalAncestorOfType<Window>() ?? throw new InvalidOperationException();
        return await new MessageWindow(message, title, ok, yes, no, cancel).ShowDialog<MessageWindow.Result>(window);
    }

    private async Task<MessageWindow.Result> CommonDialog(string message, string? title = null, bool ok = true,
        bool yes = false, bool no = false, bool cancel = false)
    {
        TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
#pragma warning disable CS8602 // 解引用可能出现空引用。
        View.Find<StackPanel>("TextInputBox").IsVisible = true;
        View.Find<TextBlock>("TitleText").Text = title ?? "对话框";
        View.Find<TextBlock>("MessageText").Text = message;
        View.Find<TextBox>("TextTextBox").Text = "";

        View.Find<Button>("ButtonOk").IsVisible = ok;
        View.Find<Button>("ButtonCancel").IsVisible = cancel;
        View.Find<Button>("ButtonYes").IsVisible = yes;
        View.Find<Button>("ButtonNo").IsVisible = no;
        View.Find<TextBox>("TextTextBox").IsVisible = false;
        View.Find<ProgressBar>("TextBoxLoadingProgressBar").IsVisible = false;

        View.Find<Button>("ButtonOk").Click += OnClickOk;
        View.Find<Button>("ButtonCancel").Click += OnClickCancel;
        View.Find<Button>("ButtonYes").Click += OnClickYes;
        View.Find<Button>("ButtonNo").Click += OnClickNo;
#pragma warning restore CS8602 // 解引用可能出现空引用。
        void OnClickOk(object? sender, RoutedEventArgs e)
        {
            tcs.SetResult(1);
            finishBinding();
        }

        void OnClickCancel(object? sender, RoutedEventArgs e)
        {
            tcs.SetResult(2);
            finishBinding();
        }

        void OnClickYes(object? sender, RoutedEventArgs e)
        {
            tcs.SetResult(3);
            finishBinding();
        }

        void OnClickNo(object? sender, RoutedEventArgs e)
        {
            tcs.SetResult(4);
            finishBinding();
        }

        void finishBinding()
        {
            View.Find<Button>("ButtonOk").Click -= OnClickOk;
            View.Find<Button>("ButtonCancel").Click -= OnClickCancel;
            View.Find<Button>("ButtonYes").Click -= OnClickYes;
            View.Find<Button>("ButtonNo").Click -= OnClickNo;
            View.Find<StackPanel>("TextInputBox").IsVisible = false;
        }

        int a = await tcs.Task;
        switch (a)
        {
            case 1:
                return MessageWindow.Result.OK;
            case 2:
                return MessageWindow.Result.Cancel;
            case 3:
                return MessageWindow.Result.Yes;
            case 4:
                return MessageWindow.Result.No;
            default:
                return MessageWindow.Result.Cancel;
        }
    }

    private async Task<String> CommonTextDialog(string message, string? title, string? textdef)
    {
        TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
        View.Find<StackPanel>("TextInputBox").IsVisible = true;
        View.Find<TextBlock>("TitleText").Text = title ?? "文本输入框";
        View.Find<TextBlock>("MessageText").Text = message;
        View.Find<TextBox>("TextTextBox").Text = textdef ?? "";

        View.Find<Button>("ButtonOk").IsVisible = true;
        View.Find<Button>("ButtonCancel").IsVisible = true;
        View.Find<Button>("ButtonYes").IsVisible = false;
        View.Find<Button>("ButtonNo").IsVisible = false;
        View.Find<TextBox>("TextTextBox").IsVisible = true;
        View.Find<ProgressBar>("TextBoxLoadingProgressBar").IsVisible = false;

        View.Find<Button>("ButtonOk").Click += OnClickOk;
        View.Find<Button>("ButtonCancel").Click += OnClickCancel;

        void OnClickOk(object? sender, RoutedEventArgs e)
        {
            tcs.SetResult(1);
            finishBinding();
        }

        void OnClickCancel(object? sender, RoutedEventArgs e)
        {
            tcs.SetResult(2);
            finishBinding();
        }

        void finishBinding()
        {
            View.Find<Button>("ButtonOk").Click -= OnClickOk;
            View.Find<Button>("ButtonCancel").Click -= OnClickCancel;
            View.Find<StackPanel>("TextInputBox").IsVisible = false;
        }

        int a = await tcs.Task;
        switch (a)
        {
            case 1:
                return View.Find<TextBox>("TextTextBox").Text;
            case 2:
                return null;
            default:
                return null;
        }
    }

    public async Task<string?> TextBoxDialog(string message, string text = "", string? title = null,
        bool isMultiline = false, bool isReadOnly = false)
    {
        if (!OperatingSystem.IsWindows())
        {
            return await CommonTextDialog(message, title, text);
        }

        Window window = View.FindLogicalAncestorOfType<Window>() ?? throw new InvalidOperationException();
        return await new TextBoxWindow(message, text, title, isMultiline, isReadOnly).ShowDialog<string?>(window);
    }

    public ILoaderWindow LoaderOpen()
    {
        if (!OperatingSystem.IsWindows())
        {
            return new LoaderWindow.LoaderWindowDroid(View);
        }

        Window window = View.FindLogicalAncestorOfType<Window>() ?? throw new InvalidOperationException();
        LoaderWindow loaderWindow = new();
        loaderWindow.ShowDelayed(window);
        return loaderWindow;
    }

    public async Task SettingsDialog()
    {
        TabItemViewModel tab = new(new SettingsViewModel());
        MainViewModel.Me.Tabs.Add(tab);
        MainViewModel.Me.TabSelected = tab;
    }

    public void SearchInCodeOpen()
    {
        if (!OperatingSystem.IsWindows())
        {
            TabItemViewModel tab = new(new SearchInCodeViewModel());
            MainViewModel.Me.Tabs.Add(tab);
            MainViewModel.Me.TabSelected = tab;
            return;
        }

        new SearchInCodeWindow()
        {
            DataContext = new SearchInCodeWindowModel(),
        }.Show();
    }

    public IInputElement? GetFocusedElement()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(View)!;
        return topLevel.FocusManager?.GetFocusedElement();
    }
}