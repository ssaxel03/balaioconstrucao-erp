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
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            (this.DataContext as MainWindowViewModel)?.AddRow();
        }

        private void SortItems(object? sender, DataGridCellEditEndedEventArgs e)
        {
            (this.DataContext as MainWindowViewModel)?.SortItems();
        }

        private void GenerateQuotePdf()
        {
            (this.DataContext as MainWindowViewModel)?.GenerateQuotePdf();
        }
    }
}