using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using desktop_manager.Models;
using System.IO;
using desktop_manager.Utility;
using desktop_manager.ViewModels;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace desktop_manager.ViewModels;

public partial class NewQuoteViewModel : ViewModelBase
{
    
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
        private bool _isCondominium; // Bound to CheckBox

        public int Partnership => IsCondominium ? 15 : 0; // Auto-updates

        partial void OnIsCondominiumChanged(bool value)
        {
            Console.WriteLine(_isCondominium);
            
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

        public void RefreshItems()
        {
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
            string filePath = "Invoice.pdf";

            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);
                
                document.Add(new Paragraph("Balaio - Contrução Civil").SetFontSize(14));

                Table table = new Table(5); // 5 Columns: ID, Description, Quantity, Unit Price, Total

                // Add Table Header
                table.AddHeaderCell("ID");
                table.AddHeaderCell("Description");
                table.AddHeaderCell("Quantity");
                table.AddHeaderCell("Unit Price");
                table.AddHeaderCell("Total");

                // Add Table Rows
                foreach (var item in Items)
                {
                    table.AddCell(item.Id);
                    table.AddCell(item.Description);
                    table.AddCell(item.Quantity.ToString());
                    table.AddCell(item.UnitPrice.ToString("C2"));
                    table.AddCell(item.Total.ToString("C2"));
                }

                document.Add(table);
                document.Close();
            }
            
            Console.WriteLine($"PDF generated: {filePath}");
        }
    
}