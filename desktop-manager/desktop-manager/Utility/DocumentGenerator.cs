using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using desktop_manager.Models;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace desktop_manager.Utility;

public static class DocumentGenerator
{
    
    private const string CompanyName = "Balaio - Construção Civil, Unipessoal, Lda";
    private const string Alvara = "92195";
    private const string Nipc = "514818506";
    private const string TechnicalDirector = "Daniel Soares";
    private const string CompanyEmail = "geral@balaioconstrucao.pt";
    private const string Website = "www.balaioconstrucao.pt";
    private const string CompanyCellphone = "+351 926 332 656 (chamada para a rede móvel nacional)";
    
    public static void GenerateQuote(string clientName, string clientAddress, string subject, ObservableCollection<Item> items, bool hasGlobalValue, bool hasDetailedValues, bool hasAutos, bool vat)
    {
        // Create payment conditions based on hasAutos flag
        string paymentConditionAutosFalse = "Na adjudicação será faturado 50% do valor global da obra. O restante valor será faturado na conclusão da obra. Estas condições poderão ser alteradas de forma a uma melhor adequação às necessidades de ambas as partes (a combinar).";
        string paymentConditionAutosTrue = "Na adjudicação será faturado 50% do valor global da obra. O restante valor deverá ser pago de acordo com autos de medição realizados no dia 25 de cada mês. Estas condições poderão ser alteradas de forma a uma melhor adequação às necessidades de ambas as partes (a combinar).";
        
        List<ParagraphTitle> paragraphTitles = new List<ParagraphTitle>()
        {
            new ParagraphTitle(1, "Prazo de execução da obra", ["A combinar."]),
            new ParagraphTitle(2, "Condições de pagamento da obra", [
                hasAutos ? paymentConditionAutosTrue : paymentConditionAutosFalse, 
                "O incumprimento do referido prazo de pagamento a que o Cliente fica obrigado implicará, por um lado a suspensão dos trabalhos até a regularização do pagamento em falta e, por outro, dará também origem ao pagamento de juros de mora sobre o montante em divida calculados à taxa legal em vigor.", 
                "Sobre o preço global da empreitada incide o IVA à taxa legal em vigor."]),
            new ParagraphTitle(3, "Garantias da obra", ["Os trabalhos executados estão garantidos em conformidade com o disposto na lei em vigor."]),
            new ParagraphTitle(4, "Propriedade da obra", ["A propriedade dos materiais a aplicar será do empreiteiro, até o pagamento integral da empreitada. Cabe ao Dono da Obra a responsabilidade sobre os materiais durante o tempo que permanecerem em obra, garantindo uma eventual indemnização ao empreiteiro caso, por qualquer causa, os materiais sofram danos e/ou perdas totais ou parciais."]),
            new ParagraphTitle(5, "Responsabilidades do Dono da Obra", ["Acesso a toda a área a intervir.",
                "Cedência de um local seguro para o acondicionamento de materiais, ferramentas e equipamentos.",
                "Licenças e autorizações necessárias para a realização da empreitada.",
                "Licenças e taxas municipais, taxas de ocupação de via pública e o respetivo pagamento.",
                "Fornecimento de água e energia elétrica."]),
            new ParagraphTitle(6, "Responsabilidades do Empreiteiro da Obra", ["Todos os trabalhos descritos serão executados segundo as normas de segurança e saúde aplicáveis e segundo as boas regras e técnicas de construção.",
                "Fornecimento e execução de mão-de-obra, material e equipamentos para a realização da obra.",
                "Remoção do entulho proveniente da obra, e limpeza de toda a área de trabalho,",
                "Apoio técnico especializado.",
                "Responsabilidade civil e seguros."]),
            new ParagraphTitle(7, "Exclusões de obra", ["A proposta não inclui qualquer tipo de trabalhos a executar que não estejam especificados na listagem abaixo citada."]),
            new ParagraphTitle(8, "Outras condições", ["Apenas nos responsabilizamos por anomalias que possam surgir após o término da empreitada, e que tenha uma relação direta sobre os trabalhos executados, e simultaneamente que a empreitada esteja totalmente liquidada.",
                "Todos os trabalhos requisitados que não estejam neste orçamento, serão orçamentados como trabalhos a mais.",
                "Não é da responsabilidade do empreiteiro a garantia da resolução dos problemas apresentados pelo Dono da obra, quando apenas se realizem intervenções pontuais.",
                "O valor global da empreitada é apenas referente à adjudicação na sua totalidade, ficando o empreiteiro com o direito de reformular o orçamento caso o Dono da Obra pretenda uma adjudicação parcial da obra orçamentada.",
                "O Dono da Obra deverá partilhar com o Empreiteiro da Obra todos os danos existentes antes do início da empreitada, com registos fotográficos."]),
            new ParagraphTitle(9, "Validade da proposta", ["A proposta é válida por 15 dias."])
        };
        
        
        // TEST VALUES
        string quoteId = "36/2025";
        string date = DateTime.Now.ToString("dd/MM/yyyy");
        
        // MAKING SURE THE DIRECTORY EXISTS AND GETTING THE CORRESPONDING STRING
        string quotesFolderPath = EnsureDirectory("Quotes");
        
        // DOCUMENT VALUES
        string fileName = clientName + " - " + subject + ".pdf";
        string filePath = Path.Combine(quotesFolderPath, fileName);
        
        // MAKING SURE FILE IS NOT OVERRIDING ANOTHER FILE
        int fileCount = 1;
        while (File.Exists(filePath))
        {
            string newFileName = $"Quote ({fileCount}).pdf";
            filePath = Path.Combine(quotesFolderPath, newFileName);
            fileCount++;
        }
        
        // GENERATING THE FILE
        using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
                
            using (PdfWriter writer = new PdfWriter(stream))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    Document doc = new Document(pdf);

                    PdfFont fontBold = GetFont("bold");
                        
                    // Set top margin to leave space for the header logos
                    doc.SetTopMargin(120);

                    // Add first page content as inline paragraphs (label in bold, value in regular)
                    AddLabelValue(doc, "Quote ID:", quoteId);
                    AddLabelValue(doc, "Date:", date);

                    // Add header for Company details
                    doc.Add(new Paragraph("Company ID")
                        .SetFont(fontBold)
                        .SetFontSize(12)
                        .SetMarginTop(20));

                    AddLabelValue(doc, "Company Name:", CompanyName);
                    AddLabelValue(doc, "Alvará nº:", Alvara);
                    AddLabelValue(doc, "NIPC:", Nipc);
                    AddLabelValue(doc, "Direção Técnica:", TechnicalDirector);
                    AddLabelValue(doc, "Email:", CompanyEmail);
                    AddLabelValue(doc, "Website:", Website);
                    AddLabelValue(doc, "Cellphone:", CompanyCellphone);

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

                    // Set global amount based on hasGlobalValue flag
                    string globalAmount = hasGlobalValue 
                        ? items.Sum(item => item.Total).ToString("C2", new CultureInfo("pt-PT")) + (vat ? " + IVA" : "")
                        : "N/A";
                        
                    AddLabelValue(doc, "Global Amount:", globalAmount);
                        
                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                    foreach (ParagraphTitle paragraphTitle in paragraphTitles)
                    {
                        AddParagraphTitle(doc, paragraphTitle);
                    }
            
                    // Flush & Close
                    doc.Close();
                }
            }
        }
            
        Console.WriteLine($"PDF generated: {filePath}");
            
        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        
    }

    private static string EnsureDirectory(string subDirectory)
    {
        string quotesFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SimpleQuotes", subDirectory);
            
        Directory.CreateDirectory(quotesFolderPath);

        return quotesFolderPath;
    }
    
    private static void AddLabelValue(Document doc, string label, string value)
    {
        PdfFont font = GetFont("");
        PdfFont fontBold = GetFont("bold");
            
        Paragraph p = new Paragraph();
        p.Add(new Text(label).SetFont(fontBold));
        p.Add(new Text(" " + value).SetFont(font));
        doc.Add(p);
    }

    private static void AddParagraphTitle(Document doc, ParagraphTitle paragraphTitle)
    {
        PdfFont font = GetFont("");
        PdfFont fontBold = GetFont("bold");

        Paragraph t = new Paragraph();
        t.Add(new Text(paragraphTitle.Index + ". " + paragraphTitle.Title).SetFont(fontBold));
        
        Paragraph p = new Paragraph().SetMarginLeft(12);

        for (int i = 0; i < paragraphTitle.Paragraphs.Count; i++)
        {
            p.Add(new Text(paragraphTitle.Index + "." + (i + 1) + ". ").SetFont(fontBold));
            p.Add(new Text(paragraphTitle.Paragraphs[i] + "\n").SetFont(font));
        }

        p.SetTextAlignment(TextAlignment.JUSTIFIED);

        doc.Add(t);
        doc.Add(p);

    }

    private static PdfFont GetFont(string type)
    {
        if (type == "bold")
        {
            return PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
        }
        return PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
    }

    private class ParagraphTitle
    {
        public int Index { get; set; }
        public string Title { get; set; }
        public List<string> Paragraphs { get; set; }

        public ParagraphTitle(int index, string title, string[] paragraphs)
        {
            this.Index = index;
            this.Title = title;
            this.Paragraphs = paragraphs.ToList();
        }
    }
}