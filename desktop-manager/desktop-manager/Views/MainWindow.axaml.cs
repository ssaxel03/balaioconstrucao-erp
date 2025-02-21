using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using desktop_manager.ViewModels;

namespace desktop_manager.Views
{
    // Ensure the class is marked as 'partial' so that it can work with the XAML file
    public partial class MainWindow : Window
    {
        private ContentControl _contentControl;
        private NewQuote _newQuoteWindow;
        private CompanyDetails _companyDetailsWindow;
        
        public MainWindow()
        {
            
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();  // Set DataContext here
            this._contentControl = this.FindControl<ContentControl>("MainContentControl");
            
            string appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SimpleQuotes");

            if (!Directory.Exists(appFolderPath))
            {
                Directory.CreateDirectory(appFolderPath);
            }
            
            this._newQuoteWindow = new NewQuote();
            this._companyDetailsWindow = new CompanyDetails();
            
            string configFolderPath = Path.Combine(appFolderPath, "config");
            
            string companyDetailsFilePath = Path.Combine(configFolderPath, "company-details.json");
            
            if (File.Exists(companyDetailsFilePath))
            {
                this._contentControl.Content = _newQuoteWindow;
            }
            else
            {
                this._contentControl.Content = _companyDetailsWindow;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void CompanyDetailsButton_Click(object? sender, RoutedEventArgs e)
        {
            this._contentControl.Content = _companyDetailsWindow;
        }
        
        public void NewQuoteButton_Click(object? sender, RoutedEventArgs e)
        {
            this._contentControl.Content = _newQuoteWindow;
        }
    }
}