using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using desktop_manager.ViewModels;

namespace desktop_manager.Views;

public partial class NewQuote : UserControl
{
    public NewQuote()
    {
        var start = DateTime.Now;
        InitializeComponent();
        
        this.DataContext = new NewQuoteViewModel();  // Set DataContext here
        this.Loaded += NewQuote_Loaded;
        
        var end = DateTime.Now;
        Console.WriteLine($"NewQuote_Loaded took {(end - start).TotalMilliseconds} ms");
    }
    
    private void NewQuote_Loaded(object? sender, RoutedEventArgs e)
    {
        ItemsDataGrid?.InvalidateMeasure();
        ItemsDataGrid?.InvalidateVisual();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void SortItems(object? sender, DataGridCellEditEndedEventArgs e)
    {
        (this.DataContext as NewQuoteViewModel)?.SortItems();
    }
}