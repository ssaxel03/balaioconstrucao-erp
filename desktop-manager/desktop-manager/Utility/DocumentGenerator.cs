using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using desktop_manager.Models; // ENSURE THIS USING IS CORRECT FOR ITEM MODEL
using iText.IO.Font.Constants;
using iText.Kernel.Colors; // NEEDED FOR BACKGROUND COLOR AND BORDERS
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw; // NEEDED FOR LINESEPARATOR
using iText.Kernel.Pdf.Event;
using iText.Layout;
using iText.Layout.Borders; // NEEDED FOR BORDER STYLING
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
            bool hasDetailedValues, // THIS FLAG CONTROLS THE ITEM TABLE STRUCTURE
            bool hasAutos,
            bool vat) // VAT FLAG USED FOR TOTAL DISPLAY
        {
            // GET THE STANDARD TERMS AND CONDITIONS FOR THE QUOTE
            List<ParagraphTitle> quoteTerms = GetQuoteTerms(hasAutos);

            // DEFINE QUOTE IDENTIFICATION DETAILS (COULD BE PARAMETERS)
            string quoteId = "36/2025";
            // USE INVARIANT CULTURE FOR CONSISTENT DATE FORMATTING REGARDLESS OF SYSTEM LOCALE
            string date = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

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
                vat, // PASS VAT FLAG
                quoteTerms,
                quoteId,
                date,
                hasDetailedValues // PASS THE FLAG CONTROLLING TABLE VIEW
            );

            // OPEN THE GENERATED PDF FILE
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
            bool vat, // ACCEPT VAT FLAG
            List<ParagraphTitle> quoteTerms,
            string quoteId,
            string date,
            bool hasDetailedValues) // ACCEPT THE FLAG CONTROLLING TABLE VIEW
        {
            // USE USING STATEMENTS FOR RESOURCE MANAGEMENT (STREAMS, WRITERS, DOCUMENTS)
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (PdfWriter writer = new PdfWriter(stream))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document doc = new Document(pdf))
            {
                // SET DOCUMENT MARGINS (TOP IS LARGE FOR HEADER, OTHERS ARE TIGHTER)
                doc.SetMargins(111f, 36f, 36f, 36f);

                // ADD THE HEADER EVENT HANDLER FOR LOGOS
                // RESETS TOP MARGIN IF LOGOS ARE MISSING OR FAIL TO LOAD
                TryAddHeaderHandler(pdf, doc, leftLogoPath, rightLogoPath);

                // --- ADD CONTENT TO THE DOCUMENT (PAGE 1) ---

                // ADD INITIAL QUOTE ID AND DATE
                AddQuoteIdentification(doc, quoteId, date);

                // ADD COMPANY INFORMATION SECTION
                AddCompanyInfo(doc);

                // ADD CLIENT INFORMATION SECTION
                AddClientInfo(doc, clientName, clientAddress);

                // ADD QUOTE SUBJECT AND GLOBAL AMOUNT SUMMARY (IF ENABLED)
                AddQuoteSummary(doc, subject, items, hasGlobalValue, vat);

                // --- PAGE BREAK BEFORE TERMS ---
                doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                // --- ADD TERMS AND CONDITIONS ---
                AddTermsAndConditions(doc, quoteTerms);

                // --- PAGE BREAK BEFORE TABLE ---
                doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                // --- ADD ITEMS TABLE ---
                // PASS VAT FLAG FOR CORRECT TOTAL DISPLAY
                AddItemsTable(doc, items, hasDetailedValues, vat, hasGlobalValue);

                // DOCUMENT IS AUTOMATICALLY SAVED AND CLOSED WHEN USING STATEMENTS END
            }
        }

        // ADDS THE HEADER EVENT HANDLER SAFELY (HANDLES FILE NOT FOUND)
        // RESETS TOP MARGIN ON FAILURE
        private static void TryAddHeaderHandler(PdfDocument pdf, Document doc, string leftLogoPath, string rightLogoPath)
        {
            try
            {
                // CREATE AND REGISTER THE HANDLER FOR BOTH LOGOS
                HeaderEventHandler headerHandler = new HeaderEventHandler(leftLogoPath, rightLogoPath, doc);
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, headerHandler);
            }
            // IF LOGOS ARE NOT FOUND
            catch (FileNotFoundException)
            {
                // RESET TOP MARGIN TO STANDARD IF HEADER CAN'T BE ADDED
                doc.SetTopMargin(36f);
            }
            // IF ANY OTHER ERROR OCCURS DURING HEADER SETUP
            catch (Exception)
            {
                // RESET TOP MARGIN TO STANDARD IF HEADER CAN'T BE ADDED
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

        // ADDS THE QUOTE SUBJECT AND CALCULATED GLOBAL AMOUNT (IF ENABLED)
        private static void AddQuoteSummary(Document doc, string subject, ObservableCollection<Item> items, bool hasGlobalValue, bool vat)
        {
            // CREATE A SECTION TITLE
            doc.Add(CreateSectionTitle("Quote details"));

            // ADD THE SUBJECT
            AddLabelValue(doc, "Subject:", subject);

            // ADD GLOBAL AMOUNT ONLY IF FLAG IS TRUE AND ITEMS EXIST
            if (hasGlobalValue && items != null)
            {
                string globalAmountString;
                // SUM TOTALS FROM THE ITEMS COLLECTION
                decimal total = items.Sum(item => item.Total);
                // FORMAT AS CURRENCY FOR PORTUGAL LOCALE
                globalAmountString = total.ToString("C2", CultureInfo.GetCultureInfo("pt-PT"));
                // APPEND VAT INFORMATION IF APPLICABLE
                if (vat)
                {
                    globalAmountString += " + IVA";
                }
                // ADD THE CALCULATED GLOBAL AMOUNT LABEL/VALUE
                AddLabelValue(doc, "Global Amount:", globalAmountString);
            }
        }

        // ADDS THE TABLE OF ITEMS TO THE DOCUMENT
        // TABLE STRUCTURE CHANGES BASED ON WHETHER DETAILED VALUES ARE SHOWN
        // VAT FLAG CONTROLS IF "+ IVA" IS APPENDED TO TOTALS
        private static void AddItemsTable(Document doc, ObservableCollection<Item> items, bool showDetailedValues, bool vat, bool globalValue) // ACCEPT VAT FLAG
        {
            // DO NOTHING IF THE ITEMS LIST IS NULL OR EMPTY
            if (items == null || items.Count == 0)
            {
                return;
            }

            // --- SETUP ---
            // CALCULATE THE GRAND TOTAL FROM ALL ITEMS
            decimal grandTotal = items.Sum(item => item.Total);
            // GET CULTURE INFO FOR CORRECT CURRENCY FORMATTING (E.G., €)
            CultureInfo portugueseCulture = CultureInfo.GetCultureInfo("pt-PT");
            // GET FONTS
            PdfFont boldFont = GetFont(FontBold);
            PdfFont regularFont = GetFont(FontRegular);
            // DEFINE COLUMN WIDTHS AS PERCENTAGES
            float[] columnWidths = { 5f, 60f, 20f }; // ITEM ID, DESCRIPTION, PRICE

            // CREATE THE TABLE OBJECT
            Table table = new Table(UnitValue.CreatePercentArray(columnWidths))
                .UseAllAvailableWidth() // MAKE TABLE USE THE FULL PAGE WIDTH MINUS MARGINS
                .SetMarginTop(10);

            // DEFINE LIGHT GRAY BACKGROUND COLOR FOR HEADERS AND TOTAL CELL
            Color lightGray = ColorConstants.LIGHT_GRAY;

            // --- TABLE HEADERS ---
            // ADD HEADER CELL FOR ITEM ID WITH BACKGROUND COLOR
            table.AddHeaderCell(new Cell().Add(new Paragraph("Item").SetFont(boldFont).SetFontSize(11f)).SetBackgroundColor(lightGray));
            // ADD HEADER CELL FOR DESCRIPTION WITH BACKGROUND COLOR
            table.AddHeaderCell(new Cell().Add(new Paragraph("Description").SetFont(boldFont).SetFontSize(11f)).SetBackgroundColor(lightGray));
            // ADD HEADER CELL FOR PRICE COLUMN (TEXT DEPENDS ON DETAILED VIEW) WITH BACKGROUND COLOR
            string priceColumnHeader = "Total";
            table.AddHeaderCell(new Cell().Add(new Paragraph(priceColumnHeader).SetFont(boldFont).SetFontSize(11f).SetTextAlignment(TextAlignment.RIGHT)).SetBackgroundColor(lightGray));

            // --- TABLE BODY ---
            if (showDetailedValues)
            {
                // DETAILED VIEW: SHOW PRICE FOR EACH ITEM AND A FINAL TOTAL ROW
                // USE A FOR LOOP (INSTEAD OF FOREACH) TO ITERATE
                int numItems = items.Count;
                for (int i = 0; i < numItems; i++)
                {
                    var item = items[i];

                    // ADD ITEM ID CELL (HANDLE POTENTIAL NULL ID)
                    table.AddCell(new Cell().Add(new Paragraph(item.Id ?? string.Empty).SetFont(regularFont).SetFontSize(11f)));
                    // ADD DESCRIPTION CELL (HANDLE POTENTIAL NULL DESCRIPTION)
                    table.AddCell(new Cell().Add(new Paragraph(item.Description ?? string.Empty).SetFont(regularFont).SetFontSize(11f)));
                    // ADD INDIVIDUAL ITEM PRICE CELL (RIGHT ALIGNED, FORMATTED AS CURRENCY)
                    table.AddCell(new Cell().Add(new Paragraph(item.Total.ToString("C2", portugueseCulture)).SetFont(regularFont).SetFontSize(11f).SetTextAlignment(TextAlignment.RIGHT)));
                }

                if (globalValue)
                {
                    // ADD AN EMPTY CELL SPANNING FIRST TWO COLUMNS TO ALIGN THE TOTAL VALUE CORRECTLY
                    // THIS CELL NOW HAS NO BORDERS AT ALL
                    Cell emptySpanCell = new Cell(1, 2).SetBorder(null);
                    table.AddCell(emptySpanCell);

                    // PREPARE TOTAL STRING WITH VAT IF NEEDED
                    string totalString = grandTotal.ToString("C2", portugueseCulture);
                    if (vat)
                    {
                        totalString += " + IVA";
                    }   
                    
                    // ADD CELL FOR THE GRAND TOTAL VALUE WITH BACKGROUND AND DEFAULT BORDERS
                    Cell totalValueCell = new Cell()
                        .Add(new Paragraph(totalString)) // USE THE PREPARED TOTAL STRING
                        .SetFont(boldFont)
                        .SetFontSize(11f)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetBackgroundColor(lightGray); // SET BACKGROUND COLOR
                    // DO NOT SET BORDER TO NULL TO RETAIN DEFAULT TABLE BORDERS

                    table.AddCell(totalValueCell);
                }
                
            }
            else
            {
                // NON DETAILED VIEW: SHOW ONLY GRAND TOTAL IN A SINGLE CELL SPANNING ALL ROWS
                int numItems = items.Count;
                // CHECK IF THERE ARE ITEMS TO DISPLAY
                if (numItems > 0)
                {
                    // PREPARE TOTAL STRING WITH VAT IF NEEDED
                    string totalString = grandTotal.ToString("C2", portugueseCulture);
                    if (vat)
                    {
                        totalString += " + IVA";
                    }

                    // CREATE THE SINGLE CELL FOR THE TOTAL PRICE
                    // IT SPANS ALL ITEM ROWS (NUMITEMS) IN THE THIRD COLUMN (1)
                    Cell totalValueSpanningCell = new Cell(numItems, 1)
                        .Add(new Paragraph(totalString)) // USE THE PREPARED TOTAL STRING
                        .SetFont(boldFont) // MAKE TOTAL BOLD
                        .SetFontSize(11f)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE) // CENTER VERTICALLY IN SPANNED AREA
                        .SetBackgroundColor(lightGray); // SET BACKGROUND COLOR

                    // LOOP THROUGH ITEMS TO ADD ID AND DESCRIPTION
                    for (int i = 0; i < numItems; i++)
                    {
                        var item = items[i];
                        // ADD ITEM ID CELL (HANDLE POTENTIAL NULL ID)
                        table.AddCell(new Cell().Add(new Paragraph(item.Id ?? string.Empty).SetFont(regularFont).SetFontSize(11f)));
                        // ADD DESCRIPTION CELL
                        table.AddCell(new Cell().Add(new Paragraph(item.Description ?? string.Empty).SetFont(regularFont).SetFontSize(11f)));

                        // ADD THE SPANNING TOTAL CELL ONLY ONCE (FOR THE FIRST ROW)
                        if (i == 0)
                        {
                            table.AddCell(totalValueSpanningCell);
                        }
                    }
                }
                // NO SEPARATE TOTAL ROW IS ADDED IN THIS CASE
            }

            // ADD THE COMPLETED TABLE TO THE DOCUMENT
            doc.Add(table);
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
                .SetFontSize(11f) // USE FLOAT FOR FONT SIZE
                .SetMarginTop(20); // ADD SPACE BEFORE THE SECTION TITLE
        }

        // ENSURES A SUBDIRECTORY EXISTS WITHIN THE USER'S DOCUMENTS/SIMPLEQUOTES FOLDER
        private static string EnsureDirectoryExists(string subDirectory)
        {
            // BUILD THE FULL PATH
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
            // CREATE A BASE FILENAME
            string baseFileName = $"{clientName} - {subject}.pdf";
            // REMOVE INVALID CHARACTERS FROM FILENAME
            string sanitizedBaseFileName = SanitizeFileName(baseFileName);
            // COMBINE FOLDER AND FILENAME
            string filePath = Path.Combine(folderPath, sanitizedBaseFileName);

            // INITIALIZE FILE COUNTER FOR UNIQUENESS CHECK
            int fileCount = 1;
            // LOOP WHILE A FILE WITH THE CURRENT NAME EXISTS
            while (File.Exists(filePath))
            {
                // CREATE A NEW FILENAME WITH A COUNTER (E.G., FILENAME (1).PDF)
                string numberedFileName = $"{Path.GetFileNameWithoutExtension(sanitizedBaseFileName)} ({fileCount}){Path.GetExtension(sanitizedBaseFileName)}";
                // UPDATE THE FILE PATH TO CHECK AGAIN
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
            // GET INVALID CHARACTERS DEFINED BY THE OS
            char[] invalidChars = Path.GetInvalidFileNameChars();
            // REPLACE EACH INVALID CHARACTER WITH AN UNDERSCORE AND REMOVE EMPTY PARTS
            // TRIM ANY TRAILING PERIODS WHICH ARE INVALID AT THE END OF FILENAMES
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }


        // ADDS A STANDARD LABEL (BOLD) AND VALUE (REGULAR) LINE TO THE DOCUMENT AT SIZE 11
        private static void AddLabelValue(Document doc, string label, string value)
        {
            // GET REQUIRED FONTS
            PdfFont regularFont = GetFont(FontRegular);
            PdfFont boldFont = GetFont(FontBold);

            // CREATE A NEW PARAGRAPH
            Paragraph p = new Paragraph();
            // ADD THE LABEL TEXT WITH BOLD FONT AND SIZE 11
            p.Add(new Text(label).SetFont(boldFont).SetFontSize(11f));
            // ADD A SPACE AND THE VALUE TEXT WITH REGULAR FONT AND SIZE 11
            p.Add(new Text(" " + value).SetFont(regularFont).SetFontSize(11f));
            // ADD THE COMPLETED PARAGRAPH TO THE DOCUMENT
            doc.Add(p);
        }

        // ADDS A NUMBERED PARAGRAPH TITLE SECTION (LIKE TERMS) TO THE DOCUMENT
        private static void AddParagraphTitleSection(Document doc, ParagraphTitle paragraphTitle)
        {
             // CHECK FOR NULL INPUT TO PREVENT ERRORS
             if (paragraphTitle == null || paragraphTitle.Paragraphs == null) return;

            // GET REQUIRED FONTS
            PdfFont regularFont = GetFont(FontRegular);
            PdfFont boldFont = GetFont(FontBold);

            // CREATE THE MAIN TITLE PARAGRAPH (E.G., "1. TITLE") WITH BOLD FONT
            Paragraph titleParagraph = new Paragraph();
            titleParagraph.Add(new Text(paragraphTitle.Index + ". " + paragraphTitle.Title).SetFont(boldFont));

            // CREATE THE PARAGRAPH FOR THE NUMBERED SUB-ITEMS WITH INDENTATION
            Paragraph contentParagraph = new Paragraph().SetMarginLeft(12);

            // LOOP THROUGH EACH SUB-PARAGRAPH TEXT
            for (int i = 0; i < paragraphTitle.Paragraphs.Count; i++)
            {
                // ADD THE SUB-ITEM NUMBER (E.G., "1.1. ") IN BOLD
                contentParagraph.Add(new Text(paragraphTitle.Index + "." + (i + 1) + ". ").SetFont(boldFont));
                // ADD THE SUB-ITEM TEXT IN REGULAR FONT FOLLOWED BY A NEWLINE
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
            // USE TRY CATCH TO HANDLE POTENTIAL ERRORS
            try
            {
                // CREATE PROCESS START INFO USING THE FILE PATH
                // SET USESHELLEXECUTE TO TRUE TO USE OS FILE ASSOCIATIONS
                ProcessStartInfo startInfo = new ProcessStartInfo(filePath)
                {
                    UseShellExecute = true
                };
                // START THE PROCESS (E.G., OPENING THE PDF IN THE DEFAULT READER)
                Process.Start(startInfo);
            }
            // CATCH ANY EXCEPTION DURING FILE OPENING
            catch (Exception)
            {
                 // FAILED TO OPEN FILE AUTOMATICALLY NO ACTION TAKEN
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
                    "Remoção do entulho proveniente da obra, e limpeza de toda a área de trabalho,",
                    "Apoio técnico especializado.",
                    "Responsabilidade civil e seguros."
                ]),
                // SECTION 7
                new ParagraphTitle(7, "Exclusões de obra", ["A proposta não inclui qualquer tipo de trabalhos a executar que não estejam especificados na listagem abaixo citada."]),
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
                // CONVERT ARRAY TO LIST FOR MUTABILITY IF NEEDED LATER
                this.Paragraphs = paragraphs.ToList();
            }
        }
    }
}