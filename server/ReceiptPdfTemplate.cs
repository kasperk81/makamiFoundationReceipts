using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace StripeExample
{
    public class ReceiptPdfTemplate : IDocument
    {
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4.Landscape());
                page.Header()
                    //.Section("Receipt")
                    .AlignCenter()
                    .ShowOnce()
                    .Element(ComposeHeader);
                page.Content()
                    //.Section("Receipt")
                    .AlignCenter()
                    .Element(ComposeContent);
                page.Footer() /*.Section("Receipt")*/
                    .Element(ComposeFooter);
            });
        }

        void ComposeHeader(IContainer container) { }

        void ComposeContent(IContainer container) { }

        void ComposeFooter(IContainer container) { }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    }
}
