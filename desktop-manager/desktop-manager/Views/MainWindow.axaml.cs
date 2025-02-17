using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using desktop_manager.ViewModels;

namespace desktop_manager.Views
{
    // Ensure the class is marked as 'partial' so that it can work with the XAML file
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();  // Set DataContext here
            this.FindControl<ContentControl>("MainContentControl").Content = new NewQuote();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}