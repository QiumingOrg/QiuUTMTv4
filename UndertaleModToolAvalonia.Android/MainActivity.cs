using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Provider;
using Avalonia;
using Avalonia.Android;
using Avalonia.Maui;
using Avalonia.Media;
using Microsoft.Maui.ApplicationModel;
using UTMTdrid;

namespace UndertaleModToolAvalonia.Android
{
    [Activity(
        Label = "UndertaleModToolAvalonia.Android",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity<App>
    {
        public const string Font = "avares://UndertaleModToolAvalonia/Assets/unifont.ttf#Unifont";
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            //RequestPermissions(["android.permission.WRITE_EXTERNAL_STORAGE"],0);
            //RequestFullscreenMode(FullscreenModeRequest.Enter, null);
            //CheckExternalStoragePermission();
            Com.Kongzue.Dialogx.DialogX.Init(Application);
            MAUIBridge.AskDialog = Bindme.dAskDialog;
            MAUIBridge.InputDialog = Bindme.dInputDialog;
            //MAUIBridge.AskDialog = async (title, message) => { return false; };
            //MAUIBridge.InputDialog = async (title, message) => { return null; };
            return base.CustomizeAppBuilder(builder)
                .UseMaui<MauiApplication>(this)
                .With(new FontManagerOptions
                {
                    DefaultFamilyName = Font
                })
                .LogToTrace()
                .UseAndroid()
                .UseSkia();
        }
        public bool CheckExternalStoragePermission()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                var result = Environment.IsExternalStorageManager;
                if (!result)
                {
                    var manage = Settings.ActionManageAppAllFilesAccessPermission;
                    Intent intent = new Intent(manage);
                    Uri uri = Uri.Parse("package:" + AppInfo.Current.PackageName);
                    intent.SetData(uri);
                    StartActivity(intent);
                }
                return result;
            }

            return true;
        }
    }
}
