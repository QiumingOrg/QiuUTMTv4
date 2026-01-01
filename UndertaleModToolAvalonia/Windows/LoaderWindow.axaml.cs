using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace UndertaleModToolAvalonia;

public interface ILoaderWindow
{
    public void EnsureShown();
    void SetMessage(string message);
    void SetStatus(string status);
    void SetValue(int value);
    void SetMaximum(int maximum);
    void SetText(string text);
    void SetTextToMessageAndStatus(string status);
    void Close();
}

public partial class LoaderWindow : Window, ILoaderWindow
{
    public string TitleText { get; set; } = "UndertaleModToolAvalonia";

    int value;
    string? message;
    string? status;
    int maximum = -1;
    bool hasClosed = false;
    Window? showOwner;

    public LoaderWindow()
    {
        Initialize();
    }

    public void Initialize()
    {
        InitializeComponent();

        Closing += (object? sender, WindowClosingEventArgs e) =>
        {
            if (!e.IsProgrammatic)
                e.Cancel = true;
            else
                hasClosed = true;
        };
    }

    public void ShowDelayed(Window owner)
    {
        showOwner = owner;
        Task.Delay(100).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (!hasClosed)
                    Show(owner);
            });
        });
    }

    public void EnsureShown()
    {
        if (showOwner is not null)
            Show(showOwner);
    }

    public void UpdateText()
    {
        MessageTextBlock.Text = $"{(!String.IsNullOrEmpty(message) ? message + " - " : "")}{value}/{maximum}{(!String.IsNullOrEmpty(status) ? ": " + status : "")}";
    }

    public void SetMessage(string message)
    {
        this.message = message;
        UpdateText();
    }

    public void SetStatus(string status)
    {
        this.status = status;
        UpdateText();
    }

    public void SetValue(int value)
    {
        this.value = value;
        LoadingProgressBar.Value = value;
        UpdateText();
    }

    public void SetMaximum(int maximum)
    {
        this.maximum = maximum;
        LoadingProgressBar.IsIndeterminate = false;
        LoadingProgressBar.Maximum = maximum;
        UpdateText();
    }

    public void SetText(string text)
    {
        MessageTextBlock.Text = text;
    }

    public void SetTextToMessageAndStatus(string status)
    {
        MessageTextBlock.Text = $"{(!String.IsNullOrEmpty(message) ? message + " " : "")} - {status}";
    }
    
    public partial class LoaderWindowDroid : ILoaderWindow
    {
        private UserControl View;
        public LoaderWindowDroid(UserControl view)
        {
            this.View=view;
            View.Find<StackPanel>("TextInputBox").IsVisible = true;
            View.Find<Button>("ButtonOk").IsVisible = false;
            View.Find<Button>("ButtonCancel").IsVisible = false;
            View.Find<Button>("ButtonYes").IsVisible = false;
            View.Find<Button>("ButtonNo").IsVisible = false;
            View.Find<TextBox>("TextTextBox").IsVisible = false;
            View.Find<ProgressBar>("TextBoxLoadingProgressBar").IsVisible = true;
            View.Find<ProgressBar>("TextBoxLoadingProgressBar").IsIndeterminate = true;
            View.Find<TextBlock>("TitleText").Text = "加载中...";
            View.Find<TextBlock>("MessageText").Text = "";
            
            View.Find<Button>("ButtonOk").Click += OnClickOk;
        }

        public void ShowOkButton(bool show=true)
        {
            View.Find<Button>("ButtonOk").IsVisible = show;
        }
        
        protected void OnClickOk(object? sender, RoutedEventArgs e)
        {
            Close();
        }
        public void EnsureShown()
        {
            View.Find<StackPanel>("TextInputBox").IsVisible = true;
        }

        public void SetMessage(string message)
        {
            View.Find<TextBlock>("TitleText").Text = message;
        }

        public void SetStatus(string status)
        {
            View.Find<TextBlock>("MessageText").Text = status;
        }

        public void SetValue(int value)
        {
            View.Find<ProgressBar>("TextBoxLoadingProgressBar").IsIndeterminate = false;
            View.Find<ProgressBar>("TextBoxLoadingProgressBar").Value = value;
        }

        public void SetMaximum(int maximum)
        {
            View.Find<ProgressBar>("TextBoxLoadingProgressBar").IsIndeterminate = false;
            View.Find<ProgressBar>("TextBoxLoadingProgressBar").Maximum = maximum;
        }

        public void SetText(string text)
        {
            View.Find<TextBlock>("MessageText").Text = text;
        }
        
        public string GetText()
        {
            return View.Find<TextBlock>("MessageText").Text;
        }

        public void SetTextToMessageAndStatus(string status)
        {
            View.Find<TextBlock>("MessageText").Text = status;
        }

        public void Close()
        {
            View.Find<StackPanel>("TextInputBox").IsVisible = false;
            View.Find<TextBlock>("MessageText").Text = "";
            View.Find<Button>("ButtonOk").Click -= OnClickOk;
        }
    }
}