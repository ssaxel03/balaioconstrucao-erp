using iText.IO.Image;
using iText.Kernel.Pdf.Event;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.IO;

namespace desktop_manager.Utility
{
    // HANDLES ADDING PAGE HEADERS (LOGOS) TO THE PDF DOCUMENT
    public class HeaderEventHandler : AbstractPdfDocumentEventHandler
    {
        // IMAGE ELEMENT FOR THE LOGO ON THE LEFT SIDE
        private readonly Image _leftLogo;
        // IMAGE ELEMENT FOR THE LOGO ON THE RIGHT SIDE (CAN BE NULL)
        private readonly Image _rightLogo;

        // TARGET HEIGHT FOR BOTH LOGOS IN POINTS
        private readonly float _logoHeight = 75f;

        // STORED DOCUMENT MARGINS FOR POSITIONING CALCULATIONS
        private float _marginLeft;
        private float _marginRight;
        private float _marginTop;

        // MANUALLY CALCULATED SCALED WIDTH FOR THE RIGHT LOGO (POINTS)
        private readonly float _rightLogoScaledWidth = 0f;

        // CONSTRUCTOR FOR A HEADER WITH ONLY A LEFT LOGO
        public HeaderEventHandler(string leftLogoPath, Document doc)
        {
            // STORE MARGINS FROM THE DOCUMENT OBJECT
            InitializeMargins(doc);

            // LOAD AND PREPARE THE LEFT LOGO IMAGE ELEMENT
            _leftLogo = CreateImageElement(leftLogoPath, _logoHeight);
            // RIGHT LOGO REMAINS NULL BY DEFAULT
        }

        // CONSTRUCTOR FOR A HEADER WITH BOTH LEFT AND RIGHT LOGOS
        public HeaderEventHandler(string leftLogoPath, string rightLogoPath, Document doc)
        {
            // STORE MARGINS FROM THE DOCUMENT OBJECT
            InitializeMargins(doc);

            // LOAD AND PREPARE THE LEFT LOGO IMAGE ELEMENT
            _leftLogo = CreateImageElement(leftLogoPath, _logoHeight);

            // LOAD IMAGE DATA FOR THE RIGHT LOGO TO CALCULATE WIDTH
            ImageData rightLogoData = LoadImageData(rightLogoPath);
            // CALCULATE THE EXPECTED SCALED WIDTH BASED ON ORIGINAL DIMENSIONS
            _rightLogoScaledWidth = CalculateExpectedScaledWidth(rightLogoData, _logoHeight);
            // CREATE THE RIGHT LOGO IMAGE ELEMENT USING THE SAME IMAGE DATA
            _rightLogo = CreateImageElementFromData(rightLogoData, _logoHeight);
        }

        // SHARED METHOD TO STORE DOCUMENT MARGINS
        private void InitializeMargins(Document doc)
        {
            _marginLeft = doc.GetLeftMargin();
            _marginRight = doc.GetRightMargin();
            _marginTop = doc.GetTopMargin();
        }

        // SHARED METHOD TO LOAD IMAGE DATA AND HANDLE FILE NOT FOUND
        private ImageData LoadImageData(string imagePath)
        {
            // CHECK IF THE IMAGE FILE EXISTS AT THE GIVEN PATH
            if (!File.Exists(imagePath))
            {
                // THROW AN EXCEPTION IF THE FILE IS MISSING
                throw new FileNotFoundException("HEADER IMAGE NOT FOUND.", imagePath);
            }
            // CREATE AND RETURN IMAGE DATA FROM THE FILE
            return ImageDataFactory.Create(imagePath);
        }

        // SHARED METHOD TO CREATE AN IMAGE ELEMENT FROM A PATH
        private Image CreateImageElement(string imagePath, float targetHeight)
        {
            // LOAD THE IMAGE DATA FIRST (INCLUDES FILE CHECK)
            ImageData imageData = LoadImageData(imagePath);
            // CREATE AND RETURN THE IMAGE ELEMENT FROM LOADED DATA
            return CreateImageElementFromData(imageData, targetHeight);
        }

        // SHARED METHOD TO CREATE AN IMAGE ELEMENT FROM EXISTING IMAGE DATA
        private Image CreateImageElementFromData(ImageData imageData, float targetHeight)
        {
            // CREATE THE IMAGE ELEMENT
            Image image = new Image(imageData);
            // SET THE IMAGE HEIGHT (EXPECTING IMPLICIT WIDTH SCALING)
            image.SetHeight(targetHeight);
            // NO SETAUTOSCALEWIDTH(TRUE) DUE TO PREVIOUS ISSUES
            return image;
        }

        // SHARED METHOD TO CALCULATE SCALED WIDTH MANUALLY
        private float CalculateExpectedScaledWidth(ImageData imageData, float targetHeight)
        {
            // GET ORIGINAL DIMENSIONS FROM IMAGE DATA
            float originalWidth = imageData.GetWidth();
            float originalHeight = imageData.GetHeight();

            // CALCULATE SCALED WIDTH MAINTAINING ASPECT RATIO
            // AVOID DIVISION BY ZERO
            if (originalHeight > 0)
            {
                return targetHeight * (originalWidth / originalHeight);
            }

            // RETURN 0 IF ORIGINAL HEIGHT IS INVALID
            return 0f;
        }

        // HANDLES THE PDF DOCUMENT EVENT (E.G., END_PAGE)
        protected override void OnAcceptedEvent(AbstractPdfDocumentEvent @event)
        {
            // ENSURE IT'S THE CORRECT EVENT TYPE AND THE LEFT LOGO IS LOADED
            if (@event is PdfDocumentEvent docEvent && _leftLogo != null)
            {
                // GET CURRENT PAGE AND ITS SIZE
                PdfPage page = docEvent.GetPage();
                Rectangle pageSize = page.GetPageSize();

                // CREATE A CANVAS TO DRAW ON THE PAGE
                // NOTE CANVAS IS DISPOSABLE BUT USING STATEMENT IS AWKWARD HERE
                // SO MANUAL CLOSE IS USED LATER
                PdfCanvas pdfCanvas = new PdfCanvas(page);
                Canvas canvas = new Canvas(pdfCanvas, pageSize);

                // POSITION AND ADD THE LEFT LOGO
                AddLeftLogo(canvas, pageSize);

                // POSITION AND ADD THE RIGHT LOGO IF IT EXISTS AND HAS VALID WIDTH
                if (_rightLogo != null && _rightLogoScaledWidth > 0)
                {
                    AddRightLogo(canvas, pageSize);
                }

                // FLUSH DRAWING OPERATIONS TO THE PAGE
                canvas.Close();
            }
            // IGNORE OTHER EVENT TYPES
            else if (!(@event is PdfDocumentEvent))
            {
                // NO ACTION NEEDED FOR UNEXPECTED EVENT TYPES
            }
        }

        // POSITIONS AND ADDS THE LEFT LOGO TO THE CANVAS
        private void AddLeftLogo(Canvas canvas, Rectangle pageSize)
        {
            // CALCULATE Y COORDINATE (BOTTOM OF LOGO AT TOP MARGIN LINE)
            float y = pageSize.GetTop() - _marginTop;
            // CALCULATE X COORDINATE (LEFT OF LOGO AT LEFT MARGIN LINE)
            float x = pageSize.GetLeft() + _marginLeft;
            // SET ABSOLUTE POSITION
            _leftLogo.SetFixedPosition(x, y);
            // ADD TO CANVAS
            canvas.Add(_leftLogo);
        }

        // POSITIONS AND ADDS THE RIGHT LOGO TO THE CANVAS
        private void AddRightLogo(Canvas canvas, Rectangle pageSize)
        {
             // CALCULATE Y COORDINATE (ALIGN WITH LEFT LOGO)
            float y = pageSize.GetTop() - _marginTop;
             // CALCULATE X COORDINATE (ALIGN RIGHT EDGE WITH RIGHT MARGIN USING MANUAL WIDTH)
            float x = pageSize.GetRight() - _marginRight - _rightLogoScaledWidth;
             // SET ABSOLUTE POSITION
            _rightLogo.SetFixedPosition(x, y);
             // ADD TO CANVAS
            canvas.Add(_rightLogo);
        }
    }
}