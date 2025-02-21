using System;
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
        this.DataContext = new NewQuoteViewModel();  // Set DataContext here
        
        InitializeComponent();

        this.Loaded += NewQuote_Loaded;
    }
    
    private void NewQuote_Loaded(object? sender, RoutedEventArgs e)
    {
        (this.DataContext as NewQuoteViewModel)?.RefreshItems();
        
        ItemsDataGrid?.InvalidateMeasure();
        ItemsDataGrid?.InvalidateVisual();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void NewItem(object? sender, RoutedEventArgs e)
    {
        (this.DataContext as NewQuoteViewModel)?.AddRow();
    }
    
    private void SortItems(object? sender, DataGridCellEditEndedEventArgs e)
    {
        (this.DataContext as NewQuoteViewModel)?.SortItems();
    }
}