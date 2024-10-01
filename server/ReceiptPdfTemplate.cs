using System;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace StripeExample
{
    public class ReceiptPdfTemplate : IDocument
    {
        private readonly ReceiptData _data;

        public ReceiptPdfTemplate(ReceiptData data)
        {
            _data = data;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Header().AlignCenter().ShowOnce().Element(ComposeHeader);
                page.Content().AlignCenter().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem()
                    .Column(column =>
                    {
                        column.Spacing(3);
                        column
                            .Item()
                            .Text("Donation Receipt")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor(Colors.Teal.Darken2);
                        column
                            .Item()
                            .Text(text =>
                            {
                                text.Span($"Receipt #: ").SemiBold();
                                text.Span($"{_data.ReceiptNumber}");
                            });

                        column
                            .Item()
                            .Text(text =>
                            {
                                text.Span("Issue date: ").SemiBold();
                                text.Span(
                                    $"{DateOnly.FromDateTime(_data.Time.ToLocalTime().DateTime)}"
                                );
                            });
                    });

                row.ConstantItem(100).Image("./MaKamiEduFndNewLogo-min.png");
            });
        }

        void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column
                    .Item()
                    .Row(row =>
                    {
                        row.Spacing(4);
                        //Donor and Donation Details
                        row.RelativeItem()
                            .ShowEntire()
                            .Column(column =>
                            {
                                column.Spacing(5);
                                column.Item().Text("Donor & Donation Details").Bold();
                                column.Item().PaddingBottom(5).PaddingTop(5).LineHorizontal(1);
                                column
                                    .Item()
                                    .Text(text =>
                                    {
                                        text.Span("Name: ").SemiBold();
                                        text.Span(_data.FullName);
                                    });
                                column
                                    .Item()
                                    .Text(text =>
                                    {
                                        text.Span("Email: ").SemiBold();
                                        text.Span(_data.Email);
                                    });
                                if (!string.IsNullOrWhiteSpace(_data.Phone))
                                {
                                    column
                                        .Item()
                                        .Text(text =>
                                        {
                                            text.Span("Phone: ").SemiBold();
                                            text.Span(_data.Phone);
                                        });
                                }
                                if (!string.IsNullOrWhiteSpace(_data.InSupportOf))
                                {
                                    column
                                        .Item()
                                        .Text(text =>
                                        {
                                            text.Span("In Support Of: ").SemiBold();
                                            text.Span(_data.InSupportOf);
                                        });
                                }
                                if (!string.IsNullOrWhiteSpace(_data.DonationDetails))
                                {
                                    column
                                        .Item()
                                        .Text(text =>
                                        {
                                            text.Span("Donation Details: ").SemiBold();
                                            text.Span(_data.DonationDetails);
                                        });
                                }
                                //donation amount
                                NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
                                nfi.CurrencyDecimalDigits = 2;
                                column
                                    .Item()
                                    .Text(text =>
                                    {
                                        text.Span("Amount: ").SemiBold();
                                        text.Span(_data.Amount.ToString("C", nfi));
                                    });
                            });
                        //Charity Details
                        row.RelativeItem()
                            .ShowEntire()
                            .Column(column =>
                            {
                                column.Spacing(5);
                                column.Item().Text("Charity Details").Bold();
                                column.Item().PaddingBottom(5).PaddingTop(5).LineHorizontal(1);
                                column
                                    .Item()
                                    .Text(text =>
                                    {
                                        text.Span("Name: ").SemiBold();
                                        text.Span("MaKami Education Foundation");
                                    });
                                column
                                    .Item()
                                    .Text(text =>
                                    {
                                        text.Span("Registered Charity Number: ").SemiBold();
                                        text.Span("862620");
                                    });
                                column
                                    .Item()
                                    .Text(text =>
                                    {
                                        text.Span("Phone: ").SemiBold();
                                        text.Span("(587) 393-7302");
                                    });
                                column
                                    .Item()
                                    .Text(text =>
                                    {
                                        text.Span("Address: ").SemiBold();
                                        text.Span(
                                            "1600 – 3800 Memorial Drive NE, Calgary, AB T2A 2K2"
                                        );
                                    });
                            });
                    });
                column.Item().PaddingTop(10).LineHorizontal(1);
                column
                    .Item()
                    .Text("A message from MaKami Education Foundation")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken2)
                    .Italic();
                column
                    .Item()
                    .PaddingTop(10)
                    .Text(
                        "Thank you for supporting MaKami Education Foundation! As a registered charity, "
                            + "100% of all donations go directly to help remove barriers to education for those in need."
                    );
            });
        }

        void ComposeFooter(IContainer container) { }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    }
}
