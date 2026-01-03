using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.ApplicationModel;

namespace UndertaleModToolAvalonia.Android;
using Com.Kongzue.Dialogx.Dialogs;
using Com.Kongzue.Dialogx.Interfaces;
using Java.Lang;
using MauiBinding.Android.DialogX.Additions;
using Object = Java.Lang.Object;

public class Bindme
{
    // ReSharper disable once InconsistentNaming
    public static async Task<bool> dAskDialog(string title, string message)
    {
        var tcs = new TaskCompletionSource<bool>();
        System.Threading.Thread.Sleep(1200);
        MainThread.BeginInvokeOnMainThread(() =>
        {
#pragma warning disable CS8602 // 解引用可能出现空引用。
            MessageDialog? msgbox = MessageDialog.Show(title,message, "是", "否")!
                .SetCancelable(false)
#pragma warning restore CS8602 // 解引用可能出现空引用。
                .SetOkButton(new ProxyOnDialogButtonClickListener(view =>
                {
                    tcs.SetResult(true);
                }))
                .SetCancelButton(new ProxyOnDialogButtonClickListener(View =>
                {
                    tcs.SetResult(false);
                }));
            new System.Threading.Thread(() =>
            {
                while(true){
                    System.Threading.Thread.Sleep(1200);
#pragma warning disable CS8602 // 解引用可能出现空引用。
                    if (!msgbox.IsShow&&!tcs.Task.IsCompleted)
#pragma warning restore CS8602 // 解引用可能出现空引用。
                        MainThread.BeginInvokeOnMainThread(() => { msgbox.Show(); });
                    else
                        break;
                }
            }).Start();
#pragma warning disable CS8602 // 解引用可能出现空引用。
            msgbox.Show();
#pragma warning restore CS8602 // 解引用可能出现空引用。
        });
        return await tcs.Task;
    }
    // ReSharper disable once InconsistentNaming
    public static async Task<string?> dInputDialog(string title, string message)
    {
        var tcs = new TaskCompletionSource<string?>();
        System.Threading.Thread.Sleep(1200);
        MainThread.BeginInvokeOnMainThread(() =>
        {
#pragma warning disable CS8602 // 解引用可能出现空引用。
            InputDialog? msgbox = InputDialog.Show(title, message, "是")
                .SetOkButton(new ProxyInputOkClickListener((view, result) =>
                {
                    tcs.SetResult(result);
                }));
            msgbox.SetCancelable(false);
#pragma warning restore CS8602 // 解引用可能出现空引用。
            new System.Threading.Thread(() =>
            {
                while(true){
                    System.Threading.Thread.Sleep(3000);
                    if (!msgbox.IsShow&&!tcs.Task.IsCompleted)
                        MainThread.BeginInvokeOnMainThread(() => { msgbox.Show(); });
                    else
                        break;
                }
            }).Start();
        });
        return await tcs.Task;
    }
    // ReSharper disable once InconsistentNaming
    public static async Task<string> dMsgDialog(string title, string message)
    {
        var tcs = new TaskCompletionSource<string>();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var msgbox = MessageDialog.Show(title, message, "好喵");
        });
        return await tcs.Task;
    }
    public class ProxyOnDialogButtonClickListener : Java.Lang.Object, IOnDialogButtonClickListener
    {
        public delegate void OnDialogButtonClick(View? view);
        public  OnDialogButtonClick Callback { get; set; }

        public ProxyOnDialogButtonClickListener(OnDialogButtonClick  callback)
        {
            this.Callback=callback;
        }
        public bool OnClick(Object? p0, View? p1)
        {
            Callback(p1);
            return false;
        }
    }
    public class ProxyInputOkClickListener : Java.Lang.Object, IOnInputDialogButtonClickListener
    {
        public delegate void OnDialogButtonClick(View? view,string? result);
        public  OnDialogButtonClick Callback { get; set; }

        public ProxyInputOkClickListener(OnDialogButtonClick  callback)
        {
            this.Callback=callback;
        }

        public bool OnClick(Object? p0, View? p1, string? p2)
        {
            Callback(p1, p2);
            return false;
        }
    }
}