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
using iText.Kernel.Pdf.Event;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace desktop_manager.Utility
{
    // GENERATES PDF QUOTE DOCUMENTS
    public static class DocumentGenerator
    {
        // COMPANY SPECIFIC CONSTANT VALUES
        private const string CompanyName = "Balaio - Construção Civil, Unipessoal, Lda";
        private const string Alvara = "92195";
        private const string Nipc = "514818506";
        private const string TechnicalDirector = "Daniel Soares";
        private const string CompanyEmail = "geral@balaioconstrucao.pt";
        private const string Website = "www.balaioconstrucao.pt";
        private const string CompanyCellphone = "+351 926 332 656 (chamada para a rede móvel nacional)";

        // FOLDER AND FILE NAME CONSTANTS
        private const string QuotesDirectoryName = "Quotes";
        private const string CompanyInfoDirectoryName = "CompanyInfo";
        private const string LeftLogoFileName = "logo.png";
        private const string RightLogoFileName = "logo-right.png";

        // FONT TYPE IDENTIFIERS
        private const string FontBold = "bold";
        private const string FontRegular = "";

        // MAIN METHOD TO GENERATE A QUOTE PDF
        public static void GenerateQuote(
            string clientName,
            string clientAddress,
            string subject,
            ObservableCollection<Item> items,
            bool hasGlobalValue,
            bool hasDetailedValues,
            bool hasAutos,
            bool vat)
        {
            // GET THE STANDARD TERMS AND CONDITIONS FOR THE QUOTE
            List<ParagraphTitle> quoteTerms = GetQuoteTerms(hasAutos);

            // DEFINE QUOTE IDENTIFICATION DETAILS (COULD BE PARAMETERS)
            string quoteId = "36/2025";
            string date = DateTime.Now.ToString("dd/MM/yyyy");

            // SETUP FOLDER PATHS
            string quotesFolderPath = EnsureDirectoryExists(QuotesDirectoryName);
            string companyInfoFolderPath = EnsureDirectoryExists(CompanyInfoDirectoryName);

            // DEFINE LOGO FILE PATHS
            string leftLogoPath = Path.Combine(companyInfoFolderPath, LeftLogoFileName);
            string rightLogoPath = Path.Combine(companyInfoFolderPath, RightLogoFileName);

            // GENERATE A UNIQUE FILE PATH FOR THE PDF
            string filePath = GenerateUniquePdfFilePath(quotesFolderPath, clientName, subject);

            // CREATE AND WRITE THE PDF DOCUMENT
            GeneratePdfFile(
                filePath,
                leftLogoPath,
                rightLogoPath,
                clientName,
                clientAddress,
                subject,
                items,
                hasGlobalValue,
                vat,
                quoteTerms,
                quoteId,
                date
            );

            // OPEN THE GENERATED PDF FILE
            // CONSIDER MOVING THIS CALL OUTSIDE THE GENERATOR CLASS
            OpenFileWithDefaultApplication(filePath);
        }

        // GENERATES AND SAVES THE ACTUAL PDF FILE CONTENT
        private static void GeneratePdfFile(
            string filePath,
            string leftLogoPath,
            string rightLogoPath,
            string clientName,
            string clientAddress,
            string subject,
            ObservableCollection<Item> items,
            bool hasGlobalValue,
            bool vat,
            List<ParagraphTitle> quoteTerms,
            string quoteId,
            string date)
        {
            // USING STATEMENTS FOR RESOURCE MANAGEMENT (STREAMS, WRITERS, DOCUMENTS)
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (PdfWriter writer = new PdfWriter(stream))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document doc = new Document(pdf))
            {
                // SET DOCUMENT MARGINS (EXAMPLE USING 1 INCH = 72 POINTS, EXCEPT TOP)
                // ADJUST AS NEEDED
                doc.SetMargins(111f, 36f, 36f, 36f);

                // ADD THE HEADER EVENT HANDLER FOR LOGOS
                // ALLOWS CONTINUATION EVEN IF LOGOS ARE MISSING
                TryAddHeaderHandler(pdf, doc, leftLogoPath, rightLogoPath);

                // --- ADD CONTENT TO THE DOCUMENT ---

                // ADD INITIAL QUOTE ID AND DATE
                AddQuoteIdentification(doc, quoteId, date);

                // ADD COMPANY INFORMATION SECTION
                AddCompanyInfo(doc);

                // ADD CLIENT INFORMATION SECTION
                AddClientInfo(doc, clientName, clientAddress);

                // ADD QUOTE SUBJECT AND GLOBAL AMOUNT SUMMARY
                AddQuoteSummary(doc, subject, items, hasGlobalValue, vat);

                // ADD A PAGE BREAK BEFORE TERMS AND CONDITIONS
                doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                // ADD THE TERMS AND CONDITIONS SECTION
                AddTermsAndConditions(doc, quoteTerms);

                // DOCUMENT IS AUTOMATICALLY SAVED AND CLOSED WHEN USING STATEMENTS END
            }
        }

        // ADDS THE HEADER EVENT HANDLER SAFELY (HANDLES FILE NOT FOUND)
        private static void TryAddHeaderHandler(PdfDocument pdf, Document doc, string leftLogoPath, string rightLogoPath)
        {
            try
            {
                // CREATE AND REGISTER THE HANDLER FOR BOTH LOGOS
                // ASSUMES HEADER EVENT HANDLER CONSTRUCTOR ACCEPTS BOTH PATHS
                HeaderEventHandler headerHandler = new HeaderEventHandler(leftLogoPath, rightLogoPath, doc);
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, headerHandler);
            }
            catch (FileNotFoundException)
            {
                doc.SetTopMargin(36f);
            }
            catch (Exception)
            {
                doc.SetTopMargin(36f);
            }
        }

        // ADDS QUOTE ID AND DATE TO THE DOCUMENT
        private static void AddQuoteIdentification(Document doc, string quoteId, string date)
        {
            AddLabelValue(doc, "Quote ID:", quoteId);
            AddLabelValue(doc, "Date:", date);
        }

        // ADDS THE COMPANY DETAILS SECTION TO THE DOCUMENT
        private static void AddCompanyInfo(Document doc)
        {
            // CREATE A SECTION TITLE
            doc.Add(CreateSectionTitle("Company ID"));

            // ADD EACH PIECE OF COMPANY INFO USING LABEL/VALUE HELPER
            AddLabelValue(doc, "Company Name:", CompanyName);
            AddLabelValue(doc, "Alvará nº:", Alvara);
            AddLabelValue(doc, "NIPC:", Nipc);
            AddLabelValue(doc, "Direção Técnica:", TechnicalDirector);
            AddLabelValue(doc, "Email:", CompanyEmail);
            AddLabelValue(doc, "Website:", Website);
            AddLabelValue(doc, "Cellphone:", CompanyCellphone);
        }

        // ADDS THE CLIENT DETAILS SECTION TO THE DOCUMENT
        private static void AddClientInfo(Document doc, string clientName, string clientAddress)
        {
            // CREATE A SECTION TITLE
            doc.Add(CreateSectionTitle("Client ID"));

            // ADD CLIENT NAME AND ADDRESS
            AddLabelValue(doc, "Client Name:", clientName);
            AddLabelValue(doc, "Client Address:", clientAddress);
        }

        // ADDS THE QUOTE SUBJECT AND CALCULATED GLOBAL AMOUNT
        private static void AddQuoteSummary(Document doc, string subject, ObservableCollection<Item> items, bool hasGlobalValue, bool vat)
        {
            // CREATE A SECTION TITLE
            doc.Add(CreateSectionTitle("Quote details"));

            // ADD THE SUBJECT
            AddLabelValue(doc, "Subject:", subject);

            // CALCULATE AND FORMAT THE GLOBAL AMOUNT STRING
            string globalAmountString = "N/A"; // DEFAULT IF NOT SHOWING GLOBAL VALUE
            if (hasGlobalValue && items != null) // CHECK ITEMS IS NOT NULL
            {
                // SUM TOTALS FROM THE ITEMS COLLECTION
                decimal total = items.Sum(item => item.Total); // ASSUMES ITEM.TOTAL IS DECIMAL
                // FORMAT AS CURRENCY FOR PORTUGAL LOCALE
                globalAmountString = total.ToString("C2", CultureInfo.GetCultureInfo("pt-PT"));
                // APPEND VAT INFORMATION IF APPLICABLE
                if (vat)
                {
                    globalAmountString += " + IVA";
                }
            }

            // ADD THE CALCULATED GLOBAL AMOUNT LABEL/VALUE
            AddLabelValue(doc, "Global Amount:", globalAmountString);
        }

        // ADDS THE NUMBERED TERMS AND CONDITIONS SECTION
        private static void AddTermsAndConditions(Document doc, List<ParagraphTitle> quoteTerms)
        {
            // CHECK IF TERMS LIST IS VALID
            if (quoteTerms == null) return;

            // LOOP THROUGH EACH TERM SECTION (PARAGRAPHTITLE OBJECT)
            foreach (ParagraphTitle termSection in quoteTerms)
            {
                // ADD THE TERM SECTION USING THE HELPER METHOD
                AddParagraphTitleSection(doc, termSection);
            }
        }

        // CREATES A STANDARD SECTION TITLE PARAGRAPH
        private static Paragraph CreateSectionTitle(string title)
        {
            // GET THE BOLD FONT
            PdfFont font = GetFont(FontBold);
            // CREATE PARAGRAPH, SET FONT, SIZE, AND TOP MARGIN
            return new Paragraph(title)
                .SetFont(font)
                .SetFontSize(12)
                .SetMarginTop(20); // ADD SPACE BEFORE THE SECTION TITLE
        }

        // ENSURES A SUBDIRECTORY EXISTS WITHIN THE USER'S DOCUMENTS/SIMPLEQUOTES FOLDER
        private static string EnsureDirectoryExists(string subDirectory)
        {
            // BUILD THE FULL PATH (E.G., C:\USERS\USER\DOCUMENTS\SIMPLEQUOTES\QUOTES)
            string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "SimpleQuotes", // MAIN APPLICATION FOLDER
                subDirectory);  // SPECIFIC SUBFOLDER (E.G., QUOTES)

            // CREATE THE DIRECTORY IF IT DOESN'T ALREADY EXIST
            Directory.CreateDirectory(folderPath);

            // RETURN THE FULL PATH TO THE DIRECTORY
            return folderPath;
        }

        // GENERATES A UNIQUE FILE PATH, AVOIDING OVERWRITES
        private static string GenerateUniquePdfFilePath(string folderPath, string clientName, string subject)
        {
            // CREATE A BASE FILENAME (SANITIZE POTENTIAL INVALID CHARS)
            string baseFileName = $"{clientName} - {subject}.pdf";
            string sanitizedBaseFileName = SanitizeFileName(baseFileName); // BASIC SANITIZATION
            string filePath = Path.Combine(folderPath, sanitizedBaseFileName);

            // CHECK FOR FILE EXISTENCE AND APPEND COUNTER IF NEEDED
            int fileCount = 1;
            // LOOP WHILE A FILE WITH THE CURRENT NAME EXISTS
            while (File.Exists(filePath))
            {
                // CREATE A NEW FILENAME WITH A COUNTER (E.G., CLIENT - SUBJ (1).PDF)
                string numberedFileName = $"{Path.GetFileNameWithoutExtension(sanitizedBaseFileName)} ({fileCount}){Path.GetExtension(sanitizedBaseFileName)}";
                // UPDATE THE FILE PATH
                filePath = Path.Combine(folderPath, numberedFileName);
                // INCREMENT THE COUNTER
                fileCount++;
            }

            // RETURN THE UNIQUE FILE PATH
            return filePath;
        }
        
        // BASIC FILE NAME SANITIZATION TO REMOVE INVALID CHARACTERS
        private static string SanitizeFileName(string fileName)
        {
            // GET INVALID CHARACTERS FOR FILE NAMES
            char[] invalidChars = Path.GetInvalidFileNameChars();
            // REPLACE EACH INVALID CHARACTER WITH AN UNDERSCORE
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }


        // ADDS A STANDARD LABEL (BOLD) AND VALUE (REGULAR) LINE TO THE DOCUMENT
        private static void AddLabelValue(Document doc, string label, string value)
        {
            // GET REQUIRED FONTS
            PdfFont regularFont = GetFont(FontRegular);
            PdfFont boldFont = GetFont(FontBold);

            // CREATE A NEW PARAGRAPH
            Paragraph p = new Paragraph();
            // ADD THE LABEL TEXT WITH BOLD FONT
            p.Add(new Text(label).SetFont(boldFont));
            // ADD A SPACE AND THE VALUE TEXT WITH REGULAR FONT
            p.Add(new Text(" " + value).SetFont(regularFont));
            // ADD THE COMPLETED PARAGRAPH TO THE DOCUMENT
            doc.Add(p);
        }

        // ADDS A NUMBERED PARAGRAPH TITLE SECTION (LIKE TERMS) TO THE DOCUMENT
        private static void AddParagraphTitleSection(Document doc, ParagraphTitle paragraphTitle)
        {
             // CHECK FOR NULL INPUT
             if (paragraphTitle == null || paragraphTitle.Paragraphs == null) return;

            // GET REQUIRED FONTS
            PdfFont regularFont = GetFont(FontRegular);
            PdfFont boldFont = GetFont(FontBold);

            // CREATE THE MAIN TITLE PARAGRAPH (E.G., "1. TITLE")
            Paragraph titleParagraph = new Paragraph();
            titleParagraph.Add(new Text(paragraphTitle.Index + ". " + paragraphTitle.Title).SetFont(boldFont));

            // CREATE THE PARAGRAPH FOR THE NUMBERED SUB-ITEMS
            Paragraph contentParagraph = new Paragraph().SetMarginLeft(12); // INDENT SUB-ITEMS

            // LOOP THROUGH EACH SUB-PARAGRAPH TEXT
            for (int i = 0; i < paragraphTitle.Paragraphs.Count; i++)
            {
                // ADD THE SUB-ITEM NUMBER (E.G., "1.1. ") IN BOLD
                contentParagraph.Add(new Text(paragraphTitle.Index + "." + (i + 1) + ". ").SetFont(boldFont));
                // ADD THE SUB-ITEM TEXT IN REGULAR FONT, FOLLOWED BY A NEWLINE
                contentParagraph.Add(new Text(paragraphTitle.Paragraphs[i] + "\n").SetFont(regularFont));
            }

            // JUSTIFY THE TEXT WITHIN THE SUB-ITEM PARAGRAPH
            contentParagraph.SetTextAlignment(TextAlignment.JUSTIFIED);

            // ADD THE TITLE AND CONTENT PARAGRAPHS TO THE DOCUMENT
            doc.Add(titleParagraph);
            doc.Add(contentParagraph);
        }

        // RETRIEVES A STANDARD PDF FONT (TIMES ROMAN OR TIMES BOLD)
        private static PdfFont GetFont(string type)
        {
            // RETURN BOLD FONT IF REQUESTED
            if (type == FontBold)
            {
                return PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
            }
            // RETURN REGULAR FONT BY DEFAULT
            return PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
        }

        // OPENS THE SPECIFIED FILE USING THE SYSTEM'S DEFAULT APPLICATION
        private static void OpenFileWithDefaultApplication(string filePath)
        {
            // USE TRY CATCH TO HANDLE POTENTIAL ERRORS (FILE NOT FOUND, NO ASSOCIATION)
            try
            {
                // CREATE PROCESS START INFO
                // SET USESHELLEXECUTE TO TRUE TO USE OS FILE ASSOCIATIONS
                ProcessStartInfo startInfo = new ProcessStartInfo(filePath)
                {
                    UseShellExecute = true
                };
                // START THE PROCESS
                Process.Start(startInfo);
            }
            catch (Exception)
            {
                 // LOGGING REMOVED AS PER REQUEST
                 // FAILED TO OPEN FILE AUTOMATICALLY
            }
        }


        // --- DATA DEFINITION HELPERS ---

        // BUILDS AND RETURNS THE LIST OF STANDARD QUOTE TERMS AND CONDITIONS
        private static List<ParagraphTitle> GetQuoteTerms(bool hasAutos)
        {
            // DEFINE PAYMENT CONDITION STRINGS BASED ON THE HASAUTOS FLAG
            string paymentConditionMain = hasAutos
                ? "Na adjudicação será faturado 50% do valor global da obra. O restante valor deverá ser pago de acordo com autos de medição realizados no dia 25 de cada mês. Estas condições poderão ser alteradas de forma a uma melhor adequação às necessidades de ambas as partes (a combinar)."
                : "Na adjudicação será faturado 50% do valor global da obra. O restante valor será faturado na conclusão da obra. Estas condições poderão ser alteradas de forma a uma melhor adequação às necessidades de ambas as partes (a combinar).";

            // DEFINE OTHER STATIC TEXTS FOR TERMS
            string paymentConditionPenalty = "O incumprimento do referido prazo de pagamento a que o Cliente fica obrigado implicará, por um lado a suspensão dos trabalhos até a regularização do pagamento em falta e, por outro, dará também origem ao pagamento de juros de mora sobre o montante em divida calculados à taxa legal em vigor.";
            string paymentConditionVat = "Sobre o preço global da empreitada incide o IVA à taxa legal em vigor.";
            // ... (define other strings if they become dynamic)

            // CREATE AND POPULATE THE LIST OF PARAGRAPHTITLE OBJECTS
            // EACH OBJECT REPRESENTS A NUMBERED SECTION IN THE TERMS
            return new List<ParagraphTitle>()
            {
                // SECTION 1
                new ParagraphTitle(1, "Prazo de execução da obra", ["A combinar."]),
                // SECTION 2
                new ParagraphTitle(2, "Condições de pagamento da obra", [
                    paymentConditionMain,
                    paymentConditionPenalty,
                    paymentConditionVat
                ]),
                // SECTION 3
                new ParagraphTitle(3, "Garantias da obra", ["Os trabalhos executados estão garantidos em conformidade com o disposto na lei em vigor."]),
                // SECTION 4
                new ParagraphTitle(4, "Propriedade da obra", ["A propriedade dos materiais a aplicar será do empreiteiro, até o pagamento integral da empreitada. Cabe ao Dono da Obra a responsabilidade sobre os materiais durante o tempo que permanecerem em obra, garantindo uma eventual indemnização ao empreiteiro caso, por qualquer causa, os materiais sofram danos e/ou perdas totais ou parciais."]),
                // SECTION 5
                new ParagraphTitle(5, "Responsabilidades do Dono da Obra", [
                    "Acesso a toda a área a intervir.",
                    "Cedência de um local seguro para o acondicionamento de materiais, ferramentas e equipamentos.",
                    "Licenças e autorizações necessárias para a realização da empreitada.",
                    "Licenças e taxas municipais, taxas de ocupação de via pública e o respetivo pagamento.",
                    "Fornecimento de água e energia elétrica."
                ]),
                // SECTION 6
                new ParagraphTitle(6, "Responsabilidades do Empreiteiro da Obra", [
                    "Todos os trabalhos descritos serão executados segundo as normas de segurança e saúde aplicáveis e segundo as boas regras e técnicas de construção.",
                    "Fornecimento e execução de mão-de-obra, material e equipamentos para a realização da obra.",
                    "Remoção do entulho proveniente da obra, e limpeza de toda a área de trabalho,", // NOTE TRAILING COMMA IN ORIGINAL
                    "Apoio técnico especializado.",
                    "Responsabilidade civil e seguros."
                ]),
                // SECTION 7
                new ParagraphTitle(7, "Exclusões de obra", ["A proposta não inclui qualquer tipo de trabalhos a executar que não estejam especificados na listagem abaixo citada."]), // NOTE: REFERS TO A "LISTAGEM ABAIXO CITADA" NOT PRESENT IN TERMS
                // SECTION 8
                new ParagraphTitle(8, "Outras condições", [
                    "Apenas nos responsabilizamos por anomalias que possam surgir após o término da empreitada, e que tenha uma relação direta sobre os trabalhos executados, e simultaneamente que a empreitada esteja totalmente liquidada.",
                    "Todos os trabalhos requisitados que não estejam neste orçamento, serão orçamentados como trabalhos a mais.",
                    "Não é da responsabilidade do empreiteiro a garantia da resolução dos problemas apresentados pelo Dono da obra, quando apenas se realizem intervenções pontuais.",
                    "O valor global da empreitada é apenas referente à adjudicação na sua totalidade, ficando o empreiteiro com o direito de reformular o orçamento caso o Dono da Obra pretenda uma adjudicação parcial da obra orçamentada.",
                    "O Dono da Obra deverá partilhar com o Empreiteiro da Obra todos os danos existentes antes do início da empreitada, com registos fotográficos."
                ]),
                // SECTION 9
                new ParagraphTitle(9, "Validade da proposta", ["A proposta é válida por 15 dias."])
            };
        }

        // --- NESTED CLASS FOR TERMS DATA ---

        // SIMPLE CLASS TO HOLD DATA FOR A NUMBERED PARAGRAPH SECTION (TITLE AND SUB-PARAGRAPHS)
        // NESTED BECAUSE IT'S ONLY USED WITHIN DOCUMENTGENERATOR
        private class ParagraphTitle
        {
            // SECTION INDEX (E.G., 1, 2, 3)
            public int Index { get; set; }
            // SECTION TITLE TEXT
            public string Title { get; set; }
            // LIST OF SUB-PARAGRAPH TEXTS WITHIN THE SECTION
            public List<string> Paragraphs { get; set; }

            // CONSTRUCTOR TO INITIALIZE THE OBJECT
            public ParagraphTitle(int index, string title, string[] paragraphs)
            {
                this.Index = index;
                this.Title = title;
                // CONVERT ARRAY TO LIST
                this.Paragraphs = paragraphs.ToList();
            }
        }
    }
}