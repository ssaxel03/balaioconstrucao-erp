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
        // Refresh items once the UI is loaded
        (this.DataContext as NewQuoteViewModel)?.RefreshItems();

        // Optionally force the DataGrid to remeasure if needed
        // Make sure ye give the DataGrid a name in yer XAML: x:Name="MyDataGrid"
        ItemsDataGrid?.InvalidateMeasure();
        ItemsDataGrid?.InvalidateVisual();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        (this.DataContext as NewQuoteViewModel)?.AddRow();
    }
    
    private void SortItems(object? sender, DataGridCellEditEndedEventArgs e)
    {
        (this.DataContext as NewQuoteViewModel)?.SortItems();
    }
}