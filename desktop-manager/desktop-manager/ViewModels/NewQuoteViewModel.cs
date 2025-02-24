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

namespace desktop_manager.ViewModels;

public partial class NewQuoteViewModel : ViewModelBase
{
    
        private static PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        private static PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
    
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
            
            string quotesFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SimpleQuotes", "Quotes");
            
            Directory.CreateDirectory(quotesFolderPath);

            string fileName = "Quote.pdf";
            string filePath = Path.Combine(quotesFolderPath, fileName);
            
            // TEST VALUES START
            
            string quoteId = "36/2025";
            string date = DateTime.Now.ToString("dd/MM/yyyy");

            // Company details
            string companyName = "Balaio - Construção Civil, Unipessoal, Lda";
            string alvara = "92195";
            string nipc = "514818506";
            string technicalDirector = "Daniel Soares";
            string companyEmail = "geral@balaioconstrucao.pt";
            string website = "www.balaioconstrucao.pt";
            string companyCellphone = "+351 926 332 656 (chamada para a rede móvel nacional)";

            // Client details
            string clientName = "Condomínio da Vinhaça";
            string clientAddress = "Rua dos Tintos 496, 4123-321 Porto";

            // Quote additional details
            string subject = "Reabilitação de adega";
            string globalAmount = "67 956,40€ + VAT";
            
            // TEST VALUES END
            
            int fileCount = 1;
            while (File.Exists(filePath))
            {
                string newFileName = $"Quote ({fileCount}).pdf";
                filePath = Path.Combine(quotesFolderPath, newFileName);
                fileCount++;
            }
            
            
            
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                PdfWriter writer = new PdfWriter(stream);
            PdfDocument pdf = new PdfDocument(writer);
            Document doc = new Document(pdf);

            // Add Logos (Company & Association)

            // Set top margin to leave space for the header logos
            doc.SetTopMargin(80);

            // Add first page content as inline paragraphs (label in bold, value in regular)
            AddLabelValue(doc, "Quote ID:", quoteId);
            AddLabelValue(doc, "Date:", date);

            // Add header for Company details
            doc.Add(new Paragraph("Company ID")
                .SetFont(fontBold)
                .SetFontSize(12)
                .SetMarginTop(20));

            AddLabelValue(doc, "Company Name:", companyName);
            AddLabelValue(doc, "Alvará nº:", alvara);
            AddLabelValue(doc, "NIPC:", nipc);
            AddLabelValue(doc, "Direção Técnica:", technicalDirector);
            AddLabelValue(doc, "Email:", companyEmail);
            AddLabelValue(doc, "Website:", website);
            AddLabelValue(doc, "Cellphone:", companyCellphone);

            // Add header for Client details
            doc.Add(new Paragraph("Client ID")
                .SetFont(fontBold)
                .SetFontSize(12)
                .SetMarginTop(20));

            AddLabelValue(doc, "Client Name:", clientName);
            AddLabelValue(doc, "Client Address:", clientAddress);

            // Add header for Quote details
            doc.Add(new Paragraph("Quote details")
                .SetFont(fontBold)
                .SetFontSize(12)
                .SetMarginTop(20));

            AddLabelValue(doc, "Subject:", subject);
            AddLabelValue(doc, "Global Amount:", globalAmount);

            // Close the document – yer voyage is complete!
            doc.Close();
                
                /*
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
                */
                
                
                
                
            }
            
            Console.WriteLine($"PDF generated: {filePath}");
            
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        
        private static void AddLabelValue(Document doc, string label, string value)
        {
            Paragraph p = new Paragraph();
            p.Add(new Text(label).SetFont(fontBold));
            p.Add(new Text(" " + value).SetFont(font));
            doc.Add(p);
        }
    
}