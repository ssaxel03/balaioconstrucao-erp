using System;
using System.Linq;
using Avalonia.Animation;

namespace desktop_manager.Models;

using CommunityToolkit.Mvvm.ComponentModel;
    public partial class Item : ObservableObject, IComparable
    {
        [ObservableProperty]
        private string _id;
        
        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private decimal _quantity;

        [ObservableProperty]
        private decimal _unitPrice;
        
        public decimal Profit => Quantity * UnitPrice * 0.3m;

        public decimal Partnership => ((Profit + (Quantity * UnitPrice)) / 0.85m) * 0.15m; 

        public decimal Total =>  Quantity * UnitPrice + Profit + Partnership;

        public Item(string id, string description, decimal quantity, decimal unitPrice)
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

        bool IdIsValid(string id)
        {
            if (id.Length < 1)
            {
                return false;
            }
            
            foreach (char ch in id)
            {
                if (!(char.IsDigit(ch) || ch.ToString() == "."))
                {
                    return false;
                }
            }

            return true;
        }

        public int CompareTo(object obj)
        {
            if (obj is not Item)
            {
                return this.GetHashCode() - obj.GetHashCode();
            }

            Item that = obj as Item;

            if (!IdIsValid(this.Id))
            {
                this.Id = "0";
            }
            
            if (!IdIsValid(that.Id))
            {
                that.Id = "0";
            }
            
            int thisId = int.Parse(string.Join("", this.Id.Split(".")));
            int thatId = int.Parse(string.Join("", that.Id.Split(".")));
            
            return thisId.CompareTo(thatId);

        }
    }