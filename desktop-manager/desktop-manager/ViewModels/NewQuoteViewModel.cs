using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using desktop_manager.Models;
using System.IO;
using desktop_manager.Utility;
using desktop_manager.ViewModels;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Event;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace desktop_manager.ViewModels;

public partial class NewQuoteViewModel : ViewModelBase
{
        [ObservableProperty]
        private string _clientName;
        
        [ObservableProperty]
        private string _clientAddress;

        [ObservableProperty]
        private string _subject;
    
        [ObservableProperty]
        private ObservableCollection<Item> _items = new()
        {
            new Item("1.1", "Laptop", 1m, 1200.00m),
            new Item("1.2", "Tablet", 2m, 400.00m),
            new Item("1.3", "Phone", 3m, 300.00m),
            new Item("1.4", "Headphones", 1m, 150.00m),
            new Item("1.5", "Desk", 5.60m, 80.00m),
        };
        
        [ObservableProperty]
        private bool _isCondominium = true; // Bound to CheckBox
        
        [ObservableProperty]
        private bool _vat = true; // Bound to CheckBox

        partial void OnIsCondominiumChanged(bool value)
        {
            Console.WriteLine(IsCondominium);
            
            foreach (Item item in Items)
            {
                item.SetPartnershipCoefficient(value ? 15 : 0);
            }
        }
        
        public void AddRow()
        {
            Items.Add(new Item("0.0", "Placeholder", 0.00m, 0.00m));
        }
        
        
        public void SortItems()
        {
            Console.WriteLine("sorting items");
            
            List<Item> sortedItems = Items.ToList();
            sortedItems.Sort(new HierarchicalIdComparer());
            
            Items.Clear();
            foreach (var item in sortedItems)
            {
                Items.Add(item);
            }
        }

        public void GenerateQuotePdf()
        {
            
            DocumentGenerator.GenerateQuote(ClientName, ClientAddress, Subject, Items, Vat);
            
        }
    
}