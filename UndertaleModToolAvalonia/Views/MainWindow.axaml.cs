using System.ComponentModel;
using Avalonia.Controls;

namespace UndertaleModToolAvalonia.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
        }
    }
}