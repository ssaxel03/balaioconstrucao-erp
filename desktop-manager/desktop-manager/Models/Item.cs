using System;
using System.Linq;
using Avalonia.Animation;

namespace desktop_manager.Models;

using CommunityToolkit.Mvvm.ComponentModel;
    public partial class Item : ObservableObject, IComparable<Item>
    {
        [ObservableProperty]
        private decimal _id;
        
        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private decimal _quantity;

        [ObservableProperty]
        private decimal _unitPrice;
        
        public decimal Profit => Quantity * UnitPrice * 0.3m;

        public decimal Partnership => ((Profit + (Quantity * UnitPrice)) / 0.85m) * 0.15m; 

        public decimal Total =>  Quantity * UnitPrice + Profit + Partnership;

        public Item(decimal id, string description, decimal quantity, decimal unitPrice)
        {
            Console.WriteLine("creating item");
            this._id = id;
            this._description = description;
            this._quantity = quantity;
            this._unitPrice = unitPrice;
        }

        partial void OnQuantityChanged(decimal oldValue, decimal newValue)
        {
            OnPropertyChanged(nameof(Profit)); // Notify UI when Quantity changes
            OnPropertyChanged(nameof(Partnership)); // Notify UI when Quantity changes
            OnPropertyChanged(nameof(Total)); // Notify UI when Quantity changes


        }

        partial void OnUnitPriceChanged(decimal oldValue, decimal newValue)
        {
            OnPropertyChanged(nameof(Profit)); // Notify UI when Quantity changes
            OnPropertyChanged(nameof(Partnership)); // Notify UI when Quantity changes
            OnPropertyChanged(nameof(Total)); // Notify UI when Quantity changes
        }

        public int CompareTo(Item? other)
        {
            return this.Id.CompareTo(other.Id);
        }
    }