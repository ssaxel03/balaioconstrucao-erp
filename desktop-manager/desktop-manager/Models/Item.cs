using System;
using System.Linq;
using Avalonia.Animation;

namespace desktop_manager.Models;

using CommunityToolkit.Mvvm.ComponentModel;
    public partial class Item : ObservableObject
    {
        [ObservableProperty]
        private string _id;
        
        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private decimal _quantity;

        [ObservableProperty]
        private decimal _unitPrice;

        [ObservableProperty]
        private decimal _partnershipCoefficient;
        
        public decimal Profit => Math.Round(Quantity * UnitPrice * 0.3m, 2, MidpointRounding.AwayFromZero);

        public decimal Partnership => Math.Round(((Profit + (Quantity * UnitPrice)) / 0.85m) * PartnershipCoefficient, 2, MidpointRounding.AwayFromZero); 

        public decimal Total => Math.Round(Quantity * UnitPrice + Profit + Partnership, 2, MidpointRounding.AwayFromZero);

        public Item(string id, string description, decimal quantity, decimal unitPrice)
        {
            Console.WriteLine("creating item");
            this._id = id;
            this._description = description;
            this._quantity = quantity;
            this._unitPrice = unitPrice;
            this._partnershipCoefficient = 0;
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

        partial void OnPartnershipCoefficientChanged(decimal oldValue, decimal newValue)
        {
            OnPropertyChanged(nameof(Partnership));
            OnPropertyChanged(nameof(Total));
        }

        public void SetPartnershipCoefficient(decimal value)
        {
            this.PartnershipCoefficient = value;
        }
    }