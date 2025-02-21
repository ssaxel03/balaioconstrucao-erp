using System;
using System.Diagnostics;
using System.IO;
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
            
            string appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Balaio-Construção Civil");

            if (!Directory.Exists(appFolderPath))
            {
                Directory.CreateDirectory(appFolderPath);
            }
            
            string configFolderPath = Path.Combine(appFolderPath, "config");
            
            string companyDetailsFilePath = Path.Combine(configFolderPath, "company-details.json");
            
            if (File.Exists(companyDetailsFilePath))
            {
                this.FindControl<ContentControl>("MainContentControl").Content = new NewQuote();
            }
            else
            {
                this.FindControl<ContentControl>("MainContentControl").Content = new CompanyDetails();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}