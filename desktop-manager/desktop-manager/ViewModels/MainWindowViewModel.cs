using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using desktop_manager.Models;

namespace desktop_manager.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        
        [ObservableProperty]
        private ObservableCollection<Item> _items = new()
        {
            new Item("1.1", "Laptop", 1.00m, 1200.00m),
            new Item("1.2", "Tablet", 2.00m, 400.00m),
            new Item("1.3", "Phone", 3.00m, 300.00m),
            new Item("1.4", "Headphones", 1.00m, 150.00m),
            new Item("1.5", "Keyboard", 1.00m, 100.00m),
        };
        
        public void AddRow()
        {
            
            Items.Add(new Item("0.0", "Placeholder", 0.00m, 0.00m));
        }
        
        public void SortItems()
        {
            Console.WriteLine("sorting items");
            List<Item> sortedItems = Items.ToList();
            sortedItems.Sort();
            Items.Clear();
            foreach (var item in sortedItems)
            {
                Items.Add(item);
            }
        }
    }
}